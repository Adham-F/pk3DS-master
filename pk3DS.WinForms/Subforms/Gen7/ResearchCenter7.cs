using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using pk3DS.Core;
using pk3DS.Core.CTR;
using pk3DS.Core.Modding;
using Keystone;
using Gee.External.Capstone;
using Gee.External.Capstone.Arm;

namespace pk3DS.WinForms
{
    public partial class ResearchCenter7 : Form
    {
        private byte[] currentFileBytes;
        private Engine keystone;
        private CapstoneArmDisassembler capstone;
        private string selectedXlsxPath;
        private string actualXlsxSourcePath;
        private List<Dictionary<string, string>> selectedResearchData;
        private NumericUpDown numTargetOverride;
        private Button btnDeploySelected;
        private string detectedTypeForOverride = "Move";

        public ResearchCenter7()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            WinFormsUtil.ApplyTheme(this);
            ApplyPremiumAesthetics();
            InitializeKeystone();
            InitializeCapstone();
            InitializeProjectExplorer();
            InitializeModernHexEditor();
            InitializeARMTranslator();
            tvExplorer.AfterSelect += (s, e) => LoadSelectedResearch();
            btnExpMoves.Click += (s, e) => ExecuteQuickExpand("Move");
            btnExpAbils.Click += (s, e) => ExecuteQuickExpand("Ability");
            btnExpItems.Click += (s, e) => ExecuteQuickExpand("Item");
        }

        private void ApplyPremiumAesthetics()
        {
            this.BackColor = Color.FromArgb(20, 20, 25);
            TC_Tabs.BackColor = Color.FromArgb(25, 25, 30);
            Tab_Modding.BackColor = Color.FromArgb(20, 20, 25);
            Tab_ARM.BackColor = Color.FromArgb(15, 15, 20);
            tvExplorer.BackColor = Color.FromArgb(25, 25, 35);
            tvExplorer.ForeColor = Color.WhiteSmoke;
            rtbAssembly.BackColor = Color.FromArgb(10, 10, 15);
            rtbAssembly.ForeColor = Color.Cyan;
            rtbHex.BackColor = Color.FromArgb(10, 10, 12);
            rtbHex.ForeColor = Color.SpringGreen;
            btnMasterDeploy.BackColor = Color.FromArgb(0, 102, 204);
        }

        private void InitializeKeystone() 
        { 
            try { 
                LogDeployment($"Process Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");
                
                // Explicitly load the native library to handle architecture-specific paths
                string libName = "keystone.dll";
                string arch = Environment.Is64BitProcess ? "x64" : "x86";
                
                // Try root first, then arch subfolder
                string probeRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, libName);
                string probeArch = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arch, libName);
                
                if (File.Exists(probeRoot)) {
                    System.Runtime.InteropServices.NativeLibrary.Load(probeRoot);
                    LogDeployment($"Pre-loaded native {libName} from root folder.");
                }
                else if (File.Exists(probeArch)) {
                    System.Runtime.InteropServices.NativeLibrary.Load(probeArch);
                    LogDeployment($"Pre-loaded native {libName} from {arch} folder.");
                }
                else {
                    LogDeployment($"Warning: {libName} not found in root or {arch} subfolder.");
                }

