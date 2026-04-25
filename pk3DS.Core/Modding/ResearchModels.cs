using System.Collections.Generic;

namespace pk3DS.Core.Modding;

public class MoveModernBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int CurrentID { get; set; } // The target move ID selected by user
    public List<MoveModernPatch> Patches { get; set; } = new();
}

public class MoveModernPatch
{
    public string Offset { get; set; }
    public string Logic { get; set; }
}

public class ItemPatch
{
    public string Name { get; set; }
    public int ItemID { get; set; }
}

public class AbilityPatch
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class PatchGroup
{
    public string Category { get; set; } = "";
    public string Module { get; set; } = "";
    public string Sheet { get; set; } = "";
    public string Target { get; set; } = "battle.cro";
    public List<BinaryPatch> Patches { get; set; } = new();
    public List<PatchParameter> Parameters { get; set; } = new();
}

public class BinaryPatch
{
    public string Offset { get; set; } = "";
    public string Signature { get; set; } = "";
    public string Hex { get; set; } = "";
    public bool IsInjection { get; set; } = false;
    public int ExpansionSize { get; set; } = 0;
    public string Note { get; set; } = "";
}

public class PatchParameter
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "int"; // int, float, hex
    public string Value { get; set; } = "";
    public string Key { get; set; } = ""; // e.g. $ITEM_ID$
}

public class ResearchProject
{
    public string Name { get; set; }
    public string TargetFile { get; set; }
    public List<ResearchAction> Actions { get; set; } = new();
}

public class ResearchAction
{
    public string Type { get; set; } // Expand, Repoint, Relocate, Patch
    public string Target { get; set; }
    public string Value { get; set; }
    public string Offset { get; set; }
}
