using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using pk3DS.Core;
using pk3DS.Core.Structures;

namespace pk3DS.WinForms;

public partial class ItemEditor7 : Form
{
    public ItemEditor7(byte[][] infiles)
    {
        files = infiles;
        itemlist[0] = "";

        InitializeComponent();
        Setup();
        ApplyUITweaks(); // Dynamically injects Import/Export buttons
    }

    private readonly byte[][] files;
    private readonly string[] itemlist = Main.Config.GetText(TextName.ItemNames);
    private readonly string[] itemflavor = Main.Config.GetText(TextName.ItemFlavor);

    private void Setup()
    {
        CB_Item.Items.AddRange(itemlist);
        CB_Item.SelectedIndex = 1;
    }

    private void ApplyUITweaks()
    {
        // Widen the form slightly to fit the new buttons
        this.Width += 100;

        Button B_ImportTxt = new Button();
        B_ImportTxt.Text = "Import .txt";
        B_ImportTxt.Size = new Size(85, 23);
        B_ImportTxt.Location = new Point(B_Table.Left - 95, B_Table.Top);
        B_ImportTxt.Click += B_ImportTxt_Click;

        Button B_ExportTxt = new Button();
        B_ExportTxt.Text = "Export .txt";
        B_ExportTxt.Size = new Size(85, 23);
        B_ExportTxt.Location = new Point(B_ImportTxt.Left - 95, B_Table.Top);
        B_ExportTxt.Click += B_ExportTxt_Click;

        // --- NEW: Make Mega Stone Button ---
        Button B_MakeMega = new Button();
        B_MakeMega.Text = "Make Mega";
        B_MakeMega.Size = new Size(85, 23);
        B_MakeMega.Location = new Point(B_ExportTxt.Left - 95, B_Table.Top);
        B_MakeMega.Click += B_MakeMega_Click;

        this.Controls.Add(B_ImportTxt);
        this.Controls.Add(B_ExportTxt);
        this.Controls.Add(B_MakeMega);

        B_ImportTxt.BringToFront();
        B_ExportTxt.BringToFront();
        B_MakeMega.BringToFront();
    }

    private void B_ExportTxt_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialog { FileName = "Items.txt", Filter = "Text File|*.txt" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        // Insert modding tips at the top of the text file!
        sb.AppendLine("// === Modding Tips ===");
        sb.AppendLine("// For adding Mega Stones, remember that the \"Packed\" part should be 31, the \"SortIndex\" should follow after 224, and Unk_0xD should be 1.");
        sb.AppendLine("// For new held battle items, the HeldEffect should be after 189, \"Packed\" should be 31.");
        sb.AppendLine("// ====================");
        sb.AppendLine();

        // Dynamically get all properties of the Item
        var props = typeof(Item).GetProperties();
        for (int i = 1; i < files.Length; i++)
        {
            var item = new Item(files[i]);
            string itemName = i < itemlist.Length ? itemlist[i] : $"Item {i}";
            sb.AppendLine($"====== {i:000} - {itemName} ======");
            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    sb.AppendLine($"{prop.Name}: {prop.GetValue(item)}");
                }
            }
            sb.AppendLine();
        }
        File.WriteAllText(sfd.FileName, sb.ToString());
        WinFormsUtil.Alert("Items exported successfully!");
    }

