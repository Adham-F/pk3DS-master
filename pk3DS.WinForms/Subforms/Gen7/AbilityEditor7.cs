using pk3DS.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class AbilityEditor7 : Form
{
    private byte[] battleCro;
    private string croPath;
    
    // Physical file offset shift: RAM Address - Shift = File Position
    private const int RAM_SHIFT = 0x6DD000; 
    private int currentTableOffset = 0x104710; 
    private int entry = -1;

    public AbilityEditor7()
    {
        croPath = Path.Combine(Main.RomFSPath, "Battle.cro");
        if (File.Exists(croPath)) battleCro = File.ReadAllBytes(croPath);
        
        InitializeComponent();
        
        CB_Ability.Items.AddRange(Main.Config.GetText(TextName.AbilityNames));
        if (CB_Ability.Items.Count > 1) CB_Ability.SelectedIndex = 1;
    }

    private void ChangeEntry(object sender, EventArgs e)
    {
        entry = CB_Ability.SelectedIndex;
        LoadEntry();
    }

    private void LoadEntry()
    {
        if (entry < 1 || battleCro == null) return;

        // 1. Show Description
        var descriptions = Main.Config.GameTextStrings[102];
        RTB_Description.Text = entry < descriptions.Length ? descriptions[entry].Replace("\\n", " ") : "";

        // 2. Find Pointer in Ability Table (0x104710)
        int tablePos = currentTableOffset + (entry * 8);
        uint ramPtr = BitConverter.ToUInt32(battleCro, tablePos + 4);
        int fileOff = (int)(ramPtr - RAM_SHIFT);

        // 3. Load 32 Bytes of Hex into the Editor
        if (fileOff > 0 && fileOff < battleCro.Length - 32)
        {
            string hex = BitConverter.ToString(battleCro, fileOff, 32).Replace("-", " ");
            // Format into 16-byte rows for readability
            RTB_HexViewer.Text = hex.Substring(0, 47) + Environment.NewLine + hex.Substring(48);
        }
        else
        {
            RTB_HexViewer.Text = "POINTER OUTSIDE READABLE DATA: 0x" + ramPtr.ToString("X8");
        }
    }

    private void B_Save_Click(object sender, EventArgs e)
    {
        if (entry < 1 || battleCro == null) return;

        int tablePos = currentTableOffset + (entry * 8);
        uint ramPtr = BitConverter.ToUInt32(battleCro, tablePos + 4);
        int fileOff = (int)(ramPtr - RAM_SHIFT);

        try
        {
            // Parse the hex string back into bytes
            string rawHex = RTB_HexViewer.Text.Replace(Environment.NewLine, " ");
            string[] hexParts = rawHex.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < Math.Min(hexParts.Length, 32); i++)
            {
                battleCro[fileOff + i] = Convert.ToByte(hexParts[i], 16);
            }

            File.WriteAllBytes(croPath, battleCro);
            WinFormsUtil.Alert("Binary patch successful at offset 0x" + fileOff.ToString("X6"));
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error("Hex Parsing Error: Ensure format is XX XX XX...\n" + ex.Message);
        }
    }
}