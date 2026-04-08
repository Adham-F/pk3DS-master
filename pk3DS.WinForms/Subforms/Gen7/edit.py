import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

text = text.replace("this.L_Team.Location = new System.Drawing.Point(-19, 267);", "this.L_Team.Location = new System.Drawing.Point(-19, 33);")

for i, x in enumerate([50, 147, 244, 341, 438, 535], 1):
    target = f"this.PB_Team{i}.ContextMenuStrip = this.mnuVSD;\n        this.PB_Team{i}.Location = new System.Drawing.Point({x}, 244);"
    target_r = f"this.PB_Team{i}.ContextMenuStrip = this.mnuVSD;\r\n        this.PB_Team{i}.Location = new System.Drawing.Point({x}, 244);"
    replacement = f"this.PB_Team{i}.Location = new System.Drawing.Point({x}, 10);"
    text = text.replace(target, replacement).replace(target_r, replacement)

text = text.replace("this.TC_trpoke.Location = new System.Drawing.Point(12, 12);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 85);")
text = text.replace("this.TC_trdata.Location = new System.Drawing.Point(250, 13);", "this.TC_trdata.Location = new System.Drawing.Point(250, 85);")
text = text.replace("this.ClientSize = new System.Drawing.Size(631, 315);", "this.ClientSize = new System.Drawing.Size(631, 388);")

# Increase TC_trdata size and properties to fit new buttons
text = text.replace("this.TC_trdata.Size = new System.Drawing.Size(371, 225);", "this.TC_trdata.Size = new System.Drawing.Size(371, 265);")
text = text.replace("this.Tab_Trainer.Size = new System.Drawing.Size(363, 199);", "this.Tab_Trainer.Size = new System.Drawing.Size(363, 239);")
text = text.replace("this.GB_AIBits.Size = new System.Drawing.Size(100, 161);", "this.GB_AIBits.Size = new System.Drawing.Size(100, 215);")
text = text.replace("this.TC_trpoke.Size = new System.Drawing.Size(232, 225);", "this.TC_trpoke.Size = new System.Drawing.Size(302, 225);")
text = text.replace("this.Tab_Stats.Size = new System.Drawing.Size(224, 199);", "this.Tab_Stats.Size = new System.Drawing.Size(294, 199);")
text = text.replace("this.FLP_Stats.Size = new System.Drawing.Size(178, 193);", "this.FLP_Stats.Size = new System.Drawing.Size(280, 193);")

for flp in ["HP", "Atk", "Def", "SpA", "SpD", "Spe", "HPType", "StatHeader", "StatsTotal"]:
    text = text.replace(f"this.FLP_{flp}.Size = new System.Drawing.Size(172, 21);", f"this.FLP_{flp}.Size = new System.Drawing.Size(262, 24);")
    text = text.replace(f"this.FLP_{flp}.Size = new System.Drawing.Size(172, 22);", f"this.FLP_{flp}.Size = new System.Drawing.Size(262, 24);")

# Remove mnuVSD instantiation block and fields
mnu_regex = re.compile(r"\s*// mnuVSD.*?(?=\s*// PB_Team2)", re.DOTALL)
text = mnu_regex.sub("\n        // PB_Team2", text)
text = re.sub(r"\s*this\.mnuVSD = new System\.Windows\.Forms\.ContextMenuStrip\(this\.components\);.*?\n", "\n", text)
text = re.sub(r"\s*this\.mnuView = new System\.Windows\.Forms\.ToolStripMenuItem\(\);.*?\n", "\n", text)
text = re.sub(r"\s*this\.mnuSet = new System\.Windows\.Forms\.ToolStripMenuItem\(\);.*?\n", "\n", text)
text = re.sub(r"\s*this\.mnuDelete = new System\.Windows\.Forms\.ToolStripMenuItem\(\);.*?\n", "\n", text)
text = re.sub(r"\s*this\.mnuVSD\.SuspendLayout\(\);.*?\n", "\n", text)
text = re.sub(r"\s*this\.mnuVSD\.ResumeLayout\(false\);.*?\n", "\n", text)
text = re.sub(r"\s*private System\.Windows\.Forms\.ContextMenuStrip mnuVSD;.*?\n", "\n", text)
text = re.sub(r"\s*private System\.Windows\.Forms\.ToolStripMenuItem mnuView;.*?\n", "\n", text)
text = re.sub(r"\s*private System\.Windows\.Forms\.ToolStripMenuItem mnuSet;.*?\n", "\n", text)
text = re.sub(r"\s*private System\.Windows\.Forms\.ToolStripMenuItem mnuDelete;.*?\n", "\n", text)


