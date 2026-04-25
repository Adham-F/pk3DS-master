namespace pk3DS.WinForms
{
    partial class ShowdownSetStorage
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
            this.LB_Sets = new System.Windows.Forms.ListBox();
            this.RTB_Preview = new System.Windows.Forms.RichTextBox();
            this.B_Add = new System.Windows.Forms.Button();
            this.B_Delete = new System.Windows.Forms.Button();
            this.B_Copy = new System.Windows.Forms.Button();
            this.B_Close = new System.Windows.Forms.Button();
            this.B_Use = new System.Windows.Forms.Button();
            this.L_Storage = new System.Windows.Forms.Label();
            this.L_Count = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LB_Sets
            // 
            this.LB_Sets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left))));
            this.LB_Sets.FormattingEnabled = true;
            this.LB_Sets.Location = new System.Drawing.Point(12, 40);
            this.LB_Sets.Name = "LB_Sets";
            this.LB_Sets.Size = new System.Drawing.Size(200, 368);
            this.LB_Sets.TabIndex = 0;
            this.LB_Sets.SelectedIndexChanged += new System.EventHandler(this.LB_Sets_SelectedIndexChanged);
            // 
            // RTB_Preview
            // 
            this.RTB_Preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RTB_Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(20)))));
            this.RTB_Preview.ForeColor = System.Drawing.Color.LightCyan;
            this.RTB_Preview.Location = new System.Drawing.Point(218, 40);
            this.RTB_Preview.Name = "RTB_Preview";
            this.RTB_Preview.ReadOnly = true;
            this.RTB_Preview.Size = new System.Drawing.Size(254, 335);
            this.RTB_Preview.TabIndex = 1;
            this.RTB_Preview.Text = "";
            // 
            // B_Add
            // 
            this.B_Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.B_Add.Location = new System.Drawing.Point(12, 415);
            this.B_Add.Name = "B_Add";
            this.B_Add.Size = new System.Drawing.Size(95, 23);
            this.B_Add.TabIndex = 2;
            this.B_Add.Text = "Add Clipboard";
            this.B_Add.UseVisualStyleBackColor = true;
            this.B_Add.Click += new System.EventHandler(this.B_Add_Click);
            // 
            // B_Delete
            // 
            this.B_Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.B_Delete.Location = new System.Drawing.Point(117, 415);
            this.B_Delete.Name = "B_Delete";
            this.B_Delete.Size = new System.Drawing.Size(95, 23);
            this.B_Delete.TabIndex = 3;
            this.B_Delete.Text = "Delete Selected";
            this.B_Delete.UseVisualStyleBackColor = true;
            this.B_Delete.Click += new System.EventHandler(this.B_Delete_Click);
            // 
            // B_Copy
            // 
            this.B_Copy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Copy.Location = new System.Drawing.Point(218, 381);
            this.B_Copy.Name = "B_Copy";
            this.B_Copy.Size = new System.Drawing.Size(120, 23);
            this.B_Copy.TabIndex = 4;
            this.B_Copy.Text = "Copy to Clipboard";
            this.B_Copy.UseVisualStyleBackColor = true;
            this.B_Copy.Click += new System.EventHandler(this.B_Copy_Click);
            // 
            // B_Close
            // 
            this.B_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Close.Location = new System.Drawing.Point(372, 415);
            this.B_Close.Name = "B_Close";
            this.B_Close.Size = new System.Drawing.Size(100, 23);
            this.B_Close.TabIndex = 5;
            this.B_Close.Text = "Close";
            this.B_Close.Click += new System.EventHandler(this.B_Close_Click);
            // 
            // B_Use
            // 
            this.B_Use.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Use.Location = new System.Drawing.Point(400, 470);
            this.B_Use.Name = "B_Use";
            this.B_Use.Size = new System.Drawing.Size(100, 30);
            this.B_Use.TabIndex = 7;
            this.B_Use.Text = "Use Selected";
            this.B_Use.UseVisualStyleBackColor = true;
            this.B_Use.Click += new System.EventHandler(this.B_Use_Click);
            // 
            // L_Storage
            // 
            this.L_Storage.AutoSize = true;
            this.L_Storage.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.L_Storage.ForeColor = System.Drawing.Color.Cyan;
            this.L_Storage.Location = new System.Drawing.Point(12, 9);
            this.L_Storage.Name = "L_Storage";
            this.L_Storage.Size = new System.Drawing.Size(306, 30);
            this.L_Storage.TabIndex = 6;
            this.L_Storage.Text = "SHOWDOWN SET STORAGE";

            // 
            // L_Count
            // 
            this.L_Count.AutoSize = true;
            this.L_Count.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.L_Count.ForeColor = System.Drawing.Color.LightGray;
            this.L_Count.Location = new System.Drawing.Point(15, 39);
            this.L_Count.Name = "L_Count";
            this.L_Count.Size = new System.Drawing.Size(81, 19);
            this.L_Count.TabIndex = 8;
            this.L_Count.Text = "Total Sets: 0";

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 520);
            this.Controls.Add(this.B_Use);
            this.Controls.Add(this.L_Count);
            this.Controls.Add(this.L_Storage);
            this.Controls.Add(this.B_Close);
            this.Controls.Add(this.B_Copy);
            this.Controls.Add(this.B_Delete);
            this.Controls.Add(this.B_Add);
            this.Controls.Add(this.RTB_Preview);
            this.Controls.Add(this.LB_Sets);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ShowdownSetStorage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Showdown Set Storage";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox LB_Sets;
        private System.Windows.Forms.RichTextBox RTB_Preview;
        private System.Windows.Forms.Button B_Add;
        private System.Windows.Forms.Button B_Delete;
        private System.Windows.Forms.Button B_Copy;
        private System.Windows.Forms.Button B_Close;
        private System.Windows.Forms.Button B_Use;
        private System.Windows.Forms.Label L_Storage;
        private System.Windows.Forms.Label L_Count;
    }
}
