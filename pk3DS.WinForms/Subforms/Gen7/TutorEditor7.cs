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
    }

    private void LoadShopOffsets()
    {
        // Verified USUM v1.0 hard-locked offsets
        // Persistent Overrides (Universal Engine tracker)
        ofs_counts = pk3DS.Core.Modding.ProjectState.Instance.GetOffset("TutorCountsOffset", 0x52D2);
        ofs_data = pk3DS.Core.Modding.ProjectState.Instance.GetOffset("TutorDataOffset", 0x54DE);

        len_BPTutor = data.Skip(ofs_counts).Take(4).ToArray();
        Text = $"Tutor Editor (Anchor: 0x{ofs_data:X})";
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
            // Scan for marker: _on_off\xFF
            byte[] sig = { 0x5F, 0x6F, 0x6E, 0x5F, 0x6F, 0x66, 0x66, 0xFF };
            int sigIdx = pk3DS.Core.Util.IndexOfBytes(codeBin, sig, 0, codeBin.Length);
            if (sigIdx >= 0)
            {
                offset = sigIdx + sig.Length; 
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
                Array.Copy(data, oldOfs, data, newOfs, oldDataLen);
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
        if (entryBPMove < 0) return;
        dgvmv.Rows.Add(1);
        int entries = dgvmv.Rows.Count;
        dgvmv.Rows[entries - 1].Cells[0].Value = (entries - 1).ToString();
        dgvmv.Rows[entries - 1].Cells[1].Value = movelist[0];
        dgvmv.Rows[entries - 1].Cells[2].Value = "4";
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
        
        // Use verified USUM v1.0 counts offset
        int c_ofs = 0x52D2;

        string cnt_txt = Path.Combine(Path.GetDirectoryName(croPath), "tutor_counts_ofs.txt");
        if (File.Exists(cnt_txt)) int.TryParse(File.ReadAllText(cnt_txt), out c_ofs);

        int[] lengths = d.Skip(c_ofs).Take(4).Select(b => (int)b).ToArray();
        return (defaultMoves, lengths);
    }
}