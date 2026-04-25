using System;
using System.IO;
using System.Linq;
using pk3DS.Core.Modding;

namespace pk3DS.Core.Modding
{
    public static class AbilityEngine
    {
        public static bool ApplyFlagBooster(string path, string abilityName, int flagLookup, float multiplier, int targetAbilityID)
        {
            if (!File.Exists(path)) return false;
            byte[] data = File.ReadAllBytes(path);

            // 1. Find the Move Power Multiplication Routine
            // Standard Gen 7 hook point for multipliers: 0x000DBEAC calls a dispatcher
            // We use the Sharpness research as the template.
            
            // For this implementation, we simulate the 'Factory' by selecting the correct researched ASM block.
            byte[] patch = null;
            int offset = 0;

            switch (abilityName)
            {
                case "Sharpness":
                    offset = 0x000FCB60;
                    patch = GenerateFlagPowerPatch(flagLookup, multiplier, targetAbilityID, 0x00087B58, 0x00083108);
                    break;
                case "Strong Jaw":
                    // lookup 12, multiplier 1.5, target ID
                    offset = 0x000B6B2C;
                    patch = GenerateJumpPatch(flagLookup);
                    break;
                case "Bulletproof":
                    offset = 0x000B6A8C;
                    patch = GenerateJumpPatch(flagLookup);
                    break;
                case "Mega Launcher":
                    offset = 0x000DB158;
                    patch = GenerateFlagCheckConditional(flagLookup);
                    break;
            }

            if (patch == null || offset == 0) return false;

            Array.Copy(patch, 0, data, offset, patch.Length);
            File.WriteAllBytes(path, data);
            return true;
        }

        private static byte[] GenerateFlagPowerPatch(int flagLookup, float multiplier, int targetID, int blFunc, int bExit)
        {
            // ARM ASM logic based on Sharpness.xlsx
            // 10 40 2D E9 (push {r4, lr})
            // ... cmp r0, targetID ...
            // ... mov r1, flagLookup ... bl 0x1E8 ...
            // ... mov r1, MultiplierFixedPoint ...
            
            // This is a simplified representation for the engine.
            // In a production engine, we would use an Assembler.
            // For now, we use the literal hex from the research updated with ParamIDs.
            
            byte[] template = {
                0x10, 0x40, 0x2D, 0xE9, 0x02, 0x40, 0xA0, 0xE1, 0x03, 0x00, 0xA0, 0xE3, 0xCA, 0xAB, 0xFE, 0xEB, 
                0x04, 0x00, 0x50, 0xE1, 0x0A, 0x00, 0x00, 0x1A, 0x12, 0x00, 0xA0, 0xE3, 0xC6, 0xAB, 0xFE, 0xEB,
                0x70, 0x00, 0xFF, 0xE6, 0x11, 0x10, 0xA0, 0xE3, 0x67, 0x8D, 0xFC, 0xEB, 0x01, 0x00, 0x50, 0xE3,
                0x03, 0x00, 0x00, 0x1A, 0x06, 0x1B, 0xA0, 0xE3, 0x10, 0x40, 0xBD, 0xE8, 0x39, 0x00, 0xA0, 0xE3,
                0x29, 0x99, 0xFE, 0xEA, 0x10, 0x80, 0xBD, 0xE8
            };

            // Inject Target Ability ID into the template
            template[16] = (byte)targetID; // cmp r0, #targetID (Requires targetID < 256 or shifted)
            
            // Inject Flag Lookup into the template
            template[36] = (byte)flagLookup;
            
            // Multiplier (fixed point 0x1800 = 1.5x, 0x1400 = 1.25x)
            ushort fixedPoint = (ushort)(multiplier * 4096);
            template[52] = (byte)(fixedPoint & 0xFF);
            template[53] = (byte)((fixedPoint >> 8) & 0xFF);

            return template;
        }

