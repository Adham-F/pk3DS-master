using pk3DS.Core;
using pk3DS.Core.CTR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class MartEditor7UU : Form
{
    private string CROPath { get { return Path.Combine(Main.RomFSPath, "Shop.cro"); } }
    private byte[] data;
    private byte[] len_Items;
    private byte[] len_BPItem;

    private int ofs_Counts = -1;
    private int ofs_Items = -1;
    private int ofs_BP = -1;
    private int ofs_Tutors = -1;
    private int cachedCodeOfs = -1;

    private string path_Counts => Path.Combine(Main.RomFSPath, "mart_counts_ofs.txt");
    private string path_Items => Path.Combine(Main.RomFSPath, "mart_items_ofs.txt");
    private string path_BP => Path.Combine(Main.RomFSPath, "mart_bp_ofs.txt");
    private string path_Tutors => Path.Combine(Main.RomFSPath, "mart_tutor_ofs.txt");
    private string codeOfsFile { get { return Path.Combine(Main.ExeFSPath, "mart_code_offset.txt"); } }

    public MartEditor7UU()
    {
        if (!File.Exists(CROPath))
        {
            WinFormsUtil.Error("CRO does not exist! Closing.", CROPath);
            Close();
        }
        InitializeComponent();

        data = File.ReadAllBytes(CROPath);
        LoadOffsets();
        
        len_BPItem = data.Skip(ofs_Counts + 4).Take(7).ToArray();
        len_Items = data.Skip(ofs_Counts + 4 + 7).Take(28).ToArray(); 

        itemlist[0] = "";
        SetupDGV();
        CB_Location.Items.AddRange(locations);
        CB_LocationBPItem.Items.AddRange(locationsBP);
        CB_Location.SelectedIndex =
            CB_LocationBPItem.SelectedIndex = 0;
    }

    private void LoadOffsets()
    {
        if (File.Exists(path_Counts)) int.TryParse(File.ReadAllText(path_Counts), out ofs_Counts);
        if (File.Exists(path_Items)) int.TryParse(File.ReadAllText(path_Items), out ofs_Items);
        if (File.Exists(path_BP)) int.TryParse(File.ReadAllText(path_BP), out ofs_BP);
        if (File.Exists(path_Tutors)) int.TryParse(File.ReadAllText(path_Tutors), out ofs_Tutors);

        bool scanNeeded = ofs_Counts <= 0 || ofs_Items <= 0 || ofs_BP <= 0 || ofs_Tutors <= 0;
        if (scanNeeded) ScanForSignatures();
    }

    private void ScanForSignatures()
    {
        // 1. Counts Table Signature: [0C, 0C, 0C, 0C, 0E, 10, 0B, 0C, 0C, 15, 0A, 09] (USUM Defaults)
        byte[] sig_counts = [0x0C, 0x0C, 0x0C, 0x0C, 0x0E, 0x10, 0x0B, 0x0C, 0x0C, 0x15, 0x0A, 0x09];
        int idx_c = Util.IndexOfBytes(data, sig_counts, 0, data.Length - sig_counts.Length);
        if (idx_c > 0) ofs_Counts = idx_c; else ofs_Counts = 0x52D2;

        // 2. Mart Item Table Signature: [Poke ball, Great Ball, Potion, Super Potion, Antidote]
        byte[] sig_items = [0x01, 0x00, 0x02, 0x00, 0x11, 0x00, 0x12, 0x00, 0x14, 0x00];
        int idx_i = Util.IndexOfBytes(data, sig_items, 0, data.Length - sig_items.Length);
        if (idx_i > 0) ofs_Items = idx_i; else ofs_Items = 0x50BC;

        // 3. BP Item Table Signature: [Rare Candy, PP Up, Max Revive, Max Elixir, HP Up]
        byte[] sig_bp = [0x32, 0x00, 0x31, 0x00, 0x0F, 0x00, 0x11, 0x00, 0x2D, 0x00];
        int idx_b = Util.IndexOfBytes(data, sig_bp, 0, data.Length - sig_bp.Length);
        if (idx_b > 0) ofs_BP = idx_b; else ofs_BP = 0x52FA;

        // 4. Tutor Table Signature: [Gyro Ball, 12, Gyro Ball, 12] (Wait, repeating Gyro ball in data?)
        // Let's use Gyro Ball (0x1C1) and 12 BP (0x0C)
        byte[] sig_tutor = [0xC1, 0x01, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00];
        int idx_t = Util.IndexOfBytes(data, sig_tutor, 0, data.Length - sig_tutor.Length);
        if (idx_t > 0) ofs_Tutors = idx_t; else ofs_Tutors = 0x54DE;

        SaveOffsets();
    }

    private void SaveOffsets()
    {
        File.WriteAllText(path_Counts, ofs_Counts.ToString());
        File.WriteAllText(path_Items, ofs_Items.ToString());
        File.WriteAllText(path_BP, ofs_BP.ToString());
        File.WriteAllText(path_Tutors, ofs_Tutors.ToString());
    }

    private void UpdateOffsets(int insertionPoint, int change)
    {
        if (ofs_Counts >= insertionPoint) ofs_Counts += change;
        if (ofs_Items >= insertionPoint) ofs_Items += change;
        if (ofs_BP >= insertionPoint) ofs_BP += change;
        if (ofs_Tutors >= insertionPoint) ofs_Tutors += change;
        SaveOffsets();
    }
    private readonly string[] itemlist = Main.Config.GetText(TextName.ItemNames);

    #region Tables
    private readonly string[] locations =
    [
        "No Trials", "1 Trial", "2 Trials", "3 Trials", "4 Trials", "5 Trials", "6 Trials", "7 Trials",
        "Konikoni City [Incenses]",
        "Konikoni City [Herbs]",
        "Hau'oli City [X Items]",
        "Route 2 [Misc]",
        "Heahea City [TMs]",
        "Royal Avenue [TMs]",
        "Route 8 [Misc]",
        "Paniola Town [Poké Balls]",
        "Malie City [TMs]",
        "Mount Hokulani [Vitamins]",
        "Seafolk Village [TMs]",
        "Konikoni City [TMs]",
        "Konikoni City [Stones]",
        "Thrifty Megamart, Left [Poké Balls]",
        "Thrifty Megamart, Middle [Misc]",
        "Thrifty Megamart, Right [Strange Souvenir]",
        "Route 3 [X Items]",
        "Konikoni City [X Items]",
        "Tapu Village [X Items]",
        "Mount Lanakila [X Items]",
    ];

    private readonly string[] locationsBP =
    [
        "Battle Royal Dome [Medicine]",
        "Battle Royal Dome [EV Training]",
        "Battle Royal Dome [Held Items]",
        "Battle Tree [Trade Evolution Items]",
        "Battle Tree [Held Items]",
        "Battle Tree [Mega Stones]",
        "Beaches [Medicine]",
    ];
    #endregion

    private void B_Save_Click(object sender, EventArgs e)
    {
        if (entryItem > -1) SetListItem();
        if (entryBPItem > -1) SetListBPItem();
        File.WriteAllBytes(CROPath, data);
        SyncMartsToCodeBin();
        Close();
    }

    private void B_Cancel_Click(object sender, EventArgs e) => Close();

    private void SetupDGV()
    {
        dgvItem.Items.AddRange(itemlist); // add only the Names
        dgvItemBP.Items.AddRange(itemlist); // add only the Names
    }

    private int entryItem = -1;
    private int entryBPItem = -1;

    private void ChangeIndexItem(object sender, EventArgs e)
    {
        if (entryItem > -1) SetListItem();
        entryItem = CB_Location.SelectedIndex;
        GetListItem();
    }

    private void ChangeIndexBPItem(object sender, EventArgs e)
    {
        if (entryBPItem > -1) SetListBPItem();
        entryBPItem = CB_LocationBPItem.SelectedIndex;
        GetListBPItem();
    }

    private void GetListItem()
    {
        dgv.Rows.Clear();
        int count = len_Items[entryItem];
        dgv.Rows.Add(count);
        var ofs = ofs_Items + (len_Items.Take(entryItem).Sum(z => z) * 2);
        for (int i = 0; i < count; i++)
        {
            dgv.Rows[i].Cells[0].Value = i.ToString();
            dgv.Rows[i].Cells[1].Value = itemlist[BitConverter.ToUInt16(data, ofs + (2 * i))];
        }
    }

    private void GetListBPItem()
    {
        dgvbp.Rows.Clear();
        int count = len_BPItem[entryBPItem];
        dgvbp.Rows.Add(count);
        var ofs = ofs_BP + (len_BPItem.Take(entryBPItem).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            dgvbp.Rows[i].Cells[0].Value = i.ToString();
            dgvbp.Rows[i].Cells[1].Value = itemlist[BitConverter.ToUInt16(data, ofs + (4 * i))];
            dgvbp.Rows[i].Cells[2].Value = BitConverter.ToUInt16(data, ofs + (4 * i) + 2).ToString();
        }
    }

    private void SetListItem()
    {
        int count = dgv.Rows.Count;
        var ofs = ofs_Items + (len_Items.Take(entryItem).Sum(z => z) * 2);
        for (int i = 0; i < count; i++)
            Array.Copy(BitConverter.GetBytes((ushort)Array.IndexOf(itemlist, dgv.Rows[i].Cells[1].Value)), 0, data, ofs + (2 * i), 2);
    }

    private void SetListBPItem()
    {
        int count = dgvbp.Rows.Count;
        var ofs = ofs_BP + (len_BPItem.Take(entryBPItem).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            int item = Array.IndexOf(itemlist, dgvbp.Rows[i].Cells[1].Value);
            Array.Copy(BitConverter.GetBytes((ushort)item), 0, data, ofs + (4 * i), 2);
            string p = dgvbp.Rows[i].Cells[2].Value.ToString();
            if (int.TryParse(p, out var price))
                Array.Copy(BitConverter.GetBytes((ushort)price), 0, data, ofs + (4 * i) + 2, 2);
        }
    }

    private void B_Randomize_Click(object sender, EventArgs e)
    {
        switch (tabControl1.SelectedIndex)
        {
            case 0:
                RandomizeItems();
                break;
            case 1:
                RandomizeBPItems();
                break;
        }
    }

    private void RandomizeItems()
    {
        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNoCancel, "Randomize mart inventories?"))
            return;

        int[] validItems = Randomizer.GetRandomItemList();

        int ctr = 0;
        Util.Shuffle(validItems);

        bool specialOnly = DialogResult.Yes == WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize only special marts?", "Will leave regular necessities intact.");
        int start = specialOnly ? 8 : 0;
        for (int i = start; i < CB_Location.Items.Count; i++)
        {
            CB_Location.SelectedIndex = i;
            for (int r = 0; r < dgv.Rows.Count; r++)
            {
                int currentItem = Array.IndexOf(itemlist, dgv.Rows[r].Cells[1].Value);
                if (CHK_XItems.Checked && XItems.Contains(currentItem))
                    continue;
                if (BannedItems.Contains(currentItem))
                    continue;
                dgv.Rows[r].Cells[1].Value = itemlist[validItems[ctr++]];
                if (ctr <= validItems.Length) continue;
                Util.Shuffle(validItems); ctr = 0;
            }
        }
        WinFormsUtil.Alert("Randomized!");
    }

    private void RandomizeBPItems()
    {
        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNoCancel, "Randomize BP inventories?"))
            return;

        int[] validItems = Randomizer.GetRandomItemList();

        int ctr = 0;
        Util.Shuffle(validItems);

        for (int i = 0; i < CB_LocationBPItem.Items.Count; i++)
        {
            CB_LocationBPItem.SelectedIndex = i;
            for (int r = 0; r < dgvbp.Rows.Count; r++)
            {
                dgvbp.Rows[r].Cells[1].Value = itemlist[validItems[ctr++]];
                if (ctr <= validItems.Length) continue;
                Util.Shuffle(validItems); ctr = 0;
            }
        }
        WinFormsUtil.Alert("Randomized!");
    }

    internal static readonly HashSet<int> XItems = [055, 056, 057, 058, 059, 060, 061, 062];

    /// <summary>
    /// Just TMs & HMs; don't want these to be changed; if changed, they are not available elsewhere ingame.
    /// </summary>
    internal static readonly HashSet<int> BannedItems =
    [
        328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348,
        349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369,
        370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 383, 384, 385, 386, 387, 388, 389, 390,
        391, 392, 393, 394, 395, 396, 397, 398, 399, 400, 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411,
        412, 413, 414, 415, 416, 417, 418, 419, 420, 421, 422, 423, 424, 425, 426, 427, 618, 619, 620, 690, 691,
        692, 693, 694, 701, 737,
    ];

    private void B_Add_Click(object sender, EventArgs e)
    {
        bool isBP = tabControl1.SelectedIndex == 1;
        int entry = isBP ? CB_LocationBPItem.SelectedIndex : CB_Location.SelectedIndex;
        if (entry < 0) return;

        if (isBP) SetListBPItem(); else SetListItem();

        int itemSize = isBP ? 4 : 2;
        int baseOfs = isBP ? ofs_BP : ofs_Items;
        byte[] counts = isBP ? len_BPItem : len_Items;
        int countOfs = isBP ? (ofs_Counts + 4) : (ofs_Counts + 4 + 7);

        int insertionPoint = baseOfs + (counts.Take(entry + 1).Sum(z => z) * itemSize);
        data = CROUtil.ExpandSegment(data, 'd', itemSize, insertionPoint);

        // Update counts in the binary
        counts[entry]++;
        data[countOfs + entry] = counts[entry];

        // Recalculate offsets after expansion!
        UpdateOffsets(insertionPoint, itemSize);

        if (isBP) GetListBPItem(); else GetListItem();
        WinFormsUtil.Alert("Item slot added. Data shifted.");
    }

    private void B_Del_Click(object sender, EventArgs e)
    {
        bool isBP = tabControl1.SelectedIndex == 1;
        int entry = isBP ? CB_LocationBPItem.SelectedIndex : CB_Location.SelectedIndex;
        if (entry < 0) return;

        byte[] counts = isBP ? len_BPItem : len_Items;
        if (counts[entry] <= 1)
        {
            WinFormsUtil.Error("Cannot delete the last item in a shop.");
            return;
        }

        if (isBP) SetListBPItem(); else SetListItem();

        int itemSize = isBP ? 4 : 2;
        int baseOfs = isBP ? ofs_BP : ofs_Items;
        int countOfs = isBP ? (ofs_Counts + 4) : (ofs_Counts + 4 + 7);

        int deletionPoint = baseOfs + (counts.Take(entry + 1).Sum(z => z) * itemSize) - itemSize;
        data = CROUtil.ExpandSegment(data, 'd', -itemSize, deletionPoint);

        counts[entry]--;
        data[countOfs + entry] = counts[entry];
        
        // Recalculate offsets after deletion!
        UpdateOffsets(deletionPoint, -itemSize);

        if (isBP) GetListBPItem(); else GetListItem();
        WinFormsUtil.Alert("Item removed. Data shifted.");
    }

    private void SyncMartsToCodeBin()
    {
        string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
        string codePath = Path.Combine(Main.ExeFSPath, binName);
        if (!File.Exists(codePath)) return;

        byte[] codeBin = File.ReadAllBytes(codePath);
        if (File.Exists(codeOfsFile))
            int.TryParse(File.ReadAllText(codeOfsFile), out cachedCodeOfs);

        if (cachedCodeOfs <= 0)
        {
            // Search for the 2-byte spaced trial count pattern for USUM: [12, 0, 12, 0, 12, 0, 12, 0]
            // These correspond to the first 4 shops which all have 12 items by default in USUM.
            byte[] pat = [0x0C, 0x00, 0x0C, 0x00, 0x0C, 0x00, 0x0C, 0x00];
            int idx = Util.IndexOfBytes(codeBin, pat, 0, codeBin.Length - pat.Length);
            if (idx >= 0)
            {
                cachedCodeOfs = idx;
                File.WriteAllText(codeOfsFile, cachedCodeOfs.ToString());
            }
        }

        if (cachedCodeOfs > 0)
        {
            for (int i = 0; i < len_Items.Length; i++)
                codeBin[cachedCodeOfs + i * 2] = len_Items[i];
            
            File.WriteAllBytes(codePath, codeBin);
        }
    }
}