using pk3DS.Core;
using System;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core.Structures;

namespace pk3DS.WinForms;

public partial class MoveEditor7 : Form
{
    private byte[][] files;
    private string[] types = Main.Config.GetText(TextName.Types);
    private string[] moveflavor = Main.Config.GetText(TextName.MoveFlavor);
    private string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private byte[][] _originalInfiles;
    private static byte[][] _sessionMoveData;

    public MoveEditor7(byte[][] infiles)
    {
        _originalInfiles = infiles;

        // 1. Load from session cache if it exists, otherwise copy from main memory
        if (_sessionMoveData != null && _sessionMoveData.Length >= infiles.Length)
        {
            files = new byte[_sessionMoveData.Length][];
            for (int i = 0; i < _sessionMoveData.Length; i++)
                files[i] = (byte[])_sessionMoveData[i].Clone();
        }
        else
        {
            files = new byte[infiles.Length][];
            for (int i = 0; i < infiles.Length; i++)
                files[i] = (byte[])infiles[i].Clone();
        }

        // 2. Auto-Sync Routine: Expands the Move GARC to match Game Text
        if (files.Length < movelist.Length)
        {
            int oldLength = files.Length;
            Array.Resize(ref files, movelist.Length);
            for (int i = oldLength; i < files.Length; i++)
            {
                files[i] = (byte[])(oldLength > 1 && files[1] != null ? files[1].Clone() : new byte[0x24]);
            }
            WinFormsUtil.Alert($"Auto-Synced: Added {movelist.Length - oldLength} new move slots to a/0/1/1 to match the Game Text Editor.");
        }

        // 3. ARM Engine Sync and Metronome Padding
        int maxMoveId = files.Length - 1;
        if (maxMoveId % 4 != 0) maxMoveId = (maxMoveId + 3) & ~3;
        int requiredLength = maxMoveId + 1;

        if (files.Length < requiredLength)
        {
            int oldLength = files.Length;
            Array.Resize(ref files, requiredLength);
            for (int i = oldLength; i < files.Length; i++)
            {
                files[i] = (byte[])(oldLength > 1 && files[1] != null ? files[1].Clone() : new byte[0x24]);
            }
        }
        
        if (movelist.Length < requiredLength)
        {
            int oldTextLength = movelist.Length;
            Array.Resize(ref movelist, requiredLength);
            for (int i = oldTextLength; i < requiredLength; i++)
            {
                movelist[i] = $"Dummy Pad Move {i}";
            }
        }

        PatchEngineMoveLimits(maxMoveId);

        // 4. Update the session cache with the fully expanded array
        _sessionMoveData = files;
        movelist[0] = "";

        InitializeComponent();
        Setup();
        RandSettings.GetFormSettings(this, groupBox1.Controls);
    }

    private readonly string[] MoveCategories = ["Status", "Physical", "Special"];
    private readonly string[] StatCategories = ["None", "Attack", "Defense", "Special Attack", "Special Defense", "Speed", "Accuracy", "Evasion", "All"];

    private static readonly string[] TargetingTypes =
    [
        "Single Adjacent Ally/Foe",
        "Any Ally", "Any Adjacent Ally", "Single Adjacent Foe", "Everyone but User", "All Foes",
        "All Allies", "Self", "All Pokémon on Field", "Single Adjacent Foe (2)", "Entire Field",
        "Opponent's Field", "User's Field", "Self",
    ];

    private static readonly string[] InflictionTypes =
    [
        "None",
        "Paralyze", "Sleep", "Freeze", "Burn", "Poison",
        "Confusion", "Attract", "Capture", "Nightmare", "Curse",
        "Taunt", "Torment", "Disable", "Yawn", "Heal Block",
        "?", "Detect", "Leech Seed", "Embargo", "Perish Song",
        "Ingrain", "??? 0x16", "??? 0x17", "Mute",
    ];

    private static readonly string[] MoveQualities =
    [
        "Only DMG",
        "No DMG -> Inflict Status", "No DMG -> -Target/+User Stat", "No DMG | Heal User", "DMG | Inflict Status", "No DMG | STATUS | +Target Stat",
        "DMG | -Target Stat", "DMG | +User Stat", "DMG | Absorbs DMG", "One-Hit KO", "Affects Whole Field",
        "Affect One Side of the Field", "Forces Target to Switch", "Unique Effect",
    ];

