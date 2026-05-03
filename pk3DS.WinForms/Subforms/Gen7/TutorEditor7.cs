using pk3DS.Core;
using pk3DS.Core.Modding;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core.CTR;

namespace pk3DS.WinForms;

public partial class TutorEditor7 : Form
{
    private readonly string CROPath = Path.Combine(Main.RomFSPath, "Shop.cro");

    private byte[] data;
    private int ofs_counts;
    private byte[] len_BPTutor;

    private const int SafeZoneBase = 0x5800; 
    private const int SafeZoneSize = 2048;

    public TutorEditor7()
    {
        if (!File.Exists(CROPath))
        {
            WinFormsUtil.Error("CRO does not exist! Closing.", CROPath);
            Close();
            return;
        }
        InitializeComponent();

        data = File.ReadAllBytes(CROPath);
        LoadShopOffsets();

        SetupDGV();
        CB_LocationBPMove.Items.AddRange(locationsTutor);
        CB_LocationBPMove.SelectedIndex = 0;
        B_AddMove.Enabled = B_DelMove.Enabled = true;
    }

    private void LoadShopOffsets()
    {
        // Use verified USUM RPT key for tutor counts
        ofs_counts = ResearchEngine.GetRelocationPatchTarget(data, 0x03C);
        if (ofs_counts != -1)
        {
            len_BPTutor = data.Skip(ofs_counts).Take(4).ToArray();
            Text = "Tutor Editor (RPT Mode)";
            return;
        }

        ofs_counts = ProjectState.Instance.GetOffset("TutorCountsOffset", 0);
        if (ofs_counts <= 0) ScanForSignatures();
        Text = "Tutor Editor (Legacy Mode)";
        len_BPTutor = data.Skip(ofs_counts).Take(4).ToArray();
    }

    private void ScanForSignatures()
    {
        byte[] sig_counts = [0x0F, 0x11, 0x11, 0x0F];
        int idx_c = Util.IndexOfBytes(data, sig_counts, 0, data.Length - sig_counts.Length);
        if (idx_c >= 0) ofs_counts = idx_c; else ofs_counts = 0x52D2;
        SaveShopOffsets();
    }

    private void SaveShopOffsets()
    {
        pk3DS.Core.Modding.ProjectState.Instance.SetOffset("TutorCountsOffset", ofs_counts);
    }

    private static readonly uint[] TutorPatchAddrs = {
        0x4C8, // Melemele
        0x4D4, // Akala
        0x4E0, // Ula'ula
        0x4EC, // Battle Tree
    };

