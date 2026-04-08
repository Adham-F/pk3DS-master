import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# Widen GB_AIBits
text = re.sub(r"this\.GB_AIBits\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.GB_AIBits.Size = new System.Drawing.Size(290, 145);", text)

# Column 1
text = re.sub(r"this\.CHK_AI0\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI0.Location = new System.Drawing.Point(10, 20);", text)
text = re.sub(r"this\.CHK_AI1\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI1.Location = new System.Drawing.Point(10, 45);", text)
text = re.sub(r"this\.CHK_AI2\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI2.Location = new System.Drawing.Point(10, 70);", text)
text = re.sub(r"this\.CHK_AI3\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI3.Location = new System.Drawing.Point(10, 95);", text)
text = re.sub(r"this\.B_Master\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_Master.Location = new System.Drawing.Point(10, 118);", text)
text = re.sub(r"this\.B_Master\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_Master.Size = new System.Drawing.Size(120, 23);", text)

# Column 2
text = re.sub(r"this\.CHK_AI4\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI4.Location = new System.Drawing.Point(145, 20);", text)
text = re.sub(r"this\.CHK_AI5\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI5.Location = new System.Drawing.Point(145, 45);", text)
text = re.sub(r"this\.CHK_AI6\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI6.Location = new System.Drawing.Point(145, 70);", text)
text = re.sub(r"this\.CHK_AI7\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.CHK_AI7.Location = new System.Drawing.Point(145, 95);", text)
text = re.sub(r"this\.B_MasterAll\.Location = new System\.Drawing\.Point\(-?\d+, -?\d+\);", "this.B_MasterAll.Location = new System.Drawing.Point(145, 118);", text)
text = re.sub(r"this\.B_MasterAll\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.B_MasterAll.Size = new System.Drawing.Size(120, 23);", text)

# TC_trdata size minimum check
# Let's just set it directly to what they requested as minimum, maybe 350x340 if it was currently 350x320
# Actually, the user says "Ensure TC_trdata.Size is at least new System.Drawing.Size(325, 340)"
# We can just set it to System.Drawing.Size(350, 340) to be safe since it was w=350 before
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(350, 340);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
