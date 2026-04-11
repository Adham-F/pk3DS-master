using System;
using System.Collections.Generic;

namespace pk3DS.Core.Modding
{
    public class RelocationEntry
    {
        public uint WriteTo { get; set; }
        public uint PatchAddr { get; set; }
        public uint Addend { get; set; }
        public int TargetSeg { get; set; }
        public byte[] RawPatch { get; set; } = new byte[12];
        public string Note { get; set; } = "";
    }

    public enum MechanicType
    {
        Move,
        Ability,
        Item,
        Global
    }

    public class MechanicChain
    {
        public string Name { get; set; } = "";
        public MechanicType Type { get; set; } = MechanicType.Global;
        public uint RootOffset { get; set; } // The base address in-file (e.g. Move Index entry)
        public List<RelocationEntry> Relocations { get; set; } = new();
    }

    public class BSSAllocation
    {
        public string Name { get; set; } = "";
        public uint Offset { get; set; }
        public uint Size { get; set; }
    }

    public class ModdingProject
    {
        public string TargetCRO { get; set; } = "battle.cro";
        public List<MechanicChain> Chains { get; set; } = new();
        public List<BSSAllocation> BSSAllocations { get; set; } = new();
        public uint LastKnownSegmentTable { get; set; }
        public uint LastKnownPatchCount { get; set; }
    }
}
