using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using pk3DS.Core.CTR;

namespace pk3DS.Core.Modding
{
    public static class UniversalPatcher
    {
        public static string GetFilePath(string target, string romfs, string exefs)
        {
            if (string.IsNullOrEmpty(target)) return null;

            // 1. Try Root Match (The most common layout for clean extractions)
            string rootPath = Path.Combine(romfs, target);
            if (File.Exists(rootPath)) return rootPath;

            // 2. Try Standard Gen 7 Subfolder Patterns (folder/file.cro)
            string name = Path.GetFileNameWithoutExtension(target);
            string subfolderPath = Path.Combine(romfs, name, target);
            if (File.Exists(subfolderPath)) return subfolderPath;

            // 3. Specific Logic for Code.bin (ExeFS)
            if (target.Equals("code.bin", StringComparison.OrdinalIgnoreCase))
            {
                string p1 = Path.Combine(exefs, "code.bin");
                if (File.Exists(p1)) return p1;
                string p2 = Path.Combine(exefs, ".code.bin");
                if (File.Exists(p2)) return p2;
            }

            // 4. Handle slash-based explicit paths
            if (target.Contains("/"))
                return Path.Combine(romfs, target.Replace("/", Path.DirectorySeparatorChar.ToString()));

            return rootPath; // Fallback to root path even if not found, to trigger manual checks later
        }

        public static bool ApplyPatchGroup(PatchGroup group, string romfs, string exefs)
        {
            string path = GetFilePath(group.Target, romfs, exefs);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new FileNotFoundException($"Cannot find target file: {group.Target} at {path}");
            }

            byte[] data = File.ReadAllBytes(path);
            bool modified = false;

            foreach (var p in group.Patches)
            {
                try
                {
                    string hexStr = p.Hex;
                    foreach (var param in group.Parameters)
                        hexStr = hexStr.Replace(param.Key, param.Value);

                    byte[] patchData = Util.StringToByteArray(hexStr);
                    int offset = -1;

                    if (!string.IsNullOrEmpty(p.Signature))
                    {
                        byte[] sig = Util.StringToByteArray(p.Signature);
                        offset = Util.IndexOfBytes(data, sig, 0, data.Length);
                    }
                    else if (!string.IsNullOrEmpty(p.Offset))
                    {
                        offset = (int)Convert.ToUInt32(p.Offset, 16);
                    }

                    if (offset < 0) continue;

                    if (path.EndsWith(".cro", StringComparison.OrdinalIgnoreCase))
                    {
                        data = CROUtil.InjectSandboxPatch(data, (uint)offset, patchData);
                    }
                    else
                    {
                        if (offset + patchData.Length > data.Length) continue;
                        Array.Copy(patchData, 0, data, offset, patchData.Length);
                    }
                    modified = true;
                }
                catch { continue; }
            }

            if (modified)
            {
                File.WriteAllBytes(path, data);
                ProjectState.Instance.AppliedPatches.Add($"{group.Module} - {group.Sheet}");
                ProjectState.Instance.Save();
                return true;
            }

            return false;
        }

        private static byte[] GetBInstruction(int from, int to)
        {
            int offset = (to - from - 8) >> 2;
            byte[] result = BitConverter.GetBytes(offset);
            result[3] = 0xEA;
            return result;
        }

        public static List<PatchGroup> LoadPatchGroups(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return new List<PatchGroup>();
            try
            {
                string json = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<List<PatchGroup>>(json) ?? new List<PatchGroup>();
            }
            catch { return new List<PatchGroup>(); }
        }
    }
}
