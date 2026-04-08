namespace pk3DS.WinForms;

partial class TMEditor7
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.dgvTM = new System.Windows.Forms.DataGridView();
        this.L_TM = new System.Windows.Forms.Label();
        this.B_RTM = new System.Windows.Forms.Button();
        this.B_ExportTxt = new System.Windows.Forms.Button();
        this.B_ImportTxt = new System.Windows.Forms.Button();
        this.B_UpdateDesc = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.dgvTM)).BeginInit();
        this.SuspendLayout();
        // 
        // dgvTM
        // 
        this.dgvTM.AllowUserToAddRows = false;
        this.dgvTM.AllowUserToDeleteRows = false;
        this.dgvTM.AllowUserToResizeColumns = false;
        this.dgvTM.AllowUserToResizeRows = false;
        this.dgvTM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                                                   | System.Windows.Forms.AnchorStyles.Left) 
                                                                  | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvTM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvTM.Location = new System.Drawing.Point(9, 25);
        this.dgvTM.Name = "dgvTM";
        this.dgvTM.Size = new System.Drawing.Size(240, 299);
        this.dgvTM.TabIndex = 1;
        // 
        // L_TM
        // 
        this.L_TM.AutoSize = true;
        this.L_TM.Location = new System.Drawing.Point(9, 9);
        this.L_TM.Name = "L_TM";
        this.L_TM.Size = new System.Drawing.Size(26, 13);
        this.L_TM.TabIndex = 2;
        this.L_TM.Text = "TM:";
        // 
        // B_RTM
        // 
        this.B_RTM.Location = new System.Drawing.Point(41, 1);
        this.B_RTM.Name = "B_RTM";
        this.B_RTM.Size = new System.Drawing.Size(75, 23);
        this.B_RTM.TabIndex = 5;
        this.B_RTM.Text = "Randomize";
        this.B_RTM.UseVisualStyleBackColor = true;
        this.B_RTM.Click += new System.EventHandler(this.B_RandomTM_Click);
        // 
        // B_ExportTxt
        // 
        this.B_ExportTxt.Location = new System.Drawing.Point(120, 1);
        this.B_ExportTxt.Name = "B_ExportTxt";
        this.B_ExportTxt.Size = new System.Drawing.Size(75, 23);
        this.B_ExportTxt.TabIndex = 6;
        this.B_ExportTxt.Text = "Export .txt";
        this.B_ExportTxt.UseVisualStyleBackColor = true;
        this.B_ExportTxt.Click += new System.EventHandler(this.B_ExportTxt_Click);
        // 
        // B_ImportTxt
        // 
        this.B_ImportTxt.Location = new System.Drawing.Point(199, 1);
        this.B_ImportTxt.Name = "B_ImportTxt";
        this.B_ImportTxt.Size = new System.Drawing.Size(75, 23);
        this.B_ImportTxt.TabIndex = 7;
        this.B_ImportTxt.Text = "Import .txt";
        this.B_ImportTxt.UseVisualStyleBackColor = true;
        this.B_ImportTxt.Click += new System.EventHandler(this.B_ImportTxt_Click);
        // 
        // B_UpdateDesc
        // 
        this.B_UpdateDesc.Location = new System.Drawing.Point(278, 1);
        this.B_UpdateDesc.Name = "B_UpdateDesc";
        this.B_UpdateDesc.Size = new System.Drawing.Size(90, 23);
        this.B_UpdateDesc.TabIndex = 8;
        this.B_UpdateDesc.Text = "Update Desc";
        this.B_UpdateDesc.UseVisualStyleBackColor = true;
        this.B_UpdateDesc.Click += new System.EventHandler(this.B_UpdateDesc_Click);
        // 
        // TMEditor7
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(380, 331);
        this.Controls.Add(this.B_UpdateDesc);
        this.Controls.Add(this.B_ImportTxt);
        this.Controls.Add(this.B_ExportTxt);
        this.Controls.Add(this.B_RTM);
        this.Controls.Add(this.L_TM);
        this.Controls.Add(this.dgvTM);
        this.MaximizeBox = false;
        this.MaximumSize = new System.Drawing.Size(520, 670);
        this.MinimumSize = new System.Drawing.Size(396, 370);
        this.Name = "TMEditor7";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "TM Editor";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
        ((System.ComponentModel.ISupportInitialize)(this.dgvTM)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGridView dgvTM;
    private System.Windows.Forms.Label L_TM;
    private System.Windows.Forms.Button B_RTM;
    private System.Windows.Forms.Button B_ExportTxt;
    private System.Windows.Forms.Button B_ImportTxt;
    private System.Windows.Forms.Button B_UpdateDesc;
}