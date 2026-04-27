using pk3DS.Core;
using System;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core.Structures;

namespace pk3DS.WinForms;

public partial class MoveEditor6 : Form
{
    public MoveEditor6(byte[][] infiles)
    {
        files = infiles;
        movelist[0] = "";

        InitializeComponent();
        Setup();
        RandSettings.GetFormSettings(this, groupBox1.Controls);
    }

    private readonly byte[][] files;
    private readonly string[] types = Main.Config.GetText(TextName.Types);
    private readonly string[] moveflavor = Main.Config.GetText(TextName.MoveFlavor);
    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private readonly string[] MoveCategories = ["Status", "Physical", "Special"];
    private readonly string[] StatCategories = ["None", "Attack", "Defense", "Special Attack", "Special Defense", "Speed", "Accuracy", "Evasion", "All"];

    private readonly string[] TargetingTypes =
    [
        "Single Adjacent Ally/Foe", "Any Ally", "Any Adjacent Ally", "Single Adjacent Foe", "Everyone but User", "All Foes",
        "All Allies", "Self", "All Pokémon on Field", "Single Adjacent Foe (2)", "Entire Field", "Opponent's Field", "User's Field", "Self",
    ];

    private readonly string[] InflictionTypes =
    [
        "None", "Paralyze", "Sleep", "Freeze", "Burn", "Poison", "Confusion", "Attract", "Capture", "Nightmare", "Curse",
        "Taunt", "Torment", "Disable", "Yawn", "Heal Block", "?", "Detect", "Leech Seed", "Embargo", "Perish Song", "Ingrain",
    ];

    private readonly string[] MoveQualities =
    [
        "Only DMG", "No DMG -> Inflict Status", "No DMG -> -Target/+User Stat", "No DMG | Heal User", "DMG | Inflict Status", "No DMG | STATUS | +Target Stat",
        "DMG | -Target Stat", "DMG | +User Stat", "DMG | Absorbs DMG", "One-Hit KO", "Affects Whole Field",
        "Affect One Side of the Field", "Forces Target to Switch", "Unique Effect",
    ];

    private void Setup()
    {
        CB_Move.Items.AddRange(movelist);
        CB_Type.Items.AddRange(types);
        CB_Category.Items.AddRange(MoveCategories);
        CB_Stat1.Items.AddRange(StatCategories);
        CB_Stat2.Items.AddRange(StatCategories);
        CB_Stat3.Items.AddRange(StatCategories);
        CB_Targeting.Items.AddRange(TargetingTypes);
        CB_Quality.Items.AddRange(MoveQualities);
        CB_Inflict.Items.AddRange(InflictionTypes);
        foreach (var s in Enum.GetNames(typeof(MoveFlag6)).Skip(1)) CLB_Flags.Items.Add(s);
        CB_Inflict.Items.Add("Special");

        CB_Move.Items.RemoveAt(0);
        CB_Move.SelectedIndex = 0;
    }

    private int entry = -1;

    private void ChangeEntry(object sender, EventArgs e)
    {
        SetEntry();
        entry = Array.IndexOf(movelist, CB_Move.Text);
        GetEntry();
    }

    private void GetEntry()
    {
        if (entry < 1) return;
        byte[] data = files[entry];
        {
            RTB.Text = moveflavor[entry].Replace("\\n", Environment.NewLine);

            var move = new Move6(data);
            CB_Type.SelectedIndex = move.Type;
            CB_Quality.SelectedIndex = move.Quality;
            CB_Category.SelectedIndex = move.Category;
            NUD_Power.Value = move.Power;
            NUD_Accuracy.Value = move.Accuracy;
            NUD_PP.Value = move.PP;
            NUD_Priority.Value = (sbyte)move.Priority;
            NUD_HitMin.Value = move.HitMin;
            NUD_HitMax.Value = move.HitMax;
            CB_Inflict.SelectedIndex = Math.Min(move.Inflict, CB_Inflict.Items.Count - 1);
            NUD_Inflict.Value = move.InflictPercent;
            NUD_0xB.Value = (byte)move.InflictCount;
            NUD_TurnMin.Value = move.TurnMin;
            NUD_TurnMax.Value = move.TurnMax;
            NUD_CritStage.Value = move.CritStage;
            NUD_Flinch.Value = move.Flinch;
            NUD_Effect.Value = move.EffectSequence;
            NUD_Recoil.Value = (sbyte)move.Recoil;
            NUD_Heal.Value = (byte)move.Healing;

            CB_Targeting.SelectedIndex = (int)move.Target;
            CB_Stat1.SelectedIndex = move.Stat1;
            CB_Stat2.SelectedIndex = move.Stat2;
            CB_Stat3.SelectedIndex = move.Stat3;
            NUD_Stat1.Value = (sbyte)move.Stat1Stage;
            NUD_Stat2.Value = (sbyte)move.Stat2Stage;
            NUD_Stat3.Value = (sbyte)move.Stat3Stage;
            NUD_StatP1.Value = move.Stat1Percent;
            NUD_StatP2.Value = move.Stat2Percent;
            NUD_StatP3.Value = move.Stat3Percent;

            var flags = (uint)move.Flags;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                CLB_Flags.SetItemChecked(i, ((flags >> i) & 1) == 1);
        }
    }