    private static readonly string[] ZMoveEffects =
    [
        "None",
        "+1 Attack",
        "+2 Attack",
        "+3 Attack",
        "+1 Defense",
        "+2 Defense",
        "+3 Defense",
        "+1 Special Attack",
        "+2 Special Attack",
        "+3 Special Attack",
        "+1 Special Defense",
        "+2 Special Defense",
        "+3 Special Defense",
        "+1 Speed",
        "+2 Speed",
        "+3 Speed",
        "+1 Accuracy",
        "+2 Accuracy",
        "+3 Accuracy",
        "+1 Evasiveness",
        "+2 Evasiveness",
        "+3 Evasiveness",
        "+1 to all (except Accuracy or Evasiveness)",
        "+2 to all (except Accuracy or Evasiveness)",
        "+3 to all (except Accuracy or Evasiveness)",
        "raises critical-hit ratio two stages",
        "resets lowered stats of the user",
        "recovers all of user's HP",
        "recovers all Hp of the Pokémon switching-in (Memento and Parting Shot)",
        "makes the user the center of attention",
        "only on Curse: recovers all HP if the user's a Ghost type, +1 Attack otherwise",
    ];

    private void Setup()
    {
        char[] ps = ['P', 'S']; // Distinguish Physical/Special Z-Moves
        for (int i = 622; i < 658; i++)
            movelist[i] += $" ({ps[i % 2]})";
        CB_Move.Items.AddRange(movelist);
        CB_Type.Items.AddRange(types);
        CB_Category.Items.AddRange(MoveCategories);
        CB_Stat1.Items.AddRange(StatCategories);
        CB_Stat2.Items.AddRange(StatCategories);
        CB_Stat3.Items.AddRange(StatCategories);
        CB_Targeting.Items.AddRange(TargetingTypes);
        CB_Quality.Items.AddRange(MoveQualities);
        CB_Inflict.Items.AddRange(InflictionTypes);
        CB_ZMove.Items.AddRange(movelist);
        var flagnames = Enum.GetNames(typeof(MoveFlag7)).Skip(1).ToArray();
        CLB_Flags.Items.AddRange(flagnames);
        CB_ZEffect.Items.AddRange(ZMoveEffects);
        CB_Inflict.Items.Add("Special");
        var refreshtypes = Enum.GetNames(typeof(RefreshType));
        CB_AfflictRefresh.Items.AddRange(refreshtypes);

        CB_Move.Items.RemoveAt(0);
        CB_Move.SelectedIndex = 0;
    }

    private int entry = -1;

    private void ChangeEntry(object sender, EventArgs e)
    {
        SetEntry();
        if (CB_Move.SelectedIndex < 0) return;
        
        entry = CB_Move.SelectedIndex + 1; 
        GetEntry();
    }

    private void GetEntry()
    {
        if (entry < 1 || entry >= files.Length) return;
        
        byte[] data = files[entry];
        
        if (data == null || data.Length < 0x24)
        {
            data = new byte[0x24];
            files[entry] = data;
        }
        
        {
            if (moveflavor != null && entry < moveflavor.Length && moveflavor[entry] != null)
                RTB.Text = moveflavor[entry].Replace("\\n", Environment.NewLine);
            else
                RTB.Text = "";

            CB_Type.SelectedIndex = data[0x00];
            CB_Quality.SelectedIndex = data[0x01];
            CB_Category.SelectedIndex = data[0x02];
            NUD_Power.Value = data[0x3];
            NUD_Accuracy.Value = data[0x4];
            NUD_PP.Value = data[0x05];
            NUD_Priority.Value = (sbyte)data[0x06];
            NUD_HitMin.Value = data[0x7] & 0xF;
            NUD_HitMax.Value = data[0x7] >> 4;
            short inflictVal = BitConverter.ToInt16(data, 0x08);
            CB_Inflict.SelectedIndex = inflictVal < 0 ? CB_Inflict.Items.Count - 1 : inflictVal;
            NUD_Inflict.Value = data[0xA];
            NUD_0xB.Value = data[0xB]; 
            NUD_TurnMin.Value = data[0xC];
            NUD_TurnMax.Value = data[0xD];
            NUD_CritStage.Value = data[0xE];
            NUD_Flinch.Value = data[0xF];
            NUD_Effect.Value = BitConverter.ToUInt16(data, 0x10);
            NUD_Recoil.Value = (sbyte)data[0x12];
            NUD_Heal.Value = data[0x13];

            CB_Targeting.SelectedIndex = data[0x14];
            CB_Stat1.SelectedIndex = data[0x15];
            CB_Stat2.SelectedIndex = data[0x16];
            CB_Stat3.SelectedIndex = data[0x17];
            NUD_Stat1.Value = (sbyte)data[0x18];
            NUD_Stat2.Value = (sbyte)data[0x19];
            NUD_Stat3.Value = (sbyte)data[0x1A];
            NUD_StatP1.Value = data[0x1B];
            NUD_StatP2.Value = data[0x1C];
            NUD_StatP3.Value = data[0x1D];

            var move = new Move7(data);
            CB_ZMove.SelectedIndex = move.ZMove;
            NUD_ZPower.Value = move.ZPower;
            CB_ZEffect.SelectedIndex = move.ZEffect;
            CB_AfflictRefresh.SelectedIndex = (int)move.RefreshAfflictType;
            NUD_RefreshAfflictPercent.Value = move.RefreshAfflictPercent;

            var flags = (uint)move.Flags;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                CLB_Flags.SetItemChecked(i, ((flags >> i) & 1) == 1);
        }
    }

