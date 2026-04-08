import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Widen Form
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(750, 550);", text)

# Team Lineup
text = re.sub(r"this\.L_Team\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Team.Location = new System.Drawing.Point(12, 30);", text)
text = re.sub(r"this\.PB_Team1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team1.Location = new System.Drawing.Point(70, 10);", text)
text = re.sub(r"this\.PB_Team2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team2.Location = new System.Drawing.Point(160, 10);", text)
text = re.sub(r"this\.PB_Team3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team3.Location = new System.Drawing.Point(250, 10);", text)
text = re.sub(r"this\.PB_Team4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team4.Location = new System.Drawing.Point(340, 10);", text)
text = re.sub(r"this\.PB_Team5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team5.Location = new System.Drawing.Point(430, 10);", text)
text = re.sub(r"this\.PB_Team6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team6.Location = new System.Drawing.Point(520, 10);", text)

# Widen and Reposition Tabs
text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 100);", text)
text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(360, 320);", text)

text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trdata.Location = new System.Drawing.Point(380, 100);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(350, 320);", text)

# Reposition Trainer Selectors
text = re.sub(r"this\.L_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerID.Location = new System.Drawing.Point(12, 435);", text)
text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(12, 450);", text)

text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(12, 485);", text)
text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(12, 500);", text)

text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(180, 485);", text)
text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(180, 500);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