    private void SetEntry()
    {
        if (entry < 1) return;
        byte[] data = files[entry];
        {
            var move = new Move6(data)
            {
                Type = CB_Type.SelectedIndex,
                Quality = CB_Quality.SelectedIndex,
                Category = CB_Category.SelectedIndex,
                Power = (int)NUD_Power.Value,
                Accuracy = (int)NUD_Accuracy.Value,
                PP = (int)NUD_PP.Value,
                Priority = (int)NUD_Priority.Value,
                HitMin = (int)NUD_HitMin.Value,
                HitMax = (int)NUD_HitMax.Value,
                Inflict = CB_Inflict.SelectedIndex,
                InflictPercent = (int)NUD_Inflict.Value,
                InflictCount = (MoveInflictDuration)NUD_0xB.Value,
                TurnMin = (int)NUD_TurnMin.Value,
                TurnMax = (int)NUD_TurnMax.Value,
                CritStage = (int)NUD_CritStage.Value,
                Flinch = (int)NUD_Flinch.Value,
                EffectSequence = (int)NUD_Effect.Value,
                Recoil = (int)NUD_Recoil.Value,
                Healing = (Heal)NUD_Heal.Value,
                Target = (MoveTarget)CB_Targeting.SelectedIndex,
                Stat1 = CB_Stat1.SelectedIndex,
                Stat2 = CB_Stat2.SelectedIndex,
                Stat3 = CB_Stat3.SelectedIndex,
                Stat1Stage = (int)NUD_Stat1.Value,
                Stat2Stage = (int)NUD_Stat2.Value,
                Stat3Stage = (int)NUD_Stat3.Value,
                Stat1Percent = (int)NUD_StatP1.Value,
                Stat2Percent = (int)NUD_StatP2.Value,
                Stat3Percent = (int)NUD_StatP3.Value,
            };

            uint flagval = 0;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                flagval |= CLB_Flags.GetItemChecked(i) ? 1u << i : 0;
            BitConverter.GetBytes(flagval).CopyTo(data, 0x1E);
        }
        files[entry] = data;
    }

    private void B_Table_Click(object sender, EventArgs e)
    {
        var items = files.Select(z => new Move6(z));
        Clipboard.SetText(TableUtil.GetTable(items, movelist));
        System.Media.SystemSounds.Asterisk.Play();
    }

    private void CloseForm(object sender, FormClosingEventArgs e)
    {
        SetEntry();
        RandSettings.SetFormSettings(this, groupBox1.Controls);
    }

    private void B_RandAll_Click(object sender, EventArgs e)
    {
        if (!CHK_Category.Checked && !CHK_Type.Checked)
        {
            WinFormsUtil.Alert("Cannot randomize Moves.", "Please check any of the options on the right to randomize Moves.");
            return;
        }

        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize Moves? Cannot undo.", "Double check options on the right before continuing.") != DialogResult.Yes) return;
        
        SetEntry();
        Random rnd = Util.Rand;
        
        for (int i = 1; i < files.Length; i++)
        {
            if (i is 165 or 174) continue; // Don't change Struggle or Curse

            byte[] data = files[i];

            if (CHK_Category.Checked && data[0x02] > 0) // Change Damage Category if Not Status
                data[0x02] = (byte)rnd.Next(1, 3);

            if (CHK_Type.Checked) // Change Move Type
                data[0x00] = (byte)rnd.Next(0, 18);
                
            files[i] = data;
        }
        
        GetEntry();
        WinFormsUtil.Alert("All Moves have been randomized!");
    }

    private void B_Metronome_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Play using Metronome Mode?", "This will set the Base PP for every other Move to 0!") != DialogResult.Yes) return;

        SetEntry();
        
        for (int i = 1; i < files.Length; i++)
        {
            byte[] data = files[i];
            
            if (i != 117 && i != 32)
                data[0x05] = 0; 
            else if (i == 117)
                data[0x05] = 40;
            else if (i == 32)
                data[0x05] = 1;
                
            files[i] = data;
        }
        
        GetEntry();
        WinFormsUtil.Alert("All Moves have had their Base PP values modified!");
    }
}