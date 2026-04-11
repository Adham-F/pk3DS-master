using pk3DS.Core;
using pk3DS.Core.Structures;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class TypeChart7 : Form
{
    private readonly int offset;
    private readonly string codebin;
    private readonly byte[] chart = new byte[TypeCount * TypeCount];
    private readonly byte[] exefs;
    private readonly string[] types = Main.Config.GetText(TextName.Types);
    private const int TypeCount = 18;
    private const int TypeWidth = 32;

    // Standard Type Order requested by user
    private static readonly int[] DisplayOrder = [0, 1, 2, 3, 4, 5, 6, 7, 16, 9, 10, 11, 12, 13, 14, 15, 17, 18];
    // pk3DS Type IDs: 00:Normal, 01:Fight, 02:Fly, 03:Poison, 04:Ground, 05:Rock, 06:Bug, 07:Ghost, 08:Steel, 09:Fire, 10:Water, 11:Grass, 12:Electric, 13:Psychic, 14:Ice, 15:Dragon, 16:Dark, 17:Fairy
    // User order: Normal, Fight, Fly, Poison, Ground, Rock, Bug, Ghost, Steel, Fire, Water, Grass, Electric, Psychic, Ice, Dragon, Dark, Fairy
    private static readonly int[] UserTypeIDs = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17];

    public TypeChart7()
    {
        if (Main.ExeFSPath == null)
        { WinFormsUtil.Alert("No exeFS code to load."); Close(); }

        string[] files = Directory.GetFiles(Main.ExeFSPath);
        if (!File.Exists(files[0]) || !Path.GetFileNameWithoutExtension(files[0]).Contains("code"))
        { WinFormsUtil.Alert("No .code.bin detected."); Close(); }

        InitializeComponent();

        codebin = files[0];
        exefs = File.ReadAllBytes(codebin);
        if (exefs.Length % 0x200 != 0) { WinFormsUtil.Alert(".code.bin not decompressed. Aborting."); Close(); }
        offset = Util.IndexOfBytes(exefs, Signature, 0x400000, 0) + Signature.Length;

        Array.Copy(exefs, offset, chart, 0, chart.Length);
        LoadSprites();
        PopulateChart();
    }

    private Image typeSprites;
    private void LoadSprites()
    {
        string path = Path.Combine(Application.StartupPath, "Resources", "img", "type_sprites.png");
        if (!File.Exists(path))
            path = Path.Combine(Application.StartupPath, "type_sprites.png"); // Fallback
        
        if (File.Exists(path)) 
            typeSprites = Image.FromFile(path);
    }

    private readonly byte[] Signature =
    [
        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
        0xC3, 0x00, 0x00, 0x00, 0xCB, 0x00, 0x00, 0x00, 0xD3, 0x00, 0x00, 0x00, 0xDB, 0x00, 0x00, 0x00,
        0xF3, 0x00, 0x00, 0x00, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
    ];

    private void PopulateChart()
    {
        // Generate the grid based on PK3DS Type IDs (internal storage order)
        var baseGrid = TypeChart.GetGrid(TypeWidth, TypeCount, chart);
        var finalBmp = new Bitmap(baseGrid.Width, baseGrid.Height);
        using (var g = Graphics.FromImage(finalBmp))
        {
            g.DrawImage(baseGrid, 0, 0);
            var font = new Font("Segoe UI", 8F, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            
            for (int i = 0; i < chart.Length; i++)
            {
                int X = i % TypeCount;
                int Y = i / TypeCount;
                byte val = chart[i];
                string symbol = "";
                Color color = Color.White;

                switch (val)
                {
                    case 0: symbol = "0"; color = Color.FromArgb(255, 100, 100); break; // No effect
                    case 2: symbol = "½"; color = Color.FromArgb(255, 200, 100); break; // NVE
                    case 8: symbol = "x2"; color = Color.FromArgb(100, 255, 100); break; // SE
                }

                if (!string.IsNullOrEmpty(symbol))
                {
                    var rect = new Rectangle(X * TypeWidth, Y * TypeWidth, TypeWidth, TypeWidth);
                    g.DrawString(symbol, font, new SolidBrush(color), rect, sf);
                }
            }
        }
        PB_Chart.Image = finalBmp;
    }

    private void B_Save_Click(object sender, EventArgs e)
    {
        chart.CopyTo(exefs, offset);
        File.WriteAllBytes(codebin, exefs);
        Close();
    }

    private void B_Cancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void MoveMouse(object sender, MouseEventArgs e)
    {
        TypeChart6.GetCoordinate((PictureBox)sender, e, out int X, out int Y);
        int index = (Y * TypeCount) + X;
        if (index >= chart.Length)
            return;
        UpdateLabel(X, Y, chart[index]);
    }

    private void ClickMouse(object sender, MouseEventArgs e)
    {
        TypeChart6.GetCoordinate((PictureBox)sender, e, out int X, out int Y);
        int index = (Y * TypeCount) + X;
        if (index >= chart.Length)
            return;

        chart[index] = TypeChart6.ToggleEffectiveness(chart[index], e.Button == MouseButtons.Left);

        UpdateLabel(X, Y, chart[index]);
        PopulateChart();
    }

    private void UpdateLabel(int X, int Y, int value)
    {
        if (value >= effects.Length || X >= types.Length || Y >= types.Length)
            return; 
        L_Hover.Text = $"[{X:00}x{Y:00}: {value:00}] {types[Y]} attacking {types[X]} {effects[value]}";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (typeSprites == null) return;

        var g = e.Graphics;
        
        // Lock grid slots to exactly the TypeWidth (32px) to prevent scaling drift
        int slotWidth = TypeWidth;
        int slotHeight = TypeWidth;

        // Draw Defender icons (Top row)
        for (int i = 0; i < TypeCount; i++)
        {
            Rectangle src = GetSpriteRect(i);
            if (src == Rectangle.Empty) continue;

            int x = PB_Chart.Left + (i * slotWidth) + (slotWidth - src.Width) / 2;
            Rectangle dest = new Rectangle(x, PB_Chart.Top - src.Height - 3, src.Width, src.Height);
            g.DrawImage(typeSprites, dest, src, GraphicsUnit.Pixel);
        }

        // Draw Attacker icons (Left column)
        for (int i = 0; i < TypeCount; i++)
        {
            Rectangle src = GetSpriteRect(i);
            if (src == Rectangle.Empty) continue;

            int y = PB_Chart.Top + (i * slotHeight) + (slotHeight - src.Height) / 2;
            Rectangle dest = new Rectangle(PB_Chart.Left - src.Width - 3, y, src.Width, src.Height);
            g.DrawImage(typeSprites, dest, src, GraphicsUnit.Pixel);
        }
    }

    private Rectangle GetSpriteRect(int typeId)
    {
        if (typeSprites == null) return Rectangle.Empty;
        
        // Auto-detect based on the sprite sheet width
        if (typeSprites.Width >= 576) 
        {
            // Horizontal strip (18 types * 32px)
            return new Rectangle(typeId * 32, 0, 32, 14);
        } 
        else if (typeSprites.Width == 192) 
        {
            // 2D Grid (6 columns * 32px = 192px width)
            int col = typeId % 6;
            int row = typeId / 6;
            return new Rectangle(col * 32, row * 14, 32, 14);
        }
        else 
        {
            // Vertical strip (1 column * 32px)
            return new Rectangle(0, typeId * 14, 32, 14);
        }
    }
    
    private readonly string[] effects =
    [
        "has no effect!",
        "",
        "is not very effective.",
        "",
        "does regular damage.",
        "", "", "",
        "is super effective!",
    ];
}