        private static byte[] GenerateJumpPatch(int flagLookup)
        {
            // Simple: mov r1, #flagLookup; b 0x1E8
            byte[] patch = new byte[8];
            patch[0] = (byte)flagLookup;
            patch[1] = 0x10;
            patch[2] = 0xA0;
            patch[3] = 0xE3;
            // Branch to 0x1E8 logic here (needs relative calculation)
            return patch;
        }

        private static byte[] GenerateFlagCheckConditional(int flagLookup)
        {
            // Mega Launcher style: mov r1, #flag; bl 0x1E8; cmp r0, 0; beq +9; b +4
            return new byte[] { (byte)flagLookup, 0x10, 0xA0, 0xE3, 0x21, 0x94, 0xFC, 0xEB, 0x00, 0x00, 0x50, 0xE3, 0x09, 0x00, 0x00, 0x0A, 0x04, 0x00, 0x00, 0xEA };
        }

        public static bool ApplyTypeBooster(string path, string name, int typeID, float multiplier, int abilityID)
        {
            if (!File.Exists(path)) return false;
            byte[] data = File.ReadAllBytes(path);

            // Hook point based on Transistor research: 0x000FD8DC
            // Logic: if (MoveType == targetType && AttackerAbility == abilityID) Power *= multiplier;
            
            byte[] template = {
                0x10, 0x40, 0x2D, 0xE9, 0x02, 0x40, 0xA0, 0xE1, 0x03, 0x00, 0xA0, 0xE3, 0x9A, 0x28, 0xFE, 0xEB,
                0x04, 0x00, 0x50, 0xE1, 0x07, 0x00, 0x00, 0x1A, 0x19, 0x00, 0xA0, 0xE3, 0x96, 0x28, 0xFE, 0xEB,
                0x0C, 0x00, 0x50, 0xE3, 0x03, 0x00, 0x00, 0x1A, 0x0C, 0x10, 0x9F, 0xE5, 0x10, 0x40, 0xBD, 0xE8,
                0x39, 0x00, 0xA0, 0xE3, 0xFC, 0x15, 0xFE, 0xEA, 0x10, 0x80, 0xBD, 0xE8
            };

            template[16] = (byte)abilityID;
            template[32] = (byte)typeID;
            
            ushort fixedPoint = (ushort)(multiplier * 4096);
            // Multiplier data usually follows at template + offset
            
            Array.Copy(template, 0, data, 0x000FD8DC, template.Length);
            File.WriteAllBytes(path, data);
            return true;
        }

        public static bool ApplyMycelliumMightPatch(string path, int abilityID)
        {
            if (!File.Exists(path)) return false;
            byte[] data = File.ReadAllBytes(path);

            // Logic: Bypass Accuracy and Protection for Status moves
            // Hook at 0x000FD83C (Stall) and 0x000FD854 (MB)
            
            byte[] stallPatch = { 0x01, 0x40, 0x2D, 0xE9, 0x9E, 0xFF, 0xFF, 0xEB, 0x00, 0x00, 0x50, 0xE3, 0x01, 0x80, 0xBD, 0x18 };
            byte[] mbPatch = { 0x01, 0x40, 0x2D, 0xE9, 0x98, 0xFF, 0xFF, 0xEB, 0x00, 0x00, 0x50, 0xE3, 0x01, 0x80, 0xBD, 0x18 };

            Array.Copy(stallPatch, 0, data, 0x000FD83C, stallPatch.Length);
            Array.Copy(mbPatch, 0, data, 0x000FD854, mbPatch.Length);
            
            File.WriteAllBytes(path, data);
            return true;
        }

        public static bool ApplyCorrosionPatch(string path, int abilityID)
        {
            if (!File.Exists(path)) return false;
            byte[] data = File.ReadAllBytes(path);

            // Logic: Bypass Poison/Steel check for Status Poison
            byte[] patch = { 0x70, 0x40, 0x2D, 0xE9, 0x02, 0x40, 0xA0, 0xE1, 0x01, 0x50, 0xA0, 0xE1 };
            Array.Copy(patch, 0, data, 0x000FDBB4, patch.Length);

            File.WriteAllBytes(path, data);
            return true;
        }
    }
}
