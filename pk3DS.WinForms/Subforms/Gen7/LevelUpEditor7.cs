using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using pk3DS.Core.Structures;
using pk3DS.Core;
using pk3DS.Core.Randomizers;
using pk3DS.WinForms.Properties;

namespace pk3DS.WinForms;

public partial class LevelUpEditor7 : Form
{
    public LevelUpEditor7(byte[][] infiles)
    {
        InitializeComponent();
        files = infiles;
        string[] species = Main.Config.GetText(TextName.SpeciesNames);
        string[][] AltForms = Main.Config.Personal.GetFormList(species, Main.Config.MaxSpeciesID);
        string[] specieslist = Main.Config.Personal.GetPersonalEntryList(AltForms, species, Main.Config.MaxSpeciesID, out baseForms, out formVal);
        specieslist[0] = movelist[0] = "";

        string[] sortedspecies = (string[])specieslist.Clone();
        Array.Resize(ref sortedspecies, Main.Config.MaxSpeciesID + 1); Array.Sort(sortedspecies);
        SetupDGV();

        var newlist = new List<ComboItem>();
        for (int i = 1; i <= Main.Config.MaxSpeciesID; i++) // add all species
            newlist.Add(new ComboItem { Text = sortedspecies[i], Value = Array.IndexOf(specieslist, sortedspecies[i]) });
        for (int i = Main.Config.MaxSpeciesID + 1; i < specieslist.Length; i++) // add all forms
            newlist.Add(new ComboItem { Text = specieslist[i], Value = i });

        CB_Species.DisplayMember = "Text";
        CB_Species.ValueMember = "Value";
        CB_Species.DataSource = newlist;
        CB_Species.SelectedIndex = 0;
        RandSettings.GetFormSettings(this, groupBox1.Controls);
        dgv.CellValueChanged += UpdateCounters;
        dgv.RowsAdded += UpdateCounters;
        dgv.RowsRemoved += UpdateCounters;
    }

    private readonly byte[][] files;
    private int entry = -1;
    private static int[] ClipboardMoves;
    private static int[] ClipboardLevels;
    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private bool dumping;
    private readonly int[] baseForms, formVal;

