using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pk3DS.Core;
using pk3DS.Core.CTR;

namespace pk3DS.WinForms
{
    public static class EnginePatcher7
    {
        public static void SyncEngineLimits(int moveCount)
        {
            if (Main.ExeFSPath == null) return;
            string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
            string codePath = Path.Combine(Main.ExeFSPath, binName);
            string battlePath = Path.Combine(Main.RomFSPath, "battle", "battle.cro");

            if (!File.Exists(codePath)) return;

            byte[] codeBin = File.ReadAllBytes(codePath);
            byte[] battleCro = File.Exists(battlePath) ? File.ReadAllBytes(battlePath) : null;
            bool needsSaveCode = false;
            bool needsSaveBattle = false;

            // 1. Patch Move ID Limits (CMP instructions)
            // Inclusive limits: we check for multiple vanilla variants
            int[] oldLimits = { 700, 716, 720, 721, 728, 737, 744, 800, 802, 805 };
            int[] regs = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }; 
            
            // Round moveCount to a valid ARM immediate if possible, or use closest lower valid
            int targetLimit = moveCount;
            
            foreach (int oldLimit in oldLimits)
            {
                if (oldLimit == targetLimit) continue;
                foreach (int r in regs)
                {
                    byte[] pattern = GetCmpInstruction(r, oldLimit);
                    byte[] patch = GetCmpInstruction(r, targetLimit);
                    if (pattern != null && patch != null)
                    {
                        if (ScanAndPatch(codeBin, pattern, patch)) {
                            needsSaveCode = true;
                        }
                        if (battleCro != null && ScanAndPatch(battleCro, pattern, patch)) {
                            needsSaveBattle = true;
                        }
                    }
                }
            }

            // 2. Safe Literal Patching (ONLY for the specific 728 limit, and only if 4-byte aligned)
            // This is safer than patching common numbers like 700.
            byte[] litPattern = BitConverter.GetBytes(728);
            byte[] litPatch = BitConverter.GetBytes(targetLimit);
            for (int i = 0x100000; i < codeBin.Length - 4; i += 4) // Skip first 1MB of code.bin for safety
            {
                if (codeBin[i] == litPattern[0] && codeBin[i+1] == litPattern[1] && codeBin[i+2] == litPattern[2] && codeBin[i+3] == litPattern[3])
                {
                    litPatch.CopyTo(codeBin, i);
                    needsSaveCode = true;
                }
            }

            if (battleCro != null)
            {
                if (ExpandMoveJumpTable(ref battleCro, targetLimit))
                {
                    needsSaveBattle = true;
                }
            }

            if (needsSaveCode) File.WriteAllBytes(codePath, codeBin);
            if (needsSaveBattle && battleCro != null) File.WriteAllBytes(battlePath, battleCro);
        }