new_controls_init = """
        this.B_Master = new System.Windows.Forms.Button();
        this.B_MasterAll = new System.Windows.Forms.Button();
        this.B_EnableMega = new System.Windows.Forms.Button();
        this.B_EnableZMove = new System.Windows.Forms.Button();
        this.TB_HPEV_Slider = new System.Windows.Forms.TrackBar();
        this.TB_ATKEV_Slider = new System.Windows.Forms.TrackBar();
        this.TB_DEFEV_Slider = new System.Windows.Forms.TrackBar();
        this.TB_SPAEV_Slider = new System.Windows.Forms.TrackBar();
        this.TB_SPDEV_Slider = new System.Windows.Forms.TrackBar();
        this.TB_SPEEV_Slider = new System.Windows.Forms.TrackBar();
        this.L_Happiness = new System.Windows.Forms.Label();
        this.TB_Happiness = new System.Windows.Forms.TextBox();
        ((System.ComponentModel.ISupportInitialize)(this.TB_HPEV_Slider)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_ATKEV_Slider)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_DEFEV_Slider)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPAEV_Slider)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPDEV_Slider)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPEEV_Slider)).BeginInit();
"""

text = text.replace("this.SuspendLayout();", "this.SuspendLayout();\n" + new_controls_init)

new_props = """
        // B_Master
        this.B_Master.Location = new System.Drawing.Point(10, 155);
        this.B_Master.Name = "B_Master";
        this.B_Master.Size = new System.Drawing.Size(80, 23);
        this.B_Master.Text = "Master";
        this.B_Master.UseVisualStyleBackColor = true;
        this.B_Master.Click += new System.EventHandler(this.B_Master_Click);
        // B_MasterAll
        this.B_MasterAll.Location = new System.Drawing.Point(10, 185);
        this.B_MasterAll.Name = "B_MasterAll";
        this.B_MasterAll.Size = new System.Drawing.Size(80, 23);
        this.B_MasterAll.Text = "Master All";
        this.B_MasterAll.UseVisualStyleBackColor = true;
        this.B_MasterAll.Click += new System.EventHandler(this.B_MasterAll_Click);
        
        // B_EnableMega
        this.B_EnableMega.Location = new System.Drawing.Point(10, 205);
        this.B_EnableMega.Name = "B_EnableMega";
        this.B_EnableMega.Size = new System.Drawing.Size(100, 23);
        this.B_EnableMega.Text = "Enable Mega";
        this.B_EnableMega.UseVisualStyleBackColor = true;
        this.B_EnableMega.Click += new System.EventHandler(this.B_EnableMega_Click);
        
        // B_EnableZMove
        this.B_EnableZMove.Location = new System.Drawing.Point(120, 205);
        this.B_EnableZMove.Name = "B_EnableZMove";
        this.B_EnableZMove.Size = new System.Drawing.Size(100, 23);
        this.B_EnableZMove.Text = "Enable Z-Move";
        this.B_EnableZMove.UseVisualStyleBackColor = true;
        this.B_EnableZMove.Click += new System.EventHandler(this.B_EnableZMove_Click);
        
        // TB_HPEV_Slider
        this.TB_HPEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_HPEV_Slider.Name = "TB_HPEV_Slider";
        this.TB_HPEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_HPEV_Slider.Maximum = 252;
        this.TB_HPEV_Slider.TickFrequency = 126;
        this.TB_HPEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);
        // TB_ATKEV_Slider
        this.TB_ATKEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_ATKEV_Slider.Name = "TB_ATKEV_Slider";
        this.TB_ATKEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_ATKEV_Slider.Maximum = 252;
        this.TB_ATKEV_Slider.TickFrequency = 126;
        this.TB_ATKEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);
        // TB_DEFEV_Slider
        this.TB_DEFEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_DEFEV_Slider.Name = "TB_DEFEV_Slider";
        this.TB_DEFEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_DEFEV_Slider.Maximum = 252;
        this.TB_DEFEV_Slider.TickFrequency = 126;
        this.TB_DEFEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);
        // TB_SPAEV_Slider
        this.TB_SPAEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_SPAEV_Slider.Name = "TB_SPAEV_Slider";
        this.TB_SPAEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_SPAEV_Slider.Maximum = 252;
        this.TB_SPAEV_Slider.TickFrequency = 126;
        this.TB_SPAEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);
        // TB_SPDEV_Slider
        this.TB_SPDEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_SPDEV_Slider.Name = "TB_SPDEV_Slider";
        this.TB_SPDEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_SPDEV_Slider.Maximum = 252;
        this.TB_SPDEV_Slider.TickFrequency = 126;
        this.TB_SPDEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);
        // TB_SPEEV_Slider
        this.TB_SPEEV_Slider.Location = new System.Drawing.Point(170, 0);
        this.TB_SPEEV_Slider.Name = "TB_SPEEV_Slider";
        this.TB_SPEEV_Slider.Size = new System.Drawing.Size(85, 20);
        this.TB_SPEEV_Slider.Maximum = 252;
        this.TB_SPEEV_Slider.TickFrequency = 126;
        this.TB_SPEEV_Slider.Scroll += new System.EventHandler(this.SyncEVSlider);

        // L_Happiness
        this.L_Happiness.Location = new System.Drawing.Point(170, 0);
        this.L_Happiness.Name = "L_Happiness";
        this.L_Happiness.Size = new System.Drawing.Size(65, 21);
        this.L_Happiness.Text = "Happiness:";
        this.L_Happiness.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // TB_Happiness
        this.TB_Happiness.Location = new System.Drawing.Point(235, 0);
        this.TB_Happiness.Name = "TB_Happiness";
        this.TB_Happiness.Size = new System.Drawing.Size(25, 20);
"""
text = text.replace("this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);", new_props + "\n        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);")

