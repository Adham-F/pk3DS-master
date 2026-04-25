using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using pk3DS.Core;
using pk3DS.Core.Modding;
using pk3DS.Core.CTR;

namespace pk3DS.WinForms;

public partial class TMEditor7 : Form
{
    public TMEditor7()
    {
        InitializeComponent();
        if (Main.ExeFSPath == null) { WinFormsUtil.Alert("No exeFS code to load."); Close(); }
        string[] files = Directory.GetFiles(Main.ExeFSPath);
        if (!File.Exists(files[0]) || !Path.GetFileNameWithoutExtension(files[0]).Contains("code")) { WinFormsUtil.Alert("No .code.bin detected."); Close(); }
        data = File.ReadAllBytes(files[0]);
        if (data.Length % 0x200 != 0) { WinFormsUtil.Alert(".code.bin not decompressed. Aborting."); Close(); }
        offset = Util.IndexOfBytes(data, Signature, 0x400000, 0) + Signature.Length;
        if (Main.Config.USUM)
            offset += 0x22;
        codebin = files[0];
        movelist[0] = "";
        SetupDGV();
        GetList();
        TB_Offset.Text = offset.ToString("X");
    }

    private static readonly byte[] Signature = [0x03, 0x40, 0x03, 0x41, 0x03, 0x42, 0x03, 0x43, 0x03]; // tail end of item::ITEM_CheckBeads
    private readonly string codebin;
    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private bool skipUpdate = false;
    private int offset = 0x0059795A; // Default
    private readonly byte[] data;
    private int dataoffset;

    private void GetDataOffset()
    {
        dataoffset = offset; // reset
    }

