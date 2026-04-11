using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using pk3DS.Core.Structures.PersonalInfo;
using pk3DS.Core;
using pk3DS.Core.Randomizers;

namespace pk3DS.WinForms;

public partial class PersonalEditor7 : Form
{
    private int[][] vanillaStats;
    
    private readonly byte[][] learnsets;
    private readonly byte[][] eggmoves;
    public PersonalEditor7(byte[][] infiles, byte[][] learnsets, byte[][] eggmoves)
    {
        this.learnsets = learnsets;
        this.eggmoves = eggmoves;
        InitializeComponent();
        helditem_boxes = [CB_HeldItem1, CB_HeldItem2, CB_HeldItem3];
        ability_boxes = [CB_Ability1, CB_Ability2, CB_Ability3];
        typing_boxes = [CB_Type1, CB_Type2];
        eggGroup_boxes = [CB_EggGroup1, CB_EggGroup2];
        byte_boxes = [TB_BaseHP, TB_BaseATK, TB_BaseDEF, TB_BaseSPA, TB_BaseSPD, TB_BaseSPE, TB_Gender, TB_HatchCycles, TB_Friendship, TB_CatchRate, TB_CallRate,
        ];
        ev_boxes = [TB_HPEVs, TB_ATKEVs, TB_DEFEVs, TB_SPEEVs, TB_SPAEVs, TB_SPDEVs];
        rstat_boxes = [CHK_rHP, CHK_rATK, CHK_rDEF, CHK_rSPA, CHK_rSPD, CHK_rSPE];
        files = infiles;
        originalFiles = infiles.Select(a => (byte[])a.Clone()).ToArray(); // Snapshots YOUR custom ROM data

        // Bind dynamic updates for the Stat Diff calculator
        TB_BaseHP.TextChanged += UpdateDynamicDiff;
        TB_BaseATK.TextChanged += UpdateDynamicDiff;
        TB_BaseDEF.TextChanged += UpdateDynamicDiff;
        TB_BaseSPA.TextChanged += UpdateDynamicDiff;
        TB_BaseSPD.TextChanged += UpdateDynamicDiff;
        TB_BaseSPE.TextChanged += UpdateDynamicDiff;

        species[0] = "---";
        abilities[0] = items[0] = moves[0] = "";
        var altForms = Main.Config.Personal.GetFormList(species, Main.Config.MaxSpeciesID);
        entryNames = Main.Config.Personal.GetPersonalEntryList(altForms, species, Main.Config.MaxSpeciesID, out baseForms, out formVal);
        TMs = TMEditor7.GetTMHMList();

        Setup();
        LoadVanillaStats();
        CB_Species.SelectedIndex = 1;
        RandSettings.GetFormSettings(this, TP_Randomizer.Controls);
    }
    #region Global Variables
    private readonly byte[][] files;

    private readonly string[] items = Main.Config.GetText(TextName.ItemNames);
    private readonly string[] moves = Main.Config.GetText(TextName.MoveNames);
    private readonly string[] species = Main.Config.GetText(TextName.SpeciesNames);
    private readonly string[] abilities = Main.Config.GetText(TextName.AbilityNames);
    //private readonly string[] forms = Main.Config.GetText(TextName.Forms);
    private readonly string[] types = Main.Config.GetText(TextName.Types);

    private readonly string[] entryNames;

    private readonly ComboBox[] helditem_boxes;
    private readonly ComboBox[] ability_boxes;
    private readonly ComboBox[] typing_boxes;
    private readonly ComboBox[] eggGroup_boxes;

    private readonly MaskedTextBox[] byte_boxes;
    private readonly MaskedTextBox[] ev_boxes;
    private readonly CheckBox[] rstat_boxes;

    private readonly string[] eggGroups = ["---", "Monster", "Water 1", "Bug", "Flying", "Field", "Fairy", "Grass", "Human-Like", "Water 3", "Mineral", "Amorphous", "Water 2", "Ditto", "Dragon", "Undiscovered",
    ];
    private readonly string[] EXPGroups = ["Medium-Fast", "Erratic", "Fluctuating", "Medium-Slow", "Fast", "Slow"];
    private readonly string[] colors = ["Red", "Blue", "Yellow", "Green", "Black", "Brown", "Purple", "Gray", "White", "Pink",
    ];
    private readonly ushort[] tutormoves = [520, 519, 518, 338, 307, 308, 434, 620];

    internal static int[] Tutors_USUM =
    [
        450, 343, 162, 530, 324, 442, 402, 529, 340, 067, 441, 253, 009, 007, 008,
        277, 335, 414, 492, 356, 393, 334, 387, 276, 527, 196, 401,      428, 406, 304, 231,
        020, 173, 282, 235, 257, 272, 215, 366, 143, 220, 202, 409,      264, 351, 352,
        380, 388, 180, 495, 270, 271, 478, 472, 283, 200, 278, 289, 446,      285,

        477, 502, 432, 710, 707, 675, 673,
    ];
    internal static int[] Tutors_USUM_Lengths = [15, 16, 15, 14, 7]; // Default lengths

