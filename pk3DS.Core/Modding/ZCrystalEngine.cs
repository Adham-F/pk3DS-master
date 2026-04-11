using System;
using System.IO;
using pk3DS.Core.CTR;

namespace pk3DS.Core.Modding
{
    public static class ZCrystalEngine
    {
        public static bool ExpandZCrystalLimit(string codePath, int newTotal)
        {
            if (!File.Exists(codePath)) return false;
            byte[] data = File.ReadAllBytes(codePath);

            // 1. Z-Crystal Table Size Check
            // Stock instruction: CMP R1, #0x23 (35 crystals)
            // USUM Offset: around 0x375EF0 or 0x375F7C
            byte[] sig = { 0x23, 0x00, 0x51, 0xE3 }; 
            int idx = Util.IndexOfBytes(data, sig, 0, data.Length);
            if (idx < 0) return false;

            // Patch with new count
            byte[] patch = BitConverter.GetBytes(0xE3510000 | (uint)newTotal);
            patch.CopyTo(data, idx);

            File.WriteAllBytes(codePath, data);
            return true;
        }
    }
}