        public static bool ExpandMoveJumpTable(ref byte[] data, int newCount)
        {
            // Move effect dispatcher pattern (USUM):
            // ADDLS PC, PC, R7, LSL #2 (07 F1 2F 90)
            
            byte[] dispatcherPattern = { 0x07, 0xF1, 0x2F, 0x90 };
            int dispatcherIdx = -1;
            for (int i = 0; i < data.Length - 4; i++)
            {
                if (data[i] == dispatcherPattern[0] && data[i+1] == dispatcherPattern[1] && 
                    data[i+2] == dispatcherPattern[2] && data[i+3] == dispatcherPattern[3])
                {
                    dispatcherIdx = i;
                    break;
                }
            }

            if (dispatcherIdx < 0) return false;

            int tableStart = dispatcherIdx + 8;
            
            // Detect current entries by scanning for branch instructions (0xEAxxxxxx)
            int currentEntries = 0;
            for (int i = tableStart; i < data.Length - 4; i += 4)
            {
                if (i + 4 > data.Length) break;
                uint word = BitConverter.ToUInt32(data, i);
                if ((word >> 24) == 0xEA) currentEntries++;
                else break;
            }
            if (currentEntries == 0) currentEntries = 721;

            int targetEntries = newCount + 1;
            if (targetEntries <= currentEntries) return false;

            // Expand CRO code segment
            int bytesToTable = (targetEntries * 4);
            int shimSize = 8;
            int expansionSize = bytesToTable + shimSize;
            
            int oldLen = data.Length;
            data = CROUtil.ExpandSegment(data, 'c', expansionSize);
            uint[] starts = CROUtil.GetSegmentStartIndices(data);
            uint codeStart = starts[0];
            uint segmentTableOffset = ReadU32(data, 0xC8);
            uint oldCodeLen = ReadU32(data, (int)segmentTableOffset + 4) - (uint)expansionSize;
            int newSpaceAbs = (int)(codeStart + oldCodeLen);

            // 1. Prepare the Shim (End of original code, start of new space)
            // pos: ADR R0, PC (E2 8F 00 00) -> R0 = pos + 8
            // pos+4: ADD PC, R0, R7, LSL #2 (E0 80 F1 07)
            WriteU32(data, 0xE28F0000, newSpaceAbs);
            WriteU32(data, 0xE080F107, newSpaceAbs + 4);

            int newTableAbs = newSpaceAbs + 8;

            // 2. Prepare the Table
            // Resolve Pound (Move 1) as template for new moves
            uint poundB = ReadU32(data, tableStart + 4);
            int poundTarget = GetBranchTarget(tableStart + 4, poundB);

            for (int i = 0; i < targetEntries; i++)
            {
                int entryAbs = newTableAbs + (i * 4);
                int target;
                if (i < currentEntries)
                {
                    uint oldB = ReadU32(data, tableStart + (i * 4));
                    target = GetBranchTarget(tableStart + (i * 4), oldB);
                }
                else
                {
                    target = poundTarget;
                }
                
                uint newB = CreateBranchInstruction(entryAbs, target);
                WriteU32(data, newB, entryAbs);
            }

            // 3. Patch Original Dispatcher
            // CMP R7, #oldLimit -> CMP R7, #newCount
            byte[] cmpNew = GetCmpInstruction(7, newCount);
            if (cmpNew != null) cmpNew.CopyTo(data, dispatcherIdx - 8);

            // ADDLS PC, PC, R7, LSL #2 -> B shim
            uint bShim = CreateBranchInstruction(dispatcherIdx, newSpaceAbs);
            WriteU32(data, bShim, dispatcherIdx);

            return true;
        }

        private static int GetBranchTarget(int pos, uint instr)
        {
            int offset = (int)(instr & 0xFFFFFF);
            if ((offset & 0x800000) != 0) offset |= unchecked((int)0xFF000000); // Sign extend
            return pos + 8 + (offset << 2);
        }

        private static uint CreateBranchInstruction(int pos, int target)
        {
            int offset = (target - pos - 8) >> 2;
            return 0xEA000000 | (uint)(offset & 0xFFFFFF);
        }

        private static uint ReadU32(byte[] data, int offset) => BitConverter.ToUInt32(data, offset);
        private static void WriteU32(byte[] data, uint value, int offset) => BitConverter.GetBytes(value).CopyTo(data, offset);


        private static bool ScanAndPatch(byte[] data, byte[] pattern, byte[] patch)
        {
            bool modified = false;
            for (int i = 0; i < data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j]) { match = false; break; }
                }
                if (match)
                {
                    patch.CopyTo(data, i);
                    modified = true;
                    i += pattern.Length - 1;
                }
            }
            return modified;
        }

        public static bool IsEnginePatched(int moveCount = 0)
        {
            string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
            string codePath = Path.Combine(Main.ExeFSPath, binName);
            if (!File.Exists(codePath)) return false;

            byte[] codeBin = File.ReadAllBytes(codePath);
            int[] regs = { 0, 1, 2, 5, 7 };
            foreach (int r in regs)
            {
                byte[] patch = GetCmpInstruction(r, moveCount);
                if (codeBin.AsSpan().IndexOf(patch) >= 0) return true;
            }
            return false;
        }

        public static byte[] GetCmpInstruction(int reg, int val)
        {
            uint uval = (uint)val;
            for (int shift = 0; shift < 32; shift += 2)
            {
                uint val_rot = ((uval << shift) & 0xFFFFFFFF) | (uval >> (32 - shift));
                if (val_rot <= 0xFF)
                {
                    return new byte[] { (byte)val_rot, (byte)(shift / 2), (byte)(0x50 + reg), 0xE3 };
                }
            }
            return null;
        }
    }
}