    private readonly int[] baseForms, formVal;
    private readonly ushort[] TMs;
    private int entry = -1;
    private static bool[] ClipboardTMs;
    private static bool[] ClipboardTutors;
    private static bool[] ClipboardBeachTutors;
    private readonly byte[][] originalFiles;
    #endregion
    private void Setup()
    {
        CLB_TM.Items.Clear();

        if (TMs.Length == 0) // No ExeFS to grab TMs from.
        {
            for (int i = 1; i <= 100; i++)
                CLB_TM.Items.Add($"TM{i:00}");
        }
        else // Use TM moves.
        {
            for (int i = 1; i <= 100; i++)
                CLB_TM.Items.Add($"TM{i:00} {moves[TMs[i - 1]]}");
        }
        foreach (ushort m in tutormoves)
            CLB_MoveTutors.Items.Add(moves[m]);

        for (int i = 0; i < entryNames.Length; i++)
            CB_Species.Items.Add($"{entryNames[i]} - {i:000}");

        foreach (ComboBox cb in helditem_boxes)
        {
            cb.Items.AddRange(items);
        }

        CB_ZItem.Items.AddRange(items);
        CB_ZBaseMove.Items.AddRange(moves);
        CB_ZMove.Items.AddRange(moves);

        foreach (ComboBox cb in ability_boxes)
        {
            cb.Items.AddRange(abilities);
        }

        foreach (ComboBox cb in typing_boxes)
        {
            cb.Items.AddRange(types);
        }

        foreach (ComboBox cb in eggGroup_boxes)
        {
            cb.Items.AddRange(eggGroups);
        }

        CB_Color.Items.AddRange(colors);

        CB_EXPGroup.Items.AddRange(EXPGroups);

        if (Main.Config.USUM)
        {
            string croPath = Path.Combine(Main.RomFSPath, "Shop.cro");
            var tutorData = TutorEditor7.GetUSUMTutorData(croPath, Tutors_USUM);
            Tutors_USUM = tutorData.moves;
            Tutors_USUM_Lengths = tutorData.lengths;

            foreach (var tutor in Tutors_USUM)
                CLB_BeachTutors.Items.Add(moves[tutor]);
        }

        // toggle usum content
        CHK_BeachTutors.Checked = CHK_BeachTutors.Visible =
            CLB_BeachTutors.Visible = CLB_BeachTutors.Enabled = L_BeachTutors.Visible = Main.Config.USUM;
    }

