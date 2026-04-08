import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Shrink Form
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(680, 450);", text)

# Compact Team Lineup
text = re.sub(r"this\.L_Team\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Team.Location = new System.Drawing.Point(12, 25);", text)
text = re.sub(r"this\.PB_Team1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team1.Location = new System.Drawing.Point(60, 10);", text)
text = re.sub(r"this\.PB_Team2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team2.Location = new System.Drawing.Point(130, 10);", text)
text = re.sub(r"this\.PB_Team3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team3.Location = new System.Drawing.Point(200, 10);", text)
text = re.sub(r"this\.PB_Team4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team4.Location = new System.Drawing.Point(270, 10);", text)
text = re.sub(r"this\.PB_Team5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team5.Location = new System.Drawing.Point(340, 10);", text)
text = re.sub(r"this\.PB_Team6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team6.Location = new System.Drawing.Point(410, 10);", text)

# Compact Tabs
text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 80);", text)
text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(325, 290);", text)

text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trdata.Location = new System.Drawing.Point(345, 80);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(325, 290);", text)

# Arrange Trainer Selectors Horizontally
text = re.sub(r"this\.L_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerID.Location = new System.Drawing.Point(12, 385);", text)
text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(12, 400);", text)

text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(180, 385);", text)
text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(180, 400);", text)

text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(350, 385);", text)
text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(350, 400);", text)

# Side-by-Side Hidden Power and Happiness
text = re.sub(r"this\.L_HPType\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_HPType.Location = new System.Drawing.Point(10, 220);", text)
text = re.sub(r"this\.CB_HPType\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_HPType.Location = new System.Drawing.Point(95, 217);", text)

text = re.sub(r"this\.L_Happiness\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Happiness.Location = new System.Drawing.Point(170, 220);", text)
text = re.sub(r"this\.TB_Happiness\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_Happiness.Location = new System.Drawing.Point(235, 217);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
