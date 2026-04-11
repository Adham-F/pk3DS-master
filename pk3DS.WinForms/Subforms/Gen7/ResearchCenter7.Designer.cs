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
            this.Tab_ResearchHub = new System.Windows.Forms.TabPage();
            this.Tab_Modding = new System.Windows.Forms.TabPage();
            this.TC_Tabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // TC_Tabs
            // 
            this.TC_Tabs.Controls.Add(this.Tab_ResearchHub);
            this.TC_Tabs.Controls.Add(this.Tab_Modding);
            this.TC_Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TC_Tabs.Location = new System.Drawing.Point(0, 0);
            this.TC_Tabs.Name = "TC_Tabs";
            this.TC_Tabs.SelectedIndex = 0;
            this.TC_Tabs.Size = new System.Drawing.Size(1000, 700);
            this.TC_Tabs.TabIndex = 0;
            // 
            // ResearchCenter7
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.TC_Tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.Name = "ResearchCenter7";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Universal Modding IDE - Generation 7";
            this.TC_Tabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl TC_Tabs;
        private System.Windows.Forms.TabPage Tab_ResearchHub;
        private System.Windows.Forms.TabPage Tab_Modding;

        // Modding Workshop 3-Pane Infrastructure
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.TreeView tvExplorer;
        private System.Windows.Forms.TabControl tcWorkbench;
        private System.Windows.Forms.TabPage Tab_LogicLab;
        private System.Windows.Forms.TabPage Tab_HexEditor;
        private System.Windows.Forms.TabPage Tab_Wizard;
        private System.Windows.Forms.PropertyGrid pgInspector;
        private System.Windows.Forms.RichTextBox rtbAssembly;
        private System.Windows.Forms.RichTextBox rtbHex;
        private System.Windows.Forms.SplitContainer splitInspector;
        
        // Modding HUD & Header Elements
        private System.Windows.Forms.Panel pnlWorkspaceHUD;
        private System.Windows.Forms.Label lblHUDTitle;
        private System.Windows.Forms.Label lblHUDDesc;
        private System.Windows.Forms.FlowLayoutPanel pnlQuickStart;
        private System.Windows.Forms.Panel pnlExplorerHeader;
        private System.Windows.Forms.Panel pnlWorkbenchHeader;
        private System.Windows.Forms.Panel pnlInspectorHeader;
        private System.Windows.Forms.Label lblExplorerTitle;
        private System.Windows.Forms.Label lblWorkbenchTitle;
        private System.Windows.Forms.Label lblInspectorTitle;
        
        // Research Hub Controls
        private System.Windows.Forms.SplitContainer splitResearch;
        private System.Windows.Forms.TreeView tvResearch;
        private System.Windows.Forms.DataGridView gvResearch;
        private System.Windows.Forms.ComboBox cbResearchTarget;
        private System.Windows.Forms.Label lblResearchTarget;
        private System.Windows.Forms.Button btnApplyResearch;
    }
}
