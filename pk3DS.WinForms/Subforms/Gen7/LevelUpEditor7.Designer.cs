namespace pk3DS.WinForms;

partial class LevelUpEditor7
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.B_Copy = new System.Windows.Forms.Button();
        this.B_Paste = new System.Windows.Forms.Button();
        this.dgv = new System.Windows.Forms.DataGridView();
        this.CB_Species = new System.Windows.Forms.ComboBox();
        this.L_Species = new System.Windows.Forms.Label();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.CHK_NoFixedDamage = new System.Windows.Forms.CheckBox();
        this.CHK_4MovesLvl1 = new System.Windows.Forms.CheckBox();
        this.L_Moves = new System.Windows.Forms.Label();
        this.NUD_Moves = new System.Windows.Forms.NumericUpDown();
        this.CHK_Expand = new System.Windows.Forms.CheckBox();
        this.L_Scale2 = new System.Windows.Forms.Label();
        this.NUD_Level = new System.Windows.Forms.NumericUpDown();
        this.L_Scale1 = new System.Windows.Forms.Label();
        this.CHK_Spread = new System.Windows.Forms.CheckBox();
        this.L_STAB = new System.Windows.Forms.Label();
        this.NUD_STAB = new System.Windows.Forms.NumericUpDown();
        this.CHK_STAB = new System.Windows.Forms.CheckBox();
        this.CHK_HMs = new System.Windows.Forms.CheckBox();
        this.PB_MonSprite = new System.Windows.Forms.PictureBox();
        this.B_AddMove = new System.Windows.Forms.Button();
        this.B_RemoveMove = new System.Windows.Forms.Button();
        this.B_Import = new System.Windows.Forms.Button();
        this.L_TotalMoves = new System.Windows.Forms.Label();
        this.L_STABCount = new System.Windows.Forms.Label();
        this.B_Metronome = new System.Windows.Forms.Button();
        this.B_ImportJSON = new System.Windows.Forms.Button();
        this.B_ImportTS = new System.Windows.Forms.Button();
        this.B_RandAll = new System.Windows.Forms.Button();
        this.B_Dump = new System.Windows.Forms.Button();
        this.B_Goto = new System.Windows.Forms.Button();
        this.NUD_FormTable = new System.Windows.Forms.NumericUpDown();
        this.TC_Pokemon = new System.Windows.Forms.TabControl();
        this.TP_General = new System.Windows.Forms.TabPage();
        this.TP_Changelog = new System.Windows.Forms.TabPage();
        this.RTB_Changelog = new System.Windows.Forms.RichTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
        this.groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_Moves)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_Level)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_STAB)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.PB_MonSprite)).BeginInit();
        this.TC_Pokemon.SuspendLayout();
        this.TP_General.SuspendLayout();
        this.TP_Changelog.SuspendLayout();
        this.SuspendLayout();
        // 
        // dgv
        // 
        this.dgv.AllowUserToResizeColumns = false;
        this.dgv.AllowUserToResizeRows = false;
        this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgv.Location = new System.Drawing.Point(8, 8);
        this.dgv.Name = "dgv";
        this.dgv.Size = new System.Drawing.Size(200, 580);
        this.dgv.TabIndex = 0;
        // 
        // CB_Species
        // 
        this.CB_Species.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
        this.CB_Species.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.CB_Species.FormattingEnabled = true;
        this.CB_Species.Location = new System.Drawing.Point(58, 12);
        this.CB_Species.Name = "CB_Species";
        this.CB_Species.Size = new System.Drawing.Size(121, 21);
        this.CB_Species.TabIndex = 1;
        this.CB_Species.SelectedIndexChanged += new System.EventHandler(this.ChangeEntry);
        // 
        // L_Species
        // 
        this.L_Species.AutoSize = true;
        this.L_Species.Location = new System.Drawing.Point(4, 15);
        this.L_Species.Name = "L_Species";
        this.L_Species.Size = new System.Drawing.Size(48, 13);
        this.L_Species.TabIndex = 2;
        this.L_Species.Text = "Species:";
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.CHK_NoFixedDamage);
        this.groupBox1.Controls.Add(this.CHK_4MovesLvl1);
        this.groupBox1.Controls.Add(this.L_Moves);
        this.groupBox1.Controls.Add(this.NUD_Moves);
        this.groupBox1.Controls.Add(this.CHK_Expand);
        this.groupBox1.Controls.Add(this.L_Scale2);
        this.groupBox1.Controls.Add(this.NUD_Level);
        this.groupBox1.Controls.Add(this.L_Scale1);
        this.groupBox1.Controls.Add(this.CHK_Spread);
        this.groupBox1.Controls.Add(this.L_STAB);
        this.groupBox1.Controls.Add(this.NUD_STAB);
        this.groupBox1.Controls.Add(this.CHK_STAB);
        this.groupBox1.Controls.Add(this.CHK_HMs);
        this.groupBox1.Location = new System.Drawing.Point(220, 60);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(160, 260);
        this.groupBox1.TabIndex = 6;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Options";
        // 
        // CHK_NoFixedDamage
        // 
        this.CHK_NoFixedDamage.AutoSize = true;
        this.CHK_NoFixedDamage.Location = new System.Drawing.Point(100, 204);
        this.CHK_NoFixedDamage.Name = "CHK_NoFixedDamage";
        this.CHK_NoFixedDamage.Size = new System.Drawing.Size(55, 17);
        this.CHK_NoFixedDamage.Text = "Fixed";
        this.CHK_NoFixedDamage.Visible = false;
        // 
        // CHK_4MovesLvl1
        // 
        this.CHK_4MovesLvl1.AutoSize = true;
        this.CHK_4MovesLvl1.Location = new System.Drawing.Point(5, 205);
        this.CHK_4MovesLvl1.Name = "CHK_4MovesLvl1";
        this.CHK_4MovesLvl1.Size = new System.Drawing.Size(70, 30);
        this.CHK_4MovesLvl1.Text = "Start with\n4 Moves";
        // 
        // L_Moves
        // 
        this.L_Moves.AutoSize = true;
        this.L_Moves.Location = new System.Drawing.Point(10, 130);
        this.L_Moves.Name = "L_Moves";
        this.L_Moves.Size = new System.Drawing.Size(42, 13);
        this.L_Moves.Text = "Moves:";
        // 
        // NUD_Moves
        // 
        this.NUD_Moves.Location = new System.Drawing.Point(53, 128);
        this.NUD_Moves.Value = new decimal(new int[] { 25, 0, 0, 0 });
        this.NUD_Moves.Name = "NUD_Moves";
        this.NUD_Moves.Size = new System.Drawing.Size(36, 20);
        // 
        // CHK_Expand
        // 
        this.CHK_Expand.AutoSize = true;
        this.CHK_Expand.Location = new System.Drawing.Point(5, 110);
        this.CHK_Expand.Name = "CHK_Expand";
        this.CHK_Expand.Size = new System.Drawing.Size(86, 17);
        this.CHK_Expand.Text = "Expand Pool";
        // 
        // L_Scale2
        // 
        this.L_Scale2.AutoSize = true;
        this.L_Scale2.Location = new System.Drawing.Point(2, 180);
        this.L_Scale2.Name = "L_Scale2";
        this.L_Scale2.Size = new System.Drawing.Size(50, 13);
        this.L_Scale2.Text = "@ Level:";
        // 
        // NUD_Level
        // 
        this.NUD_Level.Location = new System.Drawing.Point(54, 178);
        this.NUD_Level.Value = new decimal(new int[] { 75, 0, 0, 0 });
        this.NUD_Level.Name = "NUD_Level";
        this.NUD_Level.Size = new System.Drawing.Size(36, 20);
        // 
        // L_Scale1
        // 
        this.L_Scale1.AutoSize = true;
        this.L_Scale1.Location = new System.Drawing.Point(5, 165);
        this.L_Scale1.Name = "L_Scale1";
        this.L_Scale1.Size = new System.Drawing.Size(73, 13);
        this.L_Scale1.Text = "Stop Learning";
        // 
        // CHK_Spread
        // 
        this.CHK_Spread.AutoSize = true;
        this.CHK_Spread.Location = new System.Drawing.Point(5, 90);
        this.CHK_Spread.Name = "CHK_Spread";
        this.CHK_Spread.Size = new System.Drawing.Size(95, 17);
        this.CHK_Spread.Text = "Spread Evenly";
        this.CHK_Spread.Visible = false;
        // 
        // L_STAB
        // 
        this.L_STAB.AutoSize = true;
        this.L_STAB.Location = new System.Drawing.Point(6, 74);
        this.L_STAB.Name = "L_STAB";
        this.L_STAB.Size = new System.Drawing.Size(46, 13);
        this.L_STAB.Text = "% STAB";
        // 
        // NUD_STAB
        // 
        this.NUD_STAB.Location = new System.Drawing.Point(53, 72);
        this.NUD_STAB.Value = new decimal(new int[] { 52, 0, 0, 0 });
        this.NUD_STAB.Name = "NUD_STAB";
        this.NUD_STAB.Size = new System.Drawing.Size(36, 20);
        // 
        // CHK_STAB
        // 
        this.CHK_STAB.AutoSize = true;
        this.CHK_STAB.Location = new System.Drawing.Point(5, 54);
        this.CHK_STAB.Name = "CHK_STAB";
        this.CHK_STAB.Size = new System.Drawing.Size(87, 17);
        this.CHK_STAB.Text = "Bias by Type";
        this.CHK_STAB.CheckedChanged += new System.EventHandler(this.CHK_TypeBias_CheckedChanged);
        // 
        // CHK_HMs
        // 
        this.CHK_HMs.AutoSize = true;
        this.CHK_HMs.Location = new System.Drawing.Point(5, 19);
        this.CHK_HMs.Name = "CHK_HMs";
        this.CHK_HMs.Size = new System.Drawing.Size(76, 17);
        this.CHK_HMs.Text = "Allow HMs";
        // 
        // PB_MonSprite
        // 
        this.PB_MonSprite.Location = new System.Drawing.Point(210, 4);
        this.PB_MonSprite.Name = "PB_MonSprite";
        this.PB_MonSprite.Size = new System.Drawing.Size(48, 48);
        this.PB_MonSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.PB_MonSprite.TabStop = false;
        this.PB_MonSprite.BackColor = System.Drawing.Color.Transparent;
        // 
        // B_RandAll
        // 
        this.B_RandAll.Location = new System.Drawing.Point(220, 325);
        this.B_RandAll.Name = "B_RandAll";
        this.B_RandAll.Size = new System.Drawing.Size(160, 23);
        this.B_RandAll.Text = "Randomize All";
        this.B_RandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.B_RandAll.ForeColor = System.Drawing.Color.Orange;
        this.B_RandAll.Click += new System.EventHandler(this.B_RandAll_Click);
        // 
        // B_Dump
        // 
        this.B_Dump.Location = new System.Drawing.Point(220, 350);
        this.B_Dump.Name = "B_Dump";
        this.B_Dump.Size = new System.Drawing.Size(160, 23);
        this.B_Dump.Text = "Dump All";
        this.B_Dump.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.B_Dump.Click += new System.EventHandler(this.B_Dump_Click);
        // 
        // B_Goto
        // 
        this.B_Goto.Location = new System.Drawing.Point(335, 375);
        this.B_Goto.Name = "B_Goto";
        this.B_Goto.Size = new System.Drawing.Size(44, 23);
        this.B_Goto.Text = "Go to";
        this.B_Goto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.B_Goto.Click += new System.EventHandler(this.B_Goto_Click);
        // 
        // NUD_FormTable
        // 
        this.NUD_FormTable.Location = new System.Drawing.Point(220, 377);
        this.NUD_FormTable.Name = "NUD_FormTable";
        this.NUD_FormTable.Size = new System.Drawing.Size(110, 20);
        this.NUD_FormTable.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
        // 
        // B_AddMove
        // 
        this.B_AddMove.Location = new System.Drawing.Point(220, 400);
        this.B_AddMove.Name = "B_AddMove";
        this.B_AddMove.Size = new System.Drawing.Size(160, 23);
        this.B_AddMove.Text = "Add Move";
        this.B_AddMove.Click += new System.EventHandler(this.B_AddMove_Click);
        // 
        // B_RemoveMove
        // 
        this.B_RemoveMove.Location = new System.Drawing.Point(220, 425);
        this.B_RemoveMove.Name = "B_RemoveMove";
        this.B_RemoveMove.Size = new System.Drawing.Size(160, 23);
        this.B_RemoveMove.Text = "Remove Move";
        this.B_RemoveMove.Click += new System.EventHandler(this.B_RemoveMove_Click);
        // 
        // B_Import
        // 
        this.B_Import.Location = new System.Drawing.Point(220, 450);
        this.B_Import.Name = "B_Import";
        this.B_Import.Size = new System.Drawing.Size(160, 23);
        this.B_Import.Text = "Import Learnsets";
        this.B_Import.Click += new System.EventHandler(this.B_Import_Click);
        // 
        // B_ImportJSON
        // 
        this.B_ImportJSON.Location = new System.Drawing.Point(220, 475);
        this.B_ImportJSON.Name = "B_ImportJSON";
        this.B_ImportJSON.Size = new System.Drawing.Size(160, 23);
        this.B_ImportJSON.Text = "Import JSON";
        this.B_ImportJSON.Click += new System.EventHandler(this.B_ImportJSON_Click);
        // 
        // B_ImportTS
        // 
        this.B_ImportTS.Location = new System.Drawing.Point(220, 500);
        this.B_ImportTS.Name = "B_ImportTS";
        this.B_ImportTS.Size = new System.Drawing.Size(160, 23);
        this.B_ImportTS.Text = "Import TS/TSV";
        this.B_ImportTS.Click += new System.EventHandler(this.B_ImportTS_Click);
        // 
        // B_Metronome
        // 
        this.B_Metronome.Location = new System.Drawing.Point(220, 525);
        this.B_Metronome.Name = "B_Metronome";
        this.B_Metronome.Size = new System.Drawing.Size(160, 23);
        this.B_Metronome.Text = "Metronome Mode";
        this.B_Metronome.Click += new System.EventHandler(this.B_Metronome_Click);
        // 
        // B_Copy
        // 
        this.B_Copy.Location = new System.Drawing.Point(220, 550);
        this.B_Copy.Name = "B_Copy";
        this.B_Copy.Size = new System.Drawing.Size(160, 23);
        this.B_Copy.Text = "Copy Set";
        this.B_Copy.Click += new System.EventHandler(this.B_Copy_Click);
        // 
        // B_Paste
        // 
        this.B_Paste.Location = new System.Drawing.Point(220, 575);
        this.B_Paste.Name = "B_Paste";
        this.B_Paste.Size = new System.Drawing.Size(160, 23);
        this.B_Paste.Text = "Paste Set";
        this.B_Paste.Click += new System.EventHandler(this.B_Paste_Click);
        // 
        // L_TotalMoves
        // 
        this.L_TotalMoves.Location = new System.Drawing.Point(220, 5);
        this.L_TotalMoves.Name = "L_TotalMoves";
        this.L_TotalMoves.Size = new System.Drawing.Size(100, 15);
        this.L_TotalMoves.Text = "Total Moves: 0";
        // 
        // L_STABCount
        // 
        this.L_STABCount.Location = new System.Drawing.Point(220, 23);
        this.L_STABCount.Name = "L_STABCount";
        this.L_STABCount.Size = new System.Drawing.Size(100, 15);
        this.L_STABCount.Text = "STAB Moves: 0";
        // 
        // TC_Pokemon
        // 
        this.TC_Pokemon.Controls.Add(this.TP_General);
        this.TC_Pokemon.Controls.Add(this.TP_Changelog);
        this.TC_Pokemon.Location = new System.Drawing.Point(5, 40);
        this.TC_Pokemon.Name = "TC_Pokemon";
        this.TC_Pokemon.Size = new System.Drawing.Size(620, 650);
        // 
        // TP_General
        // 
        this.TP_General.Controls.Add(this.L_TotalMoves);
        this.TP_General.Controls.Add(this.L_STABCount);
        this.TP_General.Controls.Add(this.B_RandAll);
        this.TP_General.Controls.Add(this.B_Dump);
        this.TP_General.Controls.Add(this.B_Goto);
        this.TP_General.Controls.Add(this.NUD_FormTable);
        this.TP_General.Controls.Add(this.B_AddMove);
        this.TP_General.Controls.Add(this.B_RemoveMove);
        this.TP_General.Controls.Add(this.B_Import);
        this.TP_General.Controls.Add(this.B_ImportJSON);
        this.TP_General.Controls.Add(this.B_ImportTS);
        this.TP_General.Controls.Add(this.B_Metronome);
        this.TP_General.Controls.Add(this.B_Copy);
        this.TP_General.Controls.Add(this.B_Paste);
        this.TP_General.Controls.Add(this.dgv);
        this.TP_General.Controls.Add(this.groupBox1);
        this.TP_General.Location = new System.Drawing.Point(4, 22);
        this.TP_General.Name = "TP_General";
        this.TP_General.Size = new System.Drawing.Size(612, 624);
        this.TP_General.Text = "General";
        // 
        // TP_Changelog
        // 
        this.TP_Changelog.Controls.Add(this.RTB_Changelog);
        this.TP_Changelog.Location = new System.Drawing.Point(4, 22);
        this.TP_Changelog.Name = "TP_Changelog";
        this.TP_Changelog.Size = new System.Drawing.Size(332, 564);
        this.TP_Changelog.Text = "Changelog";
        // 
        // RTB_Changelog
        // 
        this.RTB_Changelog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RTB_Changelog.Name = "RTB_Changelog";
        // 
        // LevelUpEditor7
        // 
        this.ClientSize = new System.Drawing.Size(640, 700);
        this.Controls.Add(this.PB_MonSprite);
        this.Controls.Add(this.L_Species);
        this.Controls.Add(this.CB_Species);
        this.Controls.Add(this.TC_Pokemon);
        this.Name = "LevelUpEditor7";
        this.Text = "Level Up Move Editor";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
        ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_Moves)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_Level)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NUD_STAB)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.PB_MonSprite)).EndInit();
        this.TC_Pokemon.ResumeLayout(false);
        this.TP_General.ResumeLayout(false);
        this.TP_Changelog.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.DataGridView dgv;
    private System.Windows.Forms.ComboBox CB_Species;
    private System.Windows.Forms.Label L_Species;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox CHK_STAB;
    private System.Windows.Forms.CheckBox CHK_HMs;
    private System.Windows.Forms.Label L_STAB;
    private System.Windows.Forms.NumericUpDown NUD_STAB;
    private System.Windows.Forms.Label L_Scale2;
    private System.Windows.Forms.NumericUpDown NUD_Level;
    private System.Windows.Forms.Label L_Scale1;
    private System.Windows.Forms.CheckBox CHK_Spread;
    private System.Windows.Forms.Label L_Moves;
    private System.Windows.Forms.NumericUpDown NUD_Moves;
    private System.Windows.Forms.CheckBox CHK_Expand;
    private System.Windows.Forms.PictureBox PB_MonSprite;
    private System.Windows.Forms.CheckBox CHK_4MovesLvl1;
    private System.Windows.Forms.CheckBox CHK_NoFixedDamage;
    private System.Windows.Forms.Button B_Metronome;
    private System.Windows.Forms.Button B_Copy;
    private System.Windows.Forms.Button B_Paste;
    private System.Windows.Forms.Button B_AddMove;
    private System.Windows.Forms.Button B_RemoveMove;
    private System.Windows.Forms.Button B_Import;
    private System.Windows.Forms.Label L_TotalMoves;
    private System.Windows.Forms.Label L_STABCount;
    private System.Windows.Forms.TabControl TC_Pokemon;
    private System.Windows.Forms.TabPage TP_General;
    private System.Windows.Forms.TabPage TP_Changelog;
    private System.Windows.Forms.RichTextBox RTB_Changelog;
    private System.Windows.Forms.Button B_ImportJSON;
    private System.Windows.Forms.Button B_ImportTS;
    private System.Windows.Forms.Button B_RandAll;
    private System.Windows.Forms.Button B_Dump;
    private System.Windows.Forms.Button B_Goto;
    private System.Windows.Forms.NumericUpDown NUD_FormTable;
}
