namespace pk3DS.WinForms;

partial class TextEditor
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
        this.CB_Entry = new System.Windows.Forms.ComboBox();
        this.dgv = new System.Windows.Forms.DataGridView();
        this.B_NewMoveHandler = new System.Windows.Forms.Button();
        this.B_AddLineBefore = new System.Windows.Forms.Button();
        this.B_AddLine = new System.Windows.Forms.Button();
        this.B_RemoveLine = new System.Windows.Forms.Button();
        this.B_Export = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.B_Import = new System.Windows.Forms.Button();
        this.B_Randomize = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
        this.SuspendLayout();
        // 
        // CB_Entry
        // 
        this.CB_Entry.FormattingEnabled = true;
        this.CB_Entry.Location = new System.Drawing.Point(68, 7);
        this.CB_Entry.Name = "CB_Entry";
        this.CB_Entry.Size = new System.Drawing.Size(80, 21);
        this.CB_Entry.TabIndex = 5;
        this.CB_Entry.SelectedIndexChanged += new System.EventHandler(this.ChangeEntry);
        // 
        // dgv
        // 
        this.dgv.AllowUserToAddRows = false;
        this.dgv.AllowUserToDeleteRows = false;
        this.dgv.AllowUserToResizeRows = false;
        this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                 | System.Windows.Forms.AnchorStyles.Left) 
                                                                | System.Windows.Forms.AnchorStyles.Right)));
        this.dgv.BackgroundColor = System.Drawing.SystemColors.Control;
        this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgv.Location = new System.Drawing.Point(12, 33);
        this.dgv.Name = "dgv";
        this.dgv.RowHeadersVisible = false;
        this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
        this.dgv.ShowEditingIcon = false;
        this.dgv.Size = new System.Drawing.Size(876, 317);
        this.dgv.TabIndex = 0;
        // 
        // B_NewMoveHandler
        // 
        this.B_NewMoveHandler.Location = new System.Drawing.Point(420, 6);
        this.B_NewMoveHandler.Name = "B_NewMoveHandler";
        this.B_NewMoveHandler.Size = new System.Drawing.Size(115, 23);
        this.B_NewMoveHandler.TabIndex = 13;
        this.B_NewMoveHandler.Text = "New Move Handler";
        this.B_NewMoveHandler.UseVisualStyleBackColor = true;
        this.B_NewMoveHandler.Click += new System.EventHandler(this.B_NewMoveHandler_Click);
        // 
        // B_AddLineBefore
        // 
        this.B_AddLineBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.B_AddLineBefore.Location = new System.Drawing.Point(550, 6);
        this.B_AddLineBefore.Name = "B_AddLineBefore";
        this.B_AddLineBefore.Size = new System.Drawing.Size(100, 23);
        this.B_AddLineBefore.TabIndex = 12;
        this.B_AddLineBefore.Text = "Add Line Before";
        this.B_AddLineBefore.UseVisualStyleBackColor = true;
        this.B_AddLineBefore.Click += new System.EventHandler(this.B_AddLineBefore_Click);
        // 
        // B_AddLine
        // 
        this.B_AddLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.B_AddLine.Location = new System.Drawing.Point(660, 6);
        this.B_AddLine.Name = "B_AddLine";
        this.B_AddLine.Size = new System.Drawing.Size(100, 23);
        this.B_AddLine.TabIndex = 6;
        this.B_AddLine.Text = "Add Line After";
        this.B_AddLine.UseVisualStyleBackColor = true;
        this.B_AddLine.Click += new System.EventHandler(this.B_AddLine_Click);
        // 
        // B_RemoveLine
        // 
        this.B_RemoveLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.B_RemoveLine.Location = new System.Drawing.Point(770, 6);
        this.B_RemoveLine.Name = "B_RemoveLine";
        this.B_RemoveLine.Size = new System.Drawing.Size(100, 23);
        this.B_RemoveLine.TabIndex = 7;
        this.B_RemoveLine.Text = "Remove Line";
        this.B_RemoveLine.UseVisualStyleBackColor = true;
        this.B_RemoveLine.Click += new System.EventHandler(this.B_RemoveLine_Click);
        // 
        // B_Export
        // 
        this.B_Export.Location = new System.Drawing.Point(154, 6);
        this.B_Export.Name = "B_Export";
        this.B_Export.Size = new System.Drawing.Size(90, 23);
        this.B_Export.TabIndex = 8;
        this.B_Export.Text = "Export All (.txt)";
        this.B_Export.UseVisualStyleBackColor = true;
        this.B_Export.Click += new System.EventHandler(this.B_Export_Click);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 10);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(50, 13);
        this.label1.TabIndex = 9;
        this.label1.Text = "Text File:";
        // 
        // B_Import
        // 
        this.B_Import.Location = new System.Drawing.Point(250, 6);
        this.B_Import.Name = "B_Import";
        this.B_Import.Size = new System.Drawing.Size(90, 23);
        this.B_Import.TabIndex = 10;
        this.B_Import.Text = "Import All (.txt)";
        this.B_Import.UseVisualStyleBackColor = true;
        this.B_Import.Click += new System.EventHandler(this.B_Import_Click);
        // 
        // B_Randomize
        // 
        this.B_Randomize.Location = new System.Drawing.Point(346, 6);
        this.B_Randomize.Name = "B_Randomize";
        this.B_Randomize.Size = new System.Drawing.Size(70, 23);
        this.B_Randomize.TabIndex = 11;
        this.B_Randomize.Text = "Randomize";
        this.B_Randomize.UseVisualStyleBackColor = true;
        this.B_Randomize.Click += new System.EventHandler(this.B_Randomize_Click);
        // 
        // TextEditor
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(900, 362);
        this.Controls.Add(this.B_Randomize);
        this.Controls.Add(this.B_Import);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.B_Export);
        this.Controls.Add(this.B_NewMoveHandler);
        this.Controls.Add(this.B_RemoveLine);
        this.Controls.Add(this.B_AddLineBefore);
        this.Controls.Add(this.B_AddLine);
        this.Controls.Add(this.dgv);
        this.Controls.Add(this.CB_Entry);
        this.MinimumSize = new System.Drawing.Size(500, 300);
        this.Name = "TextEditor";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Text Editor";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TextEditor_FormClosing);
        ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox CB_Entry;
    private System.Windows.Forms.DataGridView dgv;
    private System.Windows.Forms.Button B_NewMoveHandler;
    private System.Windows.Forms.Button B_AddLineBefore;
    private System.Windows.Forms.Button B_AddLine;
    private System.Windows.Forms.Button B_RemoveLine;
    private System.Windows.Forms.Button B_Export;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button B_Import;
    private System.Windows.Forms.Button B_Randomize;
}