    private int GetTutorOffset(int index)
    {
        if (index < TutorPatchAddrs.Length)
        {
            int rptOfs = ResearchEngine.GetRelocationPatchTarget(data, TutorPatchAddrs[index]);
            if (rptOfs != -1) return rptOfs;
        }
        // Fallback: Legacy contiguous logic
        int baseOfs = 0x54DE;
        return baseOfs + (len_BPTutor.Take(index).Sum(z => z) * 4);
    }

    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);

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
        SyncTutorsToCodeBin();
        File.WriteAllBytes(CROPath, data);
        SaveShopOffsets();
        Close();
    }

    private void SyncTutorsToCodeBin()
    {
        if (string.IsNullOrEmpty(Main.ExeFSPath)) return;
        string binName = File.Exists(Path.Combine(Main.ExeFSPath, ".code.bin")) ? ".code.bin" : "code.bin";
        string fullCodePath = Path.Combine(Main.ExeFSPath, binName);
        if (!File.Exists(fullCodePath)) return;

        byte[] codeBin = File.ReadAllBytes(fullCodePath);
        int offset = ProjectState.Instance.TutorCodeOffset;

        if (offset <= 0)
        {
            byte[] sig = { 0x0F, 0x00, 0x11, 0x00, 0x11, 0x00, 0x0F, 0x00 };
            int sigIdx = pk3DS.Core.Util.IndexOfBytes(codeBin, sig, 0x100000, 0);
            if (sigIdx >= 0)
            {
                offset = sigIdx; 
                ProjectState.Instance.TutorCodeOffset = offset;
                ProjectState.Instance.Save();
            }
        }

        if (offset > 0)
        {
            for (int t = 0; t < 4; t++)
            {
                int destOfs = offset + (t * 2);
                if (destOfs + 1 < codeBin.Length)
                {
                    codeBin[destOfs] = (byte)len_BPTutor[t];
                    codeBin[destOfs + 1] = 0;
                }
            }
            File.WriteAllBytes(fullCodePath, codeBin);
        }
    }

    private void B_Cancel_Click(object sender, EventArgs e) => Close();

    private void SetupDGV()
    {
        dgvmvMove.Items.AddRange(movelist); // add only the Names
    }

    private int entryBPMove = -1;

    private void ChangeIndexBPMove(object sender, EventArgs e)
    {
        if (entryBPMove > -1) SetListBPMove();
        entryBPMove = CB_LocationBPMove.SelectedIndex;
        GetListBPMove();
    }

    private int FindSafeSpace(int bytes)
    {
        // Simple allocator: find first block of 0xFFFFs (unallocated/padding) in the safe zone
        for (int i = 0; i <= SafeZoneSize - bytes; i += 2)
        {
            bool free = true;
            for (int j = 0; j < bytes; j += 2)
            {
                ushort val = BitConverter.ToUInt16(data, SafeZoneBase + i + j);
                if (val != 0xFFFF && val != 0x0000)
                {
                    free = false;
                    break;
                }
            }
            if (free) return SafeZoneBase + i;
        }
        return -1;
    }

    private void GetListBPMove()
    {
        if (entryBPMove < 0 || entryBPMove >= len_BPTutor.Length) return;
        dgvmv.Rows.Clear();
        int count = len_BPTutor[entryBPMove];
        if (count > 0)
            dgvmv.Rows.Add(count);
        int ofs = GetTutorOffset(entryBPMove);
        if (ofs == -1) return;

        for (int i = 0; i < count; i++)
        {
            dgvmv.Rows[i].Cells[0].Value = i.ToString();
            int m_ofs = ofs + (4 * i);
            int p_ofs = m_ofs + 2;
            if (p_ofs + 2 > data.Length) break;

            int moveID = BitConverter.ToUInt16(data, m_ofs);
            if (moveID >= movelist.Length) moveID = 0;

            dgvmv.Rows[i].Cells[1].Value = movelist[moveID];
            dgvmv.Rows[i].Cells[2].Value = BitConverter.ToUInt16(data, p_ofs).ToString();
        }
    }

    private void SetListBPMove()
    {
        if (entryBPMove < 0 || entryBPMove >= len_BPTutor.Length) return;
        int count = dgvmv.Rows.Count;
        int tutorOfs = GetTutorOffset(entryBPMove);
        if (tutorOfs <= 0) return;

        if (count > len_BPTutor[entryBPMove])
        {
            // Expansion into Mega Safe Zone
            int required = count * 4; 
            int freeOfs = FindSafeSpace(required);
            if (freeOfs == -1)
            {
                WinFormsUtil.Alert("No space left in Safe Zone for expansion.");
                return;
            }
            
            // Wipe the rest of the zone with markers on first expansion if needed
            if (BitConverter.ToUInt16(data, SafeZoneBase) == 0)
            {
                for (int i = 0; i < SafeZoneSize; i += 2)
                    BitConverter.GetBytes((ushort)0xFFFF).CopyTo(data, SafeZoneBase + i);
            }

            ResearchEngine.RepointRelocationByOffset(data, TutorPatchAddrs[entryBPMove], (uint)freeOfs);
            tutorOfs = freeOfs;
        }

        for (int i = 0; i < count; i++)
        {
            int move = Array.IndexOf(movelist, dgvmv.Rows[i].Cells[1].Value);
            uint price = 4; uint.TryParse(dgvmv.Rows[i].Cells[2].Value.ToString(), out price);

            int m_ofs = tutorOfs + (i * 4);
            BitConverter.GetBytes((ushort)move).CopyTo(data, m_ofs);
            BitConverter.GetBytes((ushort)price).CopyTo(data, m_ofs + 2);
        }

        data[ofs_counts + entryBPMove] = (byte)count;
        len_BPTutor[entryBPMove] = (byte)count;
    }

    private void B_Randomize_Click(object sender, EventArgs e)
    {
        WinFormsUtil.Alert("Not currently implemented.");
    }

    private void B_AddMove_Click(object sender, EventArgs e)
    {
        if (entryBPMove < 0) return;
        dgvmv.Rows.Add(1);
        int idx = dgvmv.Rows.Count - 1;
        dgvmv.Rows[idx].Cells[0].Value = idx.ToString();
        dgvmv.Rows[idx].Cells[1].Value = movelist[1];
        dgvmv.Rows[idx].Cells[2].Value = "4";
    }

    private void B_DelMove_Click(object sender, EventArgs e)
    {
        if (entryBPMove < 0 || dgvmv.Rows.Count == 0) return;
        dgvmv.Rows.RemoveAt(dgvmv.Rows.Count - 1);
    }

    public static (int[] moves, int[] lengths) GetUSUMTutorData(string croPath, int[] defaultMoves)
    {
        if (!File.Exists(croPath)) return (defaultMoves, [15, 17, 17, 15]);
        byte[] d = File.ReadAllBytes(croPath);
        
        // Use RPT to find tutor counts
        int c_ofs = ResearchEngine.GetRelocationPatchTarget(d, 0x03C);
        if (c_ofs == -1) c_ofs = 0x52D2; // Fallback

        int[] lengths = d.Skip(c_ofs).Take(4).Select(b => (int)b).ToArray();
        return (defaultMoves, lengths);
    }
}