    private void SetEntry()
    {
        if (entry < 1 || entry >= files.Length) return;
        
        byte[] data = files[entry];
        
        if (data == null || data.Length < 0x24)
        {
            data = new byte[0x24];
        }
        
        {
            data[0x00] = (byte)CB_Type.SelectedIndex;
            data[0x01] = (byte)CB_Quality.SelectedIndex;
            data[0x02] = (byte)CB_Category.SelectedIndex;
            data[0x03] = (byte)NUD_Power.Value;
            data[0x04] = (byte)NUD_Accuracy.Value;
            data[0x05] = (byte)NUD_PP.Value;
            data[0x06] = (byte)(int)NUD_Priority.Value;
            data[0x07] = (byte)((byte)NUD_HitMin.Value | ((byte)NUD_HitMax.Value << 4));
            int inflictval = CB_Inflict.SelectedIndex; if (inflictval == CB_Inflict.Items.Count) inflictval = -1;
            Array.Copy(BitConverter.GetBytes((short)inflictval), 0, data, 0x08, 2);
            data[0x0A] = (byte)NUD_Inflict.Value;
            data[0x0B] = (byte)NUD_0xB.Value;
            data[0x0C] = (byte)NUD_TurnMin.Value;
            data[0x0D] = (byte)NUD_TurnMax.Value;
            data[0x0E] = (byte)NUD_CritStage.Value;
            data[0x0F] = (byte)NUD_Flinch.Value;
            Array.Copy(BitConverter.GetBytes((ushort)NUD_Effect.Value), 0, data, 0x10, 2);
            data[0x12] = (byte)(int)NUD_Recoil.Value;
            data[0x13] = (byte)NUD_Heal.Value;
            data[0x14] = (byte)CB_Targeting.SelectedIndex;
            data[0x15] = (byte)CB_Stat1.SelectedIndex;
            data[0x16] = (byte)CB_Stat2.SelectedIndex;
            data[0x17] = (byte)CB_Stat3.SelectedIndex;
            data[0x18] = (byte)(int)NUD_Stat1.Value;
            data[0x19] = (byte)(int)NUD_Stat2.Value;
            data[0x1A] = (byte)(int)NUD_Stat3.Value;
            data[0x1B] = (byte)NUD_StatP1.Value;
            data[0x1C] = (byte)NUD_StatP2.Value;
            data[0x1D] = (byte)NUD_StatP3.Value;

            var move = new Move7(data)
            {
                ZMove = CB_ZMove.SelectedIndex,
                ZPower = (int)NUD_ZPower.Value,
                ZEffect = CB_ZEffect.SelectedIndex,
                RefreshAfflictPercent = (int)NUD_RefreshAfflictPercent.Value,
                RefreshAfflictType = (RefreshType)CB_AfflictRefresh.SelectedIndex,
            };

            uint flagval = 0;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                flagval |= CLB_Flags.GetItemChecked(i) ? 1u << i : 0;
            move.Flags = (MoveFlag7)flagval;
        }
        files[entry] = data;
    }

    private void CloseForm(object sender, FormClosingEventArgs e)
    {
        try { SetEntry(); } catch { }
        RandSettings.SetFormSettings(this, groupBox1.Controls);

        if (files != null)
        {
            _sessionMoveData = new byte[files.Length][];
            for (int i = 0; i < files.Length; i++)
            {
                _sessionMoveData[i] = (byte[])files[i].Clone();
            }
        }

        if (_originalInfiles != null)
        {
            for (int i = 0; i < _originalInfiles.Length; i++)
            {
                if (i < files.Length)
                {
                    _originalInfiles[i] = files[i];
                }
            }
        }
    }

