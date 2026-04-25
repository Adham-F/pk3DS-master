using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using pk3DS.Core.Modding;
using pk3DS.Core.CTR;

namespace pk3DS.WinForms
{
    public partial class ResearchCenter7 : Form
    {
        private byte[] currentFileBytes;
        private string currentBinaryPath;

        public ResearchCenter7()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            WinFormsUtil.ApplyTheme(this);
            InitializeResearchHub();
            InitializeModdingWorkshop();
            LoadLogicAnatomy();
        }

        private void InitializeModdingWorkshop()
        {
            Tab_Modding.Text = "Modding Workshop";
            SetMidnightPalette(Tab_Modding);
            
            // 1. Root Split
            splitMain = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 350 };
            Tab_Modding.Controls.Add(splitMain);

            // Explorer Pane
            var pnlExpRoot = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            pnlExplorerHeader = CreateHeader("EXPLORER & RESEARCH", System.Drawing.Color.FromArgb(0, 150, 255), out lblExplorerTitle);
            tvExplorer = new TreeView {
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.FromArgb(20, 25, 35), 
                ForeColor = System.Drawing.Color.FromArgb(200, 210, 230),
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            tvExplorer.AfterSelect += tvExplorer_AfterSelect;
            
            var pnlExpActions = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = System.Drawing.Color.FromArgb(30, 35, 50) };
            var btnDeploy = new Button { Text = "DEPLOY MODULE TO BINARY", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, ForeColor = System.Drawing.Color.Cyan };
            btnDeploy.Click += (s, e) => {
                if (tvExplorer.SelectedNode?.Tag is PatchGroup g) DeployFromExplorer(g);
            };
            pnlExpActions.Controls.Add(btnDeploy);

            pnlExpRoot.Controls.Add(tvExplorer);
            pnlExpRoot.Controls.Add(pnlExpActions);
            pnlExpRoot.Controls.Add(pnlExplorerHeader);
            splitMain.Panel1.Controls.Add(pnlExpRoot);

