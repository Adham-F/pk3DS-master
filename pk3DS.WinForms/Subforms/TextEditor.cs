using pk3DS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class TextEditor : Form
{
    public TextEditor(string[][] infiles, string mode)
    {
        InitializeComponent();
        files = infiles;
        Mode = mode;
        
        // Populate the dropdown with friendly names if we know them
        for (int i = 0; i < files.Length; i++)
        {
            string displayName = i.ToString();
            
            // Example mapping for Gen 6 (ORAS/XY)
            if (Mode == "gametext")
            {
                if (i == 14) displayName += " - Move Names";
                else if (i == 15) displayName += " - Move Descriptions";
                else if (i == 22) displayName += " - Species Names";
                else if (i == 34) displayName += " - Item Names";
                else if (i == 36) displayName += " - Item Descriptions";
                // Add more mappings as you discover them
            }
            
            CB_Entry.Items.Add(displayName);
        }
        CB_Entry.SelectedIndex = 0;
        dgv.EditMode = DataGridViewEditMode.EditOnEnter;
    }

    private readonly string[][] files;
    private readonly string Mode;
    private int entry = -1;

    private void B_Export_Click(object sender, EventArgs e)
    {
        if (files.Length == 0) return;
        var Dump = new SaveFileDialog { Filter = "Text File|*.txt" };
        if (Dump.ShowDialog() != DialogResult.OK) return;
        bool newline = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Remove newline formatting codes? (\\n,\\r,\\c)", "Removing newline formatting will make it more readable but will prevent any importing of that dump.") == DialogResult.Yes;
        ExportTextFile(Dump.FileName, newline, files);
    }

    private void B_Import_Click(object sender, EventArgs e)
    {
        if (files.Length == 0) return;
        var Dump = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (Dump.ShowDialog() != DialogResult.OK) return;
        
        if (!ImportTextFile(Dump.FileName)) return;

        ChangeEntry(null, null);
        WinFormsUtil.Alert("Imported Text from Input Path:", Dump.FileName);
    }

    public static void ExportTextFile(string fileName, bool newline, string[][] fileData)
    {
        using var ms = new MemoryStream();
        ms.Write([0xFF, 0xFE], 0, 2); // Write Unicode BOM
        using (var tw = new StreamWriter(ms, new UnicodeEncoding()))
        {
            for (int i = 0; i < fileData.Length; i++)
            {
                string[] data = fileData[i];
                tw.WriteLine("~~~~~~~~~~~~~~~");
                tw.WriteLine("Text File : " + i);
                tw.WriteLine("~~~~~~~~~~~~~~~");
                if (data == null) continue;
                foreach (string line in data)
                {
                    tw.WriteLine(newline
                        ? line.Replace("\\n\\n", " ").Replace("\\n", " ").Replace("\\c", "").Replace("\\r", "").Replace("\\\\", "\\").Replace("\\[", "[")
                        : line);
                }
            }
        }
        File.WriteAllBytes(fileName, ms.ToArray());
    }

    private bool ImportTextFile(string fileName)
    {
        string[] fileText = File.ReadAllLines(fileName, Encoding.Unicode);
        string[][] textLines = new string[files.Length][];
        int ctr = 0;
        bool newlineFormatting = false;
        
        for (int i = 0; i < fileText.Length; i++)
        {
            string line = fileText[i];
            if (line != "~~~~~~~~~~~~~~~") continue;
            string[] brokenLine = fileText[i++ + 1].Split([" : "], StringSplitOptions.None);
            if (brokenLine.Length != 2 || Util.ToInt32(brokenLine[1]) != ctr)
            { WinFormsUtil.Error($"Invalid Line @ {i}, expected Text File : {ctr}"); return false; }
            i += 2; 
            List<string> Lines = [];
            while (i < fileText.Length && fileText[i] != "~~~~~~~~~~~~~~~")
            {
                Lines.Add(fileText[i]);
                newlineFormatting |= fileText[i].Contains("\\n"); 
                i++;
            }
            i--;
            textLines[ctr++] = [.. Lines];
        }

        if (ctr != files.Length)
        {
            WinFormsUtil.Error("The amount of Text Files in the input file does not match.", $"Received: {ctr}, Expected: {files.Length}"); return false;
        }
        if (!newlineFormatting)
        {
            WinFormsUtil.Error("The input Text Files do not have the ingame newline formatting codes (\\n,\\r,\\c).", "When exporting text, do not remove newline formatting."); return false;
        }

        for (int i = 0; i < files.Length; i++)
        {
            try { files[i] = textLines[i]; }
            catch (Exception e) { WinFormsUtil.Error($"The input Text File (# {i}) failed to convert:", e.ToString()); return false; }
        }

        return true;
    }

    private void ChangeEntry(object sender, EventArgs e)
    {
        if (entry > -1 && sender != null)
        {
            try { files[entry] = GetCurrentDGLines(); }
            catch (Exception ex) { WinFormsUtil.Error(ex.ToString()); }
        }

        entry = CB_Entry.SelectedIndex;
        SetStringsDataGridView(files[entry]);
    }

    private void SetStringsDataGridView(string[] textArray)
    {
        dgv.Rows.Clear();
        dgv.Columns.Clear();
        if (textArray == null || textArray.Length == 0) return;
        
        dgv.AllowUserToResizeColumns = false;
        DataGridViewColumn dgvLine = new DataGridViewTextBoxColumn
        {
            HeaderText = "Line", DisplayIndex = 0, Width = 32, ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
        };
        dgvLine.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        var dgvText = new DataGridViewTextBoxColumn
        {
            HeaderText = "Text", DisplayIndex = 1, SortMode = DataGridViewColumnSortMode.NotSortable, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };

        dgv.Columns.Add(dgvLine);
        dgv.Columns.Add(dgvText);
        dgv.Rows.Add(textArray.Length);

        for (int i = 0; i < textArray.Length; i++)
        {
            dgv.Rows[i].Cells[0].Value = i;
            dgv.Rows[i].Cells[1].Value = textArray[i];
        }
    }

    private string[] GetCurrentDGLines()
    {
        string[] lines = new string[dgv.RowCount];
        for (int i = 0; i < dgv.RowCount; i++)
            lines[i] = (string)dgv.Rows[i].Cells[1].Value;
        return lines;
    }
    
    private void B_NewMoveHandler_Click(object sender, EventArgs e)
    {
        if (Mode != "gametext")
        {
            WinFormsUtil.Error("This feature is only available in Game Text mode.");
            return;
        }

        int count = GetNumberPrompt("New Move Handler", "How many new moves do you want to add?\n(MUST be a multiple of 4)", 4);
        if (count < 1 || count > 100) return;

        if (count % 4 != 0)
        {
            WinFormsUtil.Error("The number of moves to add MUST be a multiple of 4 to ensure game stability.");
            return;
        }

        if (entry > -1) files[entry] = GetCurrentDGLines();

        bool isGen7 = files.Length > 118 && files[118].Length > 600 && files[118][1].Contains("Pound");
        bool isGen6 = files.Length > 15 && files[14].Length > 600 && files[14][1].Contains("Pound");

        if (!isGen7 && !isGen6)
        {
            WinFormsUtil.Error("Could not auto-detect Generation. Ensure you are loading the correct gametext files.");
            return;
        }

        if (isGen7) ProcessGen7Moves(count);
        else if (isGen6) ProcessGen6Moves(count);

        SetStringsDataGridView(files[entry]);
    }

    private void ProcessGen7Moves(int count)
    {
        List<string> file118 = new List<string>(files[118]);
        List<string> file117 = new List<string>(files[117]);
        List<string> file19 = new List<string>(files[19]);
        List<string> file13 = new List<string>(files[13]);
        List<string> file14 = new List<string>(files[14]);

        int movesAddedSoFar = file118.Count >= 729 ? (file118.Count == 729 ? 0 : file118.Count - 728) : 0;
        int startingMoveNumber = movesAddedSoFar + 1;

        int insertIndex13 = Math.Min(2916 + (movesAddedSoFar * 4), file13.Count);
        int insertIndex14 = Math.Min(2916 + (movesAddedSoFar * 4), file14.Count);

        for (int i = 0; i < count; i++)
        {
            int moveNum = startingMoveNumber + i;
            string name118 = $"Placeholder {moveNum}";
            string desc117 = $"This move serves as a placeholder, it is currently\\nnamed Placeholder {moveNum}.";
            string name19 = $"Z-Placeholder {moveNum}";

            if (moveNum == 1 && file118.Count == 729)
            {
                file118[728] = name118;
                if (file117.Count == 729) file117[728] = desc117; else file117.Add(desc117);
                if (file19.Count == 729) file19[728] = name19; else file19.Add(name19);
            }
            else
            {
                file118.Add(name118);
                file117.Add(desc117);
                file19.Add(name19);
            }

            string[] lines13 = [$"[VAR PKNAME(0000)] used Placeholder {moveNum}!", $"The wild [VAR PKNAME(0000)] used Placeholder {moveNum}!", $"The opposing [VAR PKNAME(0000)] used Placeholder {moveNum}!", $"Totem [VAR PKNAME(0000)] used Placeholder {moveNum}!"];
            file13.InsertRange(insertIndex13, lines13);
            insertIndex13 += 4;

            string[] lines14 = [$"[VAR PKNAME(0000)] used Z-Placeholder {moveNum}!", $"The wild [VAR PKNAME(0000)] used Z-Placeholder {moveNum}!", $"The opposing [VAR PKNAME(0000)] used Z-Placeholder {moveNum}!", $"Totem [VAR PKNAME(0000)] used Z-Placeholder {moveNum}!"];
            file14.InsertRange(insertIndex14, lines14);
            insertIndex14 += 4;
        }

        files[118] = file118.ToArray();
        files[117] = file117.ToArray();
        files[19] = file19.ToArray();
        files[13] = file13.ToArray();
        files[14] = file14.ToArray();

        WinFormsUtil.Alert($"{count} lines added to files 118, 117, and 19.", $"{count * 4} lines added to files 13 and 14.");
    }

    private void ProcessGen6Moves(int count)
    {
        List<string> file14 = new List<string>(files[14]);
        List<string> file15 = new List<string>(files[15]);

        int vanillaCount = 622;
        int movesAddedSoFar = file14.Count >= vanillaCount ? (file14.Count == vanillaCount ? 0 : file14.Count - (vanillaCount - 1)) : 0;
        int startingMoveNumber = movesAddedSoFar + 1;

        for (int i = 0; i < count; i++)
        {
            int moveNum = startingMoveNumber + i;
            string name = $"Placeholder {moveNum}";
            string desc = $"This move serves as a placeholder, it is currently\\nnamed Placeholder {moveNum}.";

            if (moveNum == 1 && file14.Count == vanillaCount)
            {
                file14[vanillaCount - 1] = name;
                if (file15.Count == vanillaCount) file15[vanillaCount - 1] = desc; else file15.Add(desc);
            }
            else
            {
                file14.Add(name);
                file15.Add(desc);
            }
        }

        files[14] = file14.ToArray();
        files[15] = file15.ToArray();

        WinFormsUtil.Alert($"{count} moves added to files 14 (Names) and 15 (Descriptions).");
    }

    private int GetNumberPrompt(string title, string promptText, int defaultValue)
    {
        using Form promptForm = new Form()
        {
            Width = 320, Height = 160, FormBorderStyle = FormBorderStyle.FixedDialog, Text = title, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false
        };
        Label textLabel = new Label() { Left = 20, Top = 20, Text = promptText, AutoSize = true };
        NumericUpDown nud = new NumericUpDown() { Left = 20, Top = 50, Width = 260, Minimum = 1, Maximum = 10000, Value = defaultValue };
        Button confirmation = new Button() { Text = "OK", Left = 180, Width = 100, Top = 80, DialogResult = DialogResult.OK };
        
        promptForm.Controls.Add(nud); promptForm.Controls.Add(confirmation); promptForm.Controls.Add(textLabel);
        promptForm.AcceptButton = confirmation;

        return promptForm.ShowDialog() == DialogResult.OK ? (int)nud.Value : 0;
    }

    private void B_AddLine_Click(object sender, EventArgs e)
    {
        int count = GetNumberPrompt("Add Lines", "How many lines do you want to add after the selection?", 1);
        if (count <= 0) return;

        int currentRow = 0;
        bool appendToEnd = false;
        try { currentRow = dgv.CurrentRow.Index; } catch { appendToEnd = true; } 

        if (appendToEnd)
        {
            dgv.Rows.Add(count);
        }
        else
        {
            if (dgv.Rows.Count != 1 && (currentRow < dgv.Rows.Count - 1 || currentRow == 0))
                if (ModifierKeys != Keys.Control && currentRow != 0 && WinFormsUtil.Prompt(MessageBoxButtons.YesNo, $"Inserting {count} row(s) in between lines will shift all subsequent lines.", "Continue?") != DialogResult.Yes) return;
            
            for (int i = 0; i < count; i++) dgv.Rows.Insert(currentRow + 1 + i);
        }

        for (int i = 0; i < dgv.Rows.Count; i++) dgv.Rows[i].Cells[0].Value = i.ToString();
    }

    private void B_AddLineBefore_Click(object sender, EventArgs e)
    {
        int count = GetNumberPrompt("Add Lines", "How many lines do you want to add before the selection?", 1);
        if (count <= 0) return;

        int currentRow = 0;
        bool appendToEnd = false;
        try { currentRow = dgv.CurrentRow.Index; } catch { appendToEnd = true; }

        if (appendToEnd)
        {
            dgv.Rows.Add(count);
        }
        else
        {
            if (dgv.Rows.Count != 1)
                if (ModifierKeys != Keys.Control && WinFormsUtil.Prompt(MessageBoxButtons.YesNo, $"Inserting {count} row(s) before lines will shift all subsequent lines.", "Continue?") != DialogResult.Yes) return;
            
            for (int i = 0; i < count; i++) dgv.Rows.Insert(currentRow);
        }

        for (int i = 0; i < dgv.Rows.Count; i++) dgv.Rows[i].Cells[0].Value = i.ToString();
    }

    private void B_RemoveLine_Click(object sender, EventArgs e)
    {
        if (dgv.CurrentRow == null) return;
        int currentRow = dgv.CurrentRow.Index;
        int count = GetNumberPrompt("Remove Lines", "How many lines do you want to remove?", 1);
        if (count <= 0) return;

        if (currentRow + count > dgv.Rows.Count) count = dgv.Rows.Count - currentRow; 
        if (currentRow < dgv.Rows.Count - count)
            if (ModifierKeys != Keys.Control && DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, $"Deleting {count} row(s) above other lines will shift all subsequent lines.", "Continue?")) return;

        for (int i = 0; i < count; i++) dgv.Rows.RemoveAt(currentRow);
        for (int i = 0; i < dgv.Rows.Count; i++) dgv.Rows[i].Cells[0].Value = i.ToString();
    }

    private void TextEditor_FormClosing(object sender, FormClosingEventArgs e)
    {
        dgv.EndEdit();
        if (entry > -1) files[entry] = GetCurrentDGLines();
    }

    private void B_Randomize_Click(object sender, EventArgs e)
    {
        if (Mode == "gametext" && DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomizing Game Text is dangerous!", "Continue?")) return;

        var dr = WinFormsUtil.Prompt(MessageBoxButtons.YesNoCancel, $"Yes: Randomize ALL{Environment.NewLine}No: Randomize current textfile{Environment.NewLine}Cancel: Abort");
        if (dr == DialogResult.Cancel) return;

        var drs = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, $"Smart shuffle:{Environment.NewLine}Yes: Shuffle if no Variable present{Environment.NewLine}No: Pure random!");
        if (drs == DialogResult.Cancel) return;

        bool all = dr == DialogResult.Yes;
        bool smart = drs == DialogResult.Yes;

        if (entry > -1) files[entry] = GetCurrentDGLines();

        int start = all ? 0 : entry;
        int end = all ? files.Length - 1 : entry;

        List<string> strings = [];
        for (int i = start; i <= end; i++)
        {
            string[] data = files[i];
            strings.AddRange(smart ? data.Where(line => !line.Contains('[')) : data);
        }

        string[] pool = [.. strings];
        Util.Shuffle(pool);

        int ctr = 0;
        for (int i = start; i <= end; i++)
        {
            string[] data = files[i];
            for (int j = 0; j < data.Length; j++) 
            {
                if (!smart || !data[j].Contains('[')) data[j] = pool[ctr++];
            }
            files[i] = data;
        }

        SetStringsDataGridView(files[entry]);
        WinFormsUtil.Alert("Strings randomized!");
    }
}