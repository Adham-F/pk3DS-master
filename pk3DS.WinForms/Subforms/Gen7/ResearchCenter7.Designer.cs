namespace pk3DS.WinForms
{
    partial class ResearchCenter7
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.TC_Tabs = new System.Windows.Forms.TabControl();
            this.Tab_Modding = new System.Windows.Forms.TabPage();
            this.Tab_ARM = new System.Windows.Forms.TabPage();
            
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.tvExplorer = new System.Windows.Forms.TreeView();
            this.tcWorkbench = new System.Windows.Forms.TabControl();
            this.Tab_HexEditor = new System.Windows.Forms.TabPage();
            this.rtbHex = new System.Windows.Forms.RichTextBox();
            this.dgvResearch = new System.Windows.Forms.DataGridView();
            this.rtbAssembly = new System.Windows.Forms.RichTextBox();
            this.splitInspector = new System.Windows.Forms.SplitContainer();
            this.btnMasterDeploy = new System.Windows.Forms.Button();
            this.txtAsmIn = new System.Windows.Forms.RichTextBox();
            this.txtHexOut = new System.Windows.Forms.RichTextBox();

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            this.TC_Tabs.SuspendLayout();
            this.Tab_Modding.SuspendLayout();
            this.tcWorkbench.SuspendLayout();
            this.Tab_HexEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitInspector)).BeginInit();
            this.splitInspector.Panel1.SuspendLayout();
            this.splitInspector.Panel2.SuspendLayout();
            this.splitInspector.SuspendLayout();
            this.SuspendLayout();

            // TC_Tabs
            this.TC_Tabs.Controls.Add(this.Tab_Modding);
            this.TC_Tabs.Controls.Add(this.Tab_ARM);
            this.TC_Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TC_Tabs.Location = new System.Drawing.Point(0, 0);
            this.TC_Tabs.Name = "TC_Tabs";
            this.TC_Tabs.SelectedIndex = 0;
            this.TC_Tabs.Size = new System.Drawing.Size(1200, 800);

            // Tab_Modding
            this.Tab_Modding.Controls.Add(this.splitMain);
            this.Tab_Modding.Location = new System.Drawing.Point(4, 22);
            this.Tab_Modding.Name = "Tab_Modding";
            this.Tab_Modding.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Modding.Size = new System.Drawing.Size(1192, 774);
            this.Tab_Modding.Text = "Modding Workshop";

            // Tab_ARM
            this.Tab_ARM.Location = new System.Drawing.Point(4, 22);
            this.Tab_ARM.Name = "Tab_ARM";
            this.Tab_ARM.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_ARM.Size = new System.Drawing.Size(1192, 774);
            this.Tab_ARM.Text = "ARM Translator";

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(3, 3);
            this.splitMain.Name = "splitMain";
            this.splitMain.Panel1.Controls.Add(this.tvExplorer);
            this.splitMain.Panel1.Controls.Add(this.btnMasterDeploy);
            this.splitMain.Panel2.Controls.Add(this.splitRight);
            this.splitMain.Size = new System.Drawing.Size(1186, 768);
            this.splitMain.SplitterDistance = 250;

            // tvExplorer
            this.tvExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvExplorer.Location = new System.Drawing.Point(0, 0);
            this.tvExplorer.Name = "tvExplorer";
            this.tvExplorer.Size = new System.Drawing.Size(250, 718);
            this.tvExplorer.BackColor = System.Drawing.Color.FromArgb(30, 30, 40);
            this.tvExplorer.ForeColor = System.Drawing.Color.White;
            this.tvExplorer.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // btnMasterDeploy
            this.btnMasterDeploy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMasterDeploy.Height = 50;
            this.btnMasterDeploy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMasterDeploy.Text = "DEPLOY ALL RESEARCH";
            this.btnMasterDeploy.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnMasterDeploy.ForeColor = System.Drawing.Color.White;

            // Project Expander Panel
            this.pnlExpander = new System.Windows.Forms.Panel();
            this.pnlExpander.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlExpander.Height = 165;
            this.pnlExpander.BackColor = System.Drawing.Color.FromArgb(35, 35, 45);
            this.splitMain.Panel1.Controls.Add(this.pnlExpander);
            this.splitMain.Panel1.Controls.Add(this.btnMasterDeploy);

            this.btnLinkEffect = new System.Windows.Forms.Button();
            this.btnLinkEffect.Text = "Link Effect from Template";
            this.btnLinkEffect.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLinkEffect.Height = 40;
            this.btnLinkEffect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLinkEffect.FlatAppearance.BorderSize = 0;
            this.btnLinkEffect.ForeColor = System.Drawing.Color.Gold;
            this.btnLinkEffect.BackColor = System.Drawing.Color.FromArgb(50, 50, 70);
            this.pnlExpander.Controls.Add(this.btnLinkEffect);

            this.btnExpMoves = new System.Windows.Forms.Button();
            this.btnExpMoves.Text = "Expand Moves up to 1000 slots";
            this.btnExpMoves.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExpMoves.Height = 40;
            this.btnExpMoves.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpMoves.FlatAppearance.BorderSize = 0;
            this.btnExpMoves.ForeColor = System.Drawing.Color.White;
            this.btnExpMoves.BackColor = System.Drawing.Color.FromArgb(45, 45, 60);
            this.pnlExpander.Controls.Add(this.btnExpMoves);

            this.btnExpAbils = new System.Windows.Forms.Button();
            this.btnExpAbils.Text = "Expand Abilities up to 255 slots";
            this.btnExpAbils.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExpAbils.Height = 40;
            this.btnExpAbils.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpAbils.FlatAppearance.BorderSize = 0;
            this.btnExpAbils.ForeColor = System.Drawing.Color.White;
            this.btnExpAbils.BackColor = System.Drawing.Color.FromArgb(40, 40, 55);
            this.pnlExpander.Controls.Add(this.btnExpAbils);

            this.btnExpItems = new System.Windows.Forms.Button();
            this.btnExpItems.Text = "Expand Items up to 1000 Slots";
            this.btnExpItems.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExpItems.Height = 40;
            this.btnExpItems.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpItems.FlatAppearance.BorderSize = 0;
            this.btnExpItems.ForeColor = System.Drawing.Color.White;
            this.btnExpItems.BackColor = System.Drawing.Color.FromArgb(35, 35, 50);
            this.pnlExpander.Controls.Add(this.btnExpItems);
            
            this.btnCustomPatch = new System.Windows.Forms.Button();
            this.btnCustomPatch.Text = "Apply Custom Patches";
            this.btnCustomPatch.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCustomPatch.Height = 40;
            this.btnCustomPatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCustomPatch.FlatAppearance.BorderSize = 0;
            this.btnCustomPatch.ForeColor = System.Drawing.Color.LightGreen;
            this.btnCustomPatch.BackColor = System.Drawing.Color.FromArgb(30, 45, 30);
            this.pnlExpander.Controls.Add(this.btnCustomPatch);
            
            this.btnMasterDeploy.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMasterDeploy.Click += new System.EventHandler(this.B_ExecuteUnified_Click);

            // splitRight
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Location = new System.Drawing.Point(0, 0);
            this.splitRight.Name = "splitRight";
            this.splitRight.Panel1.Controls.Add(this.tcWorkbench);
            this.splitRight.Panel2.Controls.Add(this.splitInspector);
            this.splitRight.Size = new System.Drawing.Size(932, 768);
            this.splitRight.SplitterDistance = 600;

            // tcWorkbench
            this.tcWorkbench.Controls.Add(this.Tab_HexEditor);
            this.tcWorkbench.Dock = System.Windows.Forms.DockStyle.Fill;

            // Tab_HexEditor
            this.Tab_HexEditor.Controls.Add(this.rtbHex);
            this.Tab_HexEditor.Text = "Hex Editor";

            // rtbHex
            this.rtbHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHex.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // splitInspector
            this.splitInspector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitInspector.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitInspector.Panel1.Controls.Add(this.dgvResearch);
            this.splitInspector.Panel2.Controls.Add(this.rtbAssembly);
            this.splitInspector.Size = new System.Drawing.Size(328, 768);
            this.splitInspector.SplitterDistance = 350;

            // dgvResearch
            this.dgvResearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResearch.BackgroundColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.dgvResearch.ForeColor = System.Drawing.Color.White;
            this.dgvResearch.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
            this.dgvResearch.EnableHeadersVisualStyles = false;
            this.dgvResearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvResearch.ReadOnly = true;
            this.dgvResearch.RowHeadersVisible = false;
            this.dgvResearch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            // rtbAssembly
            this.rtbAssembly.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbAssembly.BackColor = System.Drawing.Color.FromArgb(10, 10, 15);
            this.rtbAssembly.ForeColor = System.Drawing.Color.Cyan;
            this.rtbAssembly.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbAssembly.ReadOnly = true;

            // ResearchCenter7
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.TC_Tabs);
            this.Name = "ResearchCenter7";
            this.Text = "Universal Modding IDE - Generation 7";

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            this.splitRight.ResumeLayout(false);
            this.TC_Tabs.ResumeLayout(false);
            this.Tab_Modding.ResumeLayout(false);
            this.tcWorkbench.ResumeLayout(false);
            this.Tab_HexEditor.ResumeLayout(false);
            this.splitInspector.Panel1.ResumeLayout(false);
            this.splitInspector.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitInspector)).EndInit();
            this.splitInspector.ResumeLayout(false);
            this.ResumeLayout(true);
        }

        private System.Windows.Forms.TabControl TC_Tabs;
        private System.Windows.Forms.TabPage Tab_Modding;
        private System.Windows.Forms.TabPage Tab_ARM;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.TreeView tvExplorer;
        private System.Windows.Forms.TabControl tcWorkbench;
        private System.Windows.Forms.TabPage Tab_HexEditor;
        private System.Windows.Forms.RichTextBox rtbHex;
        private System.Windows.Forms.DataGridView dgvResearch;
        private System.Windows.Forms.RichTextBox rtbAssembly;
        private System.Windows.Forms.SplitContainer splitInspector;
        private System.Windows.Forms.Button btnMasterDeploy;
        private System.Windows.Forms.RichTextBox txtAsmIn;
        private System.Windows.Forms.RichTextBox txtHexOut;
        private System.Windows.Forms.Panel pnlExpander;
        private System.Windows.Forms.Button btnExpMoves;
        private System.Windows.Forms.Button btnExpAbils;
        private System.Windows.Forms.Button btnExpItems;
        private System.Windows.Forms.Button btnLinkEffect;
        private System.Windows.Forms.Button btnCustomPatch;
    }
}