    private void PatchEngineMoveLimits(int maxMoveId)
    {
        string binName = System.IO.File.Exists(System.IO.Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
        string codePath = System.IO.Path.Combine(Main.ExeFSPath, binName);
        string battlePath = System.IO.Path.Combine(Main.RomFSPath, "battle", "battle.cro");

        if (!System.IO.File.Exists(codePath) || !System.IO.File.Exists(battlePath)) return;

        byte[] codePatchR0 = GetCmpInstruction(0, maxMoveId);
        byte[] codePatchR1 = GetCmpInstruction(1, maxMoveId);
        byte[] codePatchR5 = GetCmpInstruction(5, maxMoveId);
        byte[] battlePatchR7 = GetCmpInstruction(7, maxMoveId);
        byte[] battlePatchR5 = GetCmpInstruction(5, maxMoveId);
        byte[] battlePatchR2 = GetCmpInstruction(2, maxMoveId);

        if (codePatchR0 == null || codePatchR1 == null || codePatchR5 == null || 
            battlePatchR7 == null || battlePatchR5 == null || battlePatchR2 == null)
        {
            WinFormsUtil.Error($"Failed to generate ARM patch for Move ID limit: {maxMoveId}. It is not a valid 8-bit shifted immediate.");
            return;
        }

        byte[] codeBin = System.IO.File.ReadAllBytes(codePath);
        byte[] battleCro = System.IO.File.ReadAllBytes(battlePath);

        bool needsPatch = false;

        needsPatch |= ApplyARMPatch(codeBin, 0x226680, codePatchR0);
        needsPatch |= ApplyARMPatch(codeBin, 0x2267D4, codePatchR0);
        needsPatch |= ApplyARMPatch(codeBin, 0x226B04, codePatchR1);
        needsPatch |= ApplyARMPatch(codeBin, 0x2D2C28, codePatchR5);

        needsPatch |= ApplyARMPatch(battleCro, 0x092D70, battlePatchR7);
        needsPatch |= ApplyARMPatch(battleCro, 0x093644, battlePatchR5);
        needsPatch |= ApplyARMPatch(battleCro, 0x0B0884, battlePatchR2);

        if (needsPatch)
        {
            System.IO.File.WriteAllBytes(codePath, codeBin);
            System.IO.File.WriteAllBytes(battlePath, battleCro);
            WinFormsUtil.Alert($"Engine limits automatically synced!",
                               $"Maximum Move ID in battle.cro and code.bin patched to {maxMoveId}.",
                               "Your new moves are now fully functional in battle without defaulting to null values.");
        }
    }

    private bool ApplyARMPatch(byte[] fileData, int offset, byte[] patch)
    {
        if (offset < 0 || offset + 3 >= fileData.Length) return false;

        if (fileData[offset] == patch[0] && fileData[offset + 1] == patch[1] && 
            fileData[offset + 2] == patch[2] && fileData[offset + 3] == patch[3]) return false;

        fileData[offset] = patch[0];
        fileData[offset + 1] = patch[1];
        fileData[offset + 2] = patch[2];
        fileData[offset + 3] = patch[3];
        return true;
    }

    private byte[] GetCmpInstruction(int reg, int val)
    {
        uint uval = (uint)val;
        for (int shift = 0; shift < 32; shift += 2)
        {
            uint val_rot = ((uval << shift) & 0xFFFFFFFF) | (uval >> (32 - shift));
            if (val_rot <= 0xFF)
            {
                return new byte[] { (byte)val_rot, (byte)(shift / 2), (byte)(0x50 + reg), 0xE3 };
            }
        }
        return null;
    }

    private void B_Table_Click(object sender, EventArgs e)
    {
        var items = files.Select(z => new Move7(z));
        Clipboard.SetText(TableUtil.GetTable(items, movelist));
        System.Media.SystemSounds.Asterisk.Play();
    }

    private void B_AddMove_Click(object sender, EventArgs e)
    {
        SetEntry();
        
        int newId = files.Length;
        
        Array.Resize(ref files, newId + 1);
        files[newId] = (byte[])files[1].Clone(); 
        
        Array.Resize(ref movelist, newId + 1);
        movelist[newId] = $"New Move {newId}";
        
        if (moveflavor != null)
        {
            Array.Resize(ref moveflavor, newId + 1);
            moveflavor[newId] = "Update this description in the Game Text Editor.";
        }
        
        CB_Move.Items.Add(movelist[newId]);
        CB_Move.SelectedIndex = CB_Move.Items.Count - 1;
        
        WinFormsUtil.Alert("Move slot added to Move GARC.", 
                           "You MUST open the Game Text Editor and add a matching line for Move Names and Move Descriptions for this to display and save properly in-game.");
    }

    private void B_RandAll_Click(object sender, EventArgs e)
    {
        if (!CHK_Category.Checked && !CHK_Type.Checked)
        {
            WinFormsUtil.Alert("Cannot randomize Moves.", "Please check any of the options on the right to randomize Moves.");
            return;
        }

        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize Moves? Cannot undo.", "Double check options on the right before continuing.") != DialogResult.Yes) return;
        Random rnd = Util.Rand;
        for (int i = 0; i < CB_Move.Items.Count; i++)
        {
            CB_Move.SelectedIndex = i;
            if (i is 165 or 174) continue;

            if (CB_Category.SelectedIndex > 0 && CHK_Category.Checked)
                CB_Category.SelectedIndex = rnd.Next(1, 3);

            if (CHK_Type.Checked)
                CB_Type.SelectedIndex = rnd.Next(0, 18);
        }
        WinFormsUtil.Alert("All Moves have been randomized!");
    }

    private void B_Metronome_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Play using Metronome Mode?", "This will set the Base PP for every other Move to 0!") != DialogResult.Yes) return;

        for (int i = 0; i < CB_Move.Items.Count; i++)
        {
            CB_Move.SelectedIndex = i;
            if (CB_Move.SelectedIndex != 117 && CB_Move.SelectedIndex != 32)
                NUD_PP.Value = 0;
            if (CB_Move.SelectedIndex == 117)
                NUD_PP.Value = 40;
            if (CB_Move.SelectedIndex == 32)
                NUD_PP.Value = 1;
        }
        CB_Move.SelectedIndex = 0;
        WinFormsUtil.Alert("All Moves have had their Base PP values modified!");
    }