    private void SetupDGV()
    {
        string[] sortedmoves = (string[])movelist.Clone();
        Array.Sort(sortedmoves);
        var dgvLevel = new DataGridViewTextBoxColumn();
        {
            dgvLevel.HeaderText = "Level";
            dgvLevel.DisplayIndex = 0;
            dgvLevel.Width = 45;
            dgvLevel.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        var dgvMove = new DataGridViewComboBoxColumn();
        {
            dgvMove.HeaderText = "Move";
            dgvMove.DisplayIndex = 1;
            for (int i = 0; i < movelist.Length; i++)
                dgvMove.Items.Add(sortedmoves[i]); // add only the Names

            dgvMove.Width = 135;
            dgvMove.FlatStyle = FlatStyle.Flat;
        }
        dgv.Columns.Add(dgvLevel);
        dgv.Columns.Add(dgvMove);
    }
   
private static Dictionary<string, int> formMappingCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private readonly string mapFilePath = Path.Combine(Application.StartupPath, "custom_form_mappings.txt");

    private void LoadMappingCache()
    {
        if (!File.Exists(mapFilePath)) return;
        foreach (string line in File.ReadAllLines(mapFilePath))
        {
            var parts = line.Split('=');
            if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                formMappingCache[parts[0]] = id;
        }
    }

    private void SaveMappingCache()
    {
        var lines = formMappingCache.Select(kvp => $"{kvp.Key}={kvp.Value}");
        File.WriteAllLines(mapFilePath, lines);
    }

private int PromptFormMapping(string formName)
    {
        Form prompt = new Form()
        {
            Width = 420, Height = 210, FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = "Map Custom Form", StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false, MinimizeBox = false
        };
        
        Label textLabel = new Label() { 
            Left = 15, Top = 15, Width = 380, Height = 35, 
            Text = $"Unrecognized form '{formName}'.\nSelect the Pokémon this form corresponds to:" 
        };
        
        ComboBox cb = new ComboBox() { 
            Left = 15, Top = 55, Width = 230, 
            DropDownStyle = ComboBoxStyle.DropDown,
            DisplayMember = "Text", ValueMember = "Value",
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems
        };
        
        // FIX: Add items directly to bypass deferred DataSource binding crashes
        foreach (ComboItem ci in CB_Species.Items) cb.Items.Add(ci);
        
        PictureBox pb = new PictureBox() { 
            Left = 260, Top = 55, Width = 120, Height = 100, 
            SizeMode = PictureBoxSizeMode.CenterImage, BorderStyle = BorderStyle.FixedSingle 
        };
        
        cb.SelectedIndexChanged += (sender, e) => {
            var ci = cb.SelectedItem as ComboItem;
            if (ci != null && (int)ci.Value > 0) {
                int id = (int)ci.Value;
                int s = id <= Main.Config.MaxSpeciesID ? id : baseForms[id];
                int f = id <= Main.Config.MaxSpeciesID ? 0 : formVal[id];
                
                var rawImg = WinFormsUtil.GetSprite(s, f, 0, 0, Main.Config);
                var bigImg = new Bitmap(rawImg.Width * 2, rawImg.Height * 2);
                for (int x = 0; x < rawImg.Width; x++)
                for (int y = 0; y < rawImg.Height; y++)
                {
                    Color c = rawImg.GetPixel(x, y);
                    bigImg.SetPixel(2 * x, 2 * y, c);
                    bigImg.SetPixel((2 * x) + 1, 2 * y, c);
                    bigImg.SetPixel(2 * x, (2 * y) + 1, c);
                    bigImg.SetPixel((2 * x) + 1, (2 * y) + 1, c);
                }
                pb.Image = bigImg;
            } else {
                pb.Image = null;
            }
        };
        
        Button confirmation = new Button() { Text = "Map Form", Left = 15, Width = 110, Top = 100, DialogResult = DialogResult.OK };
        Button cancel = new Button() { Text = "Skip", Left = 135, Width = 110, Top = 100, DialogResult = DialogResult.Cancel };
        
        prompt.Controls.Add(cb);
        prompt.Controls.Add(pb);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(cancel);
        prompt.Controls.Add(textLabel);
        prompt.AcceptButton = confirmation;
        prompt.CancelButton = cancel;
        
        // Safety check using the natively populated cb.Items count
        if (cb.Items.Count > 0)
        {
            cb.SelectedIndex = cb.Items.Count > 1 ? 1 : 0; 
        }
        
        return prompt.ShowDialog() == DialogResult.OK && cb.SelectedItem != null ? (int)((ComboItem)cb.SelectedItem).Value : -1;
    }
    private Learnset6 pkm;

    private void GetList()
    {
        entry = WinFormsUtil.GetIndex(CB_Species);
        int s = baseForms[entry];
        int f = formVal[entry];
        if (entry <= Main.Config.MaxSpeciesID)
            s = entry;
        int[] specForm = [s, f];
        string filename = "_" + specForm[0] + (entry > Main.Config.MaxSpeciesID ? "_" + (specForm[1] + 1) : "");
        PB_MonSprite.Image = (Bitmap)Resources.ResourceManager.GetObject(filename);

        dgv.Rows.Clear();
        byte[] input = files[entry];
        if (input.Length <= 4) { files[entry] = BitConverter.GetBytes(-1); return; }
        pkm = new Learnset6(input);

        dgv.Rows.Add(pkm.Count);

        // Fill Entries
        for (int i = 0; i < pkm.Count; i++)
        {
            dgv.Rows[i].Cells[0].Value = pkm.Levels[i];
            dgv.Rows[i].Cells[1].Value = movelist[pkm.Moves[i]];
        }

        dgv.CancelEdit();
    }

    private void SetList()
    {
        if (entry < 1 || dumping) return;
        List<int> moves = [];
        List<int> levels = [];
        for (int i = 0; i < dgv.Rows.Count - 1; i++)
        {
            int move = Array.IndexOf(movelist, dgv.Rows[i].Cells[1].Value);
            if (move < 1) continue;

            moves.Add((short)move);
            string level = (dgv.Rows[i].Cells[0].Value ?? 0).ToString();
            _ = short.TryParse(level, out var lv);
            if (lv > 100) lv = 100;
            levels.Add(lv);
        }
        pkm.Moves = [.. moves];
        pkm.Levels = [.. levels];
        files[entry] = pkm.Write();
    }

    private void ChangeEntry(object sender, EventArgs e)
    {
        SetList();
        GetList();
    }

    // Struggle, Hyperspace Fury, Dark Void
    private static readonly int[] usualBan = [165, 621, 464];

    private void B_RandAll_Click(object sender, EventArgs e)
    {
        SetList();
        var sets = files.Select(z => new Learnset6(z)).ToArray();
        var banned = new List<int>(usualBan.Concat(Legal.Z_Moves));
        if (CHK_NoFixedDamage.Checked)
            banned.AddRange(MoveRandomizer.FixedDamageMoves);

        var rand = new LearnsetRandomizer(Main.Config, sets)
        {
            Expand = CHK_Expand.Checked,
            ExpandTo = (int)NUD_Moves.Value,
            Spread = CHK_Spread.Checked,
            SpreadTo = (int)NUD_Level.Value,
            STAB = CHK_STAB.Checked,
            STABPercent = NUD_STAB.Value,
            STABFirst = CHK_STAB.Checked,
            BannedMoves = banned,
            Learn4Level1 = CHK_4MovesLvl1.Checked,
        };
        rand.Execute();
        sets.Select(z => z.Write()).ToArray().CopyTo(files, 0);
        GetList();
        WinFormsUtil.Alert("All Pokémon's Level Up Moves have been randomized!", "Press the Dump button to see the new Level Up Moves!");
    }

    private void B_Metronome_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Play using Metronome Mode?", "This will modify learnsets to only have Metronome.") != DialogResult.Yes)
            return;

        // clear all data, then only assign Metronome at Lv1
        for (int i = 0; i < CB_Species.Items.Count; i++)
        {
            CB_Species.SelectedIndex = i;
            dgv.Rows.Clear();
            dgv.Rows.Add();
            dgv.Rows[0].Cells[0].Value = 1;
            dgv.Rows[0].Cells[1].Value = movelist[118];
        }
        CB_Species.SelectedIndex = 0;
        WinFormsUtil.Alert("All Pokémon now only know the move Metronome!");
    }

private void B_AddMove_Click(object sender, EventArgs e)
    {
        dgv.Rows.Add(1, movelist[1]); // Defaults to Level 1, Pound
    }