    private void CB_Species_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (entry > -1 && !dumping) SaveEntry();
        entry = CB_Species.SelectedIndex;
        ReadEntry();
    }

    private void ByteLimiter(object sender, EventArgs e)
    {
        if (sender is not MaskedTextBox mtb)
            return;
        _ = int.TryParse(mtb.Text, out int val);
        if (Array.IndexOf(byte_boxes, mtb) > -1 && val > 255)
            mtb.Text = "255";
        else if (Array.IndexOf(ev_boxes, mtb) > -1 && val > 3)
            mtb.Text = "3";
    }

    private PersonalInfo pkm;

    private void ReadInfo()
    {
        pkm = Main.SpeciesStat[entry];

        TB_BaseHP.Text = pkm.HP.ToString("000");
        TB_BaseATK.Text = pkm.ATK.ToString("000");
        TB_BaseDEF.Text = pkm.DEF.ToString("000");
        TB_BaseSPE.Text = pkm.SPE.ToString("000");
        TB_BaseSPA.Text = pkm.SPA.ToString("000");
        TB_BaseSPD.Text = pkm.SPD.ToString("000");
        TB_HPEVs.Text = pkm.EV_HP.ToString("0");
        TB_ATKEVs.Text = pkm.EV_ATK.ToString("0");
        TB_DEFEVs.Text = pkm.EV_DEF.ToString("0");
        TB_SPEEVs.Text = pkm.EV_SPE.ToString("0");
        TB_SPAEVs.Text = pkm.EV_SPA.ToString("0");
        TB_SPDEVs.Text = pkm.EV_SPD.ToString("0");

        CB_Type1.SelectedIndex = pkm.Types[0];
        CB_Type2.SelectedIndex = pkm.Types[1];

        TB_CatchRate.Text = pkm.CatchRate.ToString("000");
        TB_Stage.Text = pkm.EvoStage.ToString("0");

        CB_HeldItem1.SelectedIndex = pkm.Items[0];
        CB_HeldItem2.SelectedIndex = pkm.Items[1];
        CB_HeldItem3.SelectedIndex = pkm.Items[2];

        TB_Gender.Text = pkm.Gender.ToString("000");
        TB_HatchCycles.Text = pkm.HatchCycles.ToString("000");
        TB_Friendship.Text = pkm.BaseFriendship.ToString("000");

        CB_EXPGroup.SelectedIndex = pkm.EXPGrowth;

        CB_EggGroup1.SelectedIndex = pkm.EggGroups[0];
        CB_EggGroup2.SelectedIndex = pkm.EggGroups[1];

        CB_Ability1.SelectedIndex = pkm.Abilities[0];
        CB_Ability2.SelectedIndex = pkm.Abilities[1];
        CB_Ability3.SelectedIndex = pkm.Abilities[2];

        TB_FormeCount.Text = pkm.FormeCount.ToString("000");
        TB_FormeSprite.Text = pkm.FormeSprite.ToString("000");

        TB_RawColor.Text = pkm.Color.ToString("000");
        CB_Color.SelectedIndex = pkm.Color & 0xF;

        TB_BaseExp.Text = pkm.BaseEXP.ToString("000");
        TB_BST.Text = pkm.BST.ToString("000");

        TB_Height.Text = ((decimal)pkm.Height / 100).ToString("00.00");
        TB_Weight.Text = ((decimal)pkm.Weight / 10).ToString("000.0");

        for (int i = 0; i < CLB_TM.Items.Count && i < pkm.TMHM.Length; i++)
            CLB_TM.SetItemChecked(i, pkm.TMHM[i]); 

        for (int i = 0; i < CLB_MoveTutors.Items.Count && i < pkm.TypeTutors.Length; i++)
            CLB_MoveTutors.SetItemChecked(i, pkm.TypeTutors[i]);

        if (Main.Config.SM || Main.Config.USUM)
        {
            PersonalInfoSM sm = (PersonalInfoSM)pkm;
            TB_CallRate.Text = sm.EscapeRate.ToString("000");
            CB_ZItem.SelectedIndex = sm.SpecialZ_Item;
            CB_ZBaseMove.SelectedIndex = sm.SpecialZ_BaseMove;
            CB_ZMove.SelectedIndex = sm.SpecialZ_ZMove;
            CHK_Variant.Checked = sm.LocalVariant;
        }
        var special = pkm.SpecialTutors;
        int currentIdx = 0;
        for (int loc = 0; loc < 4 && loc < special.Length; loc++)
        {
            if (Tutors_USUM_Lengths == null || loc >= Tutors_USUM_Lengths.Length) break;
            int count = Tutors_USUM_Lengths[loc];
            for (int b = 0; b < count && b < special[loc].Length; b++)
            {
                if (currentIdx < CLB_BeachTutors.Items.Count)
                {
                    CLB_BeachTutors.SetItemChecked(currentIdx, special[loc][b]);
                    currentIdx++;
                }
            }
        }
    }

    private void ReadEntry()
    {
        ReadInfo();

        if (dumping) return;
        int s = baseForms[entry];
        int f = formVal[entry];
        if (entry <= Main.Config.MaxSpeciesID)
            s = entry;
        var rawImg = WinFormsUtil.GetSprite(s, f, 0, 0, Main.Config);
        var bigImg = new Bitmap(rawImg.Width * 2, rawImg.Height * 2);
        for (int x = 0; x < rawImg.Width; x++)
        {
            for (int y = 0; y < rawImg.Height; y++)
            {
                Color c = rawImg.GetPixel(x, y);
                bigImg.SetPixel(2 * x, 2 * y, c);
                bigImg.SetPixel((2 * x) + 1, 2 * y, c);
                bigImg.SetPixel(2 * x, (2 * y) + 1, c);
                bigImg.SetPixel((2 * x) + 1, (2 * y) + 1, c);
            }
        }
        PB_MonSprite.Image = bigImg;
    }

    private void SavePersonal()
    {
        pkm.HP = Convert.ToByte(TB_BaseHP.Text);
        pkm.ATK = Convert.ToByte(TB_BaseATK.Text);
        pkm.DEF = Convert.ToByte(TB_BaseDEF.Text);
        pkm.SPE = Convert.ToByte(TB_BaseSPE.Text);
        pkm.SPA = Convert.ToByte(TB_BaseSPA.Text);
        pkm.SPD = Convert.ToByte(TB_BaseSPD.Text);

        pkm.EV_HP = Convert.ToByte(TB_HPEVs.Text);
        pkm.EV_ATK = Convert.ToByte(TB_ATKEVs.Text);
        pkm.EV_DEF = Convert.ToByte(TB_DEFEVs.Text);
        pkm.EV_SPE = Convert.ToByte(TB_SPEEVs.Text);
        pkm.EV_SPA = Convert.ToByte(TB_SPAEVs.Text);
        pkm.EV_SPD = Convert.ToByte(TB_SPDEVs.Text);

        pkm.CatchRate = Convert.ToByte(TB_CatchRate.Text);
        pkm.EvoStage = Convert.ToByte(TB_Stage.Text);

        pkm.Types = [CB_Type1.SelectedIndex, CB_Type2.SelectedIndex];
        pkm.Items = [CB_HeldItem1.SelectedIndex, CB_HeldItem2.SelectedIndex, CB_HeldItem3.SelectedIndex];

        pkm.Gender = Convert.ToByte(TB_Gender.Text);
        pkm.HatchCycles = Convert.ToByte(TB_HatchCycles.Text);
        pkm.BaseFriendship = Convert.ToByte(TB_Friendship.Text);
        pkm.EXPGrowth = (byte)CB_EXPGroup.SelectedIndex;
        pkm.EggGroups = [CB_EggGroup1.SelectedIndex, CB_EggGroup2.SelectedIndex];
        pkm.Abilities = [CB_Ability1.SelectedIndex, CB_Ability2.SelectedIndex, CB_Ability3.SelectedIndex];

        pkm.FormeSprite = Convert.ToUInt16(TB_FormeSprite.Text);
        pkm.FormeCount = Convert.ToByte(TB_FormeCount.Text);
        pkm.Color = (byte)(Convert.ToByte(CB_Color.SelectedIndex) | (Convert.ToByte(TB_RawColor.Text) & 0xF0));
        pkm.BaseEXP = Convert.ToUInt16(TB_BaseExp.Text);

        _ = decimal.TryParse(TB_Height.Text, out var h);
        _ = decimal.TryParse(TB_Weight.Text, out var w);
        pkm.Height = (int)(h * 100);
        pkm.Weight = (int)(w * 10);

        for (int i = 0; i < CLB_TM.Items.Count && i < pkm.TMHM.Length; i++)
            pkm.TMHM[i] = CLB_TM.GetItemChecked(i);

        for (int t = 0; t < CLB_MoveTutors.Items.Count && t < pkm.TypeTutors.Length; t++)
            pkm.TypeTutors[t] = CLB_MoveTutors.GetItemChecked(t);

        if (Main.Config.SM || Main.Config.USUM)
        {
            pkm.EscapeRate = Convert.ToByte(TB_CallRate.Text);
            PersonalInfoSM sm = (PersonalInfoSM)pkm;
            sm.SpecialZ_Item = CB_ZItem.SelectedIndex;
            sm.SpecialZ_BaseMove = CB_ZBaseMove.SelectedIndex;
            sm.SpecialZ_ZMove = CB_ZMove.SelectedIndex;
            sm.LocalVariant = CHK_Variant.Checked;
        }
        var special = pkm.SpecialTutors;
        int currentBIdx = 0;
        for (int loc = 0; loc < 4 && loc < special.Length; loc++)
        {
            if (Tutors_USUM_Lengths == null || loc >= Tutors_USUM_Lengths.Length) break;
            int count = Tutors_USUM_Lengths[loc];
            for (int b = 0; b < count && b < special[loc].Length; b++)
            {
                if (currentBIdx < CLB_BeachTutors.Items.Count)
                {
                    special[loc][b] = CLB_BeachTutors.GetItemChecked(currentBIdx);
                    currentBIdx++;
                }
            }
        }
        pkm.SpecialTutors = special;
    }

    private void SaveEntry()
    {
        SavePersonal();
        byte[] edits = pkm.Write();
        files[entry] = edits;
    }

    private void B_Randomize_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize all? Cannot undo.", "Double check Randomization settings in the Enhancements tab.") != DialogResult.Yes)
            return;
        SaveEntry();

        // input settings
        var rnd = new PersonalRandomizer(Main.SpeciesStat, Main.Config)
        {
            TypeCount = CB_Type1.Items.Count,
            ModifyCatchRate = CHK_CatchRate.Checked,
            ModifyEggGroup = CHK_EggGroup.Checked,
            ModifyStats = CHK_Stats.Checked,
            ShuffleStats = CHK_Shuffle.Checked,
            StatsToRandomize = rstat_boxes.Select(g => g.Checked).ToArray(),
            ModifyAbilities = CHK_Ability.Checked,
            ModifyLearnsetTM = CHK_TM.Checked,
            ModifyLearnsetHM = false, // no HMs in Gen 7
            ModifyLearnsetTypeTutors = CHK_Tutors.Checked,
            ModifyLearnsetMoveTutors = Main.Config.USUM && CHK_BeachTutors.Checked,
            ModifyTypes = CHK_Type.Checked,
            ModifyHeldItems = CHK_Item.Checked,
            SameTypeChance = NUD_TypePercent.Value,
            SameEggGroupChance = NUD_Egg.Value,
            StatDeviation = NUD_StatDev.Value,
            AllowWonderGuard = CHK_WGuard.Checked,
        };

        rnd.Execute();
        Main.SpeciesStat.Select(z => z.Write()).ToArray().CopyTo(files, 0);

        ReadEntry();
        WinFormsUtil.Alert("Randomized all Pokémon Personal data entries according to specification!", "Press the Dump All button to view the new Personal data!");
    }

    private void B_ModifyAll(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Modify all? Cannot undo.", "Double check Modification settings in the Enhancements tab.") != DialogResult.Yes) return;

        for (int i = 1; i < CB_Species.Items.Count; i++)
        {
            CB_Species.SelectedIndex = i; // Get new Species

            if (CHK_NoEV.Checked)
            {
                for (int z = 0; z < 6; z++)
                    ev_boxes[z].Text = 0.ToString();
            }

            if (CHK_Growth.Checked)
                CB_EXPGroup.SelectedIndex = 5;
            if (CHK_EXP.Checked)
                TB_BaseExp.Text = ((float)NUD_EXP.Value * (Convert.ToUInt16(TB_BaseExp.Text) / 100f)).ToString("000");

            if (CHK_NoTutor.Checked)
            {
                foreach (int tm in CLB_TM.CheckedIndices)
                    CLB_TM.SetItemCheckState(tm, CheckState.Unchecked);
                foreach (int mt in CLB_MoveTutors.CheckedIndices)
                    CLB_MoveTutors.SetItemCheckState(mt, CheckState.Unchecked);
                foreach (int ao in CLB_BeachTutors.CheckedIndices)
                    CLB_BeachTutors.SetItemCheckState(ao, CheckState.Unchecked);
            }

            if (CHK_FullTMCompatibility.Checked)
            {
                for (int t = 0; t < CLB_TM.Items.Count; t++)
                    CLB_TM.SetItemCheckState(t, CheckState.Checked);
            }

            if (CHK_FullMoveTutorCompatibility.Checked)
            {
                for (int m = 0; m < CLB_MoveTutors.Items.Count; m++)
                    CLB_MoveTutors.SetItemCheckState(m, CheckState.Checked);
            }

            if (CHK_FullBeachTutorCompatibility.Checked)
            {
                for (int m = 0; m < CLB_BeachTutors.Items.Count; m++)
                    CLB_BeachTutors.SetItemCheckState(m, CheckState.Checked);
            }

            if (CHK_QuickHatch.Checked)
                TB_HatchCycles.Text = 1.ToString();
            if (CHK_CallRate.Checked)
                TB_CallRate.Text = ((int)NUD_CallRate.Value).ToString();
            if (CHK_CatchRateMod.Checked)
                TB_CatchRate.Text = ((int)NUD_CatchRateMod.Value).ToString();
        }
        CB_Species.SelectedIndex = 1;
        WinFormsUtil.Alert("Modified all Pokémon Personal data entries according to specification!", "Press the Dump All button to view the new Personal data!");
    }

    private bool dumping;

    private void B_Dump_Click(object sender, EventArgs e)
    {
        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Dump all Personal Entries to Text File?"))
            return;
        var sfd = new SaveFileDialog { FileName = "Personal Entries.txt", Filter = "Text File|*.txt" };
        SystemSounds.Asterisk.Play();
        if (sfd.ShowDialog() != DialogResult.OK)
            return;

        dumping = true;
        List<string> lines = [];
        for (int i = 0; i < CB_Species.Items.Count; i++)
        {
            CB_Species.SelectedIndex = i; // Get new Species
            lines.Add("======");
            lines.Add($"{entry} - {CB_Species.Text} (Stage: {TB_Stage.Text})");
            lines.Add("======");
            lines.Add($"Base Stats: {TB_BaseHP.Text}.{TB_BaseATK.Text}.{TB_BaseDEF.Text}.{TB_BaseSPA.Text}.{TB_BaseSPD.Text}.{TB_BaseSPE.Text} (BST: {pkm.BST})");
            lines.Add($"EV Yield: {TB_HPEVs.Text}.{TB_ATKEVs.Text}.{TB_DEFEVs.Text}.{TB_SPAEVs.Text}.{TB_SPDEVs.Text}.{TB_SPEEVs.Text}");
            lines.Add($"Abilities: {CB_Ability1.Text} (1) | {CB_Ability2.Text} (2) | {CB_Ability3.Text} (H)");
            lines.Add(string.Format(CB_Type1.SelectedIndex != CB_Type2.SelectedIndex
                ? "Type: {0} / {1}"
                : "Type: {0}", CB_Type1.Text, CB_Type2.Text));

            lines.Add($"Item 1 (50%): {CB_HeldItem1.Text}");
            lines.Add($"Item 2 (5%): {CB_HeldItem2.Text}");
            lines.Add($"Item 3 (1%): {CB_HeldItem3.Text}");

            lines.Add($"EXP Group: {CB_EXPGroup.Text}");
            lines.Add(string.Format(CB_EggGroup1.SelectedIndex != CB_EggGroup2.SelectedIndex
                ? "Egg Group: {0} / {1}"
                : "Egg Group: {0}", CB_EggGroup1.Text, CB_EggGroup2.Text));
            lines.Add($"Hatch Cycles: {TB_HatchCycles.Text}");
            lines.Add($"Height: {TB_Height.Text} m, Weight: {TB_Weight.Text} kg, Color: {CB_Color.Text}");

            if (CB_ZBaseMove.SelectedIndex > 0)
                lines.Add($"{CB_ZBaseMove.Text} + {CB_ZItem.Text} => {CB_ZMove.Text}");
            lines.Add("");
        }
        string path = sfd.FileName;
        File.WriteAllLines(path, lines, Encoding.Unicode);
        dumping = false;
    }
    private void B_Copy_Click(object sender, EventArgs e)
    {
        ClipboardTMs = new bool[CLB_TM.Items.Count];
        for (int i = 0; i < CLB_TM.Items.Count; i++)
            ClipboardTMs[i] = CLB_TM.GetItemChecked(i);

        ClipboardTutors = new bool[CLB_MoveTutors.Items.Count];
        for (int i = 0; i < CLB_MoveTutors.Items.Count; i++)
            ClipboardTutors[i] = CLB_MoveTutors.GetItemChecked(i);

        if (CLB_BeachTutors.Visible)
        {
            ClipboardBeachTutors = new bool[CLB_BeachTutors.Items.Count];
            for (int i = 0; i < CLB_BeachTutors.Items.Count; i++)
                ClipboardBeachTutors[i] = CLB_BeachTutors.GetItemChecked(i);
        }
    }

    private void B_Paste_Click(object sender, EventArgs e)
    {
        if (ClipboardTMs != null)
        {
            for (int i = 0; i < CLB_TM.Items.Count && i < ClipboardTMs.Length; i++)
                CLB_TM.SetItemChecked(i, ClipboardTMs[i]);
        }

        if (ClipboardTutors != null)
        {
            for (int i = 0; i < CLB_MoveTutors.Items.Count && i < ClipboardTutors.Length; i++)
                CLB_MoveTutors.SetItemChecked(i, ClipboardTutors[i]);
        }

        if (ClipboardBeachTutors != null && CLB_BeachTutors.Visible)
        {
            for (int i = 0; i < CLB_BeachTutors.Items.Count && i < ClipboardBeachTutors.Length; i++)
                CLB_BeachTutors.SetItemChecked(i, ClipboardBeachTutors[i]);
        }
    }

    private void CHK_Stats_CheckedChanged(object sender, EventArgs e)
    {
        L_StatDev.Enabled = NUD_StatDev.Enabled = CHK_Stats.Checked;
        CHK_rHP.Enabled = CHK_rATK.Enabled = CHK_rDEF.Enabled = CHK_rSPA.Enabled = CHK_rSPD.Enabled = CHK_rSPE.Enabled = CHK_Stats.Checked;
    }

    private void CHK_Ability_CheckedChanged(object sender, EventArgs e)
    {
        CHK_WGuard.Enabled = CHK_Ability.Checked;
        if (!CHK_WGuard.Enabled)
            CHK_WGuard.Checked = false;
    }
    private void UpdateDynamicDiff(object sender, EventArgs e)
    {
        if (pkm == null || vanillaStats == null || entry < 0 || entry >= vanillaStats.Length) return;
        
        int.TryParse(TB_BaseHP.Text, out int hp);
        int.TryParse(TB_BaseATK.Text, out int atk);
        int.TryParse(TB_BaseDEF.Text, out int def);
        int.TryParse(TB_BaseSPA.Text, out int spa);
        int.TryParse(TB_BaseSPD.Text, out int spd);
        int.TryParse(TB_BaseSPE.Text, out int spe);

        bool isAlt = entry > Main.Config.MaxSpeciesID;
        int[] origValues = new int[6];
        string prefix = isAlt ? "Base Form" : "Vanilla";

        if (isAlt)
        {
            int baseID = baseForms[entry];
            var bPkm = Main.SpeciesStat[baseID];
            origValues[0] = bPkm.HP;
            origValues[1] = bPkm.ATK;
            origValues[2] = bPkm.DEF;
            origValues[3] = bPkm.SPA;
            origValues[4] = bPkm.SPD;
            origValues[5] = bPkm.SPE;
        }
        else
        {
            if (entry >= vanillaStats.Length || vanillaStats[entry] == null) return;
            origValues = vanillaStats[entry];
        }

        SetDiffLabel(L_DiffHP, hp, origValues[0]);
        SetDiffLabel(L_DiffATK, atk, origValues[1]);
        SetDiffLabel(L_DiffDEF, def, origValues[2]);
        SetDiffLabel(L_DiffSPA, spa, origValues[3]); 
        SetDiffLabel(L_DiffSPD, spd, origValues[4]); 
        SetDiffLabel(L_DiffSPE, spe, origValues[5]); 

        int origBST = origValues.Sum();
        int curBST = hp + atk + def + spa + spd + spe;
        int diff = curBST - origBST;

        if (diff > 0)
        {
            L_StatDiff.Text = $"{prefix} BST Diff: {curBST} ({diff} more than {origBST})";
            L_StatDiff.ForeColor = Color.Green;
        }
        else if (diff < 0)
        {
            L_StatDiff.Text = $"{prefix} BST Diff: {curBST} ({Math.Abs(diff)} less than {origBST})";
            L_StatDiff.ForeColor = Color.Red;
        }
        else
        {
            L_StatDiff.Text = $"{prefix} BST Diff: {curBST} (0 more than {origBST})";
            L_StatDiff.ForeColor = Color.Gray; 
        }
    }

    // Missing method restored
    private void SetDiffLabel(Label l, int current, int original)
    {
        int diff = current - original;
        if (diff > 0) { l.Text = $"+{diff}"; l.ForeColor = Color.Green; }
        else if (diff < 0) { l.Text = $"{diff}"; l.ForeColor = Color.Red; }
        else { l.Text = "0"; l.ForeColor = Color.Gray; } 
    }

    private void B_CopyPage1_Click(object sender, EventArgs e)
    {
        string[] page1Data = {
            TB_BaseHP.Text, TB_BaseATK.Text, TB_BaseDEF.Text, TB_BaseSPA.Text, TB_BaseSPD.Text, TB_BaseSPE.Text,
            CB_Type1.SelectedIndex.ToString(), CB_Type2.SelectedIndex.ToString(),
            TB_CatchRate.Text, TB_HatchCycles.Text, TB_Friendship.Text, TB_Gender.Text,
            CB_Ability1.SelectedIndex.ToString(), CB_Ability2.SelectedIndex.ToString(), CB_Ability3.SelectedIndex.ToString(),
            CB_EggGroup1.SelectedIndex.ToString(), CB_EggGroup2.SelectedIndex.ToString(),
            TB_Height.Text, TB_Weight.Text, CB_Color.SelectedIndex.ToString(),
            TB_BaseExp.Text, CB_EXPGroup.SelectedIndex.ToString(), TB_CallRate.Text,
            TB_HPEVs.Text, TB_ATKEVs.Text, TB_DEFEVs.Text, TB_SPAEVs.Text, TB_SPDEVs.Text, TB_SPEEVs.Text
        };
        Clipboard.SetText(string.Join(",", page1Data));
        System.Media.SystemSounds.Asterisk.Play();
    }

    private void B_PastePage1_Click(object sender, EventArgs e)
    {
        if (!Clipboard.ContainsText()) return;
        string[] p = Clipboard.GetText().Split(',');
        
        // Increased from 23 to 29 to account for the 6 new EV values
        if (p.Length < 29) { WinFormsUtil.Error("Invalid clipboard data. Ensure you copied a full set."); return; }

        TB_BaseHP.Text = p[0]; TB_BaseATK.Text = p[1]; TB_BaseDEF.Text = p[2]; 
        TB_BaseSPA.Text = p[3]; TB_BaseSPD.Text = p[4]; TB_BaseSPE.Text = p[5];
        CB_Type1.SelectedIndex = int.Parse(p[6]); CB_Type2.SelectedIndex = int.Parse(p[7]);
        TB_CatchRate.Text = p[8]; TB_HatchCycles.Text = p[9]; TB_Friendship.Text = p[10]; TB_Gender.Text = p[11];
        CB_Ability1.SelectedIndex = int.Parse(p[12]); CB_Ability2.SelectedIndex = int.Parse(p[13]); CB_Ability3.SelectedIndex = int.Parse(p[14]);
        CB_EggGroup1.SelectedIndex = int.Parse(p[15]); CB_EggGroup2.SelectedIndex = int.Parse(p[16]);
        
        TB_Height.Text = p[17]; 
        TB_Weight.Text = p[18]; 
        if (int.TryParse(p[19], out int colIdx) && colIdx > -1 && colIdx < CB_Color.Items.Count) CB_Color.SelectedIndex = colIdx;
        TB_BaseExp.Text = p[20];
        if (int.TryParse(p[21], out int expIdx) && expIdx > -1 && expIdx < CB_EXPGroup.Items.Count) CB_EXPGroup.SelectedIndex = expIdx;
        TB_CallRate.Text = p[22];

        // Apply EV Yields
        TB_HPEVs.Text = p[23]; 
        TB_ATKEVs.Text = p[24]; 
        TB_DEFEVs.Text = p[25]; 
        TB_SPAEVs.Text = p[26]; 
        TB_SPDEVs.Text = p[27]; 
        TB_SPEEVs.Text = p[28];

        SaveEntry();
        System.Media.SystemSounds.Asterisk.Play();
    }

    private void B_MaxCatch_Click(object sender, EventArgs e) { TB_CatchRate.Text = "255"; SaveEntry(); }
    private void B_ZeroHatch_Click(object sender, EventArgs e) { TB_HatchCycles.Text = "0"; SaveEntry(); }

    private void B_MaxCatchAll_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Apply Max Catch Rate (255) to ALL Pokémon?") != DialogResult.Yes) return;
        for (int i = 1; i < files.Length; i++) {
            Main.SpeciesStat[i].CatchRate = 255;
            files[i] = Main.SpeciesStat[i].Write();
        }
        TB_CatchRate.Text = "255";
        SaveEntry();
        WinFormsUtil.Alert("All Pokémon Catch Rates set to 255.");
    }

    private void B_ZeroHatchAll_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Apply 0 Hatch Cycles to ALL Pokémon?") != DialogResult.Yes) return;
        for (int i = 1; i < files.Length; i++) {
            Main.SpeciesStat[i].HatchCycles = 0;
            files[i] = Main.SpeciesStat[i].Write();
        }
        TB_HatchCycles.Text = "0";
        SaveEntry();
        WinFormsUtil.Alert("All Pokémon Hatch Cycles set to 0.");
    }

    private void B_JumpLevelUp_Click(object sender, EventArgs e)
    {
        if (learnsets == null) return;
        SaveEntry();
        var editor = new LevelUpEditor7(learnsets) { StartSpecies = entry };
        editor.ShowDialog();
        ReadEntry(); // Refresh in case anything changed (though unlikely for stats)
    }
    private void B_JumpEggMoves_Click(object sender, EventArgs e)
    {
        if (eggmoves == null) return;
        SaveEntry();
        var editor = new EggMoveEditor7(eggmoves) { StartSpecies = entry };
        editor.ShowDialog();
        ReadEntry();
    }
    private void LoadVanillaStats() 
    {
        if (files == null || files.Length == 0) return; // Safety check

        vanillaStats = new int[files.Length][];
        string path = Path.Combine(Application.StartupPath, "vanilla_stats.txt");

        if (File.Exists(path))
        {
            // Read from the permanent backup
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i >= files.Length) break;
                string[] parts = lines[i].Split(',');
                if (parts.Length == 6)
                {
                    vanillaStats[i] = new int[6];
                    for (int j = 0; j < 6; j++)
                        int.TryParse(parts[j], out vanillaStats[i][j]);
                }
            }
        }
        else
        {
            try
            {
                // First run: Create the permanent backup file using ALL entries in files
                List<string> lines = new List<string>();
                for (int i = 0; i < files.Length; i++)
                {
                    vanillaStats[i] = new int[6];
                    
                    if (i > 0 && files[i] != null && files[i].Length >= 6)
                    {
                        vanillaStats[i][0] = files[i][0]; // HP
                        vanillaStats[i][1] = files[i][1]; // ATK
                        vanillaStats[i][2] = files[i][2]; // DEF
                        vanillaStats[i][3] = files[i][4]; // SPA 
                        vanillaStats[i][4] = files[i][5]; // SPD 
                        vanillaStats[i][5] = files[i][3]; // SPE 
                    }
                    
                    lines.Add($"{vanillaStats[i][0]},{vanillaStats[i][1]},{vanillaStats[i][2]},{vanillaStats[i][3]},{vanillaStats[i][4]},{vanillaStats[i][5]}");
                }
                
                File.WriteAllLines(path, lines);
                WinFormsUtil.Alert("Created 'vanilla_stats.txt' successfully!", 
                                   $"It is located at:\n{path}");
            }
            catch (Exception ex)
            {
                WinFormsUtil.Alert("Failed to create vanilla_stats.txt.", "Error details:", ex.Message);
            }
        }
    }
    private void Form_Closing(object sender, FormClosingEventArgs e)
    {
        if (entry > -1) SaveEntry();
        RandSettings.SetFormSettings(this, TP_Randomizer.Controls);
    }
}