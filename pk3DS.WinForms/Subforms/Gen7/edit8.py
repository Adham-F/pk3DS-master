import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Restore GB_AIBits
text = re.sub(r"this\.GB_AIBits\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.GB_AIBits.Size = new System.Drawing.Size(150, 215);", text)

# Restore CheckBoxes
text = re.sub(r"this\.CHK_AI0\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI0.Location = new System.Drawing.Point(13, 35);", text)
text = re.sub(r"this\.CHK_AI1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI1.Location = new System.Drawing.Point(13, 49);", text)
text = re.sub(r"this\.CHK_AI2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI2.Location = new System.Drawing.Point(13, 63);", text)
text = re.sub(r"this\.CHK_AI3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI3.Location = new System.Drawing.Point(13, 77);", text)
text = re.sub(r"this\.CHK_AI4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI4.Location = new System.Drawing.Point(13, 91);", text)
text = re.sub(r"this\.CHK_AI5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI5.Location = new System.Drawing.Point(13, 105);", text)
text = re.sub(r"this\.CHK_AI6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI6.Location = new System.Drawing.Point(13, 119);", text)
text = re.sub(r"this\.CHK_AI7\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI7.Location = new System.Drawing.Point(13, 133);", text)

# Restore Buttons
text = re.sub(r"this\.B_Master\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_Master.Location = new System.Drawing.Point(10, 155);", text)
text = re.sub(r"this\.B_Master\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_Master.Size = new System.Drawing.Size(80, 23);", text)

text = re.sub(r"this\.B_MasterAll\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_MasterAll.Location = new System.Drawing.Point(10, 185);", text)
text = re.sub(r"this\.B_MasterAll\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_MasterAll.Size = new System.Drawing.Size(80, 23);", text)

# Restore TC_trdata Size
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(350, 320);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Reversal complete.")
