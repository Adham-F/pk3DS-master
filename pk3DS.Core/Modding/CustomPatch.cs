using System.Collections.Generic;

namespace pk3DS.Core.Modding
{
    /// <summary>
    /// Universal patch format for community-driven binary modifications.
    /// Supports three modes: raw hex, inline ARM assembly (Keystone), and external .asm files.
    /// Version-aware offsets allow a single patch file to target both US and UM.
    /// </summary>
    public class UniversalPatch
    {
        public int FormatVersion { get; set; } = 1;
        public string PatchName { get; set; } = "Unknown Patch";
        public string Author { get; set; } = "Community";
        public string Description { get; set; } = "";
        public List<string> TargetVersions { get; set; } = new List<string>(); // "US", "UM"
        public List<PatchEntry> Patches { get; set; } = new List<PatchEntry>();
    }

    /// <summary>
    /// A single patch entry targeting one binary file.
    /// </summary>
    public class PatchEntry
    {
        /// <summary>Target binary file: "code.bin", "Battle.cro", "Shop.cro", etc.</summary>
        public string TargetFile { get; set; } = "code.bin";

        /// <summary>Input mode: "hex", "asm", or "asm_file"</summary>
        public string Mode { get; set; } = "hex";

        /// <summary>
        /// For mode="hex": space/newline-separated hex string (e.g. "E1A00000 00F020E3")
        /// For mode="asm": ARM assembly text (e.g. "MOV R0, R0\nNOP")
        /// For mode="asm_file": unused (see AsmFilePath)
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>For mode="asm_file": relative path to .asm file from the patches/ folder.</summary>
        public string AsmFilePath { get; set; } = "";

        /// <summary>For mode="asm": base address for PC-relative instruction encoding.</summary>
        public string BaseAddress { get; set; } = "0x0";

        /// <summary>Version-specific offset configuration. Keys are "US" and/or "UM".</summary>
        public Dictionary<string, VersionOffsets> Offsets { get; set; } = new Dictionary<string, VersionOffsets>();
    }

    /// <summary>
    /// Version-specific addresses for a patch entry.
    /// </summary>
    public class VersionOffsets
    {
        /// <summary>
        /// Where to write the assembled code. "auto" = search for free space.
        /// Otherwise, a hex address like "0x0055D100".
        /// </summary>
        public string InjectAt { get; set; } = "auto";

        /// <summary>
        /// List of offsets to rewrite as branch instructions pointing to the injected code.
        /// Prefix with "bl:" for Branch-with-Link, "b:" for Branch. Default is "bl:".
        /// Examples: "bl:0x00341658", "b:0x003417D8", "0x00341658" (defaults to BL).
        /// </summary>
        public List<string> Hooks { get; set; } = new List<string>();
    }

    // --- Legacy aliases for backward compatibility ---
    // These map to the old JSON format so existing patches still work.

    /// <summary>Legacy patch format (v0). Automatically converted to UniversalPatch on load.</summary>
    public class CustomPatch
    {
        public string PatchName { get; set; } = "Unknown Patch";
        public string Author { get; set; } = "Community";
        public List<string> TargetVersions { get; set; } = new List<string>();
        public string Description { get; set; } = "";
        public List<PatchPayload> Payloads { get; set; } = new List<PatchPayload>();

        /// <summary>Convert legacy format to UniversalPatch.</summary>
        public UniversalPatch ToUniversal()
        {
            var up = new UniversalPatch
            {
                FormatVersion = 0,
                PatchName = PatchName,
                Author = Author,
                Description = Description,
                TargetVersions = TargetVersions
            };

            foreach (var p in Payloads)
            {
                var entry = new PatchEntry
                {
                    TargetFile = p.TargetFile,
                    Mode = "hex",
                    Code = p.Code
                };

                // Legacy format has no version-specific offsets — apply to both
                var vOfs = new VersionOffsets { InjectAt = "auto" };
                foreach (var hook in p.BLRepoints)
                    vOfs.Hooks.Add("bl:" + hook);

                entry.Offsets["US"] = vOfs;
                entry.Offsets["UM"] = vOfs;
                up.Patches.Add(entry);
            }

            return up;
        }
    }

    /// <summary>Legacy payload format.</summary>
    public class PatchPayload
    {
        public string TargetFile { get; set; } = "code.bin";
        public int AllocationSize { get; set; }
        public string Code { get; set; } = "";
        public List<string> BLRepoints { get; set; } = new List<string>();
    }
}