    private void SetupDGV()
    {
        dgvTM.Columns.Clear();
        var dgvIndex = new DataGridViewTextBoxColumn();
        {
            dgvIndex.HeaderText = "Index";
            dgvIndex.DisplayIndex = 0;
            dgvIndex.Width = 45;
            dgvIndex.ReadOnly = true;
            dgvIndex.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvIndex.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        var dgvMove = new DataGridViewComboBoxColumn();
        {
            dgvMove.HeaderText = "Move";
            dgvMove.DisplayIndex = 1;
            dgvMove.Items.AddRange(movelist); // add only the Names

            dgvMove.Width = 133;
            dgvMove.FlatStyle = FlatStyle.Flat;
            dgvIndex.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        dgvTM.Columns.Add(dgvIndex);
        dgvTM.Columns.Add(dgvMove);
    }

    private List<ushort> tms = [];

    private void GetList()
    {
        // Auto-detect expansion from binary
        string codePath = UniversalPatcher.GetFilePath("code.bin", Main.RomFSPath, Main.ExeFSPath);
        if (File.Exists(codePath))
        {
            byte[] codeData = File.ReadAllBytes(codePath);
            // CMP R0, #100 (0x64) vs #128 (0x80)
            int patchIdxExpanded = Util.IndexOfBytes(codeData, new byte[] { 0x80, 0x00, 0x50, 0xE3 }, 0, codeData.Length);
            
            CHK_Expanded.Checked = false; // Never auto-check
            if (patchIdxExpanded >= 0) 
            {
                CHK_Expanded.Enabled = true; // Allow user to check it manually
            }
            else 
            {
                CHK_Expanded.Enabled = false; // Lock if patch is missing
            }
        }

        dgvTM.Rows.Clear();
        tms = [];

        // Dynamic Repointing: Parse the offset box
        if (!int.TryParse(TB_Offset.Text, System.Globalization.NumberStyles.HexNumber, null, out int currentOffset))
            currentOffset = offset;

        int count = CHK_Expanded.Checked ? 128 : 100;
        if (currentOffset + (count * 2) > data.Length)
        {
             WinFormsUtil.Alert("Offset is out of bounds for the current code.bin.");
             return;
        }

        for (int i = 0; i < count; i++) 
            tms.Add(BitConverter.ToUInt16(data, currentOffset + (2 * i)));

        ushort[] tmlist = [.. tms];
        for (int i = 0; i < tmlist.Length; i++)
        { 
            dgvTM.Rows.Add(); 
            dgvTM.Rows[i].Cells[0].Value = (i + 1).ToString(); 
            
            ushort moveId = tmlist[i];
            if (moveId >= movelist.Length) moveId = 0; 
            
            dgvTM.Rows[i].Cells[1].Value = movelist[moveId]; 
        }
    }

    private void SetList()
    {
        tms = [];
        for (int i = 0; i < dgvTM.Rows.Count; i++)
        {
            var val = dgvTM.Rows[i].Cells[1].Value;
            if (val == null) tms.Add(0);
            else tms.Add((ushort)Array.IndexOf(movelist, val.ToString()));
        }

        ushort[] tmlist = [.. tms];

        if (!int.TryParse(TB_Offset.Text, System.Globalization.NumberStyles.HexNumber, null, out int currentOffset))
            currentOffset = offset;

        int count = Math.Min(tmlist.Length, CHK_Expanded.Checked ? 128 : 100);
        for (int i = 0; i < count; i++) 
            Array.Copy(BitConverter.GetBytes(tmlist[i]), 0, data, currentOffset + (2 * i), 2);

        // Update descriptions
        string[] itemDescriptions = Main.Config.GetText(TextName.ItemFlavor);
        string[] moveDescriptions = Main.Config.GetText(TextName.MoveFlavor);
        
        // TM01-TM92
        for (int i = 0; i < 92 && i < tmlist.Length; i++) 
            itemDescriptions[328 + i] = moveDescriptions[tmlist[i]];
        // TM93-TM95
        for (int i = 92; i < 95 && i < tmlist.Length; i++) 
            itemDescriptions[618 + i - 92] = moveDescriptions[tmlist[i]];
        // TM96-TM100
        for (int i = 95; i < 100 && i < tmlist.Length; i++) 
            itemDescriptions[690 + i - 95] = moveDescriptions[tmlist[i]];
            
        // Extra TMs (101-128)
        if (itemDescriptions.Length > 960)
        {
            for (int i = 100; i < 128 && i < tmlist.Length; i++)
            {
                int target = 960 + (i - 100);
                if (target < itemDescriptions.Length) 
                    itemDescriptions[target] = moveDescriptions[tmlist[i]];
            }
        }
        
        Main.Config.SetText(TextName.ItemFlavor, itemDescriptions);
    }

    private void Form_Closing(object sender, FormClosingEventArgs e)
    {
        SetList();
        File.WriteAllBytes(codebin, data);
    }

    private void B_RandomTM_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize TMs?", "Move compatibility will be the same as the base TMs.") != DialogResult.Yes) return;

        int[] randomMoves = Enumerable.Range(1, movelist.Length - 1).Select(i => i).ToArray();
        Util.Shuffle(randomMoves);

        int[] banned = [.. Legal.Z_Moves, .. new[] { 165, 464, 621 }];
        int ctr = 0;

        for (int i = 0; i < dgvTM.Rows.Count; i++)
        {
            int val = Array.IndexOf(movelist, dgvTM.Rows[i].Cells[1].Value);
            if (banned.Contains(val)) continue;
            while (banned.Contains(randomMoves[ctr])) ctr++;

            dgvTM.Rows[i].Cells[1].Value = movelist[randomMoves[ctr++]];
        }
        WinFormsUtil.Alert("Randomized!");
    }

    internal static ushort[] GetTMHMList()
    {
        if (Main.ExeFSPath == null) return [];
        string[] files = Directory.GetFiles(Main.ExeFSPath);
        if (!File.Exists(files[0]) || !Path.GetFileNameWithoutExtension(files[0]).Contains("code")) return [];
        
        byte[] data = File.ReadAllBytes(files[0]);
        int dataoffset = Util.IndexOfBytes(data, Signature, 0x400000, 0) + Signature.Length;
        if (data.Length % 0x200 != 0) return [];
        if (Main.Config.USUM) dataoffset += 0x22;

        List<ushort> tms = [];
        // Heuristic: If we are in USUM and reading from the default offset, check if data might be expanded
        // But for safety in other tools, we default to 100 unless we have a reason to expect 128.
        // Actually, let's look for a tell-tale sign or just use 128 if USUM (since that's what researcher wants).
        int count = Main.Config.USUM ? 128 : 100; 
        for (int i = 0; i < count; i++) 
            tms.Add(BitConverter.ToUInt16(data, dataoffset + (2 * i)));
        return [.. tms];
    }

    private void CHK_Expanded_CheckedChanged(object sender, EventArgs e)
    {
        if (skipUpdate) return;
        GetList();
    }

    private void B_ExportTxt_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialog { FileName = "TMs.txt", Filter = "Text File|*.txt" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string>();
        for (int i = 0; i < dgvTM.Rows.Count; i++)
        {
            string moveName = dgvTM.Rows[i].Cells[1].Value?.ToString() ?? "";
            lines.Add($"TM{i + 1:00}: {moveName}");
        }
        File.WriteAllLines(sfd.FileName, lines);
        WinFormsUtil.Alert("TM data exported!");
    }

    private void B_ImportTxt_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string[] lines = File.ReadAllLines(ofd.FileName);
        int updated = 0;
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

            // Parse lines like "TM01: Work Up" or "TM01: 526"
            int colonIdx = line.IndexOf(':');
            if (colonIdx < 0) continue;

            string tmPart = line.Substring(0, colonIdx).Trim();
            string movePart = line.Substring(colonIdx + 1).Trim();

            // Extract TM number
            if (!tmPart.StartsWith("TM", StringComparison.OrdinalIgnoreCase)) continue;
            if (!int.TryParse(tmPart.Substring(2), out int tmNum)) continue;
            int rowIdx = tmNum - 1;
            if (rowIdx < 0 || rowIdx >= dgvTM.Rows.Count) continue;

            // Try to match move by name first, then by index
            int moveIdx = Array.IndexOf(movelist, movePart);
            if (moveIdx < 0 && int.TryParse(movePart, out int moveId) && moveId >= 0 && moveId < movelist.Length)
                moveIdx = moveId;
            if (moveIdx < 0) continue;

            dgvTM.Rows[rowIdx].Cells[1].Value = movelist[moveIdx];
            updated++;
        }
        WinFormsUtil.Alert($"Imported {updated} TM entries!");
    }

    private void B_UpdateDesc_Click(object sender, EventArgs e)
    {
        const string disclaimer = "Warning: This will overwrite ALL TM item descriptions in the game text with the descriptions of the moves they currently teach.\n\n" +
                                   "This action cannot be undone. Are you sure you want to proceed?";
        
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, disclaimer) != DialogResult.Yes)
            return;

        // Build current TM list from the grid
        List<ushort> currentTMs = [];
        for (int i = 0; i < dgvTM.Rows.Count; i++)
            currentTMs.Add((ushort)Array.IndexOf(movelist, dgvTM.Rows[i].Cells[1].Value));

        ushort[] tmlist = [.. currentTMs];

        // Sync move descriptions into item descriptions (same logic as SetList)
        string[] itemDescriptions = Main.Config.GetText(TextName.ItemFlavor);
        string[] moveDescriptions = Main.Config.GetText(TextName.MoveFlavor);
        for (int i = 1 - 1; i <= 92 - 1; i++) // TM01 - TM92
            itemDescriptions[328 + i] = moveDescriptions[tmlist[i]];
        for (int i = 93 - 1; i <= 95 - 1; i++) // TM93 - TM95
            itemDescriptions[618 + i - 92] = moveDescriptions[tmlist[i]];
        for (int i = 96 - 1; i <= 100 - 1; i++) // TM96 - TM100
            itemDescriptions[690 + i - 95] = moveDescriptions[tmlist[i]];
        Main.Config.SetText(TextName.ItemFlavor, itemDescriptions);

        WinFormsUtil.Alert("TM item descriptions updated to match current moves!");
    }

    private void TB_Offset_TextChanged(object sender, EventArgs e)
    {
        if (uint.TryParse(TB_Offset.Text, System.Globalization.NumberStyles.HexNumber, null, out uint res))
        {
            offset = (int)res;
            GetList();
        }
    }
}