using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core;

namespace pk3DS.WinForms
{
    public partial class ShowdownSetStorage : Form
    {
        private readonly List<string> _sets;
        private Button B_ClearAll;
        public ShowdownSetStorage()
        {
            InitializeComponent();
            _sets = ShowdownSetManager.GetSetListStrings().ToList();
            LB_Sets.Items.AddRange(_sets.ToArray());
            L_Count.Text = $"Total Sets: {_sets.Count}";
            WinFormsUtil.ApplyCyberSlateTheme(this, WinFormsUtil.VisualTheme.Grey);
            ApplyLayoutFixes();
        }

        private void ApplyLayoutFixes()
        {
            L_Count.Top = 42;
            LB_Sets.Top = 65;
            RTB_Preview.Top = 65;
            LB_Sets.Height = this.ClientSize.Height - 120;
            RTB_Preview.Height = LB_Sets.Height - 40;
            B_Copy.Top = RTB_Preview.Bottom + 5;
            B_Copy.Left = RTB_Preview.Left + (RTB_Preview.Width - B_Copy.Width) / 2;

            int btnY = this.ClientSize.Height - 45;
            Size bSize = new Size(115, 32);
            B_Add.Size = B_Delete.Size = B_Use.Size = B_Close.Size = bSize;

            B_Add.Location = new Point(12, btnY);
            B_Delete.Location = new Point(135, btnY);
            B_Use.Location = new Point(258, btnY);
            B_Close.Location = new Point(381, btnY);

            B_ClearAll = new Button { Text = "Clear All", Size = bSize, Location = new Point(504, btnY) };
            B_ClearAll.Click += B_ClearAll_Click;
            this.Controls.Add(B_ClearAll);
        }

        private void LB_Sets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LB_Sets.SelectedIndex < 0)
            {
                RTB_Preview.Clear();
                L_Count.Text = $"Total Sets: {LB_Sets.Items.Count}";
                return;
            }
            L_Count.Text = $"Selected Set: {LB_Sets.SelectedIndex + 1} / {LB_Sets.Items.Count}";
            RTB_Preview.Text = ShowdownSetManager.GetSetText(LB_Sets.SelectedIndex);
        }


        private void B_Add_Click(object sender, EventArgs e)
        {
            string text = Clipboard.GetText().Trim();
            if (string.IsNullOrWhiteSpace(text)) { WinFormsUtil.Alert("Clipboard is empty!"); return; }

            var parts = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            int added = 0;
            foreach (var part in parts)
            {
                string p = part.Trim();
                if (string.IsNullOrWhiteSpace(p)) continue;
                string name = ShowdownSetManager.GetNickname(p);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = WinFormsUtil.PromptInput($"Enter a nickname for this Pokémon:\n{p.Split('\n')[0]}", "Add Showdown Set");
                    if (name == null) break;
                }
                ShowdownSetManager.AddSet(p, name);
                added++;
            }
            if (added > 0) { RefreshList(); WinFormsUtil.Alert($"Added {added} set(s)!"); }
        }

        private void B_ClearAll_Click(object sender, EventArgs e)
        {
            if (LB_Sets.Items.Count == 0) return;
            if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Are you sure you want to delete ALL sets?") != DialogResult.Yes) return;
            ShowdownSetManager.ClearAll();
            RefreshList();
        }

        private void B_Delete_Click(object sender, EventArgs e)
        {
            if (LB_Sets.SelectedIndex < 0) return;
            if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Delete this set?") != DialogResult.Yes) return;

            ShowdownSetManager.RemoveSet(LB_Sets.SelectedIndex);
            RefreshList();
        }

        private void RefreshList()
        {
            LB_Sets.Items.Clear();
            var sets = ShowdownSetManager.GetSetListStrings();
            LB_Sets.Items.AddRange(sets.ToArray());
            L_Count.Text = $"Total Sets: {sets.Length}";
            RTB_Preview.Clear();
        }

        private void B_Copy_Click(object sender, EventArgs e)
        {
            if (LB_Sets.SelectedIndex < 0) return;
            Clipboard.SetText(ShowdownSetManager.GetSetText(LB_Sets.SelectedIndex));
            WinFormsUtil.Alert("Set copied to clipboard!");
        }

        public string SelectedSet { get; private set; }
        private void B_Use_Click(object sender, EventArgs e)
        {
            if (LB_Sets.SelectedIndex < 0) return;
            SelectedSet = ShowdownSetManager.GetSetText(LB_Sets.SelectedIndex);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void B_Close_Click(object sender, EventArgs e) => Close();
    }
}
