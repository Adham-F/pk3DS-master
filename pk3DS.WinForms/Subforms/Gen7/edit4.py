import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Expand Form
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(650, 520);", text)

# Team Lineup
text = re.sub(r"this\.L_Team\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Team.Location = new System.Drawing.Point(12, 15);", text)
text = re.sub(r"this\.PB_Team1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team1.Location = new System.Drawing.Point(60, 10);", text)
text = re.sub(r"this\.PB_Team2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team2.Location = new System.Drawing.Point(110, 10);", text)
text = re.sub(r"this\.PB_Team3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team3.Location = new System.Drawing.Point(160, 10);", text)
text = re.sub(r"this\.PB_Team4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team4.Location = new System.Drawing.Point(210, 10);", text)
text = re.sub(r"this\.PB_Team5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team5.Location = new System.Drawing.Point(260, 10);", text)
text = re.sub(r"this\.PB_Team6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.PB_Team6.Location = new System.Drawing.Point(310, 10);", text)

# Expand and Reposition Tabs
text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 80);", text)
text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(310, 310);", text)
text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trdata.Location = new System.Drawing.Point(330, 80);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(300, 310);", text)

# Reposition the Trainer Selectors
text = re.sub(r"this\.L_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerID.Location = new System.Drawing.Point(12, 400);", text)
text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(12, 415);", text)

text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(12, 445);", text)
text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(12, 460);", text)

text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(180, 445);", text)
text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(180, 460);", text)

# Fix Happiness and Hidden Power
text = re.sub(r"this\.L_HPType\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_HPType.Location = new System.Drawing.Point(10, 220);", text)
text = re.sub(r"this\.CB_HPType\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_HPType.Location = new System.Drawing.Point(100, 217);", text)

text = re.sub(r"this\.L_Happiness\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Happiness.Location = new System.Drawing.Point(10, 250);", text)
text = re.sub(r"this\.TB_Happiness\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_Happiness.Location = new System.Drawing.Point(100, 247);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
