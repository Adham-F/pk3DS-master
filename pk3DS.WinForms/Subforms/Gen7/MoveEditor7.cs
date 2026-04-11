using pk3DS.Core;
using pk3DS.Core.CTR;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using pk3DS.Core.Structures;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace pk3DS.WinForms;

public partial class MoveEditor7 : Form
{
    private byte[][] files;
    public byte[][] Files => files;
    private string[] types = Main.Config.GetText(TextName.Types);
    private string[] moveflavor = Main.Config.GetText(TextName.MoveFlavor);
    private string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private byte[][] _originalInfiles;
    private static byte[][] _sessionMoveData;
    private Dictionary<int, int> AnimationMap = new Dictionary<int, int>();
    private Dictionary<string, int> AnimHashes = new Dictionary<string, int>();
    private string AnimationMapPath => Path.Combine(Path.GetDirectoryName(Main.RomFSPath) ?? "", "move_anims.json");
    private byte[][] animFiles;

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
                files[i] = (byte[])(oldLength > 1 && files[1] != null ? files[1].Clone() : new byte[0x28]);
            }
            if (oldLength < movelist.Length)
                WinFormsUtil.Alert($"Auto-Synced: Added {movelist.Length - oldLength} new move slots to match the Game Text Editor.");
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
                files[i] = (byte[])(oldLength > 1 && files[1] != null ? files[1].Clone() : new byte[0x28]);
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

        // 4. Load Animation Map & Hashes
        LoadAnimationMap();
        LoadAnimationHashes();

        InitializeComponent();
        
        // 5. Update the session cache with the fully expanded array
        _sessionMoveData = files;
        movelist[0] = "";
        
        Setup();
        RandSettings.GetFormSettings(this, groupBox1.Controls);

        _originalMoves = files.Select(f => (byte[])f.Clone()).ToArray();
        LoadLogs();
    }

    private void LoadLogs()
    {
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Main.RomFSPath) ?? "", "move_changelog.json");
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            var logs = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(json);
            if (logs != null) _changeLogs = logs;
            UpdateLogView();
        }
        catch { }
    }

    private void SaveLogs()
    {
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Main.RomFSPath) ?? "", "move_changelog.json");
            var json = JsonSerializer.Serialize(_changeLogs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { }
    }

    private void B_VanillaLog_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Title = "Select Vanilla Move GARC (a/0/1/1)";
            ofd.Filter = "All Files|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                byte[] vanillaData = System.IO.File.ReadAllBytes(ofd.FileName);
                byte[][] vanillaMoves = null;

                // Check if it is a GARC file. 3DS files use little-endian, so "GARC" is stored as "CRAG".
                bool isGarc = vanillaData.Length > 4 && 
                             ((vanillaData[0] == 'G' && vanillaData[1] == 'A' && vanillaData[2] == 'R' && vanillaData[3] == 'C') ||
                              (vanillaData[0] == 'C' && vanillaData[1] == 'R' && vanillaData[2] == 'A' && vanillaData[3] == 'G'));

                if (isGarc)
                {
                    var garc = new GARCFile(new GARC.MemGARC(vanillaData), null, ofd.FileName);
                    if (garc.Files.Length > 0)
                    {
                        vanillaMoves = Mini.UnpackMini(garc.Files[0], "WD");
                    }
                }
                else
                {
                    vanillaMoves = Mini.UnpackMini(vanillaData, "WD");
                }

                if (vanillaMoves == null || vanillaMoves.Length == 0)
                {
                    WinFormsUtil.Error("Could not unpack vanilla move data. Ensure you selected a valid move GARC.");
                    return;
                }

                // 1. Set the Vanilla moves as the background reference baseline
                _originalInfiles = vanillaMoves;

                // 2. Clear current session logs to prevent duplication
                _changeLogs.Clear();
                
                // 3. Scan for existing differences and add them to the log
                for (int i = 1; i < Math.Min(files.Length, vanillaMoves.Length); i++)
                {
                    var oldMove = new Move7(vanillaMoves[i]);
                    var newMove = new Move7(files[i]);

                    if (oldMove.Power != newMove.Power) AddBatchLog(i, "Power", oldMove.Power, newMove.Power);
                    if (oldMove.Type != newMove.Type) AddBatchLog(i, "Type", types[oldMove.Type], types[newMove.Type]);
                    if (oldMove.Accuracy != newMove.Accuracy) AddBatchLog(i, "Accuracy", oldMove.Accuracy, newMove.Accuracy);
                    if (oldMove.PP != newMove.PP) AddBatchLog(i, "PP", oldMove.PP, newMove.PP);
                    
                    string[] cats = { "Status", "Physical", "Special" };
                    if (oldMove.Category != newMove.Category) AddBatchLog(i, "Category", cats[oldMove.Category], cats[newMove.Category]);
                    if (oldMove.Priority != newMove.Priority) AddBatchLog(i, "Priority", oldMove.Priority, newMove.Priority);
                    if (oldMove.CritStage != newMove.CritStage) AddBatchLog(i, "Crit Stage", oldMove.CritStage, newMove.CritStage);
                    if (oldMove.Flinch != newMove.Flinch) AddBatchLog(i, "Flinch %", oldMove.Flinch, newMove.Flinch);
                    if (oldMove.Flags != newMove.Flags) AddBatchLog(i, "Flags", oldMove.Flags, newMove.Flags);
                    if (oldMove.ZPower != newMove.ZPower) AddBatchLog(i, "Z-Power", oldMove.ZPower, newMove.ZPower);
                }

                // 4. Save to JSON and update the UI
                SaveLogs();
                _manualLogNotes = "";
                UpdateLogView();
                
                tcMain.SelectedTab = tpLog;
                WinFormsUtil.Alert("Vanilla baseline loaded!", "All existing changes between your ROM and the Vanilla baseline have been generated in the log.");
            }
            catch (Exception ex)
            {
                WinFormsUtil.Error("Failed to process vanilla file: " + ex.Message);
            }
        }
    }

    private void AddBatchLog(int index, string property, object oldVal, object newVal)
    {
        if (!_changeLogs.ContainsKey(index)) _changeLogs[index] = new List<string>();
        _changeLogs[index].Add($"[Vanilla Compare] {property}: {oldVal} -> {newVal}");
    }

    private byte[][] _originalMoves;
    private Dictionary<int, List<string>> _changeLogs = new Dictionary<int, List<string>>();

    private void LogChange(int index, string property, object oldVal, object newVal)
    {
        if (!_changeLogs.ContainsKey(index)) _changeLogs[index] = new List<string>();
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = $"[{timestamp}] {property}: {oldVal} -> {newVal}";
        
        // Don't log if the last log for this property was the same value
        var last = _changeLogs[index].LastOrDefault(l => l.Contains(property));
        if (last != null && last.EndsWith(newVal.ToString())) return;

        _changeLogs[index].Add(entry);
        SaveLogs();
        
        // Only trigger automatic update if user hasn't started manual notes
        if (string.IsNullOrEmpty(_manualLogNotes) || _manualLogNotes.StartsWith("=== SESSION"))
        {
            _manualLogNotes = ""; // Force full rebuild
            UpdateLogView();
        }
    }

    private bool _isUpdatingLog = false;
    private string _manualLogNotes = "";
    private void OnLogChanged(object sender, EventArgs e)
    {
        if (_isUpdatingLog || rtbLog == null) return;
        _manualLogNotes = rtbLog.Text;
    }

    private void UpdateLogView()
    {
        if (rtbLog == null) return;
        _isUpdatingLog = true;
        
        // If we have manual notes, we append them or use them as the base
        if (string.IsNullOrEmpty(_manualLogNotes) || _manualLogNotes.StartsWith("=== SESSION"))
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== SESSION CHANGE LOG ===");
            sb.AppendLine("Changes are tracked until you close the editor.");
            sb.AppendLine("YOU CAN MANUALLY TYPE NOTES OR DELETE REVERTED CHANGES HERE.");
            sb.AppendLine();

            foreach (var kvp in _changeLogs.OrderBy(k => k.Key))
            {
                sb.AppendLine($"--- Move {kvp.Key:000} ({movelist[kvp.Key]}) ---");
                foreach (var log in kvp.Value)
                    sb.AppendLine(log);
                sb.AppendLine();
            }
            rtbLog.Text = sb.ToString();
            _manualLogNotes = rtbLog.Text;
        }
        else
        {
            rtbLog.Text = _manualLogNotes;
        }
        
        _isUpdatingLog = false;
    }

    private void TrackComparison(int index)
    {
        if (index < 1 || index >= files.Length) return;
        var oldMove = new Move7(_originalMoves[index]);
        var newMove = new Move7(files[index]);

        // Compare key properties
        if (oldMove.Power != newMove.Power) LogChange(index, "Power", oldMove.Power, newMove.Power);
        if (oldMove.Type != newMove.Type) LogChange(index, "Type", types[oldMove.Type], types[newMove.Type]);
        if (oldMove.Accuracy != newMove.Accuracy) LogChange(index, "Accuracy", oldMove.Accuracy, newMove.Accuracy);
        if (oldMove.PP != newMove.PP) LogChange(index, "PP", oldMove.PP, newMove.PP);
        if (oldMove.Category != newMove.Category) LogChange(index, "Category", MoveCategories[oldMove.Category], MoveCategories[newMove.Category]);
        if (oldMove.Priority != newMove.Priority) LogChange(index, "Priority", oldMove.Priority, newMove.Priority);
        if (oldMove.CritStage != newMove.CritStage) LogChange(index, "Crit Stage", oldMove.CritStage, newMove.CritStage);
        if (oldMove.Flinch != newMove.Flinch) LogChange(index, "Flinch %", oldMove.Flinch, newMove.Flinch);
        if (oldMove.Flags != newMove.Flags) LogChange(index, "Flags", oldMove.Flags, newMove.Flags);
        if (oldMove.ZPower != newMove.ZPower) LogChange(index, "Z-Power", oldMove.ZPower, newMove.ZPower);
    }

    private void LoadAnimationHashes()
    {
        var garc = Main.Config.GetGARCData("move_anim");
        if (garc == null) return;
        animFiles = garc.Files;
        if (animFiles == null) return;

        for (int i = 1; i < Math.Min(animFiles.Length, 600); i++) // Only hash original base moves for speed
        {
            if (animFiles[i] == null || animFiles[i].Length == 0) continue;
            string hash = GetHash(animFiles[i]);
            if (!AnimHashes.ContainsKey(hash))
                AnimHashes[hash] = i;
        }
    }

    private string GetHash(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA1.Create();
        return Convert.ToBase64String(sha.ComputeHash(data));
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

    private void LoadAnimationMap()
    {
        AnimationMap.Clear();
        if (!File.Exists(AnimationMapPath)) return;
        try
        {
            foreach (var line in File.ReadAllLines(AnimationMapPath))
            {
                var parts = line.Split('=');
                if (parts.Length == 2 && int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int anim))
                    AnimationMap[id] = anim;
            }
        }
        catch { }
    }

    private void SaveAnimationMap()
    {
        try
        {
            var lines = AnimationMap.Where(kvp => kvp.Key != kvp.Value).Select(kvp => $"{kvp.Key}={kvp.Value}");
            File.WriteAllLines(AnimationMapPath, lines);
        }
        catch { }
    }

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
        if (WinFormsUtil.ShowExtendedLogic)
        {
            string[] custom = ["Slicing", "Biting", "Bullet", "Pulse", "Wind", "Light"];
            for (int i = 0; i < custom.Length; i++)
            {
                int idx = Array.IndexOf(flagnames, $"F{18 + i}");
                if (idx >= 0) flagnames[idx] = custom[i];
            }
        }
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
        UpdateLogView();
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
            var move = new Move7(data);
            CB_Type.SelectedIndex = move.Type;
            CB_Quality.SelectedIndex = move.Quality;
            CB_Category.SelectedIndex = move.Category;
            NUD_Power.Value = move.Power;
            NUD_Accuracy.Value = move.Accuracy;
            NUD_PP.Value = move.PP;
            NUD_Priority.Value = (sbyte)move.Priority;
            NUD_HitMin.Value = move.HitMin;
            NUD_HitMax.Value = move.HitMax;
            NUD_Inflict.Value = move.Inflict;
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

            CB_ZMove.SelectedIndex = move.ZMove;
            NUD_ZPower.Value = move.ZPower;
            CB_ZEffect.SelectedIndex = move.ZEffect;

            CB_AfflictRefresh.SelectedIndex = (int)move.RefreshAfflictType;
            NUD_RefreshAfflictPercent.Value = move.RefreshAfflictPercent;

            var flags = (uint)move.Flags;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                CLB_Flags.SetItemChecked(i, ((flags >> i) & 1) == 1);

            // Sync Animation ID (Override -> Auto-Detection -> Native ID)
            if (AnimationMap.TryGetValue(entry, out int anim))
            {
                NUD_AnimID.Value = anim;
            }
            else if (animFiles != null && entry < animFiles.Length)
            {
                string hash = GetHash(animFiles[entry]);
                if (AnimHashes.TryGetValue(hash, out int matchedID))
                    NUD_AnimID.Value = matchedID;
                else
                    NUD_AnimID.Value = entry;
            }
            else
            {
                NUD_AnimID.Value = entry; // Default Link to self
            }

            // Sync Flavor Text
            if (moveflavor != null && entry < moveflavor.Length)
                RTB_MoveDesc.Text = moveflavor[entry].Replace("\\n", Environment.NewLine);
        }
    }

    private void SetEntry()
    {
        if (entry < 1 || entry >= files.Length) return;
        TrackComparison(entry);
        
        byte[] data = files[entry];
        {
            var move = new Move7(data)
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
                Inflict = (int)NUD_Inflict.Value,
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

                ZMove = CB_ZMove.SelectedIndex,
                ZPower = (int)NUD_ZPower.Value,
                ZEffect = CB_ZEffect.SelectedIndex,

                RefreshAfflictType = (RefreshType)CB_AfflictRefresh.SelectedIndex,
                RefreshAfflictPercent = (int)NUD_RefreshAfflictPercent.Value,
            };

            uint flagval = 0;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                flagval |= CLB_Flags.GetItemChecked(i) ? 1u << i : 0;
            move.Flags = (MoveFlag7)flagval;

            // Sync Animation ID to Map (Only store overrides)
            int selectedAnim = (int)NUD_AnimID.Value;
            if (selectedAnim != entry)
                AnimationMap[entry] = selectedAnim;
            else
                AnimationMap.Remove(entry);
        }
        files[entry] = data;
    }

    private void CloseForm(object sender, FormClosingEventArgs e)
    {
        try { SetEntry(); } catch { }
        SaveAnimationMap();
        SaveLogs();
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
        int moveSize = 0x28; // Move7 size
        
        Array.Resize(ref files, newId + 1);
        files[newId] = (byte[])(files.Length > 1 ? files[1].Clone() : new byte[moveSize]);
        
        Array.Resize(ref movelist, newId + 1);
        movelist[newId] = $"New Move {newId}";
        
        if (moveflavor != null)
        {
            Array.Resize(ref moveflavor, newId + 1);
            moveflavor[newId] = "Update this description in the Game Text Editor.";
        }
        
        CB_Move.Items.Add(movelist[newId]);
        CB_Move.SelectedIndex = CB_Move.Items.Count - 1;

        // Auto-patch engine limits and sync animation
        EnginePatcher7.SyncEngineLimits(files.Length - 1);
        PerformAnimationSync();
        
        WinFormsUtil.Alert("Move slot added and engine patched!", 
                           "Animation has been synced (cloned) to Pound by default.",
                           "You MUST open the Game Text Editor and add a matching line for Move Names and Move Descriptions for the new move to display properly.");
    }

    private void B_ChampionsPP_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Apply Pokemon Champions PP settings to all moves?", "5->8, 10->12, 15->16, 20+->20") != DialogResult.Yes) return;
        
        for (int i = 1; i < files.Length; i++)
        {
            var move = new Move7(files[i]);
            int pp = move.PP;
            
            if (pp > 0 && pp <= 5) move.PP = 8;
            else if (pp > 5 && pp <= 10) move.PP = 12;
            else if (pp > 10 && pp <= 15) move.PP = 16;
            else if (pp > 15) move.PP = 20;
            
            files[i] = move.Write();
        }
        
        entry = -1;
        if (CB_Move.SelectedIndex >= 0) { entry = CB_Move.SelectedIndex + 1; GetEntry(); }
        WinFormsUtil.Alert("Champions PP settings applied to all moves!");
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

        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "This will repack the Move Data into the ROM (a/0/1/1).", "Continue?"))
            return;

        // 1. Pack Mini-pack (WD)
        byte[] mini = Mini.PackMini(files, "WD");

        // 2. Load and Update GARC
        var gm = Main.Config.GARCMoves;
        gm.Files[0] = mini;
        gm.Save();

        // 3. Patch Engine and Sync Animations
        EnginePatcher7.SyncEngineLimits(files.Length - 1);
        PerformAnimationSync();

        WinFormsUtil.Alert("Move GARC (a/0/1/1) repacked and saved!", "Engine limits verified and animations synced.");
    }

    private void PerformAnimationSync()
    {
        var animGARC = Main.Config.GetGARCData("move_anim_prop"); // a/0/2/1
        if (animGARC == null) return;

        bool modified = false;
        int maxFile = animGARC.Files.Length;

        // Ensure GARC has enough slots
        if (maxFile < files.Length)
        {
            var animFiles = animGARC.Files.ToList();
            byte[] poundAnim = animFiles.Count > 1 ? animFiles[1] : new byte[0];
            while (animFiles.Count < files.Length)
            {
                animFiles.Add((byte[])poundAnim.Clone());
            }
            animGARC.Files = animFiles.ToArray();
            maxFile = animGARC.Files.Length;
            modified = true;
        }

        // Apply Mappings (Global Sync)
        for (int i = 1; i < files.Length; i++)
        {
            int animID = AnimationMap.ContainsKey(i) ? AnimationMap[i] : i;
            if (animID >= maxFile) continue;

            byte[] source = animGARC.Files[animID];
            byte[] target = animGARC.Files[i];

            if (!source.SequenceEqual(target))
            {
                animGARC.Files[i] = (byte[])source.Clone();
                modified = true;
            }
        }

        if (modified)
        {
            animGARC.Save();
            SaveAnimationMap();
        }
    }

    private void B_SyncAnim_Click(object sender, EventArgs e)
    {
        PerformAnimationSync();
        WinFormsUtil.Alert("Animation syncing complete!", "The animation GARC (a/0/2/1) has been updated based on your Move -> Animation ID mappings.");
    }

    private void B_CopyAnim_Click(object sender, EventArgs e)
    {
        if (entry < 1) return;
        AnimationMap[entry] = (int)NUD_CopyAnim.Value;
        NUD_AnimID.Value = NUD_CopyAnim.Value;
        PerformAnimationSync();
        WinFormsUtil.Alert("Animation Updated!", $"Move {entry} is now mapped to use Move {NUD_CopyAnim.Value}'s animation.");
    }

    private void B_ImportTxt_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string[] lines = System.IO.File.ReadAllLines(ofd.FileName);
        int currentId = -1;
        int updated = 0;
        string[] categories = ["Status", "Physical", "Special"];

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("==="))
            {
                // Parse "=== 001: Pound ==="
                string inner = line.Trim(' ', '=');
                int colonIdx = inner.IndexOf(':');
                if (colonIdx > 0 && int.TryParse(inner.Substring(0, colonIdx).Trim(), out int id))
                    currentId = id;
                continue;
            }
            if (currentId < 1 || currentId >= files.Length) continue;

            byte[] data = files[currentId];
            if (data == null || data.Length < 0x24) continue;

            int kvSep = line.IndexOf(':');
            if (kvSep < 0) continue;
            string key = line.Substring(0, kvSep).Trim();
            string val = line.Substring(kvSep + 1).Trim();

            switch (key)
            {
                case "Type":
                    int ti = Array.IndexOf(types, val);
                    if (ti >= 0) data[0x00] = (byte)ti;
                    else if (byte.TryParse(val, out byte tb)) data[0x00] = tb;
                    break;
                case "Category":
                    int ci = Array.IndexOf(categories, val);
                    if (ci >= 0) data[0x02] = (byte)ci;
                    else if (byte.TryParse(val, out byte cb)) data[0x02] = cb;
                    break;
                case "Power": if (byte.TryParse(val, out byte pw)) data[0x03] = pw; break;
                case "Accuracy": if (byte.TryParse(val, out byte ac)) data[0x04] = ac; break;
                case "PP": if (byte.TryParse(val, out byte pp)) data[0x05] = pp; break;
                case "Priority": if (int.TryParse(val, out int pr)) data[0x06] = (byte)(sbyte)pr; break;
                case "Hits":
                    var hp = val.Split('-');
                    if (hp.Length == 2 && byte.TryParse(hp[0], out byte mn) && byte.TryParse(hp[1], out byte mx))
                        data[0x07] = (byte)(mn | (mx << 4));
                    break;
                case "CritStage": if (byte.TryParse(val, out byte cs)) data[0x0E] = cs; break;
                case "Flinch": if (byte.TryParse(val, out byte fl)) data[0x0F] = fl; break;
                case "Effect": if (ushort.TryParse(val, out ushort ef)) Array.Copy(BitConverter.GetBytes(ef), 0, data, 0x10, 2); break;
                case "Recoil": if (int.TryParse(val, out int rc)) data[0x12] = (byte)(sbyte)rc; break;
                case "Heal": if (byte.TryParse(val, out byte hl)) data[0x13] = hl; break;
                case "Targeting": if (byte.TryParse(val, out byte tg)) data[0x14] = tg; break;
                case "Inflict": if (short.TryParse(val, out short inf)) Array.Copy(BitConverter.GetBytes(inf), 0, data, 0x08, 2); break;
                case "InflictPercent": if (byte.TryParse(val, out byte ip)) data[0x0A] = ip; break;
                case "TurnMin": if (byte.TryParse(val, out byte tmn)) data[0x0C] = tmn; break;
                case "TurnMax": if (byte.TryParse(val, out byte tmx)) data[0x0D] = tmx; break;
                case "Quality": if (byte.TryParse(val, out byte ql)) data[0x01] = ql; break;
                case "0xB": if (byte.TryParse(val, out byte xb)) data[0x0B] = xb; break;
                case "Flags":
                    if (uint.TryParse(val, out uint fg))
                    {
                        var m = new Move7(data) { Flags = (MoveFlag7)fg };
                    }
                    break;
            }
            updated++;
        }
        // Refresh the current entry display
        entry = -1;
        if (CB_Move.SelectedIndex >= 0) { entry = CB_Move.SelectedIndex + 1; GetEntry(); }
        WinFormsUtil.Alert("Imported move data! ({updated} fields processed)");
    }

    private void B_JumpText_Click(object sender, EventArgs e)
    {
        if (entry < 1) { WinFormsUtil.Alert("Select a move first."); return; }
        string moveName = entry < movelist.Length ? movelist[entry] : $"Move {entry}";
        WinFormsUtil.Alert(
            $"Move: {moveName} (ID: {entry})",
            "To edit the name/description, open the Game Text Editor and go to:",
            $"  • Move Names → line {entry}",
            $"  • Move Descriptions → line {entry}",
            "The text file indices are: MoveNames & MoveFlavor."
        );
    }

    private void B_RandAll_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize all moves using the selected options?") != DialogResult.Yes) return;
        
        var rand = new Random();
        for (int i = 1; i < files.Length; i++)
        {
            var move = new Move7(files[i]);
            if (CHK_Type.Checked) move.Type = (byte)rand.Next(types.Length);
            if (CHK_Category.Checked) move.Category = (byte)rand.Next(3);
            files[i] = move.Write();
        }
        
        entry = -1;
        if (CB_Move.SelectedIndex >= 0) { entry = CB_Move.SelectedIndex + 1; GetEntry(); }
        WinFormsUtil.Alert("Randomization complete!");
    }
    private void B_ExportTxt_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Text Export functionality not ported in the recent refactor.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}