                keystone = new Engine(Architecture.ARM, Mode.ARM); 
                LogDeployment("ARM Translator Engine (Keystone) initialized successfully.");
            } catch (Exception ex) { 
                LogDeployment($"ARM Translator Warning: {ex.Message}");
                if (ex.InnerException != null) LogDeployment($"Details: {ex.InnerException.Message}");
            } 
        }
        private void InitializeCapstone() { try { capstone = CapstoneDisassembler.CreateArmDisassembler(ArmDisassembleMode.Arm); } catch { } }

        private void InitializeProjectExplorer()
        {
            tvExplorer.Nodes.Clear();
            var root = tvExplorer.Nodes.Add("INTERNAL RESEARCH (Built-in)");
            root.ForeColor = Color.Gold;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string prefix = "pk3DS.WinForms.Resources.Research.";
            var resources = assembly.GetManifestResourceNames().Where(r => r.StartsWith(prefix)).OrderBy(r => r);
            foreach (var res in resources)
            {
                string rel = res.Substring(prefix.Length);
                string[] p = rel.Split('.');
                string fn = p[p.Length - 2] + ".xlsx";
                string cat = p.Length > 2 ? p[p.Length - 3] : "General";
                var catNode = root.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == cat) ?? root.Nodes.Add(cat);
                catNode.ForeColor = Color.SkyBlue;
                var node = catNode.Nodes.Add(fn);
                node.Tag = "res:" + res;
                node.ForeColor = Color.LightGray;
            }
            string armPath = FindArmFunctionsPath();
            if (Directory.Exists(armPath)) { var ext = tvExplorer.Nodes.Add("EXTERNAL RESEARCH (Local)"); PopulateDirectory(ext, armPath); }
            root.Expand();
        }

        private string FindArmFunctionsPath()
        {
            string p = Path.Combine(Main.RomFSPath, "..", "research");
            return Directory.Exists(p) ? p : Path.Combine(Application.StartupPath, "research");
        }

        private void PopulateDirectory(TreeNode node, string path)
        {
            foreach (var d in Directory.GetDirectories(path)) PopulateDirectory(node.Nodes.Add(Path.GetFileName(d)), d);
            foreach (var f in Directory.GetFiles(path, "*.xlsx")) { var n = node.Nodes.Add(Path.GetFileName(f)); n.Tag = f; n.ForeColor = Color.LightGray; }
        }

        private void InitializeModernHexEditor() { rtbHex.Font = new Font("Consolas", 10F); }

        private void InitializeARMTranslator()
        {
            Tab_ARM.SuspendLayout();
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            Tab_ARM.Controls.Add(split);
            
            this.txtAsmIn.Dock = DockStyle.Fill;
            this.txtAsmIn.BackColor = Color.FromArgb(10, 10, 15);
            this.txtAsmIn.ForeColor = Color.White;
            this.txtAsmIn.Font = new Font("Consolas", 11F);
            this.txtAsmIn.BorderStyle = BorderStyle.None;
            this.txtAsmIn.Text = "; Enter ARM Assembly here (e.g. mov r0, r0)\n";
            
            this.txtHexOut.Dock = DockStyle.Fill;
            this.txtHexOut.BackColor = Color.FromArgb(10, 10, 15);
            this.txtHexOut.ForeColor = Color.SpringGreen;
            this.txtHexOut.Font = new Font("Consolas", 11F);
            this.txtHexOut.BorderStyle = BorderStyle.None;
            this.txtHexOut.Text = keystone == null ? "; Keystone engine failed to load. Ensure keystone.dll is present." : "; Hex Output";
            
            split.Panel1.Controls.Add(this.txtAsmIn);
            split.Panel2.Controls.Add(this.txtHexOut);
            
            this.txtAsmIn.TextChanged += (s, e) => UpdateAsmToHex();
            Tab_ARM.ResumeLayout(true);
        }

        private void UpdateAsmToHex()
        {
            if (keystone == null) {
                txtHexOut.Text = "; Keystone engine failed to load.";
                return;
            }
            var sb = new StringBuilder();
            foreach (var l in txtAsmIn.Lines) { 
                if (string.IsNullOrWhiteSpace(l) || l.Trim().StartsWith(";")) continue; 
                try {
                    var r = keystone.Assemble(l, 0); 
                    if (r != null) foreach (byte b in r.Buffer) sb.Append(b.ToString("X2") + " "); 
                    sb.AppendLine(); 
                } catch (Exception ex) {
                    sb.AppendLine($"; Error: {ex.Message}");
                }
            }
            txtHexOut.Text = sb.ToString();
        }

        private void LoadSelectedResearch()
        {
            if (tvExplorer.SelectedNode?.Tag is not string path) return;
            actualXlsxSourcePath = path; selectedXlsxPath = path;
            if (path.StartsWith("res:"))
            {
                var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(path.Substring(4));
                if (stream != null) { string tmp = Path.Combine(Path.GetTempPath(), "pk3ds_tmp.xlsx"); using (var fs = new FileStream(tmp, FileMode.Create)) stream.CopyTo(fs); selectedXlsxPath = tmp; }
            }
            try {
                selectedResearchData = XlsxResearchParser.ReadSheet(selectedXlsxPath, "Custom Relocation Patches");
                if (selectedResearchData.Count == 0) selectedResearchData = XlsxResearchParser.ReadSheet(selectedXlsxPath, "Sheet1");
                dgvResearch.Columns.Clear(); dgvResearch.Rows.Clear();
                if (selectedResearchData.Count > 0) { foreach (var k in selectedResearchData[0].Keys) dgvResearch.Columns.Add(k, k); foreach (var r in selectedResearchData) dgvResearch.Rows.Add(r.Values.ToArray()); }
                UpdateHexViewFromSelection();
                EnsureDeploySelectedButton();
            } catch (Exception ex) { LogDeployment($"Load Error: {ex.Message}"); }
        }

        private string GetTargetPath(string target, string actualXlsxSourcePath)
        {
            bool isCode = target.ToLower().Contains("code") || (actualXlsxSourcePath != null && actualXlsxSourcePath.ToLower().Contains("code"));
            
            // Standard mappings
            string lower = target.ToLower();
            if (lower == "main") isCode = true; // 'main' is usually code.bin in USUM research

            if (isCode)
            {
                if (string.IsNullOrEmpty(Main.ExeFSPath)) return null;
                string p1 = Path.Combine(Main.ExeFSPath, "code.bin");
                string p2 = Path.Combine(Main.ExeFSPath, ".code.bin");
                if (File.Exists(p1)) return p1;
                if (File.Exists(p2)) return p2;
                if (!lower.Contains("cro")) return null; // If it was explicitly a .cro, keep searching
            }

            if (string.IsNullOrEmpty(Main.RomFSPath)) return null;

            // Common research name mappings for CROs
            var candidates = new List<string> { target };
            if (!target.Contains(".")) candidates.Add(target + ".cro");
            if (lower == "main") { candidates.Add("shop.cro"); candidates.Add("title.cro"); }
            if (lower == "field") candidates.Add("Field.cro");
            if (lower == "battle") candidates.Add("Battle.cro");
            if (lower == "bag") candidates.Add("Bag.cro");
            if (lower == "shop") candidates.Add("shop.cro");

            foreach (var c in candidates)
            {
                string p1 = Path.Combine(Main.RomFSPath, c);
                string p2 = Path.Combine(Main.RomFSPath, "battle", c);
                if (File.Exists(p1)) return p1;
                if (File.Exists(p2)) return p2;
            }
            return null;
        }

        private void UpdateHexViewFromSelection()
        {
            if (string.IsNullOrEmpty(selectedXlsxPath)) return;
            string target = "Battle.cro";
            var meta = XlsxResearchParser.ReadSheet(selectedXlsxPath, "Table Locations and Sizes");
            if (meta.Count > 0 && meta.Any(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase))))
            {
                var row = meta.First(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase)));
                var fileKey = row.Keys.First(k => k.Equals("File", StringComparison.OrdinalIgnoreCase));
                target = row[fileKey];
            }
            else if (actualXlsxSourcePath != null && actualXlsxSourcePath.ToLower().Contains("code")) 
            {
                target = "code.bin";
            }
            else 
            {
                var sheetNames = XlsxResearchParser.GetSheetNames(selectedXlsxPath);
                foreach (var s in sheetNames) {
                    var rows = XlsxResearchParser.ReadSheet(selectedXlsxPath, s);
                    int maxCheck = Math.Min(rows.Count, 10);
                    for (int i = 0; i < maxCheck; i++) {
                        var r = rows[i];
                        string offKey = r.Keys.FirstOrDefault(k => k.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0);
                        if (offKey != null && r.ContainsKey(offKey) && uint.TryParse(r[offKey].Replace("0x", "").Replace("0X", "").Trim(), System.Globalization.NumberStyles.HexNumber, null, out uint off)) {
                            if (off > 0x180000) { target = "code.bin"; break; }
                        }
                    }
                    if (target == "code.bin") break;
                }
            }

            string path = GetTargetPath(target, actualXlsxSourcePath);
            if (File.Exists(path))
            {
                currentFileBytes = File.ReadAllBytes(path);
                var highlights = new List<long>(); uint jump = 0; bool first = true;
                foreach (var r in selectedResearchData) {
                    string offKey = r.Keys.FirstOrDefault(k => k.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0);
                    string hexKey = r.Keys.FirstOrDefault(k => k.IndexOf("Hex", StringComparison.OrdinalIgnoreCase) >= 0);
                    
                    if (offKey == null || !r.ContainsKey(offKey)) continue;
                    if (!uint.TryParse(r[offKey].Replace("0x", "").Trim(), System.Globalization.NumberStyles.HexNumber, null, out uint off)) continue;
                    
                    if (first) jump = off;
                    int len = 4;
                    if (hexKey != null && r.ContainsKey(hexKey)) {
                        byte[] p = Util.StringToByteArray(r[hexKey].Replace("0x", "").Replace("0X", ""));
                        if (p != null) len = p.Length;
                    }
                    
                    for (int l = 0; l < len; l++) highlights.Add(off + l);
                    first = false;
                }
                ShowHexData(currentFileBytes, jump, highlights, Path.GetFileName(path));
            } else LogDeployment($"Hex Error: {target} not found.");
        }

        private void ShowHexData(byte[] data, uint offset, List<long> highlights, string fileName)
        {
            rtbHex.Clear(); rtbHex.SuspendLayout();
            rtbHex.SelectionColor = Color.Yellow; rtbHex.AppendText($"--- SOURCE: {fileName} ---\n\n");
            int start = (int)(offset & ~0xF), len = Math.Min(data.Length - start, 0x1000);
            for (int i = 0; i < len; i += 16) {
                int curr = start + i; rtbHex.SelectionColor = Color.Gray; rtbHex.AppendText($"{curr:X8}: ");
                for (int j = 0; j < 16; j++) {
                    int p = curr + j; if (p < data.Length) { rtbHex.SelectionColor = highlights.Contains(p) ? Color.Red : Color.White; rtbHex.AppendText($"{data[p]:X2} "); }
                }
                rtbHex.AppendText("\n");
            }
            rtbHex.ResumeLayout(); tcWorkbench.SelectedTab = Tab_HexEditor;
        }

        private Button btnDynamicDump;
        private void EnsureDeploySelectedButton()
        {
            if (btnDeploySelected != null) return;
            var pnl = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10), BackColor = Color.FromArgb(30, 30, 40) };
            numTargetOverride = new NumericUpDown { Dock = DockStyle.Top, Maximum = 1000 };
            pnl.Controls.Add(numTargetOverride); pnl.Controls.Add(new Label { Text = "OVERRIDE INDEX:", Dock = DockStyle.Top, ForeColor = Color.Cyan });
            splitInspector.Panel2.Controls.Add(pnl);
            btnDeploySelected = new Button { Text = "Apply Selected to Patch", Dock = DockStyle.Top, Height = 50, FlatStyle = FlatStyle.Flat, BackColor = Color.Teal, ForeColor = Color.White };
            splitInspector.Panel2.Controls.Add(btnDeploySelected);
            btnDeploySelected.Click += (s, e) => ExecuteSmartApply();
            
            btnDynamicDump = new Button { Text = "Dump Dynamic Table Offsets", Dock = DockStyle.Top, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.Indigo, ForeColor = Color.White };
            splitInspector.Panel2.Controls.Add(btnDynamicDump);
            btnDynamicDump.Click += (s, e) => ExecuteDynamicDump();
        }

        private void ExecuteDynamicDump()
        {
            string path = GetTargetPath("Battle.cro", null);
            if (path == null || !File.Exists(path)) { LogDeployment("Battle.cro not found. Dump aborted."); return; }
            byte[] cro = File.ReadAllBytes(path);
            var sb = new StringBuilder();
            sb.AppendLine("=== DYNAMIC TABLE DUMP ===");
            
            string[] types = { "Move", "Ability", "Item" };
            foreach (var t in types) {
                int tblBase = ResearchEngine.GetRelocationTableBase(cro, t);
                if (tblBase == -1) { sb.AppendLine($"\n[!] {t} Table Base not found."); continue; }
                sb.AppendLine($"\n--- {t} Table ---");
                sb.AppendLine($"Table Base (Relocation Patched): 0x{tblBase:X}");
                
                int limitCheckIdx = -1;
                uint xMin = 0, xMax = 0;
                if (t == "Item") { xMin = 800; xMax = 1005; }
                else if (t == "Ability") { xMin = 200; xMax = 256; }
                else if (t == "Move") { xMin = 700; xMax = 805; }
                uint limit = 0;
                
                for (int i = 0; i < cro.Length - 4; i += 4) {
                    uint xWord = BitConverter.ToUInt32(cro, i);
                    if ((xWord & 0xFFF00000) == 0xE3500000 || (xWord & 0xFFF00000) == 0xE3510000 || (xWord & 0xFFF00000) == 0xE3520000) {
                        uint xImm = xWord & 0xFF;
                        uint xRot = (xWord >> 8) & 0xF;
                        uint val = (xImm >> (int)(xRot * 2)) | (xImm << (int)(32 - (xRot * 2)));
                        if (val >= xMin && val <= xMax) { limitCheckIdx = i; limit = val; break; }
                    }
                }
                
                if (limitCheckIdx != -1) sb.AppendLine($"Detected Limit: {limit} (at 0x{limitCheckIdx:X})");
                else { limit = xMax; sb.AppendLine($"Limit check not found. Assuming max: {limit}"); }
                
                for (int i = 0; i < limit; i++) {
                    int entryAddr = tblBase + (i * 8);
                    if (entryAddr + 8 > cro.Length) break;
                    uint index = BitConverter.ToUInt32(cro, entryAddr);
                    if (index == 0 && i != 0) continue; // Skip 0-index after the first (usually blank)
                    
                    int patchIdx = pk3DS.Core.CTR.CROUtil.FindRelocationPatchIndex(cro, (uint)(entryAddr + 4));
                    if (patchIdx == -1) {
                        // sb.AppendLine($"[{t} {i}] Entry: 0x{entryAddr:X} -> No CallFunc Relocation Found");
                        continue;
                    }
                    var patch = pk3DS.Core.CTR.CROUtil.GetRelocationEntry(cro, patchIdx);
                    uint callFuncAddr = patch.Addend + pk3DS.Core.CTR.CROUtil.GetSegmentStartIndices(cro)[patch.TargetSeg];
                    
                    string line = $"[{t} {index}] Entry: 0x{entryAddr:X} -> CallFunc: 0x{callFuncAddr:X}";
                    
                    if (callFuncAddr + 20 <= cro.Length) {
                        uint movInstr = BitConverter.ToUInt32(cro, (int)callFuncAddr);
                        uint funcCount = movInstr & 0xFF; // rough approximation of MOV R?, #Imm
                        
                        uint ptrAddr = callFuncAddr + 16;
                        int ptrPatchIdx = pk3DS.Core.CTR.CROUtil.FindRelocationPatchIndex(cro, ptrAddr);
                        if (ptrPatchIdx != -1) {
                            var ptrPatch = pk3DS.Core.CTR.CROUtil.GetRelocationEntry(cro, ptrPatchIdx);
                            uint subListAddr = ptrPatch.Addend + pk3DS.Core.CTR.CROUtil.GetSegmentStartIndices(cro)[ptrPatch.TargetSeg];
                            line += $"  [Count: {funcCount} | SubList: 0x{subListAddr:X}]";
                        }
                    }
                    sb.AppendLine(line);
                }
            }
            rtbAssembly.Clear();
            rtbAssembly.SelectionColor = Color.MediumPurple;
            rtbAssembly.AppendText(sb.ToString());
            tcWorkbench.SelectedTab = Tab_ARM; // Show results in ARM tab
            LogDeployment("Dynamic table dump completed successfully! Results in ARM Translator tab.");
        }

        private void B_ExecuteUnified_Click(object sender, EventArgs e) { ExecuteMasterDeployment(); }

        private void ExecuteMasterDeployment()
        {
            rtbAssembly.Clear(); string p = FindArmFunctionsPath();
            if (Directory.Exists(p)) foreach (var f in Directory.GetFiles(p, "*.xlsx", SearchOption.AllDirectories)) ProcessXlsxPatch(f);
        }

        private void ProcessXlsxPatch(string path)
        {
            try {
                LogDeployment($"Analyzing patch: {Path.GetFileName(path)}");
                var allSheets = XlsxResearchParser.GetSheetNames(path);

                // --- Multi-file mode: sheets named "edited code.bin", "edited bag.cro", etc. ---
                // Each "edited X" sheet independently targets the file named X.
                var editedSheets = allSheets
                    .Where(s => s.StartsWith("edited ", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (editedSheets.Count > 0)
                {
                    foreach (var sheet in editedSheets)
                    {
                        string fileName = sheet.Substring("edited ".Length).Trim(); // e.g. "code.bin", "bag.cro"
                        string full = GetTargetPath(fileName, path);
                        if (full == null || !File.Exists(full))
                        {
                            LogDeployment($"[{sheet}] '{fileName}' not found — skipping.");
                            continue;
                        }

                        LogDeployment($"[{sheet}] → {Path.GetFileName(full)}");
                        byte[] bin = File.ReadAllBytes(full);
                        var rows = XlsxResearchParser.ReadSheet(path, sheet);
                        int count = ApplyPatchRows(bin, rows, fileName);

                        if (count > 0)
                        {
                            if (fileName.ToLower().EndsWith(".cro"))
                                CROUtil.UpdateHashes(bin);
                            File.WriteAllBytes(full, bin);
                            LogDeployment($"[{sheet}] Patched {count} bytes to {Path.GetFileName(full)}.");
                        }
                        else
                        {
                            LogDeployment($"[{sheet}] No patchable rows found.");
                        }
                    }
                    UpdateHexViewFromSelection();
                    return;
                }

                // --- Legacy single-file mode (existing behaviour) ---
                string target = "Battle.cro";
                var meta = XlsxResearchParser.ReadSheet(path, "Table Locations and Sizes");
                if (meta.Count > 0 && meta.Any(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase))))
                {
                    var row = meta.First(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase)));
                    var fileKey = row.Keys.First(k => k.Equals("File", StringComparison.OrdinalIgnoreCase));
                    target = row[fileKey];
                }
                else if (path.ToLower().Contains("code") || (actualXlsxSourcePath != null && actualXlsxSourcePath.ToLower().Contains("code")))
                {
                    target = "code.bin";
                }
                else if (path.ToLower().Contains("shop"))
                {
                    target = "shop.cro";
                }
                else if (path.ToLower().Contains("field"))
                {
                    target = "Field.cro";
                }
                else if (path.ToLower().Contains("bag"))
                {
                    target = "Bag.cro";
                }
                else
                {
                    foreach (var s in allSheets) {
                        var rows = XlsxResearchParser.ReadSheet(path, s);
                        int maxCheck = Math.Min(rows.Count, 10);
                        for (int i = 0; i < maxCheck; i++) {
                            var r = rows[i];
                            string offKey = r.Keys.FirstOrDefault(k => k.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0);
                            if (offKey != null && r.ContainsKey(offKey) && uint.TryParse(r[offKey].Replace("0x", "").Replace("0X", "").Trim(), System.Globalization.NumberStyles.HexNumber, null, out uint off)) {
                                if (off > 0x180000) { target = "code.bin"; break; }
                            }
                        }
                        if (target == "code.bin") break;
                    }
                }

                string fullPath = GetTargetPath(target, actualXlsxSourcePath ?? path);
                if (fullPath == null || !File.Exists(fullPath)) {
                    LogDeployment($"Target Not Found: '{target}'. Please ensure ExeFS/RomFS is loaded.");
                    return;
                }

                LogDeployment($"Targeting: {Path.GetFileName(fullPath)}");
                byte[] binLegacy = File.ReadAllBytes(fullPath);
                bool modified = false;

                foreach (var r in meta) {
                    if (r.Keys.Any(k => k.Equals("Expansion", StringComparison.OrdinalIgnoreCase)) && int.TryParse(r.Values.First(), out int sz)) {
                        LogDeployment($"Expanding segment by {sz} bytes...");
                        binLegacy = CROUtil.ExpandSegment(binLegacy, 'c', sz);
                        modified = true;
                    }
                }

                int legacyCount = 0;
                foreach (var s in allSheets) {
                    if (s.Equals("Table Locations and Sizes", StringComparison.OrdinalIgnoreCase) || s.Equals("Metadata", StringComparison.OrdinalIgnoreCase)) continue;
                    var rows = XlsxResearchParser.ReadSheet(path, s);
                    if (rows.Count == 0) continue;
                    LogDeployment($"Sheet '{s}' columns: {string.Join(", ", rows[0].Keys)}");
                    legacyCount += ApplyPatchRows(binLegacy, rows, target);
                }

                if (legacyCount > 0 || modified) {
                    if (target.ToLower().EndsWith(".cro"))
                        CROUtil.UpdateHashes(binLegacy);
                    File.WriteAllBytes(fullPath, binLegacy);
                    LogDeployment($"Successfully patched {legacyCount} bytes to {Path.GetFileName(fullPath)}.");
                    UpdateHexViewFromSelection();
                } else {
                    LogDeployment("No changes applied. Check spreadsheet columns (Offset, Hex).");
                }
            } catch (Exception ex) { LogDeployment($"Critical Error: {ex.Message}"); }
        }

        /// <summary>
        /// Applies all patchable rows from an xlsx sheet to <paramref name="bin"/>.
        /// Returns the total number of bytes written.
        /// </summary>
        private int ApplyPatchRows(byte[] bin, List<Dictionary<string, string>> rows, string target)
        {
            int count = 0;
            foreach (var r in rows)
            {
                // Prefer "in-file" columns (actual file offsets) over "Address" columns
                // (which contain virtual memory addresses, e.g. loaded at 0x100000 for code.bin).
                string offKey = r.Keys.FirstOrDefault(k => k.IndexOf("in-file", StringComparison.OrdinalIgnoreCase) >= 0)
                             ?? r.Keys.FirstOrDefault(k => k.IndexOf("Offset",  StringComparison.OrdinalIgnoreCase) >= 0)
                             ?? r.Keys.FirstOrDefault(k => k.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0);
                // Prefer "Hex US" over any generic Hex column.
                string hexKey = r.Keys.FirstOrDefault(k => k.Equals("Hex US", StringComparison.OrdinalIgnoreCase))
                             ?? r.Keys.FirstOrDefault(k => k.IndexOf("Hex", StringComparison.OrdinalIgnoreCase) >= 0);

                if (offKey == null || hexKey == null) continue;

                string offStr = r[offKey].Replace("0x", "").Trim();
                if (string.IsNullOrEmpty(offStr) || !uint.TryParse(offStr, System.Globalization.NumberStyles.HexNumber, null, out uint off)) continue;

                // If the column is an "Address"-type (virtual) and we're targeting code.bin,
                // auto-subtract the 0x100000 ExeFS load base to get the real file offset.
                bool isAddressColumn = offKey.IndexOf("Address", StringComparison.OrdinalIgnoreCase) >= 0
                                    && offKey.IndexOf("in-file", StringComparison.OrdinalIgnoreCase) < 0;
                bool isCodeBin = target.ToLower().Contains("code");
                if (isAddressColumn && isCodeBin && off >= 0x100000u)
                    off -= 0x100000u;

                string hexStr = r[hexKey].Trim().Replace("0x", "").Replace("0X", "");
                if (string.IsNullOrEmpty(hexStr)) continue;

                byte[] p = Util.StringToByteArray(hexStr);
                if (p == null || p.Length == 0) {
                    LogDeployment($"Failed to parse hex: '{hexStr}' at 0x{off:X}");
                    continue;
                }

                // 0xCCCCCCCC is a padding/trap marker used in research spreadsheets to denote
                // unused space between injected code blocks. Never write these to the file.
                if (p.Length == 4 && p[0] == 0xCC && p[1] == 0xCC && p[2] == 0xCC && p[3] == 0xCC)
                    continue;

                if (numTargetOverride.Value > 0 && target.ToLower().Contains("battle.cro")) {
                    int b = ResearchEngine.GetRelocationTableBase(bin, detectedTypeForOverride);
                    if (b != -1) off = (uint)(b + ((int)numTargetOverride.Value * 4));
                }

                // Modular TM Item ID support
                string tmKey = r.Keys.FirstOrDefault(k => k.IndexOf("TM #", StringComparison.OrdinalIgnoreCase) >= 0);
                if (tmKey != null && int.TryParse(r[tmKey], out int tmNum) && tmNum >= 108 && numTargetOverride.Value > 0)
                {
                    // If this is the TM Item ID table, override the hex with the new calculated Item ID
                    // Base ID comes from numTargetOverride
                    int newItemID = (int)numTargetOverride.Value + (tmNum - 108);
                    p = BitConverter.GetBytes((ushort)newItemID);
                }

                if (off + p.Length <= bin.Length) {
                    Array.Copy(p, 0, bin, (int)off, p.Length);
                    count += p.Length;
                } else {
                    LogDeployment($"Skip Out of Bounds: 0x{off:X} (Size {p.Length})");
                }
            }
            return count;
        }

        private void ExecuteSmartApply()
        {
            if (string.IsNullOrEmpty(selectedXlsxPath)) return;
            string dir = Path.GetDirectoryName(actualXlsxSourcePath) ?? "";
            if (dir.Contains("Move") || dir.Contains("Ability") || dir.Contains("Item")) {
                detectedTypeForOverride = dir.Contains("Ability") ? "Ability" : (dir.Contains("Item") ? "Item" : "Move");
                var dr = MessageBox.Show($"Perform Modular Link for {detectedTypeForOverride}?", "Modular Link", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes) { var res = WinFormsUtil.PromptInput("Link to ID:", "801"); if (int.TryParse(res, out int id)) numTargetOverride.Value = id; }
            }
            ProcessXlsxPatch(selectedXlsxPath);
            numTargetOverride.Value = 0;
        }

        private void ExecuteQuickExpand(string type)
        {
            try {
                string cp = Path.Combine(Main.ExeFSPath, "code.bin");
                if (!File.Exists(cp)) cp = Path.Combine(Main.ExeFSPath, ".code.bin");
                string bp = File.Exists(Path.Combine(Main.RomFSPath, "Battle.cro")) ? Path.Combine(Main.RomFSPath, "Battle.cro") : Path.Combine(Main.RomFSPath, "battle", "Battle.cro");
                
                int currentCount = 0;
                if (type == "Move") currentCount = Main.Config.GetText(TextName.MoveNames).Length;
                else if (type == "Ability") currentCount = Main.Config.GetText(TextName.AbilityNames).Length;
                else if (type == "Item") currentCount = Main.Config.GetText(TextName.ItemNames).Length;
                
                Form dialog = new Form() { Width = 320, Height = 190, Text = $"Expand {type}", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false };
                Label lblCurrent = new Label() { Left = 20, Top = 20, Width = 250, Text = $"Current {type} count: {currentCount}", ForeColor = Color.White };
                Label lblAdd = new Label() { Left = 20, Top = 50, Width = 120, Text = $"Amount to ADD:", ForeColor = Color.White };
                NumericUpDown nudAdd = new NumericUpDown() { Left = 150, Top = 48, Width = 120, Minimum = 0, Maximum = 2000, Value = 0 };
                Label lblNewTotal = new Label() { Left = 20, Top = 80, Width = 250, Text = $"New Total: {currentCount}", ForeColor = Color.Cyan };
                Label lblNote = new Label() { Left = 20, Top = 100, Width = 260, Text = "", ForeColor = Color.FromArgb(200, 200, 100), Font = new Font("Segoe UI", 7.5f) };
                
                // Moves must be added in multiples of 4 (ARM immediate encoding requirement)
                if (type == "Move") {
                    nudAdd.Increment = 4;
                    nudAdd.Value = 4;
                    lblNote.Text = "Moves must be added in multiples of 4.";
                }
                
                nudAdd.ValueChanged += (s, e) => { lblNewTotal.Text = $"New Total: {currentCount + (int)nudAdd.Value}"; };
                
                Button btnOk = new Button() { Text = "Expand", Left = 110, Top = 120, Width = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.Teal };
                Button btnCancel = new Button() { Text = "Cancel", Left = 200, Top = 120, Width = 70, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 50) };
                
                dialog.BackColor = Color.FromArgb(30, 30, 40);
                dialog.Controls.AddRange(new Control[] { lblCurrent, lblAdd, nudAdd, lblNewTotal, lblNote, btnOk, btnCancel });
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;
                
                if (dialog.ShowDialog() != DialogResult.OK) return;
                
                int added = (int)nudAdd.Value;
                if (added <= 0) return;
                // Round up to multiple of 4 for Moves (ARM CMP immediate encoding)
                if (type == "Move" && added % 4 != 0) added = ((added + 3) / 4) * 4;
                int lim = currentCount + added;

                if (type != "Move") {
                    byte[] cd = File.ReadAllBytes(cp); 
                    uint old = type switch { "Ability" => 234u, "Item" => 800u, _ => 0 };
                    if (ResearchEngine.PatchLimitCheck(cd, old, (uint)lim)) File.WriteAllBytes(cp, cd);
                    ResearchEngine.ExpandRelocationTable(bp, type, lim);
                }

                if (type == "Move") {
                    // 1. Expand Move GARC (a/0/1/1) using Pound (move 1) as template
                    string moveGarcPath = Path.Combine(Main.RomFSPath, "a/0/1/1");
                    byte[] poundTemplate = null;
                    if (File.Exists(moveGarcPath)) {
                        byte[] raw = File.ReadAllBytes(moveGarcPath);
                        var tmpGarc = new pk3DS.Core.CTR.GARC.MemGARC(raw);
                        var miniData = tmpGarc.GetFile(0);
                        var miniFiles = pk3DS.Core.CTR.Mini.UnpackMini(miniData, "WD");
                        if (miniFiles != null && miniFiles.Length > 1)
                            poundTemplate = (byte[])miniFiles[1].Clone(); // Move 1 = Pound
                    }
                    if (poundTemplate == null) {
                        poundTemplate = new byte[0x28];
                        poundTemplate[0x04] = 100; poundTemplate[0x05] = 5; // Accuracy=100, PP=5
                    }
                    // Clear the flags on the template so new moves start clean
                    var tmpl = new pk3DS.Core.Structures.Move7(poundTemplate);
                    tmpl.Flags = pk3DS.Core.Structures.MoveFlag7.None;
                    poundTemplate = tmpl.Write();
                    ResearchEngine.ExpandGARC(moveGarcPath, lim, 0x28, true, poundTemplate);

                    // 2. Patch engine limits in code.bin AND Battle.cro
                    EnginePatcher7.SyncEngineLimits(lim);
                    LogDeployment("Patched engine limits (code.bin + Battle.cro).");

                    // 3. Expand animation GARCs (Shunted to 1237+ to protect Z-moves/Megas)
                    try {
                        var animPropGARC = Main.Config.GetGARCData("move_anim_prop");
                        var animVisGARC = Main.Config.GetGARCData("move_anim");
                        int safeZoneStart = 1237;
                        
                        if (animPropGARC != null) {
                            var pFiles = animPropGARC.Files.ToList();
                            byte[] pTemplate = pFiles.Count > 1 ? (byte[])pFiles[1].Clone() : new byte[0];
                            // Pad up to the safe zone if necessary
                            while (pFiles.Count < safeZoneStart) pFiles.Add(new byte[pTemplate.Length]);
                            // Add slots for new moves
                            while (pFiles.Count < safeZoneStart + added) pFiles.Add((byte[])pTemplate.Clone());
                            animPropGARC.Files = pFiles.ToArray();
                            animPropGARC.Save();
                            LogDeployment("Expanded animation property GARC (Shunted).");
                        }
                        if (animVisGARC != null) {
                            var vFiles = animVisGARC.Files.ToList();
                            byte[] vTemplate = vFiles.Count > 1 ? (byte[])vFiles[1].Clone() : new byte[0];
                            while (vFiles.Count < safeZoneStart) vFiles.Add(new byte[vTemplate.Length]);
                            while (vFiles.Count < safeZoneStart + added) vFiles.Add((byte[])vTemplate.Clone());
                            animVisGARC.Files = vFiles.ToArray();
                            animVisGARC.Save();
                            LogDeployment("Expanded animation visual GARC (Shunted).");
                        }
                        
                        // 4. Apply Animation Redirection Patch to Battle.cro
                        ApplyAnimationRedirectionPatch();
                    } catch (Exception animEx) { LogDeployment($"Animation expansion warning: {animEx.Message}"); }
                }
                else if (type == "Item") ResearchEngine.ExpandGARC(Path.Combine(Main.RomFSPath, "a/0/1/9"), lim, 0x30, false);
                
                string textARC = Path.Combine(Main.RomFSPath, Main.Config.GetGARCFileName("gametext").Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(textARC)) {
                    var garc = new pk3DS.Core.CTR.GARC.MemGARC(File.ReadAllBytes(textARC));
                    byte[][] files = garc.Files;
                    
                    void ExpandTextFile(int fileIndex, int numLines, Func<int, int, string> placeholderFunc) {
                        if (fileIndex >= files.Length) return;
                        string[] lines = TextFile.GetStrings(Main.Config, files[fileIndex]);
                        int oldLen = lines.Length;
                        
                        // Check if last line is a blank sentinel — insert BEFORE it
                        bool hasSentinel = oldLen > 0 && string.IsNullOrEmpty(lines[oldLen - 1]);
                        int insertAt = hasSentinel ? oldLen - 1 : oldLen;
                        int newTotal = oldLen + (added * numLines);
                        
                        var newLines = new string[newTotal];
                        // Copy lines before insert point
                        Array.Copy(lines, 0, newLines, 0, insertAt);
                        // Insert new entries
                        for (int i = 0; i < added * numLines; i++) {
                            int itemIdx = i / numLines;
                            int subIdx = i % numLines;
                            newLines[insertAt + i] = placeholderFunc(currentCount + itemIdx, subIdx);
                        }
                        // Copy sentinel and anything after it
                        if (hasSentinel) {
                            for (int i = insertAt; i < oldLen; i++)
                                newLines[insertAt + (added * numLines) + (i - insertAt)] = lines[i];
                        }
                        files[fileIndex] = TextFile.GetBytes(Main.Config, newLines);
                    }

                    if (type == "Move") {
                        // File 118: Move Names
                        ExpandTextFile(118, 1, (idx, _) => $"Placeholder Move {idx}");
                        // File 117: Move Flavor (Description)
                        ExpandTextFile(117, 1, (idx, _) => $"This is for Placeholder Move {idx}.");
                        // File 19: Z-Move Names
                        ExpandTextFile(19, 1, (idx, _) => $"Z-Placeholder Move {idx}");

                        // File 13: Battle messages (Normal moves)
                        ExpandTextFile(13, 4, (idx, line) => line switch {
                            0 => $"[VAR PKNAME(0000)] used Placeholder Move {idx}!",
                            1 => $"The wild [VAR PKNAME(0000)] used Placeholder Move {idx}!",
                            2 => $"The opposing [VAR PKNAME(0000)] used Placeholder Move {idx}!",
                            3 => $"Totem [VAR PKNAME(0000)] used Placeholder Move {idx}!",
                            _ => ""
                        });

                        // File 14: Battle messages (Z-moves)
                        ExpandTextFile(14, 4, (idx, line) => line switch {
                            0 => $"[VAR PKNAME(0000)] used Z-Placeholder Move {idx}!",
                            1 => $"The wild [VAR PKNAME(0000)] used Z-Placeholder Move {idx}!",
                            2 => $"The opposing [VAR PKNAME(0000)] used Z-Placeholder Move {idx}!",
                            3 => $"Totem [VAR PKNAME(0000)] used Z-Placeholder Move {idx}!",
                            _ => ""
                        });
                    } else if (type == "Item") {
                        ExpandTextFile(39, 1, (idx, _) => $"New Item {idx}");
                        ExpandTextFile(40, 1, (idx, _) => $"New Item {idx}");
                        ExpandTextFile(41, 1, (idx, _) => "A newly added item.");
                        ExpandTextFile(42, 1, (idx, _) => "A newly added item.");
                    } else if (type == "Ability") {
                        ExpandTextFile(101, 1, (idx, _) => $"New Ability {idx}");
                        ExpandTextFile(102, 1, (idx, _) => "A newly added ability.");
                    }
                    
                    garc.Files = files;
                    File.WriteAllBytes(textARC, garc.Data);
                }

                // 5. Write animation map entries for new moves (anim ID = 1 = Pound)
                if (type == "Move") {
                    string animMapPath = Path.Combine(Path.GetDirectoryName(Main.RomFSPath) ?? "", "move_anims.json");
                    var animLines = new List<string>();
                    if (File.Exists(animMapPath))
                        animLines.AddRange(File.ReadAllLines(animMapPath));
                    for (int i = currentCount; i < lim; i++)
                        animLines.Add($"{i}=1");
                    File.WriteAllLines(animMapPath, animLines);
                    LogDeployment($"Set animation ID=1 (Pound) for moves {currentCount}-{lim - 1}.");
                }
                
                Main.Config.InitializeAll(); 
                LogDeployment($"Successfully expanded {type}s. Added {added} slots. New total: {lim}.");

                    // For Moves: open the Move Editor to the first new slot
                if (type == "Move") {
                    LogDeployment("Opening Move Editor to configure new moves...");
                    var mainForm = Application.OpenForms.OfType<Main>().FirstOrDefault();
                    if (mainForm != null) {
                        var openMethod = mainForm.GetType().GetMethod("B_Move_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        openMethod?.Invoke(mainForm, new object[] { null, EventArgs.Empty });
                    }
                }
            } catch (Exception ex) { LogDeployment($"Error expanding {type}: {ex.Message}"); }
        }

        private void ApplyAnimationRedirectionPatch()
        {
            string bp = GetTargetPath("Battle.cro", null);
            if (bp == null || !File.Exists(bp)) return;
            byte[] cro = File.ReadAllBytes(bp);
            
            // Apply the "Move Alternate Animations stock replacement" logic
            // This shunts moves > 800 to animation slots 1237+
            uint[] patch = {
                0xE92D003C, 0xE2410C01, 0xE3500022, 0x1A000003, 0xE3520000, 0x01A00001, 0x11A00002, 0xEA000018,
                0xE3520001, 0x2A000004, 0xE3510E32, 0x9A000014, 0xE59F0060, 0xE0800081, 0xEA000014, 0x059F3058,
                0xE3A05041, 0x0A000006, 0xE3520003, 0x359F304C, 0x33A05005, 0x059F3048, 0x03A05002, 0x859F3044,
                0x83A05002, 0xE3A04000, 0xE0832104, 0xE1D200B0, 0xE1500001, 0x0A000004, 0xE2844001, 0xE1540005,
                0x3AFFFFF8, 0xE1A00001, 0xEA000000, 0xE1D200B2, 0xE8BD003C, 0xE12FFF1E, 0x000001B4
            };
            
            int offset = 0x00077628;
            if (offset + (patch.Length * 4) <= cro.Length) {
                for (int i = 0; i < patch.Length; i++)
                    Array.Copy(BitConverter.GetBytes(patch[i]), 0, cro, offset + (i * 4), 4);
                
                CROUtil.UpdateHashes(cro);
                File.WriteAllBytes(bp, cro);
                LogDeployment("Applied animation redirection patch to Battle.cro.");
            }
        }

        private void LogDeployment(string m) { if (rtbAssembly.InvokeRequired) rtbAssembly.Invoke(new Action(() => LogDeployment(m))); else rtbAssembly.AppendText($"[{DateTime.Now:T}] {m}\n"); }
    }
}