    private void B_RemoveMove_Click(object sender, EventArgs e)
    {
        if (dgv.CurrentRow != null && !dgv.CurrentRow.IsNewRow)
            dgv.Rows.Remove(dgv.CurrentRow);
    }

    private void UpdateCounters(object sender, EventArgs e)
    {
        if (entry < 1 || dumping || pkm == null) return;
        int moveCount = 0;
        int stabCount = 0;
        var pkmTypes = Main.SpeciesStat[entry].Types;
        var moveData = Main.Config.Moves;

        for (int i = 0; i < dgv.Rows.Count - 1; i++)
        {
            var cellVal = dgv.Rows[i].Cells[1].Value;
            if (cellVal == null) continue;
            int move = Array.IndexOf(movelist, cellVal);
            if (move > 0)
            {
                moveCount++;
                if (pkmTypes.Contains(moveData[move].Type))
                    stabCount++;
            }
        }
        if (L_TotalMoves != null) L_TotalMoves.Text = $"Total Moves: {moveCount}";
        if (L_STABCount != null) L_STABCount.Text = $"STAB Moves: {stabCount}";
    }

    private void B_Dump_Click(object sender, EventArgs e)
    {
        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Dump all Level Up Moves to TSV Text File?")) return;

        dumping = true;
        var lines = new List<string>();
        foreach (ComboItem item in CB_Species.Items)
        {
            if ((int)item.Value == 0) continue;
            int targetEntry = (int)item.Value;
            byte[] rawInput = files[targetEntry];
            if (rawInput.Length <= 4) continue;
            
            var tempPkm = new Learnset6(rawInput);
            string name = item.Text.Split('-')[0].Trim();
            string line = $"{targetEntry}\t{name}";
            
            for (int j = 0; j < tempPkm.Count; j++)
                line += $"\t{tempPkm.Levels[j]}\t{movelist[tempPkm.Moves[j]]}";
                
            lines.Add(line);
        }
        var sfd = new SaveFileDialog { FileName = "LevelUp_TSV.txt", Filter = "Text File|*.txt" };
        SystemSounds.Asterisk.Play();
        if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllLines(sfd.FileName, lines, Encoding.Unicode);
        dumping = false;
    }