    private void B_SyncBSEQ_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog fbd = new FolderBrowserDialog())
        {
            fbd.Description = "Select your unpacked Battle Sequence (bseq) folder to automatically expand it.";
            if (fbd.ShowDialog() != DialogResult.OK) return;

            string folderPath = fbd.SelectedPath;
            string[] folderFiles = System.IO.Directory.GetFiles(folderPath).OrderBy(f => f).ToArray();

            if (folderFiles.Length == 0)
            {
                WinFormsUtil.Error("The selected folder is empty.");
                return;
            }

            int targetCount = movelist.Length;
            if (folderFiles.Length >= targetCount)
            {
                WinFormsUtil.Alert("Your bseq folder already has enough files to support your expanded moves!", 
                                   $"Files found: {folderFiles.Length}");
                return;
            }

            string templatePath = folderFiles.Length > 1 ? folderFiles[1] : folderFiles[0];
            byte[] templateData = System.IO.File.ReadAllBytes(templatePath);

            int added = 0;
            string extension = System.IO.Path.GetExtension(folderFiles[0]);

            for (int i = folderFiles.Length; i < targetCount; i++)
            {
                string newFileName = i.ToString("D3") + extension; 
                string newFilePath = System.IO.Path.Combine(folderPath, newFileName);

                System.IO.File.WriteAllBytes(newFilePath, templateData);
                added++;
            }

            WinFormsUtil.Alert($"Successfully generated {added} new animation files.", 
                               $"The folder now contains exactly {targetCount} files, syncing perfectly with your Move Editor.",
                               "Repack this folder with the pk3DS GARC Tool and insert it into your ROM.");
        }
    }

    private void B_SaveExport_Click(object sender, EventArgs e)
    {
        try { SetEntry(); } catch { } 

        using (FolderBrowserDialog fbd = new FolderBrowserDialog())
        {
            fbd.Description = "Select a folder to export the expanded Move Data (.bin files) for repacking.";
            if (fbd.ShowDialog() != DialogResult.OK) return;

            int count = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(fbd.SelectedPath, i.ToString("D3") + ".bin"), files[i]);
                    count++;
                }
            }
            WinFormsUtil.Alert($"Successfully exported {count} move files!",
                               "If you expanded the move list, pk3DS cannot save the memory array directly.",
                               "You MUST repack this folder using the pk3DS GARC Tool and replace your a/0/1/1 GARC.");
        }
    }
}