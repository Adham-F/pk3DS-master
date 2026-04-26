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
    private int ptr_Items = -1;
    private int ptr_BP = -1;
    private int ptr_Counts = -1;
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

        // Safety: Ensure counts are loaded AFTER we are sure about ofs_Counts
        if (ofs_Counts > 0 && ofs_Counts < data.Length - 40)
        {
            len_BPItem = data.Skip(ofs_Counts + 4).Take(7).ToArray();
            len_Items = data.Skip(ofs_Counts + 11).Take(28).ToArray();
        }
        else
        {
            len_BPItem = new byte[7];
            len_Items = new byte[28];
        }

        itemlist[0] = "";
        SetupDGV();
        CB_Location.Items.AddRange(locations);
        CB_LocationBPItem.Items.AddRange(locationsBP);
        CB_Location.SelectedIndex =
            CB_LocationBPItem.SelectedIndex = 0;

        B_Add.Enabled = B_Del.Enabled = false;
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
        // Immutable Tutor counts are the safest anchor: [15, 17, 17, 15]
        byte[] sig_counts = [0x0F, 0x11, 0x11, 0x0F];
        int idx_c = Util.IndexOfBytes(data, sig_counts, 0, data.Length - sig_counts.Length);
        if (idx_c >= 0) ofs_Counts = idx_c; else ofs_Counts = 0x52D2;

        // Mart items pattern: Poke Ball (0x04), Great Ball (0x03), Ultra Ball (0x02)
        byte[] sig_items = [0x04, 0x00, 0x03, 0x00, 0x02, 0x00, 0x05, 0x00];
        int idx_i = Util.IndexOfBytes(data, sig_items, 0, data.Length - sig_items.Length);
        if (idx_i >= 0) ofs_Items = idx_i; else ofs_Items = 0x50BC;

        // BP items pattern: Rare Candy (0x32) at Battle Royal Dome
        byte[] sig_bp = [0x32, 0x00, 0x30, 0x00, 0x21, 0x01, 0x10, 0x00];
        int idx_b = Util.IndexOfBytes(data, sig_bp, 0, data.Length - sig_bp.Length);
        if (idx_b >= 0) ofs_BP = idx_b; else ofs_BP = 0x5412;

        // Tutor data pattern: Bind (0x0182)
        byte[] sig_t = [0x82, 0x01, 0x04, 0x00, 0xAC, 0x01, 0x0C, 0x00];
        int idx_t = Util.IndexOfBytes(data, sig_t, 0, data.Length - sig_t.Length);
        if (idx_t >= 0) ofs_Tutors = idx_t; else ofs_Tutors = 0x54DE;

        // Dynamic Pointer Hunting
        // Search for the Item Pointer Table by looking for the current offset of the first Mart
        byte[] pI = BitConverter.GetBytes((uint)ofs_Items);
        ptr_Items = Util.IndexOfBytes(data, pI, 0, data.Length - 4);

        byte[] pB = BitConverter.GetBytes((uint)ofs_BP);
        ptr_BP = Util.IndexOfBytes(data, pB, 0, data.Length - 4);

        // Pointer to the count table (anchor to Tutors)
        byte[] pC = BitConverter.GetBytes((uint)ofs_Counts);
        ptr_Counts = Util.IndexOfBytes(data, pC, 0, data.Length - 4);

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

        // Update Pointer Tables in the binary
        SyncPointerTables(insertionPoint, change);

        SaveOffsets();
    }

    private void SyncPointerTables(int insertionPoint, int change)
    {
        if (ptr_Items >= 0)
        {
            for (int i = 0; i < 28; i++) // Mart Item Pointers
            {
                int pOfs = ptr_Items + i * 4;
                uint val = BitConverter.ToUInt32(data, pOfs);
                if (val >= (uint)insertionPoint) 
                    Array.Copy(BitConverter.GetBytes(val + change), 0, data, pOfs, 4);
            }
        }
        if (ptr_BP >= 0)
        {
            for (int i = 0; i < 7; i++) // BP Item Pointers
            {
                int pOfs = ptr_BP + i * 4;
                uint val = BitConverter.ToUInt32(data, pOfs);
                if (val >= (uint)insertionPoint) 
                    Array.Copy(BitConverter.GetBytes(val + change), 0, data, pOfs, 4);
            }
        }
        if (ptr_Counts >= 0)
        {
            for (int i = 0; i < 3; i++) // Pointer to Counts Tables (Mart, BP, Tutor)
            {
                int pOfs = ptr_Counts + i * 4;
                uint val = BitConverter.ToUInt32(data, pOfs);
                if (val >= (uint)insertionPoint) 
                    Array.Copy(BitConverter.GetBytes(val + change), 0, data, pOfs, 4);
            }
        }
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
        dgv.DataError += (s, e) => e.ThrowException = false;
        dgvbp.DataError += (s, e) => e.ThrowException = false;
    }

    private int entryItem = -1;
    private int entryBPItem = -1;

    private void ChangeIndexItem(object sender, EventArgs e)
    {
        if (entryItem > -1) SetListItem();
        entryItem = CB_Location.SelectedIndex;
        if (entryItem >= 0) GetListItem();
    }

    private void ChangeIndexBPItem(object sender, EventArgs e)
    {
        if (entryBPItem > -1) SetListBPItem();
        entryBPItem = CB_LocationBPItem.SelectedIndex;
        if (entryBPItem >= 0) GetListBPItem();
    }

    private void GetListItem()
    {
        try
        {
            if (entryItem < 0 || entryItem >= len_Items.Length) 
                return;

            dgv.Rows.Clear();
            int count = len_Items[entryItem];
            if (count > 100) count = 100; // Sanity check to prevent UI hang/crash
            
            dgv.Rows.Add(count);
            var ofs = ofs_Items + (len_Items.Take(entryItem).Sum(z => z) * 2);
            for (int i = 0; i < count; i++)
            {
                if (ofs + (2 * i) + 1 >= data.Length) break;
                ushort itemID = BitConverter.ToUInt16(data, ofs + (2 * i));
                
                dgv.Rows[i].Cells[0].Value = i.ToString();
                string val;
                if (itemID < itemlist.Length)
                    val = itemlist[itemID];
                else
                    val = $"(Unknown: {itemID})";

                var cell = (DataGridViewComboBoxCell)dgv.Rows[i].Cells[1];
                if (!cell.Items.Contains(val))
                    cell.Items.Add(val);
                cell.Value = val;
            }
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error("Crash in GetListItem", $"entryItem: {entryItem}", $"len_Items: {len_Items?.Length}", ex.ToString());
        }
    }

    private void GetListBPItem()
    {
        try
        {
            if (entryBPItem < 0 || entryBPItem >= len_BPItem.Length) 
                return;

            dgvbp.Rows.Clear();
            int count = len_BPItem[entryBPItem];
            if (count > 100) count = 100;

            dgvbp.Rows.Add(count);
            var ofs = ofs_BP + (len_BPItem.Take(entryBPItem).Sum(z => z) * 4);
            for (int i = 0; i < count; i++)
            {
                if (ofs + (4 * i) + 3 >= data.Length) break;
                ushort itemID = BitConverter.ToUInt16(data, ofs + (4 * i));
                ushort price = BitConverter.ToUInt16(data, ofs + (4 * i) + 2);

                dgvbp.Rows[i].Cells[0].Value = i.ToString();
                string val;
                if (itemID < itemlist.Length)
                    val = itemlist[itemID];
                else
                    val = $"(Unknown: {itemID})";

                var cell = (DataGridViewComboBoxCell)dgvbp.Rows[i].Cells[1];
                if (!cell.Items.Contains(val))
                    cell.Items.Add(val);
                cell.Value = val;

                dgvbp.Rows[i].Cells[2].Value = price.ToString();
            }
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error("Crash in GetListBPItem", $"entryBPItem: {entryBPItem}", $"len_BPItem: {len_BPItem?.Length}", ex.ToString());
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
        WinFormsUtil.Alert("Add/Delete functionality is temporarily disabled due to unresolved stability issues with shop expansion in USUM.");
        return;
        /*
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
        
        // Recalculate stale count table offset after expansion
        if (countOfs >= insertionPoint) countOfs += itemSize;
        data[countOfs + entry] = counts[entry];

        // Recalculate global offsets after expansion!
        UpdateOffsets(insertionPoint, itemSize);

        // Sync and refresh
        if (isBP) { entryBPItem = entry; GetListBPItem(); } else { entryItem = entry; GetListItem(); }
        WinFormsUtil.Alert("Item slot added. Data shifted.");
        */
    }

    private void B_Del_Click(object sender, EventArgs e)
    {
        WinFormsUtil.Alert("Add/Delete functionality is temporarily disabled due to unresolved stability issues with shop expansion in USUM.");
        return;
        /*
        bool isBP = tabControl1.SelectedIndex == 1;
        int entry = isBP ? CB_LocationBPItem.SelectedIndex : CB_Location.SelectedIndex;
        if (entry < 0) return;

        byte[] counts = isBP ? len_BPItem : len_Items;
        if (counts[entry] <= 1)
        {
            WinFormsUtil.Alert("Cannot delete last item.");
            return;
        }

        int itemSize = isBP ? 4 : 2;
        int baseOfs = isBP ? ofs_BP : ofs_Items;
        int countOfs = isBP ? (ofs_Counts + 4) : (ofs_Counts + 11);
        int deletionPoint = baseOfs + ((counts.Take(entry + 1).Sum(z => z) - 1) * itemSize);

        data = CROUtil.ExpandSegment(data, 'd', -itemSize, deletionPoint);
        counts[entry]--;
        
        if (countOfs >= deletionPoint) countOfs -= itemSize;
        data[countOfs + entry] = counts[entry];

        UpdateOffsets(deletionPoint, -itemSize);

        if (isBP) { entryBPItem = entry; GetListBPItem(); } else { entryItem = entry; GetListItem(); }
        WinFormsUtil.Alert("Item slot deleted. Data shifted.");
        */
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
        }

        // NEW: Multi-file offset synchronization
        SyncExecutablePointers(codeBin);
            
        File.WriteAllBytes(codePath, codeBin);
    }

    private void SyncExecutablePointers(byte[] codeBin)
    {
        // Define the mapping of Vanilla -> Current offsets
        // Vanilla USUM: Items=0x50BC, Counts=0x52D2, BP=0x5412, Tutor=0x54DE
        Dictionary<uint, uint> map = new Dictionary<uint, uint>
        {
            { 0x50BC, (uint)ofs_Items },
            { 0x52D2, (uint)ofs_Counts },
            { 0x5412, (uint)ofs_BP },
            { 0x54DE, (uint)ofs_Tutors }
        };

        for (int i = 0; i < codeBin.Length - 4; i += 4)
        {
            uint val = BitConverter.ToUInt32(codeBin, i);
            if (map.TryGetValue(val, out uint newVal) && val != newVal)
            {
                Array.Copy(BitConverter.GetBytes(newVal), 0, codeBin, i, 4);
            }
        }
    }
}