namespace pk3DS.WinForms
{
    partial class CROExpander
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
            this.CB_CRO = new System.Windows.Forms.ComboBox();
            this.B_Browse = new System.Windows.Forms.Button();
            this.L_Status = new System.Windows.Forms.Label();
            this.TC_Main = new System.Windows.Forms.TabControl();
            this.Tab_Expand = new System.Windows.Forms.TabPage();
            this.GB_Expand = new System.Windows.Forms.GroupBox();
            this.RB_Reloc = new System.Windows.Forms.RadioButton();
            this.RB_BSS = new System.Windows.Forms.RadioButton();
            this.RB_Data = new System.Windows.Forms.RadioButton();
            this.RB_Code = new System.Windows.Forms.RadioButton();
            this.L_Amount = new System.Windows.Forms.Label();
            this.NUD_Pages = new System.Windows.Forms.NumericUpDown();
            this.B_ApplyExpand = new System.Windows.Forms.Button();
            this.Tab_Repoint = new System.Windows.Forms.TabPage();
            this.GB_Repoint = new System.Windows.Forms.GroupBox();
            this.L_NewAddr = new System.Windows.Forms.Label();
            this.NUD_NewAddr = new System.Windows.Forms.NumericUpDown();
            this.L_TableLen = new System.Windows.Forms.Label();
            this.NUD_TableLen = new System.Windows.Forms.NumericUpDown();
            this.L_SearchMethod = new System.Windows.Forms.Label();
            this.CB_SearchMethod = new System.Windows.Forms.ComboBox();
            this.L_FindAddr = new System.Windows.Forms.Label();
            this.NUD_FindAddr = new System.Windows.Forms.NumericUpDown();
            this.B_ApplyTable = new System.Windows.Forms.Button();
            this.B_ApplyFunc = new System.Windows.Forms.Button();
            this.B_Save = new System.Windows.Forms.Button();
            this.TC_Main.SuspendLayout();
            this.Tab_Expand.SuspendLayout();
            this.GB_Expand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_Pages)).BeginInit();
            this.Tab_Repoint.SuspendLayout();
            this.GB_Repoint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_NewAddr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_TableLen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_FindAddr)).BeginInit();
            this.SuspendLayout();
            // 
            // CB_CRO
            // 
            this.CB_CRO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_CRO.FormattingEnabled = true;
            this.CB_CRO.Location = new System.Drawing.Point(12, 12);
            this.CB_CRO.Name = "CB_CRO";
            this.CB_CRO.Size = new System.Drawing.Size(183, 21);
            this.CB_CRO.TabIndex = 0;
            this.CB_CRO.SelectedIndexChanged += new System.EventHandler(this.CB_CRO_SelectedIndexChanged);
            // 
            // B_Browse
            // 
            this.B_Browse.Location = new System.Drawing.Point(201, 10);
            this.B_Browse.Name = "B_Browse";
            this.B_Browse.Size = new System.Drawing.Size(75, 23);
            this.B_Browse.TabIndex = 1;
            this.B_Browse.Text = "Browse...";
            this.B_Browse.UseVisualStyleBackColor = true;
            this.B_Browse.Click += new System.EventHandler(this.B_Browse_Click);
            // 
            // L_Status
            // 
            this.L_Status.AutoSize = true;
            this.L_Status.Location = new System.Drawing.Point(12, 40);
            this.L_Status.Name = "L_Status";
            this.L_Status.Size = new System.Drawing.Size(126, 13);
            this.L_Status.TabIndex = 2;
            this.L_Status.Text = "Select a CRO to expand.";
            // 
            // TC_Main
            // 
            this.TC_Main.Controls.Add(this.Tab_Expand);
            this.TC_Main.Controls.Add(this.Tab_Repoint);
            this.TC_Main.Location = new System.Drawing.Point(12, 56);
            this.TC_Main.Name = "TC_Main";
            this.TC_Main.SelectedIndex = 0;
            this.TC_Main.Size = new System.Drawing.Size(264, 187);
            this.TC_Main.TabIndex = 3;
            // 
            // Tab_Expand
            // 
            this.Tab_Expand.Controls.Add(this.GB_Expand);
            this.Tab_Expand.Location = new System.Drawing.Point(4, 22);
            this.Tab_Expand.Name = "Tab_Expand";
            this.Tab_Expand.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Expand.Size = new System.Drawing.Size(256, 161);
            this.Tab_Expand.TabIndex = 0;
            this.Tab_Expand.Text = "Expansion";
            this.Tab_Expand.UseVisualStyleBackColor = true;
            // 
            // GB_Expand
            // 
            this.GB_Expand.Controls.Add(this.RB_Reloc);
            this.GB_Expand.Controls.Add(this.RB_BSS);
            this.GB_Expand.Controls.Add(this.RB_Data);
            this.GB_Expand.Controls.Add(this.RB_Code);
            this.GB_Expand.Controls.Add(this.L_Amount);
            this.GB_Expand.Controls.Add(this.NUD_Pages);
            this.GB_Expand.Controls.Add(this.B_ApplyExpand);
            this.GB_Expand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GB_Expand.Location = new System.Drawing.Point(3, 3);
            this.GB_Expand.Name = "GB_Expand";
            this.GB_Expand.Size = new System.Drawing.Size(250, 155);
            this.GB_Expand.TabIndex = 0;
            this.GB_Expand.TabStop = false;
            this.GB_Expand.Text = "Expand Segment";
            // 
            // RB_Reloc
            // 
            this.RB_Reloc.AutoSize = true;
            this.RB_Reloc.Location = new System.Drawing.Point(124, 43);
            this.RB_Reloc.Name = "RB_Reloc";
            this.RB_Reloc.Size = new System.Drawing.Size(107, 17);
            this.RB_Reloc.TabIndex = 6;
            this.RB_Reloc.Text = "Relocation Table";
            this.RB_Reloc.UseVisualStyleBackColor = true;
            // 
            // RB_BSS
            // 
            this.RB_BSS.AutoSize = true;
            this.RB_BSS.Location = new System.Drawing.Point(124, 20);
            this.RB_BSS.Name = "RB_BSS";
            this.RB_BSS.Size = new System.Drawing.Size(46, 17);
            this.RB_BSS.TabIndex = 5;
            this.RB_BSS.Text = ".bss";
            this.RB_BSS.UseVisualStyleBackColor = true;
            // 
            // RB_Data
            // 
            this.RB_Data.AutoSize = true;
            this.RB_Data.Location = new System.Drawing.Point(18, 43);
            this.RB_Data.Name = "RB_Data";
            this.RB_Data.Size = new System.Drawing.Size(50, 17);
            this.RB_Data.TabIndex = 4;
            this.RB_Data.Text = ".data";
            this.RB_Data.UseVisualStyleBackColor = true;
            // 
            // RB_Code
            // 
            this.RB_Code.AutoSize = true;
            this.RB_Code.Checked = true;
            this.RB_Code.Location = new System.Drawing.Point(18, 20);
            this.RB_Code.Name = "RB_Code";
            this.RB_Code.Size = new System.Drawing.Size(53, 17);
            this.RB_Code.TabIndex = 3;
            this.RB_Code.TabStop = true;
            this.RB_Code.Text = ".code";
            this.RB_Code.UseVisualStyleBackColor = true;
            // 
            // L_Amount
            // 
            this.L_Amount.AutoSize = true;
            this.L_Amount.Location = new System.Drawing.Point(15, 78);
            this.L_Amount.Name = "L_Amount";
            this.L_Amount.Size = new System.Drawing.Size(84, 13);
            this.L_Amount.TabIndex = 2;
            this.L_Amount.Text = "Amount (Units):";
            // 
            // NUD_Pages
            // 
            this.NUD_Pages.Location = new System.Drawing.Point(125, 76);
            this.NUD_Pages.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.NUD_Pages.Name = "NUD_Pages";
            this.NUD_Pages.Size = new System.Drawing.Size(51, 20);
            this.NUD_Pages.TabIndex = 1;
            this.NUD_Pages.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // B_ApplyExpand
            // 
            this.B_ApplyExpand.Location = new System.Drawing.Point(15, 115);
            this.B_ApplyExpand.Name = "B_ApplyExpand";
            this.B_ApplyExpand.Size = new System.Drawing.Size(225, 23);
            this.B_ApplyExpand.TabIndex = 0;
            this.B_ApplyExpand.Text = "Apply Expansion";
            this.B_ApplyExpand.UseVisualStyleBackColor = true;
            this.B_ApplyExpand.Click += new System.EventHandler(this.B_ApplyExpand_Click);
            // 
            // Tab_Repoint
            // 
            this.Tab_Repoint.Controls.Add(this.GB_Repoint);
            this.Tab_Repoint.Location = new System.Drawing.Point(4, 22);
            this.Tab_Repoint.Name = "Tab_Repoint";
            this.Tab_Repoint.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Repoint.Size = new System.Drawing.Size(256, 161);
            this.Tab_Repoint.TabIndex = 1;
            this.Tab_Repoint.Text = "Repoint/Move";
            this.Tab_Repoint.UseVisualStyleBackColor = true;
            // 
            // GB_Repoint
            // 
            this.GB_Repoint.Controls.Add(this.L_NewAddr);
            this.GB_Repoint.Controls.Add(this.NUD_NewAddr);
            this.GB_Repoint.Controls.Add(this.L_TableLen);
            this.GB_Repoint.Controls.Add(this.NUD_TableLen);
            this.GB_Repoint.Controls.Add(this.L_SearchMethod);
            this.GB_Repoint.Controls.Add(this.CB_SearchMethod);
            this.GB_Repoint.Controls.Add(this.L_FindAddr);
            this.GB_Repoint.Controls.Add(this.NUD_FindAddr);
            this.GB_Repoint.Controls.Add(this.B_ApplyTable);
            this.GB_Repoint.Controls.Add(this.B_ApplyFunc);
            this.GB_Repoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GB_Repoint.Location = new System.Drawing.Point(3, 3);
            this.GB_Repoint.Name = "GB_Repoint";
            this.GB_Repoint.Size = new System.Drawing.Size(250, 155);
            this.GB_Repoint.TabIndex = 0;
            this.GB_Repoint.TabStop = false;
            this.GB_Repoint.Text = "Repoint Logic";
            // 
            // L_NewAddr
            // 
            this.L_NewAddr.AutoSize = true;
            this.L_NewAddr.Location = new System.Drawing.Point(12, 100);
            this.L_NewAddr.Name = "L_NewAddr";
            this.L_NewAddr.Size = new System.Drawing.Size(71, 13);
            this.L_NewAddr.TabIndex = 9;
            this.L_NewAddr.Text = "New Address:";
            // 
            // NUD_NewAddr
            // 
            this.NUD_NewAddr.Hexadecimal = true;
            this.NUD_NewAddr.Location = new System.Drawing.Point(111, 98);
            this.NUD_NewAddr.Maximum = new decimal(new int[] {
            268435456,
            0,
            0,
            0});
            this.NUD_NewAddr.Name = "NUD_NewAddr";
            this.NUD_NewAddr.Size = new System.Drawing.Size(120, 20);
            this.NUD_NewAddr.TabIndex = 8;
            // 
            // L_TableLen
            // 
            this.L_TableLen.AutoSize = true;
            this.L_TableLen.Location = new System.Drawing.Point(12, 74);
            this.L_TableLen.Name = "L_TableLen";
            this.L_TableLen.Size = new System.Drawing.Size(73, 13);
            this.L_TableLen.TabIndex = 7;
            this.L_TableLen.Text = "Table Length:";
            // 
            // NUD_TableLen
            // 
            this.NUD_TableLen.Hexadecimal = true;
            this.NUD_TableLen.Location = new System.Drawing.Point(111, 72);
            this.NUD_TableLen.Maximum = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.NUD_TableLen.Name = "NUD_TableLen";
            this.NUD_TableLen.Size = new System.Drawing.Size(120, 20);
            this.NUD_TableLen.TabIndex = 6;
            // 
            // L_SearchMethod
            // 
            this.L_SearchMethod.AutoSize = true;
            this.L_SearchMethod.Location = new System.Drawing.Point(12, 23);
            this.L_SearchMethod.Name = "L_SearchMethod";
            this.L_SearchMethod.Size = new System.Drawing.Size(59, 13);
            this.L_SearchMethod.TabIndex = 5;
            this.L_SearchMethod.Text = "Search By:";
            // 
            // CB_SearchMethod
            // 
            this.CB_SearchMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CB_SearchMethod.FormattingEnabled = true;
            this.CB_SearchMethod.Items.AddRange(new object[] {
            "Written To Location",
            "Target Address"});
            this.CB_SearchMethod.Location = new System.Drawing.Point(111, 20);
            this.CB_SearchMethod.Name = "CB_SearchMethod";
            this.CB_SearchMethod.Size = new System.Drawing.Size(121, 21);
            this.CB_SearchMethod.TabIndex = 4;
            // 
            // L_FindAddr
            // 
            this.L_FindAddr.AutoSize = true;
            this.L_FindAddr.Location = new System.Drawing.Point(12, 48);
            this.L_FindAddr.Name = "L_FindAddr";
            this.L_FindAddr.Size = new System.Drawing.Size(76, 13);
            this.L_FindAddr.TabIndex = 3;
            this.L_FindAddr.Text = "Find Address: ";
            // 
            // NUD_FindAddr
            // 
            this.NUD_FindAddr.Hexadecimal = true;
            this.NUD_FindAddr.Location = new System.Drawing.Point(111, 46);
            this.NUD_FindAddr.Maximum = new decimal(new int[] {
            268435456,
            0,
            0,
            0});
            this.NUD_FindAddr.Name = "NUD_FindAddr";
            this.NUD_FindAddr.Size = new System.Drawing.Size(120, 20);
            this.NUD_FindAddr.TabIndex = 2;
            // 
            // B_ApplyTable
            // 
            this.B_ApplyTable.Location = new System.Drawing.Point(15, 126);
            this.B_ApplyTable.Name = "B_ApplyTable";
            this.B_ApplyTable.Size = new System.Drawing.Size(100, 23);
            this.B_ApplyTable.TabIndex = 0;
            this.B_ApplyTable.Text = "Move Table";
            this.B_ApplyTable.UseVisualStyleBackColor = true;
            this.B_ApplyTable.Click += new System.EventHandler(this.B_ApplyTable_Click);
            // 
            // B_ApplyFunc
            // 
            this.B_ApplyFunc.Location = new System.Drawing.Point(132, 126);
            this.B_ApplyFunc.Name = "B_ApplyFunc";
            this.B_ApplyFunc.Size = new System.Drawing.Size(100, 23);
            this.B_ApplyFunc.TabIndex = 1;
            this.B_ApplyFunc.Text = "Repoint Func";
            this.B_ApplyFunc.UseVisualStyleBackColor = true;
            this.B_ApplyFunc.Click += new System.EventHandler(this.B_ApplyFunc_Click);
            // 
            // B_Save
            // 
            this.B_Save.Location = new System.Drawing.Point(12, 245);
            this.B_Save.Name = "B_Save";
            this.B_Save.Size = new System.Drawing.Size(264, 30);
            this.B_Save.TabIndex = 4;
            this.B_Save.Text = "Save Changes to CRO";
            this.B_Save.UseVisualStyleBackColor = true;
            this.B_Save.Click += new System.EventHandler(this.B_Save_Click);
            // 
            // CROExpander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 287);
            this.Controls.Add(this.B_Save);
            this.Controls.Add(this.TC_Main);
            this.Controls.Add(this.L_Status);
            this.Controls.Add(this.B_Browse);
            this.Controls.Add(this.CB_CRO);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CROExpander";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CRO Expander Tool";
            this.TC_Main.ResumeLayout(false);
            this.Tab_Expand.ResumeLayout(false);
            this.GB_Expand.ResumeLayout(false);
            this.GB_Expand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_Pages)).EndInit();
            this.Tab_Repoint.ResumeLayout(false);
            this.GB_Repoint.ResumeLayout(false);
            this.GB_Repoint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_NewAddr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_TableLen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_FindAddr)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ComboBox CB_CRO;
        private System.Windows.Forms.Button B_Browse;
        private System.Windows.Forms.Label L_Status;
        private System.Windows.Forms.TabControl TC_Main;
        private System.Windows.Forms.TabPage Tab_Expand;
        private System.Windows.Forms.GroupBox GB_Expand;
        private System.Windows.Forms.RadioButton RB_Reloc;
        private System.Windows.Forms.RadioButton RB_BSS;
        private System.Windows.Forms.RadioButton RB_Data;
        private System.Windows.Forms.RadioButton RB_Code;
        private System.Windows.Forms.Label L_Amount;
        private System.Windows.Forms.NumericUpDown NUD_Pages;
        private System.Windows.Forms.Button B_ApplyExpand;
        private System.Windows.Forms.TabPage Tab_Repoint;
        private System.Windows.Forms.GroupBox GB_Repoint;
        private System.Windows.Forms.Label L_FindAddr;
        private System.Windows.Forms.NumericUpDown NUD_FindAddr;
        private System.Windows.Forms.Button B_ApplyTable;
        private System.Windows.Forms.Button B_ApplyFunc;
        private System.Windows.Forms.Button B_Save;
        private System.Windows.Forms.Label L_SearchMethod;
        private System.Windows.Forms.ComboBox CB_SearchMethod;
        private System.Windows.Forms.Label L_TableLen;
        private System.Windows.Forms.NumericUpDown NUD_TableLen;
        private System.Windows.Forms.Label L_NewAddr;
        private System.Windows.Forms.NumericUpDown NUD_NewAddr;
    }
}
