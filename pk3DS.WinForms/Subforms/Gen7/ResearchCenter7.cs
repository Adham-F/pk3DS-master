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

        private void InitializeKeystone() { try { keystone = new Engine(Architecture.ARM, Mode.ARM); } catch { } }
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
            
            this.txtHexOut.Dock = DockStyle.Fill;
            this.txtHexOut.BackColor = Color.FromArgb(10, 10, 15);
            this.txtHexOut.ForeColor = Color.SpringGreen;
            this.txtHexOut.Font = new Font("Consolas", 11F);
            this.txtHexOut.BorderStyle = BorderStyle.None;
            
            split.Panel1.Controls.Add(this.txtAsmIn);
            split.Panel2.Controls.Add(this.txtHexOut);
            
            this.txtAsmIn.TextChanged += (s, e) => UpdateAsmToHex();
            Tab_ARM.ResumeLayout(true);
        }

        private void UpdateAsmToHex()
        {
            if (keystone == null) return;
            var sb = new StringBuilder();
            foreach (var l in txtAsmIn.Lines) { if (string.IsNullOrWhiteSpace(l)) continue; var r = keystone.Assemble(l, 0); if (r != null) foreach (byte b in r.Buffer) sb.Append(b.ToString("X2") + " "); sb.AppendLine(); }
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

        private void UpdateHexViewFromSelection()
        {
            if (string.IsNullOrEmpty(selectedXlsxPath)) return;
            string target = "Battle.cro";
            var meta = XlsxResearchParser.ReadSheet(selectedXlsxPath, "Table Locations and Sizes");
            if (meta.Count > 0 && meta.Any(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase))))
            {
                var row = meta.First(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase)));
                target = row.Values.First();
            }
            else if (actualXlsxSourcePath.ToLower().Contains("code")) target = "code.bin";

            string path = target.ToLower().Contains("code") ? (File.Exists(Path.Combine(Main.ExeFSPath, "code.bin")) ? Path.Combine(Main.ExeFSPath, "code.bin") : Path.Combine(Main.ExeFSPath, ".code.bin")) : (File.Exists(Path.Combine(Main.RomFSPath, target)) ? Path.Combine(Main.RomFSPath, target) : Path.Combine(Main.RomFSPath, "battle", target));
            if (File.Exists(path))
            {
                currentFileBytes = File.ReadAllBytes(path);
                var highlights = new List<long>(); uint jump = 0; bool first = true;
                foreach (var r in selectedResearchData) {
                    if (!r.ContainsKey("Offset")) continue;
                    if (!uint.TryParse(r["Offset"].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out uint off)) continue;
                    if (first) jump = off;
                    int len = r.ContainsKey("Hex") ? Util.StringToByteArray(r["Hex"]).Length : 4;
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
                string target = "Battle.cro"; 
                var meta = XlsxResearchParser.ReadSheet(path, "Table Locations and Sizes");
                if (meta.Count > 0 && meta.Any(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase))))
                {
                    var row = meta.First(r => r.Keys.Any(k => k.Equals("File", StringComparison.OrdinalIgnoreCase)));
                    target = row.Values.First();
                }
                else if (path.ToLower().Contains("code") || actualXlsxSourcePath.ToLower().Contains("code")) 
                {
                    target = "code.bin";
                }

                string full = target.ToLower().Contains("code") ? (File.Exists(Path.Combine(Main.ExeFSPath, "code.bin")) ? Path.Combine(Main.ExeFSPath, "code.bin") : Path.Combine(Main.ExeFSPath, ".code.bin")) : (File.Exists(Path.Combine(Main.RomFSPath, target)) ? Path.Combine(Main.RomFSPath, target) : Path.Combine(Main.RomFSPath, "battle", target));
                
                if (!File.Exists(full)) {
                    LogDeployment($"Target Not Found: {full}");
                    return;
                }

                LogDeployment($"Targeting: {Path.GetFileName(full)}");
                byte[] bin = File.ReadAllBytes(full);
                bool modified = false;

                foreach (var r in meta) {
                    if (r.Keys.Any(k => k.Equals("Expansion", StringComparison.OrdinalIgnoreCase)) && int.TryParse(r.Values.First(), out int s)) {
                        LogDeployment($"Expanding segment by {s} bytes...");
                        bin = CROUtil.ExpandSegment(bin, 'c', s);
                        modified = true;
                    }
                }

                int count = 0; 
                var sheets = XlsxResearchParser.GetSheetNames(path);
                foreach (var s in sheets) {
                    if (s.Equals("Table Locations and Sizes", StringComparison.OrdinalIgnoreCase) || s.Equals("Metadata", StringComparison.OrdinalIgnoreCase)) continue;
                    
                    var rows = XlsxResearchParser.ReadSheet(path, s);
                    if (rows.Count == 0) continue;
                    
                    // Log detected columns for the first row to help debugging
                    var firstRow = rows[0];
                    LogDeployment($"Sheet '{s}' columns: {string.Join(", ", firstRow.Keys)}");

                    foreach (var r in rows) {
                        string offKey = r.Keys.FirstOrDefault(k => k.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0);
                        string hexKey = r.Keys.FirstOrDefault(k => k.IndexOf("Hex", StringComparison.OrdinalIgnoreCase) >= 0);
                        
                        if (offKey == null || hexKey == null) continue;

                        string offStr = r[offKey].Replace("0x", "").Trim();
                        if (string.IsNullOrEmpty(offStr) || !uint.TryParse(offStr, System.Globalization.NumberStyles.HexNumber, null, out uint off)) continue;
                        
                        string hexStr = r[hexKey].Trim();
                        if (string.IsNullOrEmpty(hexStr)) continue;

                        byte[] p = Util.StringToByteArray(hexStr);
                        if (p == null || p.Length == 0) continue;

                        if (numTargetOverride.Value > 0 && target.ToLower().Contains("battle.cro")) {
                            int b = ResearchEngine.GetRelocationTableBase(bin, detectedTypeForOverride);
                            if (b != -1) off = (uint)(b + ((int)numTargetOverride.Value * 4));
                        }

                        if (off + p.Length <= bin.Length) { 
                            Array.Copy(p, 0, bin, (int)off, p.Length); 
                            count += p.Length; 
                        } else {
                            LogDeployment($"Skip Out of Bounds: 0x{off:X} (Size {p.Length})");
                        }
                    }
                }
                if (count > 0 || modified) {
                    File.WriteAllBytes(full, bin);
                    LogDeployment($"Successfully patched {count} bytes to {Path.GetFileName(full)}.");
                    UpdateHexViewFromSelection();
                } else {
                    LogDeployment("No changes applied. Check spreadsheet columns (Offset, Hex).");
                }
            } catch (Exception ex) { LogDeployment($"Critical Error: {ex.Message}"); }
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
            int max = type == "Ability" ? 255 : 1000;
            string res = WinFormsUtil.PromptInput($"Target {type} count:", max.ToString());
            if (!int.TryParse(res, out int lim)) return;
            try {
                string cp = Path.Combine(Main.ExeFSPath, "code.bin"), bp = File.Exists(Path.Combine(Main.RomFSPath, "Battle.cro")) ? Path.Combine(Main.RomFSPath, "Battle.cro") : Path.Combine(Main.RomFSPath, "battle", "Battle.cro");
                byte[] cd = File.ReadAllBytes(cp); uint old = type switch { "Move" => 737u, "Ability" => 234u, "Item" => 800u, _ => 0 };
                if (ResearchEngine.PatchLimitCheck(cd, old, (uint)lim)) File.WriteAllBytes(cp, cd);
                ResearchEngine.ExpandRelocationTable(bp, type, lim);
                if (type == "Move") ResearchEngine.ExpandGARC(Path.Combine(Main.RomFSPath, "a/0/1/1"), lim, 0x28);
                else if (type == "Item") ResearchEngine.ExpandGARC(Path.Combine(Main.RomFSPath, "a/0/1/9"), lim, 0x30);
                Main.Config.InitializeAll(); LogDeployment($"Expanded {type}s to {lim}.");
            } catch (Exception ex) { LogDeployment($"Error: {ex.Message}"); }
        }

        private void LogDeployment(string m) { if (rtbAssembly.InvokeRequired) rtbAssembly.Invoke(new Action(() => LogDeployment(m))); else rtbAssembly.AppendText($"[{DateTime.Now:T}] {m}\n"); }
    }
}