    private void B_Import_Click(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string[] lines = File.ReadAllLines(ofd.FileName);
        int count = 0;

        foreach (string line in lines)
        {
            string[] parts = line.Split('\t');
            if (parts.Length < 2) continue;

            if (int.TryParse(parts[0], out int targetId) && targetId > 0 && targetId < files.Length)
            {
                List<int> newLevels = new List<int>();
                List<int> newMoves = new List<int>();

                for (int i = 2; i < parts.Length - 1; i += 2)
                {
                    if (int.TryParse(parts[i], out int lvl))
                    {
                        string moveName = parts[i + 1].Trim();
                        int moveId = Array.IndexOf(movelist, moveName);
                        if (moveId > 0)
                        {
                            newLevels.Add(lvl > 100 ? 100 : lvl);
                            newMoves.Add(moveId);
                        }
                    }
                }

                byte[] input = files[targetId];
                if (input.Length > 4)
                {
                    var modPkm = new Learnset6(input);
                    modPkm.Levels = newLevels.ToArray();
                    modPkm.Moves = newMoves.ToArray();
                    files[targetId] = modPkm.Write();
                    count++;
                }
            }
        }
        GetList();
        WinFormsUtil.Alert($"Imported native learnsets for {count} Pokémon.");
    }

private void B_ApplyModern_Click(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        LoadMappingCache(); // Load globally saved forms
        string[] lines = File.ReadAllLines(ofd.FileName);
        int successCount = 0;

        // Take a snapshot of the files before we start editing them
        byte[][] backupFiles = files.Select(a => (byte[])a.Clone()).ToArray();

        var nameToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (ComboItem item in CB_Species.Items)
        {
            if ((int)item.Value == 0) continue; 
            string cleanName = item.Text.Split('-')[0].Trim(); 
            if (!nameToId.ContainsKey(cleanName)) nameToId[cleanName] = (int)item.Value;
        }

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split('\t');
            if (parts.Length < 4) continue; 

            string monName = parts[1].Trim();
            int targetId = -1;

            if (nameToId.ContainsKey(monName)) targetId = nameToId[monName];
            else if (formMappingCache.ContainsKey(monName)) targetId = formMappingCache[monName];
            else
            {
                targetId = PromptFormMapping(monName);
                if (targetId == -2) // ABORT ALL trigger
                {
                    if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Import Aborted.", "Do you want to SAVE the progress made so far?\n\nYes = Save partial progress\nNo = Discard and revert everything"))
                    {
                        // Restore snapshot
                        for (int b = 0; b < files.Length; b++) files[b] = (byte[])backupFiles[b].Clone();
                        WinFormsUtil.Alert("Import aborted. All changes reverted.");
                        GetList();
                        return;
                    }
                    break; // End loop but keep changes
                }
                if (targetId <= 0) continue; // User clicked Skip
                
                formMappingCache[monName] = targetId; 
                SaveMappingCache(); // Save immediately to hard drive
            }

            if (targetId <= 0 || targetId >= files.Length) continue;

            List<int> newLevels = new List<int>();
            List<int> newMoves = new List<int>();

            for (int i = 2; i < parts.Length - 1; i += 2)
            {
                if (int.TryParse(parts[i], out int lvl))
                {
                    string moveName = parts[i + 1].Trim();
                    int moveId = Array.IndexOf(movelist, moveName);
                    
                    if (moveId > 0) 
                    {
                        newLevels.Add(lvl > 100 ? 100 : lvl);
                        newMoves.Add(moveId);
                    }
                }
            }

            byte[] newBytes = files[targetId];
            if (newBytes.Length > 4)
            {
                var modPkm = new Learnset6(newBytes);
                modPkm.Levels = newLevels.ToArray();
                modPkm.Moves = newMoves.ToArray();
                files[targetId] = modPkm.Write();
                successCount++;
            }
        }
        GetList(); 
        WinFormsUtil.Alert($"Modern update complete!\nUpdated {successCount} Level-Up learnsets.");
    }

    private void B_Copy_Click(object sender, EventArgs e)
    {
        SetList(); // Forces the UI data to save to the pkm object first
        if (pkm == null) return;
        
        ClipboardMoves = (int[])pkm.Moves.Clone();
        ClipboardLevels = (int[])pkm.Levels.Clone();
    }

    private void B_Paste_Click(object sender, EventArgs e)
    {
        if (ClipboardMoves == null || ClipboardLevels == null) return;
        
        pkm.Moves = (int[])ClipboardMoves.Clone();
        pkm.Levels = (int[])ClipboardLevels.Clone();
        
        files[entry] = pkm.Write(); // Commit changes to the file array
        GetList(); // Refresh the DataGridView UI with the new data
    }
    private void Form_Closing(object sender, FormClosingEventArgs e)
    {
        SetList();
        RandSettings.SetFormSettings(this, groupBox1.Controls);
    }
    private void CHK_TypeBias_CheckedChanged(object sender, EventArgs e)
    {
        NUD_STAB.Enabled = CHK_STAB.Checked;
        NUD_STAB.Value = CHK_STAB.Checked ? 52 : NUD_STAB.Minimum;
    }

    public void CalcStats() // Debug Function
    {
        Move[] MoveData = Main.Config.Moves;
        int movectr = 0;
        int max = 0;
        int spec = 0;
        int stab = 0;
        for (int i = 0; i < Main.Config.MaxSpeciesID; i++)
        {
            byte[] movedata = files[i];
            int movecount = (movedata.Length - 4) / 4;
            if (movecount == 65535)
                continue;
            movectr += movecount; // Average Moves
            if (max < movecount) { max = movecount; spec = i; } // Max Moves (and species)
            for (int m = 0; m < movedata.Length / 4; m++)
            {
                int move = BitConverter.ToUInt16(movedata, m * 4);
                if (move == 65535)
                {
                    movectr--;
                    continue;
                }
                if (Main.SpeciesStat[i].Types.Contains(MoveData[move].Type))
                    stab++;
            }
        }
        WinFormsUtil.Alert($"Moves Learned: {movectr}\r\nMost Learned: {max} @ {spec}\r\nSTAB Count: {stab}");
    }
}