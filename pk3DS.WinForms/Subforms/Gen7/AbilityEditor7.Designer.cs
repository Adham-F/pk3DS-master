namespace pk3DS.WinForms;

partial class AbilityEditor7
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.CB_Ability = new System.Windows.Forms.ComboBox();
        this.L_Ability = new System.Windows.Forms.Label();
        this.B_Save = new System.Windows.Forms.Button();
        this.RTB_Description = new System.Windows.Forms.RichTextBox();
        this.RTB_HexViewer = new System.Windows.Forms.RichTextBox();
        this.L_HexLabel = new System.Windows.Forms.Label();
        this.L_Helper = new System.Windows.Forms.Label();

        this.SuspendLayout();

        // Ability Selector
        this.L_Ability.Text = "Ability Index:";
        this.L_Ability.Location = new System.Drawing.Point(12, 15);
        this.CB_Ability.Location = new System.Drawing.Point(90, 12);
        this.CB_Ability.Size = new System.Drawing.Size(200, 21);
        this.CB_Ability.SelectedIndexChanged += new System.EventHandler(this.ChangeEntry);

        // Save Button
        this.B_Save.Location = new System.Drawing.Point(440, 10);
        this.B_Save.Size = new System.Drawing.Size(130, 26);
        this.B_Save.Text = "Write Hex to File";
        this.B_Save.Click += new System.EventHandler(this.B_Save_Click);

        // Description Box
        this.RTB_Description.Location = new System.Drawing.Point(15, 45);
        this.RTB_Description.Size = new System.Drawing.Size(555, 45);
        this.RTB_Description.ReadOnly = true;
        this.RTB_Description.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        // Hex Viewer Label
        this.L_HexLabel.Location = new System.Drawing.Point(15, 105);
        this.L_HexLabel.Text = "Live Binary Stream (battle.cro):";
        this.L_HexLabel.AutoSize = true;

        // The Hex Editor Box
        this.RTB_HexViewer.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        this.RTB_HexViewer.ForeColor = System.Drawing.Color.Cyan;
        this.RTB_HexViewer.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
        this.RTB_HexViewer.Location = new System.Drawing.Point(15, 125);
        this.RTB_HexViewer.Size = new System.Drawing.Size(555, 200);
        this.RTB_HexViewer.Text = "";

        // Helper Label
        this.L_Helper.Location = new System.Drawing.Point(15, 335);
        this.L_Helper.Size = new System.Drawing.Size(555, 100);
        this.L_Helper.Text = "INSTRUCTIONS:\n1. Select an ability to jump to its code.\n2. Edit the Hex bytes above directly (XX format).\n3. Standard ARM MOV parameters: [Value] 10 A0 E3 (Stat) | [Value] 00 A0 E3 (Type).\n4. Click 'Write Hex to File' to commit changes.";

        // Form Setup
        this.ClientSize = new System.Drawing.Size(585, 450);
        this.Controls.Add(this.L_Helper);
        this.Controls.Add(this.L_HexLabel);
        this.Controls.Add(this.RTB_HexViewer);
        this.Controls.Add(this.RTB_Description);
        this.Controls.Add(this.B_Save);
        this.Controls.Add(this.L_Ability);
        this.Controls.Add(this.CB_Ability);
        this.Name = "AbilityEditor7";
        this.Text = "Battle.cro Hex Patcher";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.ComboBox CB_Ability;
    private System.Windows.Forms.Label L_Ability;
    private System.Windows.Forms.Button B_Save;
    private System.Windows.Forms.RichTextBox RTB_Description;
    private System.Windows.Forms.RichTextBox RTB_HexViewer;
    private System.Windows.Forms.Label L_HexLabel;
    private System.Windows.Forms.Label L_Helper;
}