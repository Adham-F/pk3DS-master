using pk3DS.Core;
using pk3DS.Core.Modding;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class TutorEditor7 : Form
{
    private readonly string CROPath = Path.Combine(Main.RomFSPath, "Shop.cro");

    private byte[] data;
    private int ofs_counts;
    private int ofs_data;
    private byte[] len_BPTutor;

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
        B_AddMove.Enabled = B_DelMove.Enabled = false;
    }

    private void LoadShopOffsets()
    {
        ofs_counts = ProjectState.Instance.GetOffset("TutorCountsOffset", 0);
        ofs_data = ProjectState.Instance.GetOffset("TutorDataOffset", 0);

        if (ofs_counts <= 0 || ofs_data <= 0)
            ScanForSignatures();

        len_BPTutor = data.Skip(ofs_counts).Take(4).ToArray();
        Text = $"Tutor Editor (Anchor: 0x{ofs_data:X})";
    }

    private void ScanForSignatures()
    {
        // Counts table in Shop.cro for USUM: [Tutor0, Tutor1, Tutor2, Tutor3, ?x4, BP0...BP6, Mart0...]
        // Tutor counts are 15, 17, 17, 15
        byte[] sig_counts = [0x0F, 0x11, 0x11, 0x0F];
        int idx_c = Util.IndexOfBytes(data, sig_counts, 0, data.Length - sig_counts.Length);
        if (idx_c >= 0) ofs_counts = idx_c; else ofs_counts = 0x52D2;

        // Tutor data table starts with first move of Big Wave Beach (usually 0x0182 = Bind)
        byte[] sig_data = [0x82, 0x01, 0x04, 0x00, 0xAC, 0x01, 0x0C, 0x00];
        int idx_d = Util.IndexOfBytes(data, sig_data, 0, data.Length - sig_data.Length);
        if (idx_d >= 0) ofs_data = idx_d; else ofs_data = 0x54DE;
        
        SaveShopOffsets();
    }

    private void SaveShopOffsets()
    {
        pk3DS.Core.Modding.ProjectState.Instance.SetOffset("TutorCountsOffset", ofs_counts);
        pk3DS.Core.Modding.ProjectState.Instance.SetOffset("TutorDataOffset", ofs_data);
    }

    private int ofs_BPTutor => ofs_data;

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
            // USUM Tutor Limit pattern: [15, 0, 17, 0, 17, 0, 15, 0]
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
            int totalMoves = data[ofs_counts] + data[ofs_counts + 1] + data[ofs_counts + 2] + data[ofs_counts + 3];
            for (int i = 0; i < totalMoves; i++)
            {
                int srcOfs = ofs_data + (i * 4);
                int destOfs = offset + (i * 2);
                if (destOfs + 1 >= codeBin.Length) break;
                
                codeBin[destOfs] = data[srcOfs];
                codeBin[destOfs + 1] = data[srcOfs + 1];
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

    private void GetListBPMove()
    {
        if (entryBPMove < 0 || entryBPMove >= len_BPTutor.Length) return;
        dgvmv.Rows.Clear();
        int count = len_BPTutor[entryBPMove];
        dgvmv.Rows.Add(count);
        var ofs = ofs_BPTutor + (len_BPTutor.Take(entryBPMove).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            dgvmv.Rows[i].Cells[0].Value = i.ToString();
            int m_ofs = ofs + (4 * i);
            int p_ofs = ofs + (4 * i) + 2;

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
        if (count != len_BPTutor[entryBPMove])
        {
            // Expansion triggered!
            // Shift whole table to end of .data segment if it's the first time
            if (ofs_data < 0x8000) // If still in original region 
            {
                int oldOfs = ofs_data;
                int oldDataLen = len_BPTutor.Sum(z => z) * 4;
                int newDataLen = oldDataLen + 0x1000;
                int newOfs = data.Length;
                
                data = pk3DS.Core.CTR.CROUtil.ExpandSegment(data, 'd', newDataLen);
                pk3DS.Core.CTR.CROUtil.RelocateTable(data, (uint)oldOfs - pk3DS.Core.CTR.CROUtil.GetSegmentStartIndices(data)[2], 2, (uint)newOfs, oldDataLen);
                ofs_data = newOfs;
            }
        }

        var ofs = ofs_BPTutor + (len_BPTutor.Take(entryBPMove).Sum(z => z) * 4);
        for (int i = 0; i < count; i++)
        {
            int item = Array.IndexOf(movelist, dgvmv.Rows[i].Cells[1].Value);
            int price = 4; int.TryParse(dgvmv.Rows[i].Cells[2].Value.ToString(), out price);

            int m_ofs = ofs + (4 * i);
            int p_ofs = ofs + (4 * i) + 2;

            Array.Copy(BitConverter.GetBytes((ushort)item), 0, data, m_ofs, 2);
            Array.Copy(BitConverter.GetBytes((ushort)price), 0, data, p_ofs, 2);
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
        WinFormsUtil.Alert("Add/Delete functionality is temporarily disabled due to unresolved stability issues with tutor expansion in USUM.");
        return;
        /*
        if (entryBPMove < 0) return;
        dgvmv.Rows.Add(1);
        int entries = dgvmv.Rows.Count;
        dgvmv.Rows[entries - 1].Cells[0].Value = (entries - 1).ToString();
        dgvmv.Rows[entries - 1].Cells[1].Value = movelist[0];
        dgvmv.Rows[entries - 1].Cells[2].Value = "4";
        */
    }

    private void B_DelMove_Click(object sender, EventArgs e)
    {
        WinFormsUtil.Alert("Add/Delete functionality is temporarily disabled due to unresolved stability issues with tutor expansion in USUM.");
        return;
        /*
        if (entryBPMove < 0 || dgvmv.Rows.Count == 0) return;
        dgvmv.Rows.RemoveAt(dgvmv.Rows.Count - 1);
        */
    }

    public static (int[] moves, int[] lengths) GetUSUMTutorData(string croPath, int[] defaultMoves)
    {
        if (!File.Exists(croPath)) return (defaultMoves, [15, 17, 17, 15]);
        byte[] d = File.ReadAllBytes(croPath);
        
        // Use verified USUM v1.0 counts offset
        int c_ofs = 0x52D2;

        string cnt_txt = Path.Combine(Path.GetDirectoryName(croPath), "tutor_counts_ofs.txt");
        if (File.Exists(cnt_txt)) int.TryParse(File.ReadAllText(cnt_txt), out c_ofs);

        int[] lengths = d.Skip(c_ofs).Take(4).Select(b => (int)b).ToArray();
        return (defaultMoves, lengths);
    }
}