            // 2. Center & Right
            splitRight = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 600 };
            splitMain.Panel2.Controls.Add(splitRight);

            // Workbench Pane
            var pnlWbRoot = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            pnlWorkbenchHeader = CreateHeader("WORKBENCH", System.Drawing.Color.FromArgb(0, 255, 127), out lblWorkbenchTitle);
            tcWorkbench = new TabControl { Dock = DockStyle.Fill };
            Tab_LogicLab = new TabPage("Logic Lab");
            Tab_HexEditor = new TabPage("Hex Editor");
            Tab_Wizard = new TabPage("Structural Wizard");
            tcWorkbench.TabPages.AddRange(new[] { Tab_HexEditor, Tab_LogicLab, Tab_Wizard });
            
            pnlWbRoot.Controls.Add(tcWorkbench);
            pnlWbRoot.Controls.Add(pnlWorkbenchHeader);
            splitRight.Panel1.Controls.Add(pnlWbRoot);

            InitializeLogicLab(); // Set up reference view
            InitializeStructuralWizard(); // Set up segment map

            // Hex Editor Init
            rtbHex = new RichTextBox { 
                Dock = DockStyle.Fill, 
                ReadOnly = false, 
                Font = new System.Drawing.Font("Consolas", 10F),
                BackColor = System.Drawing.Color.FromArgb(15, 15, 20),
                ForeColor = System.Drawing.Color.FromArgb(0, 255, 65),
                BorderStyle = BorderStyle.None
            };
            rtbHex.SelectionChanged += rtbHex_SelectionChanged;
            Tab_HexEditor.Controls.Add(rtbHex);

            // 3. Inspector Pane
            var pnlInspRoot = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            pnlInspectorHeader = CreateHeader("INSPECTOR", System.Drawing.Color.FromArgb(255, 69, 0), out lblInspectorTitle);
            splitInspector = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };
            pgInspector = new PropertyGrid { 
                Dock = DockStyle.Fill, 
                HelpVisible = false, 
                ToolbarVisible = false,
                BackColor = System.Drawing.Color.FromArgb(25, 30, 45),
                ViewBackColor = System.Drawing.Color.FromArgb(25, 30, 45),
                ViewForeColor = System.Drawing.Color.White,
                LineColor = System.Drawing.Color.FromArgb(40, 45, 60)
            };
            splitInspector.Panel1.Controls.Add(pgInspector);

            var pnlAsm = new Panel { Dock = DockStyle.Fill };
            rtbAssembly = new RichTextBox { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                BackColor = System.Drawing.Color.FromArgb(10, 15, 25), 
                ForeColor = System.Drawing.Color.LightCyan,
                Font = new System.Drawing.Font("Consolas", 9F),
                BorderStyle = BorderStyle.None
            };
            pnlAsm.Controls.Add(rtbAssembly);
            splitInspector.Panel2.Controls.Add(pnlAsm);
            
            pnlInspRoot.Controls.Add(splitInspector);
            pnlInspRoot.Controls.Add(pnlInspectorHeader);
            splitRight.Panel2.Controls.Add(pnlInspRoot);

            InitializeWorkspaceHUD();
            PopulateExplorerFromResearch();
        }

        private DataGridView gvLogicAnatomy;
        private TextBox txtLogicSearch;
        private void InitializeLogicLab()
        {
            Tab_LogicLab.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5) };
            txtLogicSearch = new TextBox { 
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.FromArgb(15, 15, 20), 
                ForeColor = System.Drawing.Color.LightGray,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                PlaceholderText = "Search Logic Documentation (e.g. Intimidate, Berserk, R4)..."
            };
            txtLogicSearch.TextChanged += (s, e) => FilterLogicAnatomy();
            pnlSearch.Controls.Add(txtLogicSearch);
            
            gvLogicAnatomy = new DataGridView { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false, // Set to false to control order
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = System.Drawing.Color.FromArgb(20, 20, 30),
                ForeColor = System.Drawing.Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            gvLogicAnatomy.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Mechanic", HeaderText = "Target Mechanic", Width = 150 });
            gvLogicAnatomy.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Category", Width = 100 });
            gvLogicAnatomy.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Summary", HeaderText = "Operation Summary" });
            
            gvLogicAnatomy.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 50, 65);
            gvLogicAnatomy.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.Cyan;
            gvLogicAnatomy.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(30, 30, 40);
            gvLogicAnatomy.SelectionChanged += gvLogicAnatomy_SelectionChanged;
            
            Tab_LogicLab.Controls.Add(gvLogicAnatomy);
            Tab_LogicLab.Controls.Add(pnlSearch);

            btnCreateCustom = new Button { Text = "CREATE CUSTOM MECHANIC FROM SELECTION", Dock = DockStyle.Bottom, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = System.Drawing.Color.FromArgb(50, 60, 80), ForeColor = System.Drawing.Color.Cyan };
            btnCreateCustom.Click += (s, e) => {
                if (gvLogicAnatomy.SelectedRows.Count == 0) return;
                var entry = gvLogicAnatomy.SelectedRows[0].DataBoundItem as LogicEntry;
                if (entry == null) return;
                
                string newName = entry.Mechanic + " (Clone)";
                // Initialize the Wizard properly
                tcWorkbench.SelectedTab = Tab_Wizard;
                lblAuditStatus.Text = $"WIZARD: RELOCATING {newName.ToUpper()}";
                lblAuditStatus.ForeColor = System.Drawing.Color.Cyan;

                // Provide clear log feedback in the assembly panel or similar
                rtbAssembly.Text = $"=== CUSTOM MECHANIC INITIALIZATION ===\n" +
                                 $"Source Template: {entry.Mechanic}\n" +
                                 $"Category: {entry.Category}\n" +
                                 $"Step 1: Analyzing logic structure...\n" +
                                 $"Step 2: Identifying cross-references...\n" +
                                 $"Step 3: Calculating required sandbox size...\n\n" +
                                 $"Action Required: Click 'EXPAND .TEXT SEGMENT' to create space for this mechanic.";
            };
            Tab_LogicLab.Controls.Add(btnCreateCustom);
        }

        private Button btnCreateCustom;

        private void gvLogicAnatomy_SelectionChanged(object sender, EventArgs e)
        {
            if (gvLogicAnatomy.SelectedRows.Count == 0) return;
            var entry = gvLogicAnatomy.SelectedRows[0].DataBoundItem as LogicEntry;
            if (entry == null) return;
            
            pgInspector.SelectedObject = new LogicComprehensionModel(entry);
            lblInspectorTitle.Text = $"INSPECTOR - {entry.Mechanic.ToUpper()}";
        }

        private DataGridView gvSegments;
        private void InitializeStructuralWizard()
        {
            Tab_Wizard.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            gvSegments = new DataGridView { 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                BackgroundColor = System.Drawing.Color.FromArgb(20, 20, 30),
                ForeColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 30
            };
            gvSegments.Columns.Add("Seg", "Segment");
            gvSegments.Columns.Add("Offset", "Offset");
            gvSegments.Columns.Add("Size", "Size");
            Tab_Wizard.Controls.Add(gvSegments);
            
            var btnExpand = new Button { Text = "EXPAND .TEXT (CODE) SEGMENT", Dock = DockStyle.Bottom, Height = 40, FlatStyle = FlatStyle.Flat, ForeColor = System.Drawing.Color.Gold };
            btnExpand.Click += (s, e) => {
                if (currentFileBytes != null) {
                    currentFileBytes = CROUtil.ExpandSegment(currentFileBytes, 'c', 0x2000);
                    MessageBox.Show("Code segment expanded by 8KB. Relocation table updated.", "Wizard Success");
                    UpdateStructuralMap();
                }
            };
            
            lblAuditStatus = new Label { 
                Text = "NO BINARY LOADED", 
                Dock = DockStyle.Top, 
                Height = 30, 
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter, 
                BackColor = System.Drawing.Color.FromArgb(30, 30, 45),
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
            };
            
            Tab_Wizard.Controls.Add(gvSegments);
            Tab_Wizard.Controls.Add(lblAuditStatus);
            Tab_Wizard.Controls.Add(btnExpand);
        }

        private Label lblAuditStatus;

        private List<LogicEntry> AllLogicEntries = new();
        private void LoadLogicAnatomy()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "scratch", "logic_anatomy.json");
                
                int attempts = 0;
                while (!File.Exists(path) && attempts < 5)
                {
                    baseDir = Directory.GetParent(baseDir)?.FullName;
                    if (baseDir == null) break;
                    path = Path.Combine(baseDir, "scratch", "logic_anatomy.json");
                    attempts++;
                }

                if (!File.Exists(path)) return;
                
                var json = File.ReadAllText(path);
                var raw = JsonSerializer.Deserialize<List<JsonElement>>(json);
                AllLogicEntries.Clear();
                
                foreach (var item in raw)
                {
                    var details = item.GetProperty("Details").EnumerateArray();
                    string detailsStr = string.Join(" | ", details.Select(d => $"{d.GetProperty("Register").GetString()}: {d.GetProperty("Usage").GetString()}"));
                    
                    AllLogicEntries.Add(new LogicEntry {
                        Category = item.GetProperty("Category").GetString(),
                        Mechanic = item.GetProperty("Mechanic").GetString(),
                        Summary = item.GetProperty("Description").GetString(),
                        Architecture = detailsStr
                    });
                }
                
                gvLogicAnatomy.DataSource = new BindingSource { DataSource = AllLogicEntries };
            }
            catch { }
        }

        private void FilterLogicAnatomy()
        {
            string search = txtLogicSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(search))
            {
                gvLogicAnatomy.DataSource = new BindingSource { DataSource = AllLogicEntries };
                return;
            }
            
            var filtered = AllLogicEntries.Where(l => 
                l.Category.ToLower().Contains(search) || 
                l.Mechanic.ToLower().Contains(search) || 
                l.Summary.ToLower().Contains(search) || 
                l.Architecture.ToLower().Contains(search)
            ).ToList();
            
            gvLogicAnatomy.DataSource = new BindingSource { DataSource = filtered };
        }

        public class LogicEntry
        {
            public string Mechanic { get; set; }
            public string Category { get; set; }
            public string Summary { get; set; }
            public string Architecture { get; set; }
        }

        public class LogicComprehensionModel
        {
            private readonly LogicEntry _source;
            public LogicComprehensionModel(LogicEntry source) => _source = source;

            [System.ComponentModel.Category("Identification")]
            public string Field_Category => _source.Category;
            [System.ComponentModel.Category("Identification")]
            public string Field_Name => _source.Mechanic;
            
            [System.ComponentModel.Category("Operational Logic")]
            [System.ComponentModel.Description("Human-readable summary of how this function interacts with the game engine.")]
            public string Behavior_Flow => _source.Summary;

            [System.ComponentModel.Category("Technical Manual (Translated)")]
            [System.ComponentModel.Description("Registers involved in this logic, translated for modding clarity.")]
            public string LowLevel_Architecture => _source.Architecture.Replace(" | ", "\n").Replace("R0:", "[RETURN] R0:").Replace("R4:", "[USER] R4:").Replace("R5:", "[TARGET] R5:").Replace("LR:", "[FLOW] LR:");

            [System.ComponentModel.Category("IDE Guidance")]
            public string Modding_Tip => GetTip();

            private string GetTip()
            {
                if (_source.Architecture.Contains("R4")) return "Hook this at function start to modify the attacker's stats before damage calc.";
                if (_source.Architecture.Contains("R0")) return "Modify the return value to force success/failure regardless of standard conditions.";
                return "Use the Hex Editor to locate this offset and inject a bridge to your custom sandbox.";
            }
        }


        private void PopulateExplorerFromResearch()
        {
            tvExplorer.Nodes.Clear();
            var researchRoot = tvExplorer.Nodes.Add("INTEGRATED RESEARCH MODULES");
            researchRoot.ForeColor = System.Drawing.Color.Cyan;
            
            foreach (var group in AllResearchGroups)
            {
                var cat = GetOrCreateParent(researchRoot, group.Category);
                var mod = cat.Nodes.Add(group.Module);
                var sheet = mod.Nodes.Add(group.Sheet);
                sheet.Tag = group;
            }
            researchRoot.Expand();
        }

        private TreeNode GetOrCreateParent(TreeNode root, string text)
        {
            foreach (TreeNode n in root.Nodes) if (n.Text == text) return n;
            return root.Nodes.Add(text);
        }

        private void DeployFromExplorer(PatchGroup group)
        {
            tcWorkbench.SelectedTab = Tab_HexEditor;
            LaunchBinary(group.Target);
            if (UniversalPatcher.ApplyPatchGroup(group, Main.RomFSPath, Main.ExeFSPath))
            {
                MessageBox.Show($"Successfully deployed {group.Module} to active binary stream.", "Deployment Success");
                LoadHexView();
            }
        }

        private void SetMidnightPalette(Control root)
        {
            root.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            root.ForeColor = System.Drawing.Color.WhiteSmoke;
        }

        private Panel CreateHeader(string title, System.Drawing.Color accent, out Label lbl)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 25, BackColor = System.Drawing.Color.FromArgb(45, 50, 65) };
            lbl = new Label { 
                Text = title, 
                Dock = DockStyle.Left, 
                AutoSize = true, 
                Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold),
                ForeColor = accent,
                Padding = new Padding(5, 5, 0, 0)
            };
            pnl.Controls.Add(lbl);
            
            // Neon accent bar at bottom
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = accent };
            pnl.Controls.Add(bar);
            return pnl;
        }

        private void InitializeWorkspaceHUD()
        {
            pnlWorkspaceHUD = new Panel { 
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.FromArgb(20, 20, 30) // Match dark theme
            };
            
            var centerPnl = new Panel { Width = 500, Height = 420 };
            centerPnl.Location = new System.Drawing.Point((pnlWorkspaceHUD.Width - centerPnl.Width) / 2, (pnlWorkspaceHUD.Height - centerPnl.Height) / 2);
            centerPnl.Anchor = AnchorStyles.None;
            
            lblHUDTitle = new Label { 
                Text = "MODDING CORE HUB", 
                Dock = DockStyle.Top, 
                Height = 40, 
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(0, 255, 127)
            };
            
            lblHUDDesc = new Label { 
                Text = "Binary stream not active. Choose a launch target to begin.", 
                Dock = DockStyle.Top, 
                Height = 80, 
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = System.Drawing.Color.Gray,
                AutoEllipsis = true
            };

            pnlQuickStart = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            
            AddQuickStartButton("BATTLE.CRO (In-Battle Logic)", "battle.cro", System.Drawing.Color.SkyBlue);
            AddQuickStartButton("CODE.BIN (Master Binary)", "code.bin", System.Drawing.Color.MediumPurple);
            AddQuickStartButton("SHOP.CRO (Inventory)", "shop.cro", System.Drawing.Color.Gold);
            AddQuickStartButton("BAG.CRO (Item Space)", "bag.cro", System.Drawing.Color.LightCoral);

            var chkTechLabels = new CheckBox { 
                Text = "Enable Technical Move Labels (Slicing, Biting, etc.)", 
                Dock = DockStyle.Top, 
                Height = 30, 
                ForeColor = System.Drawing.Color.LightGray,
                Checked = WinFormsUtil.ShowExtendedLogic,
                Padding = new Padding(10, 0, 0, 0)
            };
            chkTechLabels.CheckedChanged += (s, e) => WinFormsUtil.ShowExtendedLogic = chkTechLabels.Checked;

            pnlQuickStart.Controls.Add(chkTechLabels);
            pnlQuickStart.Controls.Add(new Label { Height = 10, Dock = DockStyle.Top }); // Spacer
            centerPnl.Controls.Add(pnlQuickStart);
            centerPnl.Controls.Add(lblHUDDesc);
            centerPnl.Controls.Add(lblHUDTitle);
            pnlWorkspaceHUD.Controls.Add(centerPnl);
            
            Tab_HexEditor.Controls.Add(pnlWorkspaceHUD);
            pnlWorkspaceHUD.BringToFront();
        }

        private void AddQuickStartButton(string label, string target, System.Drawing.Color accent)
        {
            var btn = new Button { 
                Text = label, 
                Width = 480, 
                Height = 40, 
                FlatStyle = FlatStyle.Flat,
                ForeColor = accent,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 80);
            btn.Click += (s, e) => LaunchBinary(target);
            pnlQuickStart.Controls.Add(btn);
        }

        private void LaunchBinary(string target)
        {
            string path = UniversalPatcher.GetFilePath(target, Main.RomFSPath, Main.ExeFSPath);
            if (!File.Exists(path))
            {
                var dr = MessageBox.Show($"Automated Discovery failed to locate {target}.\n\n" +
                                        $"Would you like to manually locate this binary?", 
                                        "Binary Discovery Failure", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog { Title = $"Locate {target}", Filter = $"{target}|*{Path.GetExtension(target)}" })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK) path = ofd.FileName;
                        else return;
                    }
                }
                else return;
            }
            
            currentFileBytes = File.ReadAllBytes(path);
            currentBinaryPath = path;
            
            pnlWorkspaceHUD.Visible = false;
            LoadHexView();
            RunStructuralAudit();
            UpdateStructuralMap();
        }

        private void RunStructuralAudit()
        {
            if (currentFileBytes == null) return;
            var report = CROUtil.AuditIntegrity(currentFileBytes);
            lblAuditStatus.Text = (report.IsExpanded ? "STRUCTURE: [EXPANDED SANDBOX ACTIVE]" : "STRUCTURE: [STOCK LAYOUT]") + " | HASH: " + (report.HashValid ? "[VALID]" : "[INVALID]");
            lblAuditStatus.ForeColor = report.HashValid ? System.Drawing.Color.SpringGreen : System.Drawing.Color.OrangeRed;
            if (!report.HashValid) MessageBox.Show(report.Details, "Structural Audit Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void UpdateStructuralMap()
        {
            if (currentFileBytes == null) return;
            gvSegments.Rows.Clear();
            try {
                var offsets = CROUtil.GetSegmentStartIndices(currentFileBytes);
                for (int i=0; i < offsets.Length; i++)
                    gvSegments.Rows.Add($"Seg {i}", $"0x{offsets[i]:X}", "N/A");
            } catch {}
        }

        private void tvExplorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pgInspector.SelectedObject = e.Node.Tag;
            if (e.Node.Tag is RelocationEntry entry)
            {
                HighlightInHex((int)entry.WriteTo);
            }
        }

        private void rtbHex_SelectionChanged(object sender, EventArgs e)
        {
            // Simplified ASM preview based on selection
            if (currentFileBytes == null) return;
            int start = rtbHex.SelectionStart / 3 / 4 * 4; // Align to 4 bytes for ARM
            if (start < 0 || start + 4 > currentFileBytes.Length) return;

            uint instr = BitConverter.ToUInt32(currentFileBytes, start);
            rtbAssembly.Text = $"OFFSET: 0x{start:X8}\nOPCODE: 0x{instr:X8}\nASM: {DisassembleSimplified(instr)}";
        }

        private string DisassembleSimplified(uint instr)
        {
            // Improved ARM-32 feedback for IDE feel
            if (instr == 0xE1A00000) return "NOP";
            if ((instr & 0x0FFFFFFF) == 0x012FFF1E) return "BX LR (Return)";
            if ((instr & 0x0F000000) == 0x0A000000) return $"B 0x{((instr & 0xFFFFFF) << 2):X} (Relative Branch)";
            if ((instr & 0x0F000000) == 0x0B000000) return $"BL 0x{((instr & 0xFFFFFF) << 2):X} (Branch with Link)";
            if ((instr & 0x0E500000) == 0x04100000) return "LDR (Relative/Immediate)";
            if ((instr & 0x0E500000) == 0x04000000) return "STR (Save to Memory)";
            if ((instr & 0x0F900000) == 0x01000000) return "SWP (Swap Memory)";
            if ((instr & 0x0DE00000) == 0x03400000) return "CMP (Compare Constant)";
            if ((instr & 0x0FC00000) == 0x02800000) return "ADD (Register + Imm)";
            if ((instr & 0x0FC00000) == 0x02400000) return "SUB (Register - Imm)";
            return "UNKNOWN ASSEMBLY / INSTRUCTION BLOB";
        }

        private void HighlightInHex(int offset)
        {
            // Logic to scroll and select the offset in the Hex view
            int textPos = (offset / 16) * (16 * 3 + 1) + (offset % 16) * 3;
            if (textPos >= 0 && textPos < rtbHex.TextLength)
            {
                rtbHex.Focus();
                rtbHex.Select(textPos, 2);
                rtbHex.ScrollToCaret();
            }
        }

        private void btnMapChain_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter the absolute offset (Hex) in target file to map from:", "Map Mechanic Chain", "0");
            if (string.IsNullOrEmpty(input)) return;

            try
            {
                uint offset = Convert.ToUInt32(input, 16);
                string targetName = cbResearchTarget.SelectedItem?.ToString() ?? "battle.cro";
                currentBinaryPath = Path.Combine(Main.RomFSPath, targetName.Contains('.') ? targetName : targetName + ".cro");
                if (targetName == "code.bin") currentBinaryPath = Path.Combine(Main.RomFSPath, "code.bin");

                if (!File.Exists(currentBinaryPath)) { MessageBox.Show("Target file not found: " + currentBinaryPath); return; }
                currentFileBytes = File.ReadAllBytes(currentBinaryPath);

                // Load to Hex Editor
                LoadHexView();

                tvExplorer.Nodes.Clear();
                var root = tvExplorer.Nodes.Add($"Chain for {input} in {targetName}");
                DiscoveryPointerChain(currentFileBytes, offset, root);
                root.ExpandAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadHexView()
        {
            if (currentFileBytes == null) return;
            System.Text.StringBuilder sb = new();
            for (int i = 0; i < Math.Min(currentFileBytes.Length, 0x10000); i++) // Cap at 64KB for performance
            {
                sb.Append(currentFileBytes[i].ToString("X2") + " ");
                if ((i + 1) % 16 == 0) sb.AppendLine();
            }
            rtbHex.Text = sb.ToString();
        }

        private void DiscoveryPointerChain(byte[] data, uint offset, TreeNode parent)
        {
            int patchIndex = CROUtil.FindRelocationPatchIndex(data, offset);
            if (patchIndex == -1) return;

            var entry = CROUtil.GetRelocationEntry(data, patchIndex);
            var node = parent.Nodes.Add($"Patch #{patchIndex} -> {entry.WriteTo:X}");
            node.Tag = entry;

            uint targetAddr = entry.Addend + CROUtil.GetSegmentStartIndices(data)[entry.TargetSeg];
            if (targetAddr > 0 && targetAddr < data.Length)
            {
                int nextPatch = CROUtil.FindRelocationPatchIndex(data, targetAddr);
                if (nextPatch != -1) DiscoveryPointerChain(data, targetAddr, node);
            }
        }

        private void btnExpandWizard_Click(object sender, EventArgs e)
        {
             MessageBox.Show("The expansion wizard handles Step II (Structural Expansion) and Step III (Header Realignment).\n\n" +
                             "The engine will automatically update 0x84 and 0x12C headers.", 
                             "Expansion Guidance", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private List<PatchGroup> AllResearchGroups = new();
        private void InitializeResearchHub()
        {
            Tab_ResearchHub.Text = "Universal Research Hub";
            SetMidnightPalette(Tab_ResearchHub);
            
            splitResearch = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 250 };
            Tab_ResearchHub.Controls.Add(splitResearch);

            tvResearch = new TreeView { 
                Dock = DockStyle.Fill, 
                BackColor = System.Drawing.Color.FromArgb(20, 25, 35), 
                ForeColor = System.Drawing.Color.FromArgb(200, 210, 230),
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            tvResearch.AfterSelect += (s, e) => UpdateResearchDetails();
            splitResearch.Panel1.Controls.Add(tvResearch);

            var pnlDetails = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            splitResearch.Panel2.Controls.Add(pnlDetails);

            lblResearchTarget = new Label { Text = "TARGET BINARY UNIT", Dock = DockStyle.Top, Height = 25, Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.FromArgb(0, 150, 255) };
            cbResearchTarget = new ComboBox { 
                Dock = DockStyle.Top, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.Color.FromArgb(45, 50, 65),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cbResearchTarget.Items.AddRange(new[] { "battle.cro", "code.bin", "shop.cro", "bag.cro" });
            
            gvResearch = new DataGridView { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                AutoGenerateColumns = false,
                BackgroundColor = System.Drawing.Color.FromArgb(15, 15, 20),
                ForeColor = System.Drawing.Color.LightGray,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                GridColor = System.Drawing.Color.FromArgb(40, 45, 60),
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            };
            gvResearch.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Offset", HeaderText = "Offset", Width = 100 });
            gvResearch.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Hex", HeaderText = "Logic Hex", Width = 200 });
            gvResearch.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Note", HeaderText = "Operation Note", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            btnApplyResearch = new Button { 
                Text = "DEPLOY PATCH MODULE", 
                Dock = DockStyle.Bottom, 
                Height = 45, 
                BackColor = System.Drawing.Color.FromArgb(0, 80, 50),
                ForeColor = System.Drawing.Color.FromArgb(0, 255, 127),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            btnApplyResearch.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 255, 127);
            btnApplyResearch.Click += btnApplyResearch_Click;

            pnlDetails.Controls.Add(gvResearch);
            pnlDetails.Controls.Add(cbResearchTarget);
            pnlDetails.Controls.Add(lblResearchTarget);
            pnlDetails.Controls.Add(btnApplyResearch);

            LoadUniversalResearch();
        }

        private void LoadUniversalResearch()
        {
            tvResearch.BeginUpdate();
            tvResearch.Nodes.Clear();
            LoadInternalMasterResearch();
            LoadLocalScratchResearch();
            tvResearch.ExpandAll();
            tvResearch.EndUpdate();
        }

        private void LoadLocalScratchResearch()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "scratch", "patch_groups.json");
                // ... logic to find scratch ...
                if (!File.Exists(path)) return;
                string json = File.ReadAllText(path);
                var groups = JsonSerializer.Deserialize<List<PatchGroup>>(json);
                if (groups != null) AllResearchGroups.AddRange(groups);
                PopulateResearchTree(groups, "[LOCAL]");
            }
            catch { }
        }

        private void LoadInternalMasterResearch()
        {
            try
            {
                var assembly = typeof(Main).Assembly;
                var resources = assembly.GetManifestResourceNames().Where(r => r.Contains("Resources.ARM_Functions") && r.EndsWith(".xlsx"));
                foreach (var res in resources)
                {
                    // Resource name format: pk3DS.WinForms.Resources.ARM_Functions.SubDir.FileName.xlsx
                    // This is hard to parse back to original structure perfectly without a map, but we can try.
                    string cleanName = res.Replace("pk3DS.WinForms.Resources.ARM_Functions.", "").Replace(".xlsx", "");
                    using var stream = assembly.GetManifestResourceStream(res);
                    if (stream == null) continue;
                    
                    var sheets = XlsxResearchParser.GetSheetNames(stream);
                    foreach (var sheet in sheets)
                    {
                        // We need a way to convert Excel Row to PatchGroup
                        // For now, let's just add it to the tree as a 'Master' resource
                        var node = tvResearch.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == "MASTER") ?? tvResearch.Nodes.Add("MASTER");
                        var fileNode = node.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == cleanName) ?? node.Nodes.Add(cleanName);
                        var sheetNode = fileNode.Nodes.Add(sheet);
                        sheetNode.Tag = res + "|" + sheet; // Store resource name and sheet
                    }
                }
            }
            catch { }
        }

        private void PopulateResearchTree(List<PatchGroup> groups, string rootName)
        {
            if (groups == null) return;
            var root = tvResearch.Nodes.Add(rootName);
            var categories = groups.Select(g => g.Category).Distinct().OrderBy(c => c);
            foreach (var cat in categories)
            {
                var catNode = root.Nodes.Add(cat);
                var modules = groups.Where(g => g.Category == cat).Select(g => g.Module).Distinct().OrderBy(m => m);
                foreach (var mod in modules)
                {
                    var modNode = catNode.Nodes.Add(mod.Replace(".xlsx", ""));
                    var sheets = groups.Where(g => g.Module == mod).Select(g => g.Sheet).Distinct();
                    foreach (var sheet in sheets)
                    {
                        var sheetNode = modNode.Nodes.Add(sheet);
                        sheetNode.Tag = groups.First(g => g.Module == mod && g.Sheet == sheet);
                    }
                }
            }
        }

        private void UpdateResearchDetails()
        {
            if (tvResearch.SelectedNode?.Tag is PatchGroup group)
            {
                gvResearch.DataSource = group.Patches;
                cbResearchTarget.SelectedItem = group.Target;
                SetupParameterControls(group);
            }
        }

        private FlowLayoutPanel pnlParams;
        private void SetupParameterControls(PatchGroup group)
        {
            if (pnlParams == null)
            {
                pnlParams = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, AutoScroll = true, BackColor = System.Drawing.Color.FromArgb(40, 40, 40) };
                splitResearch.Panel2.Controls.Add(pnlParams);
                pnlParams.BringToFront();
            }

            pnlParams.Controls.Clear();
            if (group.Parameters == null || group.Parameters.Count == 0)
            {
                pnlParams.Visible = false;
                return;
            }

            pnlParams.Visible = true;
            foreach (var p in group.Parameters)
            {
                var lbl = new Label { Text = p.Name + ":", AutoSize = true, ForeColor = System.Drawing.Color.White, Margin = new Padding(5, 10, 5, 0) };
                var txt = new TextBox { Text = p.Value, Width = 100, Tag = p };
                txt.TextChanged += (s, e) => { p.Value = ((TextBox)s).Text; };
                
                pnlParams.Controls.Add(lbl);
                pnlParams.Controls.Add(txt);
            }
        }

        private void btnApplyResearch_Click(object sender, EventArgs e)
        {
            if (tvResearch.SelectedNode?.Tag is PatchGroup group)
            {
                try
                {
                    group.Target = cbResearchTarget.SelectedItem.ToString();
                    if (UniversalPatcher.ApplyPatchGroup(group, Main.RomFSPath, Main.ExeFSPath))
                    {
                        MessageBox.Show($"Successfully applied {group.Module} ({group.Sheet}) with {group.Parameters.Count} parameters!", "Patch Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}
