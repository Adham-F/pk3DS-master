import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Widen Form
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(760, 500);", text)

# Expand TabControls
text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 80);", text)
text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(350, 340);", text)

text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TC_trdata.Location = new System.Drawing.Point(370, 80);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(370, 340);", text)

# Expand AI Bits
text = re.sub(r"this\.GB_AIBits\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.GB_AIBits.Size = new System.Drawing.Size(340, 145);", text)

# Column 1 to X=10
text = re.sub(r"this\.CHK_AI0\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI0.Location = new System.Drawing.Point(10, 20);", text)
text = re.sub(r"this\.CHK_AI1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI1.Location = new System.Drawing.Point(10, 45);", text)
text = re.sub(r"this\.CHK_AI2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI2.Location = new System.Drawing.Point(10, 70);", text)
text = re.sub(r"this\.CHK_AI3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI3.Location = new System.Drawing.Point(10, 95);", text)

text = re.sub(r"this\.B_Master\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_Master.Location = new System.Drawing.Point(10, 118);", text)
text = re.sub(r"this\.B_Master\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_Master.Size = new System.Drawing.Size(120, 23);", text)

# Column 2 to X=170
text = re.sub(r"this\.CHK_AI4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI4.Location = new System.Drawing.Point(170, 20);", text)
text = re.sub(r"this\.CHK_AI5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI5.Location = new System.Drawing.Point(170, 45);", text)
text = re.sub(r"this\.CHK_AI6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI6.Location = new System.Drawing.Point(170, 70);", text)
text = re.sub(r"this\.CHK_AI7\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI7.Location = new System.Drawing.Point(170, 95);", text)

text = re.sub(r"this\.B_MasterAll\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_MasterAll.Location = new System.Drawing.Point(170, 118);", text)
text = re.sub(r"this\.B_MasterAll\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_MasterAll.Size = new System.Drawing.Size(120, 23);", text)

# Re-align Trainer Selectors
text = re.sub(r"this\.L_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerID.Location = new System.Drawing.Point(12, 435);", text)
text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(12, 450);", text)

text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(200, 435);", text)
text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(200, 450);", text)

text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(390, 435);", text)
text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(390, 450);", text)


with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