text = text.replace("this.GB_AIBits.Controls.Add(this.CHK_AI0);", "this.GB_AIBits.Controls.Add(this.CHK_AI0);\n        this.GB_AIBits.Controls.Add(this.B_Master);\n        this.GB_AIBits.Controls.Add(this.B_MasterAll);")
text = text.replace("this.Tab_Trainer.Controls.Add(this.L_Item_3);", "this.Tab_Trainer.Controls.Add(this.L_Item_3);\n        this.Tab_Trainer.Controls.Add(this.B_EnableMega);\n        this.Tab_Trainer.Controls.Add(this.B_EnableZMove);")

text = text.replace("this.FLP_HP.Controls.Add(this.Stat_HP);", "this.FLP_HP.Controls.Add(this.Stat_HP);\n        this.FLP_HP.Controls.Add(this.TB_HPEV_Slider);")
text = text.replace("this.FLP_Atk.Controls.Add(this.Stat_ATK);", "this.FLP_Atk.Controls.Add(this.Stat_ATK);\n        this.FLP_Atk.Controls.Add(this.TB_ATKEV_Slider);")
text = text.replace("this.FLP_Def.Controls.Add(this.Stat_DEF);", "this.FLP_Def.Controls.Add(this.Stat_DEF);\n        this.FLP_Def.Controls.Add(this.TB_DEFEV_Slider);")
text = text.replace("this.FLP_SpA.Controls.Add(this.Stat_SPA);", "this.FLP_SpA.Controls.Add(this.Stat_SPA);\n        this.FLP_SpA.Controls.Add(this.TB_SPAEV_Slider);")
text = text.replace("this.FLP_SpD.Controls.Add(this.Stat_SPD);", "this.FLP_SpD.Controls.Add(this.Stat_SPD);\n        this.FLP_SpD.Controls.Add(this.TB_SPDEV_Slider);")
text = text.replace("this.FLP_Spe.Controls.Add(this.Stat_SPE);", "this.FLP_Spe.Controls.Add(this.Stat_SPE);\n        this.FLP_Spe.Controls.Add(this.TB_SPEEV_Slider);")
text = text.replace("this.FLP_HPType.Controls.Add(this.CB_HPType);", "this.FLP_HPType.Controls.Add(this.CB_HPType);\n        this.FLP_HPType.Controls.Add(this.L_Happiness);\n        this.FLP_HPType.Controls.Add(this.TB_Happiness);")

endinits = """
        ((System.ComponentModel.ISupportInitialize)(this.TB_HPEV_Slider)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_ATKEV_Slider)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_DEFEV_Slider)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPAEV_Slider)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPDEV_Slider)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.TB_SPEEV_Slider)).EndInit();
"""
text = text.replace("this.ResumeLayout(false);", endinits + "\n        this.ResumeLayout(false);")

fields = """
    private System.Windows.Forms.Button B_Master;
    private System.Windows.Forms.Button B_MasterAll;
    private System.Windows.Forms.Button B_EnableMega;
    private System.Windows.Forms.Button B_EnableZMove;
    private System.Windows.Forms.TrackBar TB_HPEV_Slider;
    private System.Windows.Forms.TrackBar TB_ATKEV_Slider;
    private System.Windows.Forms.TrackBar TB_DEFEV_Slider;
    private System.Windows.Forms.TrackBar TB_SPAEV_Slider;
    private System.Windows.Forms.TrackBar TB_SPDEV_Slider;
    private System.Windows.Forms.TrackBar TB_SPEEV_Slider;
    private System.Windows.Forms.Label L_Happiness;
    private System.Windows.Forms.TextBox TB_Happiness;
}"""
text = text.rsplit("}", 1)[0] + fields

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Done")
