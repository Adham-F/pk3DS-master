import re

path = r"c:\Users\fulto\Downloads\3DS\pk3DS-master\pk3DS.WinForms\Subforms\Gen7\SMTE.Designer.cs"
with open(path, "r", encoding="utf-8-sig") as f:
    text = f.read()

# 1. Resize Form and TabControls
text = re.sub(r"this\.ClientSize = new System\.Drawing\.Size\(\d+, \d+\);", "this.ClientSize = new System.Drawing.Size(620, 500);", text)

text = re.sub(r"this\.TC_trpoke\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TC_trpoke.Location = new System.Drawing.Point(12, 85);", text)
text = re.sub(r"this\.TC_trpoke\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trpoke.Size = new System.Drawing.Size(280, 290);", text)

text = re.sub(r"this\.TC_trdata\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TC_trdata.Location = new System.Drawing.Point(300, 85);", text)
text = re.sub(r"this\.TC_trdata\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TC_trdata.Size = new System.Drawing.Size(300, 290);", text)

# 2. Add L_TrainerID if it doesn't exist
if "L_TrainerID" not in text:
    text = text.replace("private System.Windows.Forms.ComboBox CB_TrainerID;", "private System.Windows.Forms.ComboBox CB_TrainerID;\n    private System.Windows.Forms.Label L_TrainerID;")
    
    init_lbl = "this.L_TrainerID = new System.Windows.Forms.Label();\n        "
    text = text.replace("this.CB_TrainerID = new System.Windows.Forms.ComboBox();", init_lbl + "this.CB_TrainerID = new System.Windows.Forms.ComboBox();")
    
    props = """// 
        // L_TrainerID
        // 
        this.L_TrainerID.AutoSize = true;
        this.L_TrainerID.Name = "L_TrainerID";
        this.L_TrainerID.Size = new System.Drawing.Size(57, 13);
        this.L_TrainerID.TabIndex = 65;
        this.L_TrainerID.Text = "Trainer ID:";
        """
    text = text.replace("// \n        // CB_TrainerID", props + "// \n        // CB_TrainerID")
    
    # We will add it to this.Controls down below, next to CB_TrainerID
    text = text.replace("this.Controls.Add(this.CB_TrainerID);", "this.Controls.Add(this.CB_TrainerID);\n        this.Controls.Add(this.L_TrainerID);")

# 3. Reposition the Trainer Selectors (Bottom)
text = re.sub(r"this\.CB_TrainerID\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_TrainerID.Location = new System.Drawing.Point(12, 385);", text)

if "this.L_TrainerID.Location" in text:
    text = re.sub(r"this\.L_TrainerID\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_TrainerID.Location = new System.Drawing.Point(12, 370);", text)
else:
    text = text.replace('this.L_TrainerID.Name = "L_TrainerID";', 'this.L_TrainerID.Location = new System.Drawing.Point(12, 370);\n        this.L_TrainerID.Name = "L_TrainerID";')

text = re.sub(r"this\.CB_Trainer_Class\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_Trainer_Class.Location = new System.Drawing.Point(12, 425);", text)
text = re.sub(r"this\.L_Trainer_Class\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_Trainer_Class.Location = new System.Drawing.Point(12, 410);", text)

text = re.sub(r"this\.TB_TrainerName\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TB_TrainerName.Location = new System.Drawing.Point(200, 425);", text)
text = re.sub(r"this\.L_TrainerName\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_TrainerName.Location = new System.Drawing.Point(200, 410);", text)

# 4. Check Event Binding
if "this.CB_TrainerID.SelectedIndexChanged +=" not in text:
    # it might have been placed correctly. We know from earlier inspection it was.
    # But just in case:
    print("Warning: Event binding might be missing")

# 5. Restore Hidden Power and Happiness (Stats Tab)
# Rename Label_HiddenPowerPrefix to L_HPType
text = text.replace("Label_HiddenPowerPrefix", "L_HPType")

# Remove from FLP_HPType if they are there
text = text.replace("this.FLP_HPType.Controls.Add(this.L_HPType);\n", "")
text = text.replace("this.FLP_HPType.Controls.Add(this.CB_HPType);\n", "")

# We can completely remove FLP_HPType from FLP_Stats
text = text.replace("this.FLP_Stats.Controls.Add(this.FLP_HPType);\n", "")

# Add directly to Tab_Stats.Controls
# First, wipe existing additions to Tab_Stats if they involve L_Happiness to avoid duplicates 
# from our previous script.
text = re.sub(r"this\.Tab_Stats\.Controls\.Add\(this\.FLP_Stats\);\s*(this\.Tab_Stats\.Controls\.Add[^\n]+\n\s*)*", 
              "this.Tab_Stats.Controls.Add(this.FLP_Stats);\n        this.Tab_Stats.Controls.Add(this.L_HPType);\n        this.Tab_Stats.Controls.Add(this.CB_HPType);\n        this.Tab_Stats.Controls.Add(this.L_Happiness);\n        this.Tab_Stats.Controls.Add(this.TB_Happiness);\n", 
              text)

# Set their locations and size
text = re.sub(r"this\.L_HPType\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_HPType.Location = new System.Drawing.Point(10, 215);", text)
text = re.sub(r"this\.CB_HPType\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.CB_HPType.Location = new System.Drawing.Point(100, 212);", text)

text = re.sub(r"this\.L_Happiness\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.L_Happiness.Location = new System.Drawing.Point(10, 245);", text)
text = re.sub(r"this\.TB_Happiness\.Location = new System\.Drawing\.Point\(\d+, \d+\);", "this.TB_Happiness.Location = new System.Drawing.Point(100, 242);", text)
text = re.sub(r"this\.TB_Happiness\.Size = new System\.Drawing\.Size\(\d+, \d+\);", "this.TB_Happiness.Size = new System.Drawing.Size(50, 20);", text)

with open(path, "w", encoding="utf-8-sig") as f:
    f.write(text)

print("Replacement complete.")