private void B_ImportTxt_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string[] lines = File.ReadAllLines(ofd.FileName);
        int currentIndex = -1;
        
        // Fix: Use 'object' to box the Item struct. This satisfies both the null checks
        // and C#'s requirement for modifying structs via Reflection.
        object currentItemObj = null; 
        var props = typeof(Item).GetProperties().ToDictionary(p => p.Name, p => p);

        foreach (string line in lines)
        {
            // Ignore empty lines and our comment lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

            if (line.StartsWith("======"))
            {
                // Save the previous item before moving to the next
                if (currentIndex != -1 && currentItemObj != null)
                {
                    files[currentIndex] = ((Item)currentItemObj).Write();
                }

                // Parse the new index
                string[] parts = line.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[1], out int index))
                {
                    currentIndex = index;
                    if (currentIndex < files.Length)
                    {
                        currentItemObj = new Item(files[currentIndex]); // Box the new struct
                    }
                    else
                    {
                        currentItemObj = null;
                    }
                }
                continue;
            }

            if (currentItemObj == null) continue;

            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                string propName = line.Substring(0, colonIndex).Trim();
                string propVal = line.Substring(colonIndex + 1).Trim();

                if (props.TryGetValue(propName, out var prop) && prop.CanWrite)
                {
                    try
                    {
                        // Convert text value back to the proper data type (byte, short, int, etc.)
                        object val = Convert.ChangeType(propVal, prop.PropertyType);
                        prop.SetValue(currentItemObj, val); // Modifies the boxed struct
                    }
                    catch { } // Ignore lines that can't be parsed properly
                }
            }
        }
        
        // Save the very last item in the loop
        if (currentIndex != -1 && currentItemObj != null)
        {
            files[currentIndex] = ((Item)currentItemObj).Write();
        }

        GetEntry(); // Refresh UI
        WinFormsUtil.Alert("Items imported successfully!");
    }

    private int entry = -1;

    private void B_MakeMega_Click(object sender, EventArgs e)
    {
        if (Grid.SelectedObject == null) return;

        object itemObj = Grid.SelectedObject;
        var type = itemObj.GetType();

        // Safely set properties based on the exact names the PropertyGrid uses
        var propPacked = type.GetProperty("Packed");
        if (propPacked != null) propPacked.SetValue(itemObj, Convert.ChangeType(31, propPacked.PropertyType));

        var propUnk = type.GetProperty("Unk_0xD");
        if (propUnk != null) propUnk.SetValue(itemObj, Convert.ChangeType(1, propUnk.PropertyType));

        var propSort = type.GetProperty("SortIndex");
        if (propSort != null) 
        {
            // Defaulting to 225 as the baseline "after 224"
            propSort.SetValue(itemObj, Convert.ChangeType(225, propSort.PropertyType)); 
        }

        // Re-assign the boxed object back to the grid to force a visual refresh
        Grid.SelectedObject = itemObj;
        Grid.Refresh();

        WinFormsUtil.Alert("Item updated with base Mega Stone values!", "SortIndex set to 225. If you are adding multiple custom Mega Stones, remember to manually increase this number by 1 for each new stone.");
    }

    private void ChangeEntry(object sender, EventArgs e)
    {
        SetEntry();
        entry = CB_Item.SelectedIndex;
        L_Index.Text = $"Index: {entry:000}";
        GetEntry();
    }

    private void GetEntry()
    {
        if (entry < 1) return;
        Grid.SelectedObject = new Item(files[entry]);

        RTB.Text = itemflavor[entry].Replace("\\n", Environment.NewLine);
    }

    private void SetEntry()
    {
        if (entry < 1) return;
        files[entry] = ((Item)Grid.SelectedObject).Write();
    }

    private void Form_Closing(object sender, FormClosingEventArgs e)
    {
        SetEntry();
    }

    private static readonly byte[] ItemIconTableSignature =
    [
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x01, 0x01, 0x00,
    ];

    public static int GetItemMapOffset()
    {
        if (Main.ExeFSPath == null) { WinFormsUtil.Alert("No exeFS code to load."); return -1; }
        string[] exefsFiles = Directory.GetFiles(Main.ExeFSPath);
        if (!File.Exists(exefsFiles[0]) || !Path.GetFileNameWithoutExtension(exefsFiles[0]).Contains("code")) { WinFormsUtil.Alert("No .code.bin detected."); return -1; }
        byte[] data = File.ReadAllBytes(exefsFiles[0]);

        byte[] reference = ItemIconTableSignature;

        return Util.IndexOfBytes(data, reference, 0x400000, 0) - 2 + reference.Length;
    }

    private void B_Table_Click(object sender, EventArgs e)
    {
        var items = files.Select(z => new Item(z));
        Clipboard.SetText(TableUtil.GetTable(items, itemlist));
        System.Media.SystemSounds.Asterisk.Play();
    }
}