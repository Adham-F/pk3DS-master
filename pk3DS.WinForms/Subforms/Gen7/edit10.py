import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# 1. Reposition GB_AIBits to avoid clipping on the right
text = re.sub(r"this\.GB_AIBits\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.GB_AIBits.Location = new System.Drawing.Point(175, 2);", text)

# 2. Relocate Battle Mode controls underneath GB_AIBits
text = re.sub(r"this\.L_Mode\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_Mode.Location = new System.Drawing.Point(175, 222);", text)
text = re.sub(r"this\.CB_Mode\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_Mode.Location = new System.Drawing.Point(245, 219);", text)

# Adjust CB_Mode size just in case, typical combo box is ~90 to 100 wide
# But let's just make sure we are not touching anything else.

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Fix applied.")
