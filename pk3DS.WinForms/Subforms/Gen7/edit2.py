import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# 1. Resize form
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(600, 480);", text)

# 2. PB_Team locations
text = re.sub(r"this\.PB_Team1\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team1.Location = new System.Drawing.Point(12, 10);", text)
text = re.sub(r"this\.PB_Team2\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team2.Location = new System.Drawing.Point(62, 10);", text)
text = re.sub(r"this\.PB_Team3\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team3.Location = new System.Drawing.Point(112, 10);", text)
text = re.sub(r"this\.PB_Team4\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team4.Location = new System.Drawing.Point(162, 10);", text)
text = re.sub(r"this\.PB_Team5\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team5.Location = new System.Drawing.Point(212, 10);", text)
text = re.sub(r"this\.PB_Team6\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.PB_Team6.Location = new System.Drawing.Point(262, 10);", text)

# 3. Y coordinates of TabControls + Resizing they and their TabPages
text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 75);", text)
text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TC_trdata.Location = new System.Drawing.Point(320, 75);", text)

text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(302, 335);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(371, 335);", text)
text = re.sub(r"this\.Tab_Main\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.Tab_Main.Size = new System.Drawing.Size(294, 309);", text)
text = re.sub(r"this\.Tab_Stats\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.Tab_Stats.Size = new System.Drawing.Size(294, 309);", text)
text = re.sub(r"this\.Tab_Moves\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.Tab_Moves.Size = new System.Drawing.Size(294, 309);", text)
text = re.sub(r"this\.Tab_Trainer\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.Tab_Trainer.Size = new System.Drawing.Size(363, 309);", text)

# 4. Remove Trainer controls from Tab_Trainer and add to Form
text = text.replace("this.Tab_Trainer.Controls.Add(this.TB_TrainerName);\n", "")
text = text.replace("this.Tab_Trainer.Controls.Add(this.L_Trainer_Class);\n", "")
text = text.replace("this.Tab_Trainer.Controls.Add(this.CB_Trainer_Class);\n", "")
text = text.replace("this.Tab_Trainer.Controls.Add(this.L_TrainerName);\n", "")

text = text.replace("this.Controls.Add(this.CB_TrainerID);", "this.Controls.Add(this.CB_TrainerID);\n        this.Controls.Add(this.TB_TrainerName);\n        this.Controls.Add(this.L_Trainer_Class);\n        this.Controls.Add(this.CB_Trainer_Class);\n        this.Controls.Add(this.L_TrainerName);")

text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(430, 420);", text)
text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(12, 423);", text)
text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(90, 420);", text)
text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(170, 423);", text)
text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(245, 420);", text)

# 5. Fix Happiness controls (Remove from FLP_HPType, Add to Tab_Stats.Controls directly)
text = text.replace("this.FLP_HPType.Controls.Add(this.L_Happiness);\n", "")
text = text.replace("this.FLP_HPType.Controls.Add(this.TB_Happiness);\n", "")
text = text.replace("this.Tab_Stats.Controls.Add(this.FLP_Stats);", "this.Tab_Stats.Controls.Add(this.FLP_Stats);\n        this.Tab_Stats.Controls.Add(this.L_Happiness);\n        this.Tab_Stats.Controls.Add(this.TB_Happiness);")

# Position them at X=15, Y=260 inside Tab_Stats
text = re.sub(r"this\.L_Happiness\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_Happiness.Location = new System.Drawing.Point(15, 260);", text)
text = re.sub(r"this\.TB_Happiness\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TB_Happiness.Location = new System.Drawing.Point(85, 258);", text)

# Make sure Width of TB_Happiness is 50
text = re.sub(r"this\.TB_Happiness\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TB_Happiness.Size = new System.Drawing.Size(50, 20);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
