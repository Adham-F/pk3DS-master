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
            string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
            string codePath = Path.Combine(Main.ExeFSPath, binName);
            string battlePath = Path.Combine(Main.RomFSPath, "battle", "battle.cro");

            if (!File.Exists(codePath) || !File.Exists(battlePath)) return;

            byte[] codeBin = File.ReadAllBytes(codePath);
            byte[] battleCro = File.ReadAllBytes(battlePath);

            bool needsSaveCode = false;
            bool needsSaveBattle = false;

            // 1. Patch Move ID Limits (CMP instructions)
            // We search for the old limit (720 or 0x2D0) and replace with new limit.
            int oldLimit = 720;
            // Iterate through common registers used for limit checks
            int[] regs = { 0, 1, 2, 5, 7 }; 
            foreach (int r in regs)
            {
                byte[] pattern = GetCmpInstruction(r, oldLimit);
                byte[] patch = GetCmpInstruction(r, moveCount);
                if (ScanAndPatch(codeBin, pattern, patch)) needsSaveCode = true;
                if (ScanAndPatch(battleCro, pattern, patch)) needsSaveBattle = true;
            }

            // 2. Expand Move Effect Jump Table in battle.cro
            if (ExpandMoveJumpTable(ref battleCro, moveCount))
                needsSaveBattle = true;

            if (needsSaveCode) File.WriteAllBytes(codePath, codeBin);
            if (needsSaveBattle) File.WriteAllBytes(battlePath, battleCro);
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

            int tableOffset = dispatcherIdx + 8; // Pointer to the start of the relative offset table
            int currentEntries = 721; // Default Move count + 1 (0 is null)
            int bytesToAdd = (newCount + 1 - currentEntries) * 4;

            if (bytesToAdd <= 0) return false;

            // Simple expansion: Append bytes to the segment containing the table.
            // In battle.cro, the dispatcher is in segment 0 (Code).
            data = CROUtil.ExpandSegment(data, 'c', bytesToAdd, tableOffset + (currentEntries * 4));
            
            return true;
        }

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
