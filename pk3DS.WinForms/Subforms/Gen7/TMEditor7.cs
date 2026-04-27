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

        // Universal TM Table Detection
        // TM01: Work Up (526), TM02: Dragon Claw (337), TM03: Psyshock (473)
        // Little-endian ushorts: [0x0E, 0x02, 0x51, 0x01, 0xD9, 0x01]
        byte[] tmSig = [0x0E, 0x02, 0x51, 0x01, 0xD9, 0x01];
        int foundOfs = Util.IndexOfBytes(data, tmSig, 0x100000, 0);
        if (foundOfs >= 0)
        {
            offset = foundOfs;
        }
        else
        {
            // Fallback to standard signature search
            offset = Util.IndexOfBytes(data, Signature, 0x400000, 0) + Signature.Length;
            if (Main.Config.USUM) offset += 0x22;
        }
        codebin = files[0];
        movelist[0] = "";

        // Auto-detect expansion start ID
        DetectExpansionStartID();

        SetupDGV();
        GetList();
        TB_Offset.Text = offset.ToString("X");

        // Show TM expansion info when patch is detected
        int tmCount = (int)NUD_TMCount.Value;
        if (tmCount > 100)
        {
            string msg = $"TM/HM Expansion Patch Detected — {tmCount} TMs\n\n"
                + "Slot Configuration:\n"
                + "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
                + "• TM01–TM107: Uses standard/HM slots.\n"
                + "• TM108+: Automatically mapped to Item ID " + expandedTMStartID + ".\n\n"
                + "(If it says ID 328, it is just a placeholder. It means you haven't expanded anything yet!)\n\n"
                + "The editor has automatically detected your custom Item ID range from code.bin.";
            WinFormsUtil.Alert(msg);
        }
    }

    private int expandedTMStartID = 960;
    private void DetectExpansionStartID()
    {
        try {
            if (File.Exists(codebin)) {
                byte[] d = File.ReadAllBytes(codebin);
                // TM Item Table for 108+ is usually at 0x4BB794 in patched USUM
                if (d.Length > 0x4BB794 + 2) {
                    int id = BitConverter.ToUInt16(d, 0x4BB794);
                    if (id >= 100 && id < 2000) expandedTMStartID = id;
                }
            }
        } catch { expandedTMStartID = 960; }
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

    private int GetTMOffset(int index)
    {
        // TM01 to TM100 are always contiguous from the detected base
        if (index < 100) return offset + (2 * index);

        // For expanded TMs (101+), we check if there's a second table (sandbox)
        // or if they are contiguous. Most expansion patches jump at 108.
        if (index >= 107)
        {
             // If a known sandbox offset is provided in the textbox or research, use it.
             // Otherwise, check for the 108+ sandbox (0x4BB794) if it contains a move ID.
             if (offset < 0x100000 && data.Length > 0x4BB794 + 2)
                 return 0x4BB794 + (2 * (index - 107));
        }
        return offset + (2 * index);
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
        // Auto-detect expansion from binary by scanning for CMP instructions
        string codePath = codebin;
        if (File.Exists(codePath))
        {
            byte[] codeData = File.ReadAllBytes(codePath);
            int detectedCount = DetectTMCount(codeData);
            if (detectedCount > 0 && detectedCount != (int)NUD_TMCount.Value)
            {
                skipUpdate = true;
                NUD_TMCount.Value = Math.Min(detectedCount, NUD_TMCount.Maximum);
                skipUpdate = false;
            }
        }

        dgvTM.Rows.Clear();
        tms = [];

        // Dynamic Repointing: Parse the offset box
        if (!int.TryParse(TB_Offset.Text, System.Globalization.NumberStyles.HexNumber, null, out int currentOffset))
            currentOffset = offset;

        int count = (int)NUD_TMCount.Value;
        if (currentOffset + (count * 2) > data.Length)
        {
             WinFormsUtil.Alert("Offset is out of bounds for the current code.bin.");
             return;
        }

        for (int i = 0; i < count; i++) 
            tms.Add(BitConverter.ToUInt16(data, GetTMOffset(i)));

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

    /// <summary>
    /// Scans code.bin for the CMP instruction that originally checked #100 (0x64) for TM count.
    /// Decodes ARM rotated immediates to return the actual patched value.
    /// </summary>
    private static int DetectTMCount(byte[] codeData)
    {
        // The original instruction is CMP R0, #0x64 (100) => E3 50 00 64
        // After patching, the immediate changes. We look for CMP R0, #imm near the TM check region.
        // CMP Rn, #imm has opcode mask 0x0FF00000 == 0x03500000 (for R0) 
        // We scan for any CMP that's > 100 and <= 128, which would indicate a TM expansion.
        
        int bestCount = 0;
        
        for (int i = 0; i < codeData.Length - 4; i += 4)
        {
            uint word = BitConverter.ToUInt32(codeData, i);
            // Match CMP R0, #imm (E3 50 0r ii) - condition AL, opcode CMP, Rn=R0
            if ((word & 0xFFF00000) != 0xE3500000) continue;
            
            // Decode ARM rotated immediate
            uint imm8 = word & 0xFF;
            uint rot = (word >> 8) & 0xF;
            uint value = (imm8 >> (int)(rot * 2)) | (imm8 << (int)(32 - rot * 2));
            if (rot == 0) value = imm8;
            
            // We're looking for a value that replaced the original 100 (0x64)
            // Valid TM counts: anything from 101 to 128
            if (value > 100 && value <= 128)
            {
                if ((int)value > bestCount)
                    bestCount = (int)value;
            }
        }
        
        return bestCount > 0 ? bestCount : 100; // default to 100 if no expansion detected
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

        int count = Math.Min(tmlist.Length, (int)NUD_TMCount.Value);
        for (int i = 0; i < count; i++) 
            Array.Copy(BitConverter.GetBytes(tmlist[i]), 0, data, GetTMOffset(i), 2);

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
            
        // Extra TMs (101-107) - Item IDs 721-727
        for (int i = 100; i < 107 && i < tmlist.Length; i++)
            itemDescriptions[721 + (i - 100)] = moveDescriptions[tmlist[i]];

        // Extra TMs (108+) - Start from expandedTMStartID (default 960)
        if (itemDescriptions.Length > expandedTMStartID)
        {
            for (int i = 107; i < tmlist.Length; i++)
            {
                int target = expandedTMStartID + (i - 107);
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

        int count = DetectTMCount(data);

        List<ushort> tms = [];
        for (int i = 0; i < count; i++) 
            tms.Add(BitConverter.ToUInt16(data, dataoffset + (2 * i)));
        return [.. tms];
    }

    private void NUD_TMCount_ValueChanged(object sender, EventArgs e)
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