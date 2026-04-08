using pk3DS.Core;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class TutorEditor7 : Form
{
    private readonly string CROPath = Path.Combine(Main.RomFSPath, "Shop.cro");

    // The signature string "_on_off\xFF" that precedes the tutor move table in code.bin
    private static readonly byte[] CodeBinTutorSignature = new byte[] 
    { 
        0x5F, 0x6F, 0x6E, 0x5F, 0x6F, 0x66, 0x66, 0xFF
    };

    private int cachedTutorOffset = -1;

    public TutorEditor7()
    {
        if (!File.Exists(CROPath))
        {
            WinFormsUtil.Error("CRO does not exist! Closing.", CROPath);
            Close();
        }
        InitializeComponent();

        data = File.ReadAllBytes(CROPath);
        len_BPTutor = data.Skip(0x52D2).Take(4).ToArray();

        SetupDGV();
        CB_LocationBPMove.Items.AddRange(locationsTutor);
        CB_LocationBPMove.SelectedIndex = 0;
    }

    private const int ofs_BPTutor = 0x54DE;
    private readonly byte[] len_BPTutor;

    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private readonly byte[] data;

    private readonly string[] locationsTutor =
    [
        "Big Wave Beach",
        "Heahea Beach",
        "Ula'ula Beach",
        "Battle Tree",
    ];

    private void B_Save_Click(object sender, EventArgs e)
    {
        if (entryBPMove > -1) SetListBPMove();

        // 1. Compile the synchronized array of all current tutor moves across all beaches
        int totalMoves = len_BPTutor.Sum(z => z);
        ushort[] currentTutorMoves = new ushort[totalMoves];
        int ofs = ofs_BPTutor;
        for (int i = 0; i < totalMoves; i++)
        {
            currentTutorMoves[i] = BitConverter.ToUInt16(data, ofs + (4 * i));
        }

        // 2. Push the array to code.bin to align with Personal bitflags
        SyncTutorsToCodeBin(currentTutorMoves);

        // 3. Save the Shop.cro to update the in-game NPC menus
        File.WriteAllBytes(CROPath, data);
        Close();
    }

    private void SyncTutorsToCodeBin(ushort[] currentTutorMoves)
    {
        // Handle variations in unpacker naming conventions using ExeFSPath
        string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
        string fullCodePath = Path.Combine(Main.ExeFSPath, binName);
        if (!File.Exists(fullCodePath)) return;

        byte[] codeBin = File.ReadAllBytes(fullCodePath);
        string offsetFile = Path.Combine(Main.ExeFSPath, "tutor_offset.txt");

        // Load cached offset to survive app restarts and overwritten vanilla patterns
        if (cachedTutorOffset <= 0 && File.Exists(offsetFile))
        {
            if (int.TryParse(File.ReadAllText(offsetFile), out int savedOfs))
                cachedTutorOffset = savedOfs;
        }

        // Scan for the '_on_off\xFF' signature. The tutor table starts right after it.
        if (cachedTutorOffset <= 0)
        {
            int sigIdx = -1;
            for (int i = 0; i < codeBin.Length - CodeBinTutorSignature.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < CodeBinTutorSignature.Length; j++)
                {
                    if (codeBin[i + j] != CodeBinTutorSignature[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) 
                {
                    sigIdx = i;
                    break;
                }
            }
            if (sigIdx >= 0)
            {
                cachedTutorOffset = sigIdx + CodeBinTutorSignature.Length;
                File.WriteAllText(offsetFile, cachedTutorOffset.ToString());
            }
        }

        if (cachedTutorOffset > 0)
        {
            // Write tutor moves at 2-byte stride (plain ushort array in code.bin)
            for (int i = 0; i < currentTutorMoves.Length; i++)
            {
                byte[] moveBytes = BitConverter.GetBytes(currentTutorMoves[i]);
                codeBin[cachedTutorOffset + (i * 2)] = moveBytes[0];
                codeBin[cachedTutorOffset + (i * 2) + 1] = moveBytes[1];
            }
            File.WriteAllBytes(fullCodePath, codeBin);
        }
        else
        {
            WinFormsUtil.Alert("Failed to locate Tutor Table in code.bin.", "Ensure code.bin contains the '_on_off' marker string.");
        }
    }

    private void B_Cancel_Click(object sender, EventArgs e) => Close();

    private void SetupDGV()
    {
        dgvmvMove.Items.AddRange(movelist); 
    }

    private int entryBPMove = -1;

    private void ChangeIndexBPMove(object sender, EventArgs e)
    {
        if (entryBPMove > -1) SetListBPMove();
        entryBPMove = CB_LocationBPMove.SelectedIndex;
        GetListBPMove();
    }

    private void GetListBPMove()
    {
        dgvmv.Rows.Clear();
        int count = len_BPTutor[entryBPMove];
        dgvmv.Rows.Add(count);
        var ofs = ofs_BPTutor + (len_BPTutor.Take(entryBPMove).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            dgvmv.Rows[i].Cells[0].Value = i.ToString();
            dgvmv.Rows[i].Cells[1].Value = movelist[BitConverter.ToUInt16(data, ofs + (4 * i))];
            dgvmv.Rows[i].Cells[2].Value = BitConverter.ToUInt16(data, ofs + (4 * i) + 2).ToString();
        }
    }

    private void SetListBPMove()
    {
        int count = dgvmv.Rows.Count;
        var ofs = ofs_BPTutor + (len_BPTutor.Take(entryBPMove).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            int item = Array.IndexOf(movelist, dgvmv.Rows[i].Cells[1].Value);
            Array.Copy(BitConverter.GetBytes((ushort)item), 0, data, ofs + (4 * i), 2);
            string p = dgvmv.Rows[i].Cells[2].Value.ToString();
            if (int.TryParse(p, out var price))
                Array.Copy(BitConverter.GetBytes((ushort)price), 0, data, ofs + (4 * i) + 2, 2);
        }
    }

    private void B_Randomize_Click(object sender, EventArgs e)
    {
        WinFormsUtil.Alert("Not currently implemented.");
    }

    public static int[] GetUSUMTutors(string croPath, int[] fallback)
    {
        if (!File.Exists(croPath)) return fallback;
        try
        {
            byte[] fileData = File.ReadAllBytes(croPath);
            byte[] tutorLengths = fileData.Skip(0x52D2).Take(4).ToArray();
            int totalMoves = tutorLengths.Sum(z => z);
            
            if (totalMoves == 0) return fallback;
            
            int[] tutors = new int[totalMoves];
            int ofs = 0x54DE; 
            for (int i = 0; i < totalMoves; i++)
            {
                tutors[i] = BitConverter.ToUInt16(fileData, ofs + (4 * i));
            }
            return tutors;
        }
        catch 
        { 
            return fallback; 
        }
    }

    private void B_ExportTxt_Click(object sender, EventArgs e)
    {
        if (entryBPMove > -1) SetListBPMove();

        var sfd = new SaveFileDialog { FileName = "Tutors.txt", Filter = "Text File|*.txt" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var lines = new System.Collections.Generic.List<string>();
        for (int loc = 0; loc < locationsTutor.Length; loc++)
        {
            lines.Add($"=== {locationsTutor[loc]} ===");
            int count = len_BPTutor[loc];
            var ofs = ofs_BPTutor + (len_BPTutor.Take(loc).Sum(z => z) * 4);
            for (int i = 0; i < count; i++)
            {
                string moveName = movelist[BitConverter.ToUInt16(data, ofs + (4 * i))];
                int price = BitConverter.ToUInt16(data, ofs + (4 * i) + 2);
                lines.Add($"{i}: {moveName} | {price}");
            }
            lines.Add("");
        }
        File.WriteAllLines(sfd.FileName, lines);
        WinFormsUtil.Alert("Tutor data exported!");
    }

    private void B_ImportTxt_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog { Filter = "Text File|*.txt" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string[] lines = File.ReadAllLines(ofd.FileName);
        int currentLoc = -1;
        int currentOfs = 0;
        int currentCount = 0;
        int updated = 0;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("==="))
            {
                string locName = line.Trim('=', ' ');
                currentLoc = -1;
                for (int i = 0; i < locationsTutor.Length; i++)
                    if (locationsTutor[i] == locName) { currentLoc = i; break; }
                if (currentLoc >= 0)
                {
                    currentOfs = ofs_BPTutor + (len_BPTutor.Take(currentLoc).Sum(z => z) * 4);
                    currentCount = len_BPTutor[currentLoc];
                }
                continue;
            }

            if (currentLoc < 0) continue;
            int colonIdx = line.IndexOf(':');
            if (colonIdx < 0) continue;
            if (!int.TryParse(line.Substring(0, colonIdx).Trim(), out int idx)) continue;
            if (idx < 0 || idx >= currentCount) continue;

            string rest = line.Substring(colonIdx + 1).Trim();
            string[] parts = rest.Split('|');
            string moveName = parts[0].Trim();
            int moveIdx = Array.IndexOf(movelist, moveName);
            if (moveIdx < 0) continue;

            Array.Copy(BitConverter.GetBytes((ushort)moveIdx), 0, data, currentOfs + (4 * idx), 2);
            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int price))
                Array.Copy(BitConverter.GetBytes((ushort)price), 0, data, currentOfs + (4 * idx) + 2, 2);
            updated++;
        }

        if (entryBPMove > -1) GetListBPMove();
        WinFormsUtil.Alert($"Imported {updated} tutor entries!");
    }
}