using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RLC_Configurator
{
    public partial class FormMainScreen : Form
    {

        #region Objects: Enumerators

        public enum AppState
        {
            NoFileLoaded,
            FileLoaded,
            FileNeedsSave,
        }

        #endregion

        #region Objects: General

        public ModComm rlcComm;

        public AppState state;

        #endregion

        #region Objects: Actual Values

        public ModGroup actGroup;

        #endregion


        #region Public Properties: General
        
        public List<long> AllRegisters
        {
            get
            {
                List<long> ret = new List<long>();
                
                int i;
                int min = AppSett.Default.RLC_Registers_Min;
                int max = AppSett.Default.RLC_Registers_Max + 1;

                for (i = min; i < max; i++) ret.Add(i);

                ret.Sort();

                return ret;
            }
        }
        public List<long> AllUsedRegs
        {
            get
            {
                List<long> ret = new List<long>();

                if (this.rlcComm != null)
                {
                    ret = this.rlcComm.UsedModRegs;
                }

                ret.Sort();

                return ret;
            }
        }
        public List<long> AllFreeRegs
        {
            get
            {
                List<long> ret = new List<long>();
                List<long> allRegs = this.AllRegisters;
                List<long> allUsed = this.AllUsedRegs;
                
                if( (allRegs != null) && (allUsed != null) )
                {
                    ret = allRegs.Except(allUsed).ToList<long>();
                }

                ret.Sort();

                return ret;
            }
        }

        public List<ModAdd> AllAdds
        {
            get
            {
                List<ModAdd> ret = new List<ModAdd>();

                int i;
                int min = AppSett.Default.RLC_ModAdd_Min;
                int max = AppSett.Default.RLC_ModAdd_Max;

                for (i = min; i < max; i++) ret.Add(new ModAdd(i,ModAdd.ModLoc.HoldingRegisters));

                return ret;
            }
        }
        public List<ModAdd> AllUsedAdds
        {
            get
            {
                List<ModAdd> ret = new List<ModAdd>();

                if (this.rlcComm != null)
                {
                    ret = this.rlcComm.UsedModAdds;
                }

                ret.Sort();

                return ret;
            }
        }
        public List<ModAdd> AllFreeAdds
        {
            get
            {
                List<ModAdd> ret = new List<ModAdd>();
                List<ModAdd> allAdd = this.AllAdds;
                List<ModAdd> allUsedAdd = this.AllUsedAdds;

                if ((allAdd != null) && (allUsedAdd != null))
                {
                    ret.AddRange(allAdd.Except(allUsedAdd).ToList<ModAdd>());

                    List<ModAdd> allBits = allUsedAdd.FindAll(x => !(x.AllBitsUsed));
                    if (allBits != null)
                    {
                        foreach (var elem in allBits)
                        {
                            ret.Add(new ModAdd(elem.Address, elem.BitMaskInv, ModAdd.ModLoc.HoldingRegisters));
                        }
                    }

                    ret.Sort();
                }

                return ret;
            }
        }

        public List<string> AllBits
        {
            get
            {
                List<string> ret = new List<string>();

                int i;
                int min = 0;
                int max = 16;

                ret.Add(AppSett.Default.RLC_Bits_None);

                for (i = min; i < max; i++) ret.Add(i.ToString());

                return ret;
            }
        }

        public List<string> AllGroupNames
        {
            get
            {
                List<string> ret = new List<string>();

                if (this.rlcComm != null)
                {
                    ret = this.rlcComm.GroupsNames;
                }

                return ret;
            }
        }
        string UniqueGroupName
        {
            get
            {
                string ret = null;
                string temp;
                List<string> allGroupNames = this.AllGroupNames;

                if (allGroupNames != null)
                {
                    int i = 1;

                    temp = AppSett.Default.App_Def_GroupName + " " + i.ToString();

                    while (this.AllGroupNames.Contains(temp, StringComparer.CurrentCultureIgnoreCase) && (i <= AppSett.Default.RLC_GroupNum_Max))
                    {
                        i++;
                        temp = AppSett.Default.App_Def_GroupName + " " + i.ToString();
                    }

                    ret = temp;
                }

                return ret;
            }
        }

        public int NumOfFreeRegs
        {
            get
            {
                int ret = 0;

                if (this.AllFreeRegs != null) ret = this.AllFreeRegs.Count;

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsFreeRegAvail
        {
            get
            {
                bool ret = false;

                ret = this.NumOfFreeRegs > 0;

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors

        public FormMainScreen()
        {
            InitializeComponent();
            this.Text = AppSett.Default.App_Name + " (" + AppSett.Default.App_Platform + ") " + AppSett.Default.App_Version;
            InitializeAll();
            this.state = AppState.NoFileLoaded;
        }


        #endregion


        #region Methods: General

        /// <summary>
        /// Selects the last group
        /// </summary>
        public void SelectLastGroup()
        {
            try
            {
                if (this.treeViewGroups != null)
                {
                    if (this.treeViewGroups.Enabled)
                    {
                        this.treeViewGroups.SelectedNode = this.treeViewGroups.Nodes[this.treeViewGroups.Nodes.Count - 1];
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        #endregion

        #region Methods: Show Functions

        /// <summary>
        /// Shows error message with specified text and with description of the exception
        /// </summary>
        /// <param name="Text">A text to show</param>
        /// <param name="Exception">A exception to show. Omitted if null</param>
        public void ShowErr(string Text, Exception e)
        {
            string mess = "";
            mess = mess + Text + "\n\r\n\r";

            if (e != null)
            {
                mess = mess + "Source:\n\r";
                mess = mess + e.TargetSite + "\n\r\n\r";

                mess = mess + "Description:\n\r";
                mess = mess + e.Message;
            }

            MessageBox.Show(mess, AppSett.Default.App_Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows information message for specified object and path
        /// </summary>
        /// <param name="Text">A text to show</param>
        /// <param name="Object">A specified object</param>
        /// <param name="Path">A specified path</param>
        public void ShowInf(string Text, string Object, string Path)
        {
            string mess = "";
            mess = mess + "Object: " + Object + "\n\r\n\r";
            mess = mess + "Path:   " + Path + "\n\r\n\r";
            mess = mess + Text + "\n\r\n\r";

            MessageBox.Show(mess, AppSett.Default.App_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows information message for specified text
        /// </summary>
        /// <param name="Text">A text to show</param>
        public void ShowInf(string Text)
        {
            MessageBox.Show(Text, AppSett.Default.App_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows communication header
        /// </summary>
        public void ShowHeader()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    int i;

                    if (this.rlcComm.HeaderAuthor != null)
                    {
                        this.textBoxAuthor.Enabled = true;
                        this.textBoxAuthor.Text = this.rlcComm.HeaderAuthor;
                    }

                    if (this.rlcComm.HeaderComp != null)
                    {
                        this.textBoxComp.Enabled = true;
                        this.textBoxComp.Text = this.rlcComm.HeaderComp;
                    }

                    if (this.rlcComm.HeaderDesc != null)
                    {
                        this.textBoxDesc.Enabled = true;
                        this.textBoxDesc.Text = this.rlcComm.HeaderDesc;
                    }

                    if (this.rlcComm.HeaderEmail != null)
                    {
                        this.textBoxEmail.Enabled = true;
                        this.textBoxEmail.Text = this.rlcComm.HeaderEmail;
                    }

                    if (this.rlcComm.HeaderName != null)
                    {
                        this.textBoxName.Enabled = true;
                        this.textBoxName.Text = this.rlcComm.HeaderName;
                    }

                    if (this.rlcComm.HeaderProject != null)
                    {
                        this.textBoxProject.Enabled = true;
                        this.textBoxProject.Text = this.rlcComm.HeaderProject;
                    }

                    if (this.rlcComm.HeaderRev != null)
                    {
                        this.textBoxRev.Enabled = true;
                        this.textBoxRev.Text = this.rlcComm.HeaderRev;
                    }

                    if (this.rlcComm.IsDefinedDrop)
                    {
                        this.numericUpDownDrop.Enabled = true;
                        this.numericUpDownDrop.Value = this.rlcComm.LocDrop;
                    }

                    if (this.rlcComm.HWAddDec != null)
                    {
                        this.textBoxHWAddDec.Text = this.rlcComm.HWAddDec;
                    }

                    if (this.rlcComm.HWAddHex != null)
                    {
                        this.textBoxHWAddHex.Text = this.rlcComm.HWAddHex;
                    }

                    if (this.rlcComm.IsDefinedFullLoc)
                    {
                        this.comboBoxLoc.Enabled = true;

                        for (i = 0; i < this.comboBoxLoc.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.LocFull, this.comboBoxLoc.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxLoc.SelectedItem = this.comboBoxLoc.Items[i];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows communication link parameters
        /// </summary>
        public void ShowLinkPar()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    int i;

                    if (this.rlcComm.ParBaud != null)
                    {
                        this.comboBoxBaud.Enabled = true;

                        for (i = 0; i < this.comboBoxBaud.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParBaud.ToString(), this.comboBoxBaud.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxBaud.SelectedItem = this.comboBoxBaud.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParDataBits != null)
                    {
                        this.comboBoxDataBits.Enabled = true;

                        for (i = 0; i < this.comboBoxDataBits.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParDataBits.ToString(), this.comboBoxDataBits.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxDataBits.SelectedItem = this.comboBoxDataBits.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParParity != null)
                    {
                        this.comboBoxParity.Enabled = true;

                        for (i = 0; i < this.comboBoxParity.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParParity.ToString(), this.comboBoxParity.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxParity.SelectedItem = this.comboBoxParity.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParStopBits != null)
                    {
                        this.comboBoxStopBits.Enabled = true;

                        string temp = null;

                        if (this.rlcComm.ParStopBits == ModComm.CommStopBits.one) temp = AppSett.Default.RLC_StopBits[0];
                        if (this.rlcComm.ParStopBits == ModComm.CommStopBits.oneAndHalf) temp = AppSett.Default.RLC_StopBits[1];
                        if (this.rlcComm.ParStopBits == ModComm.CommStopBits.two) temp = AppSett.Default.RLC_StopBits[2];

                        for (i = 0; i < this.comboBoxStopBits.Items.Count; i++)
                        {
                            if (String.Compare(temp, this.comboBoxStopBits.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxStopBits.SelectedItem = this.comboBoxStopBits.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParDuplex != null)
                    {
                        this.comboBoxDuplex.Enabled = true;

                        for (i = 0; i < this.comboBoxDuplex.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParDuplex.ToString(), this.comboBoxDuplex.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxDuplex.SelectedItem = this.comboBoxDuplex.Items[i];
                                break;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows communication card parameters
        /// </summary>
        public void ShowCardPar()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    int i;

                    if (this.rlcComm.ParPlatform != null)
                    {
                        this.comboBoxPlatform.Enabled = true;

                        for (i = 0; i < this.comboBoxPlatform.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParPlatform.ToString(), this.comboBoxPlatform.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxPlatform.SelectedItem = this.comboBoxPlatform.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParFlowCtrl != null)
                    {
                        this.comboBoxFlowCtrl.Enabled = true;

                        for (i = 0; i < this.comboBoxFlowCtrl.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParFlowCtrl.ToString(), this.comboBoxFlowCtrl.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxFlowCtrl.SelectedItem = this.comboBoxFlowCtrl.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParSysLog != null)
                    {
                        this.comboBoxSysLog.Enabled = true;

                        for (i = 0; i < this.comboBoxSysLog.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParSysLog.ToString(), this.comboBoxSysLog.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxSysLog.SelectedItem = this.comboBoxSysLog.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParBackupMode != null)
                    {
                        this.comboBoxBackupModeAct.Enabled = true;

                        for (i = 0; i < this.comboBoxBackupModeAct.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParBackupMode.ToString(), this.comboBoxBackupModeAct.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxBackupModeAct.SelectedItem = this.comboBoxBackupModeAct.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParList != null)
                    {
                        this.comboBoxList.Enabled = true;

                        for (i = 0; i < this.comboBoxBackupModeAct.Items.Count; i++)
                        {
                            if (String.Compare(this.rlcComm.ParList.ToString(), this.comboBoxList.Items[i].ToString(), true) == 0)
                            {
                                this.comboBoxList.SelectedItem = this.comboBoxList.Items[i];
                                break;
                            }
                        }
                    }

                    if (this.rlcComm.ParRetries != null)
                    {
                        this.numericUpDownRetries.Enabled = true;

                        this.numericUpDownRetries.Value = this.rlcComm.ParRetries;
                    }

                    if (this.rlcComm.ParMsgTout != null)
                    {
                        this.numericUpDownMsgTout.Enabled = true;

                        this.numericUpDownMsgTout.Value = this.rlcComm.ParMsgTout;
                    }

                    if (this.rlcComm.ParInterMsgDel != null)
                    {
                        this.numericUpDownIntMsgDel.Enabled = true;

                        this.numericUpDownIntMsgDel.Value = this.rlcComm.ParInterMsgDel;
                    }

                    if (this.rlcComm.ParStatHoldTime != null)
                    {
                        this.numericUpDownStatHoldTime.Enabled = true;

                        this.numericUpDownStatHoldTime.Value = this.rlcComm.ParStatHoldTime;
                    }

                    if (this.rlcComm.ParWdTime != null)
                    {
                        this.numericUpDownWDTime.Enabled = true;

                        this.numericUpDownWDTime.Value = this.rlcComm.ParWdTime;
                    }

                    if (this.rlcComm.ParStatReg != null)
                    {
                        this.numericUpDownLinkStatReg.Enabled = true;

                        this.numericUpDownLinkStatReg.Value = this.rlcComm.ParStatReg;
                    }

                    if (this.rlcComm.ParCtrlReg != null)
                    {
                        this.numericUpDownCtrlReg.Enabled = true;

                        this.numericUpDownCtrlReg.Value = this.rlcComm.ParCtrlReg;
                    }

                    if (this.rlcComm.ParRedReg != null)
                    {
                        this.numericUpDownExOnceReg.Enabled = true;

                        this.numericUpDownExOnceReg.Value = this.rlcComm.ParRedReg;
                    }

                    if (this.rlcComm.ParDiagReg != null)
                    {
                        this.numericUpDownDiagReg.Enabled = true;

                        this.numericUpDownDiagReg.Value = this.rlcComm.ParDiagReg;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows communication text
        /// </summary>
        public void ShowText()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    List<string> text = this.rlcComm.TextFull;

                    if (text != null)
                    {
                        this.richTextBoxText.Clear();

                        foreach (var elem in text)
                        {
                            this.richTextBoxText.AppendText(elem + "\n");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Groups
        /// </summary>
        public void ShowGroups()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    if (this.rlcComm.Groups != null)
                    {
                        TreeNode tempNode;
                        string tempText = null;

                        this.treeViewGroups.Enabled = true;
                        this.treeViewGroups.Nodes.Clear();

                        foreach (var elem in this.rlcComm.Groups)
                        {
                            tempText = elem.Index.ToString() + ":  " + elem.Name;

                            tempNode = this.treeViewGroups.Nodes.Add(tempText);

                            tempNode.Tag = elem;
                            tempNode.ToolTipText = elem.Description;
                        }

                        this.buttonGroupAdd.Enabled = true;

                        this.groupBoxGroupsList.Text = AppSett.Default.App_Group_GroupBox + " (" + this.treeViewGroups.Nodes.Count.ToString() + ")";
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Sel Group Parameters
        /// </summary>
        public void ShowGroupPar()
        {
            try
            {
                int i;
                
                if (this.rlcComm != null)
                {
                    if (this.actGroup != null)
                    {

                        this.numericUpDownGroupIndex.Value = this.actGroup.Index;

                        if (this.actGroup.Name != null)
                        {
                            this.textBoxGroupName.Text = this.actGroup.Name;
                            this.textBoxGroupName.Enabled = true;
                        }

                        if (this.actGroup.Description != null)
                        { 
                            this.textBoxGroupDesc.Text = this.actGroup.Description;
                            this.textBoxGroupDesc.Enabled = true;
                        }

                        if (this.actGroup.Operation != null)
                        {
                            for (i = 0; i < this.comboBoxGroupOper.Items.Count; i++)
                            {
                                if (String.Compare(this.actGroup.Operation.ToString(), this.comboBoxGroupOper.Items[i].ToString(), true) == 0)
                                {
                                    this.comboBoxGroupOper.SelectedItem = this.comboBoxGroupOper.Items[i];
                                    break;
                                }
                            }

                            if ((this.actGroup.Operation == ModGroup.GroupOperation.PERIODIC) && !(this.IsFreeRegAvail)) this.comboBoxGroupOper.Enabled = false;
                            this.comboBoxGroupOper.Enabled = true;
                        }

                        if (this.actGroup.Function != null)
                        {
                            for (i = 0; i < this.comboBoxGroupFunct.Items.Count; i++)
                            {
                                if (String.Compare(this.actGroup.Function.ToString(), this.comboBoxGroupFunct.Items[i].ToString(), true) == 0)
                                {
                                    this.comboBoxGroupFunct.SelectedItem = this.comboBoxGroupFunct.Items[i];
                                    break;
                                }
                            }
                            this.comboBoxGroupFunct.Enabled = true;
                        }

                        
                        this.numericUpDownGroupSlave.Value = this.actGroup.Slave;
                        this.numericUpDownGroupSlave.Enabled = true;

                        if (this.actGroup.Signals != null)
                        {
                            this.numericUpDownGroupSig.Value = this.actGroup.Signals.Count;
                            this.numericUpDownGroupSig.Enabled = true;
                        }

                        if (this.actGroup.UsedModRegs != null)
                        {
                            this.numericUpDownGroupReg.Value = this.actGroup.UsedModRegs.Count;
                            this.numericUpDownGroupReg.Enabled = true;
                        }

                        
                        this.numericUpDownGroupTrig.Value = this.actGroup.Triggerreg;
                        if (this.actGroup.Operation == ModGroup.GroupOperation.TRIGGERED)
                        {
                            this.numericUpDownGroupTrig.Enabled = true;
                        }

                        
                        this.numericUpDownGroupGate.Value = this.actGroup.Gatereg;
                        if (this.actGroup.Operation == ModGroup.GroupOperation.GATED)
                        {
                            this.numericUpDownGroupGate.Enabled = true;
                        }

                        this.numericUpDownGroupInt.Value = this.actGroup.Interval;
                        if (this.actGroup.Operation == ModGroup.GroupOperation.PERIODIC)
                        {
                            this.numericUpDownGroupInt.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Sel Group Signals
        /// </summary>
        public void ShowGroupSig()
        {
            try
            {
                int i;

                if (this.rlcComm != null)
                {
                    if (this.actGroup != null)
                    {
                        if (this.actGroup.Signals != null)
                        {

                            this.dataGridViewSignals.Enabled = true;
                            this.dataGridViewSignals.Rows.Clear();

                            foreach (var elem in this.actGroup.Signals) ShowSignalInTable(elem);

                            this.tabPageGroupSig.Text = AppSett.Default.App_Group_SigBox + " (" + this.dataGridViewSignals.Rows.Count.ToString() + ")";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows signal in table
        /// </summary>
        /// <param name="Signal">A signal to show</param>
        public void ShowSignalInTable(ModSignal Signal)
        {
            try
            {
                if ( (Signal != null) && (this.dataGridViewSignals != null))
                {
                    int ind = this.dataGridViewSignals.Rows.Add();

                    if (this.dataGridViewSignals.Rows[ind] != null)
                    {
                        DataGridViewComboBoxCell cellCombo;
                        int i;

                        this.dataGridViewSignals.Rows[ind].Tag = Signal;
                        
                        // Register Type
                        ShowSigRegType(ind, Signal);

                        // Register Number
                        ShowSigRegNum(ind, Signal);

                        // Signal Type
                        ShowSigType(ind, Signal);

                        // Modbus Address
                        ShowSigModAdd(ind, Signal);

                        // Bit
                        ShowSigModBit(ind, Signal);

                        // Tag Name
                        ShowSigTag(ind, Signal);

                        // Description
                        ShowSigDesc(ind, Signal);

                        // Conv Type
                        ShowSigConvType(ind, Signal);

                        // C0
                        ShowSigConv0(ind, Signal);

                        // C2
                        ShowSigConv2(ind, Signal);

                        // C3
                        ShowSigConv3(ind, Signal);

                        // C4
                        ShowSigConv4(ind, Signal);

                        // C5
                        ShowSigConv5(ind, Signal);

                        // C6
                        ShowSigConv6(ind, Signal);
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows signal in table
        /// </summary>
        /// <param name="Signal">A signal to show</param>
        public void ShowSignalInTable(ModPoint Signal)
        {
            try
            {
                if ((Signal != null) && (this.dataGridViewSignals != null))
                {
                    int ind = this.dataGridViewSignals.Rows.Add();

                    if (this.dataGridViewSignals.Rows[ind] != null)
                    {
                        DataGridViewComboBoxCell cellCombo;
                        int i;

                        this.dataGridViewSignals.Rows[ind].Tag = Signal;

                        // Register Type
                        ShowSigRegType(ind, Signal);

                        // Register Number
                        ShowSigRegNum(ind, Signal);

                        // Signal Type
                        ShowSigType(ind, Signal);

                        // Data Type
                        ShowSigDataType(ind, Signal);

                        // Modbus Address
                        ShowSigModAdd(ind, Signal);

                        // Bit
                        ShowSigModBit(ind, Signal);

                        // Tag Name
                        ShowSigTag(ind, Signal);

                        // Description
                        ShowSigDesc(ind, Signal);

                        // Conv Type
                        ShowSigConvType(ind, Signal);

                        // C0
                        ShowSigConv0(ind, Signal);

                        // C2
                        ShowSigConv2(ind, Signal);

                        // C3
                        ShowSigConv3(ind, Signal);

                        // C4
                        ShowSigConv4(ind, Signal);

                        // C5
                        ShowSigConv5(ind, Signal);

                        // C6
                        ShowSigConv6(ind, Signal);
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Register Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigRegType(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[0] as DataGridViewComboBoxCell;
                    
                    if ((Signal.RegTypeChar != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.RegTypeChar.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }

                        if (Signal.RegType == ModSignal.SigRegTypes.DTYPE) cellCombo.ReadOnly = true;
                        else cellCombo.ReadOnly = false;
                    } 
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Register Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigRegType(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[0] as DataGridViewComboBoxCell;

                    if ((Signal.Register.RegTypeChar != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.Register.RegTypeChar.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }

                        if (Signal.Register.Type == ModReg.ModRegType.DTYPE) cellCombo.ReadOnly = true;
                        else cellCombo.ReadOnly = false;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Register Number
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigRegNum(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.RegNumberString != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[1].Value = Signal.RegNumberString;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Register Number
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigRegNum(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Register.NumberStr != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[1].Value = Signal.Register.NumberStr;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigType(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[2] as DataGridViewComboBoxCell;
                    if ((Signal.SigType != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.SigType.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }

                        cellCombo.ReadOnly = true;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Signal Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigType(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[2] as DataGridViewComboBoxCell;
                    if ((Signal.SigType != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.SigType.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }

                        cellCombo.ReadOnly = true;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Data Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigDataType(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[3] as DataGridViewComboBoxCell;
                    if ((Signal.DataType != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.DataType.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Address
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigModAdd(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.ModAddressString != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[4].Value = Signal.ModAddress.AddressStr;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Address
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigModAdd(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Address.AddressStr != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[4].Value = Signal.Address.AddressStr;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Address Bit
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigModBit(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[5] as DataGridViewComboBoxCell;
                    if (cellCombo != null)
                    {
                        if (Signal.IsBitDefined)
                        {
                            for (i = 0; i < cellCombo.Items.Count; i++)
                            {
                                if (String.Compare(Signal.ModAddress.FirstSetBit.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                                {
                                    cellCombo.Value = cellCombo.Items[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cellCombo.Value = AppSett.Default.RLC_Bits_None;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Address Bit
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigModBit(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[5] as DataGridViewComboBoxCell;
                    if (cellCombo != null)
                    {
                        if (Signal.Address.IsBinaryType)
                        {
                            for (i = 0; i < cellCombo.Items.Count; i++)
                            {
                                if (String.Compare(Signal.Address.FirstSetBit.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                                {
                                    cellCombo.Value = cellCombo.Items[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cellCombo.Value = AppSett.Default.RLC_Bits_None;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Tag Name
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigTag(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.TagName != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[6].Value = Signal.TagName;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Tag Name
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigTag(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.TagName != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[6].Value = Signal.TagName;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Description
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigDesc(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Description != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[7].Value = Signal.Description;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Description
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigDesc(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Description != null)
                    {
                        this.dataGridViewSignals.Rows[RowInd].Cells[7].Value = Signal.Description;
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConvType(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[8] as DataGridViewComboBoxCell;
                    if ((Signal.ConvType != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.ConvType.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv Type
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConvType(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    int i;
                    DataGridViewComboBoxCell cellCombo = this.dataGridViewSignals.Rows[RowInd].Cells[8] as DataGridViewComboBoxCell;
                    if ((Signal.Conversion.Type != null) && (cellCombo != null))
                    {
                        for (i = 0; i < cellCombo.Items.Count; i++)
                        {
                            if (String.Compare(Signal.Conversion.ConvNum.ToString(), cellCombo.Items[i].ToString(), true) == 0)
                            {
                                cellCombo.Value = cellCombo.Items[i];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv0
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv0(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv0 != null)
                    {
                        if (Signal.IsConv0Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].Value = Signal.Conv0;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv0
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv0(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff1 != null)
                    {
                        if (Signal.Conversion.IsConv1Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].Value = Signal.Conversion.Coeff1;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[9].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv2
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv2(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv2 != null)
                    {
                        if (Signal.IsConv2Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].Value = Signal.Conv2;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv2
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv2(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff2 != null)
                    {
                        if (Signal.Conversion.IsConv2Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].Value = Signal.Conversion.Coeff2;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[10].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv3
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv3(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv3 != null)
                    {
                        if (Signal.IsConv3Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].Value = Signal.Conv3;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv3
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv3(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff3 != null)
                    {
                        if (Signal.Conversion.IsConv3Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].Value = Signal.Conversion.Coeff3;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[11].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv4
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv4(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv4 != null)
                    {
                        if (Signal.IsConv4Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].Value = Signal.Conv4;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv4
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv4(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff4 != null)
                    {
                        if (Signal.Conversion.IsConv4Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].Value = Signal.Conversion.Coeff4;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[12].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv5
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv5(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv5 != null)
                    {
                        if (Signal.IsConv5Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].Value = Signal.Conv5;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv5
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv5(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff5 != null)
                    {
                        if (Signal.Conversion.IsConv5Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].Value = Signal.Conversion.Coeff5;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[13].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv6
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv6(int RowInd, ModSignal Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conv6 != null)
                    {
                        if (Signal.IsConv6Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].Value = Signal.Conv6;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        /// <summary>
        /// Shows Modbus Conv6
        /// </summary>
        /// <param name="RowInd">A row index</param>
        /// <param name="Signal">A signal to show</param>
        public void ShowSigConv6(int RowInd, ModPoint Signal)
        {
            try
            {
                if (Signal != null)
                {
                    if (Signal.Conversion.Coeff6 != null)
                    {
                        if (Signal.Conversion.IsConv6Pos)
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].ReadOnly = false;
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].Value = Signal.Conversion.Coeff6;
                        }
                        else
                        {
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].ReadOnly = true;
                            this.dataGridViewSignals.Rows[RowInd].Cells[14].Value = "";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErr("An error has occured!", e);
            }
        }

        #endregion

        #region Methods: Empty Functions

        /// <summary>
        /// Empties All
        /// </summary>
        public void EmptyAll()
        {
            try
            {
                EmptyObjects();
                EmptyHeader();
                EmptyLinkPar();
                EmptyCardPar();
                EmptyGroupList();
                EmptyGroupPar();
                EmptyGroupSig();
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Objects
        /// </summary>
        public void EmptyObjects()
        {
            try
            {
                this.rlcComm = null;
                this.state = AppState.NoFileLoaded;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Header
        /// </summary>
        public void EmptyHeader()
        {
            try
            {
                this.textBoxAuthor.Enabled = false;
                this.textBoxAuthor.Text = "";

                this.textBoxComp.Enabled = false;
                this.textBoxComp.Text = "";

                this.textBoxDesc.Enabled = false;
                this.textBoxDesc.Text = "";

                this.numericUpDownDrop.Enabled = false;
                this.numericUpDownDrop.Value = AppSett.Default.ModComm_Def_LocDrop;

                this.textBoxEmail.Enabled = false;
                this.textBoxEmail.Text = "";

                this.textBoxHWAddDec.Enabled = false;
                this.textBoxHWAddDec.Text = "";

                this.textBoxHWAddHex.Enabled = false;
                this.textBoxHWAddHex.Text = "";

                this.textBoxName.Enabled = false;
                this.textBoxName.Text = "";

                this.textBoxProject.Enabled = false;
                this.textBoxProject.Text = "";

                this.textBoxRev.Enabled = false;
                this.textBoxRev.Text = "";

                this.comboBoxLoc.Enabled = false;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empty Link Parameters
        /// </summary>
        public void EmptyLinkPar()
        {
            try
            {
                this.comboBoxBaud.Enabled = false;

                this.comboBoxDataBits.Enabled = false;

                this.comboBoxDuplex.Enabled = false;

                this.comboBoxStopBits.Enabled = false;

                this.comboBoxParity.Enabled = false;


            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Card Parameters
        /// </summary>
        public void EmptyCardPar()
        {
            try
            {
                this.comboBoxPlatform.Enabled = false;

                this.comboBoxFlowCtrl.Enabled = false;

                this.comboBoxSysLog.Enabled = false;

                this.comboBoxBackupModeAct.Enabled = false;

                this.comboBoxList.Enabled = false;

                this.numericUpDownRetries.Enabled = false;
                this.numericUpDownRetries.Value = AppSett.Default.ModComm_Def_Retries;

                this.numericUpDownMsgTout.Enabled = false;
                this.numericUpDownMsgTout.Value = AppSett.Default.ModComm_Def_MsgTout;

                this.numericUpDownIntMsgDel.Enabled = false;
                this.numericUpDownIntMsgDel.Value = AppSett.Default.ModComm_Def_InterMsgDel;

                this.numericUpDownStatHoldTime.Enabled = false;
                this.numericUpDownStatHoldTime.Value = AppSett.Default.ModComm_Def_StatHoldTime;

                this.numericUpDownWDTime.Enabled = false;
                this.numericUpDownWDTime.Value = AppSett.Default.ModComm_Def_WDTime;

                this.numericUpDownLinkStatReg.Enabled = false;
                this.numericUpDownLinkStatReg.Value = AppSett.Default.ModComm_Def_StatReg;

                this.numericUpDownCtrlReg.Enabled = false;
                this.numericUpDownCtrlReg.Value = AppSett.Default.ModComm_Def_CtrlReg;

                this.numericUpDownExOnceReg.Enabled = false;
                this.numericUpDownExOnceReg.Value = AppSett.Default.ModComm_Def_RedReg;

                this.numericUpDownDiagReg.Enabled = false;
                this.numericUpDownDiagReg.Value = AppSett.Default.ModComm_Def_DiagReg;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Group List
        /// </summary>
        public void EmptyGroupList()
        {
            try
            {
                this.treeViewGroups.Nodes.Clear();
                this.treeViewGroups.Enabled = false;

                this.buttonGroupAdd.Enabled = false;
                this.buttonGroupRem.Enabled = false;

                this.groupBoxGroupsList.Text = AppSett.Default.App_Group_GroupBox;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Sel Group Pars
        /// </summary>
        public void EmptyGroupPar()
        {
            try
            {
                this.numericUpDownGroupIndex.Enabled = false;
                this.numericUpDownGroupIndex.Value = AppSett.Default.ModGroup_Def_Index;

                this.textBoxGroupName.Enabled = false;
                this.textBoxGroupName.Text = "";

                this.textBoxGroupDesc.Enabled = false;
                this.textBoxGroupDesc.Text = "";
                

                this.comboBoxGroupOper.Enabled = false;

                this.comboBoxGroupFunct.Enabled = false;
                
                this.numericUpDownGroupInt.Enabled = false;
                this.numericUpDownGroupInt.Value = AppSett.Default.ModGroup_Def_Int;

                this.numericUpDownGroupTrig.Enabled = false;
                this.numericUpDownGroupTrig.Value = AppSett.Default.ModGroup_Def_Trigg;

                this.numericUpDownGroupGate.Enabled = false;
                this.numericUpDownGroupGate.Value = AppSett.Default.ModGroup_Def_Gate;

                this.numericUpDownGroupSlave.Enabled = false;
                this.numericUpDownGroupSlave.Value = AppSett.Default.ModGroup_Def_Slave;

                this.numericUpDownGroupSig.Enabled = false;
                this.numericUpDownGroupSig.Value = 0;

                this.numericUpDownGroupReg.Enabled = false;
                this.numericUpDownGroupReg.Value = 0;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Empties Sel Group Signals
        /// </summary>
        public void EmptyGroupSig()
        {
            try
            {
                
                this.dataGridViewSignals.Enabled = false;
                this.dataGridViewSignals.Rows.Clear();
                this.tabPageGroupSig.Text = AppSett.Default.App_Group_SigBox;
            }
            catch (Exception e) { }
        }

        #endregion

        #region Methods: Initialize Functions

        /// <summary>
        /// Initializes all
        /// </summary>
        public void InitializeAll()
        {
            try
            {
                InitializeHeader();
                InitializeLinkParameters();
                InitializeCardParameters();
                InitializeGroupParameters();
                InitializeGroupSignals();
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Initializes Header
        /// </summary>
        public void InitializeHeader()
        {
            try
            {
                int i;

                EmptyHeader();

                this.comboBoxLoc.Items.Clear();

                for (i = 0; i < AppSett.Default.RLC_HWLoc.Count; i++)
                {
                    this.comboBoxLoc.Items.Add(AppSett.Default.RLC_HWLoc[i]);
                }

                this.numericUpDownDrop.Minimum = AppSett.Default.RLC_Drop_Min;
                this.numericUpDownDrop.Maximum = AppSett.Default.RLC_Drop_Max;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Initializes Link Parameters
        /// </summary>
        public void InitializeLinkParameters()
        {
            try
            {
                int i;

                EmptyLinkPar();

                this.comboBoxBaud.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Baud.Count; i++)
                {
                    this.comboBoxBaud.Items.Add(AppSett.Default.RLC_Baud[i]);
                }

                this.comboBoxDataBits.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_DataBits.Count; i++)
                {
                    this.comboBoxDataBits.Items.Add(AppSett.Default.RLC_DataBits[i]);
                }

                this.comboBoxParity.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Parity.Count; i++)
                {
                    this.comboBoxParity.Items.Add(AppSett.Default.RLC_Parity[i]);
                }

                this.comboBoxStopBits.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_StopBits.Count; i++)
                {
                    this.comboBoxStopBits.Items.Add(AppSett.Default.RLC_StopBits[i]);
                }

                this.comboBoxDuplex.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Duplex.Count; i++)
                {
                    this.comboBoxDuplex.Items.Add(AppSett.Default.RLC_Duplex[i]);
                }
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Initializes Card Parameters
        /// </summary>
        public void InitializeCardParameters()
        {
            try
            {
                int i;

                EmptyCardPar();

                this.comboBoxPlatform.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Platform.Count; i++)
                {
                    this.comboBoxPlatform.Items.Add(AppSett.Default.RLC_Platform[i]);
                }

                this.comboBoxFlowCtrl.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_FlowCtrl.Count; i++)
                {
                    this.comboBoxFlowCtrl.Items.Add(AppSett.Default.RLC_FlowCtrl[i]);
                }

                this.comboBoxSysLog.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_SysLog.Count; i++)
                {
                    this.comboBoxSysLog.Items.Add(AppSett.Default.RLC_SysLog[i]);
                }

                this.comboBoxBackupModeAct.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_BackupModeReact.Count; i++)
                {
                    this.comboBoxBackupModeAct.Items.Add(AppSett.Default.RLC_BackupModeReact[i]);
                }

                this.comboBoxList.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_List.Count; i++)
                {
                    this.comboBoxList.Items.Add(AppSett.Default.RLC_List[i]);
                }

                this.numericUpDownRetries.Minimum = AppSett.Default.RLC_Retries_Min;
                this.numericUpDownRetries.Maximum = AppSett.Default.RLC_Retries_Max;

                this.numericUpDownMsgTout.Minimum = AppSett.Default.RLC_MsgTout_Min;
                this.numericUpDownMsgTout.Maximum = AppSett.Default.RLC_MsgTout_Max;

                this.numericUpDownIntMsgDel.Minimum = AppSett.Default.RLC_InterMsgDel_Min;
                this.numericUpDownIntMsgDel.Maximum = AppSett.Default.RLC_InterMsgDel_Max;

                this.numericUpDownStatHoldTime.Minimum = AppSett.Default.RLC_StatHoldTime_Min;
                this.numericUpDownStatHoldTime.Maximum = AppSett.Default.RLC_StatHoldTime_Max;

                this.numericUpDownWDTime.Minimum = AppSett.Default.RLC_WDTime_Min;
                this.numericUpDownWDTime.Maximum = AppSett.Default.RLC_WDTime_Max;

                this.numericUpDownLinkStatReg.Minimum = AppSett.Default.RLC_LinkStatReg_Min;
                this.numericUpDownLinkStatReg.Maximum = AppSett.Default.RLC_LinkStatReg_Max;

                this.numericUpDownCtrlReg.Minimum = AppSett.Default.RLC_CtrlReg_Min;
                this.numericUpDownCtrlReg.Maximum = AppSett.Default.RLC_CtrlReg_Max;

                this.numericUpDownExOnceReg.Minimum = AppSett.Default.RLC_ExOnceReg_Min;
                this.numericUpDownExOnceReg.Maximum = AppSett.Default.RLC_ExOnceReg_Max;

                this.numericUpDownDiagReg.Minimum = AppSett.Default.RLC_Diag_Min;
                this.numericUpDownDiagReg.Maximum = AppSett.Default.RLC_Diag_Max;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Initializes Group Parameters
        /// </summary>
        public void InitializeGroupParameters()
        {
            try
            {
                int i;

                EmptyGroupPar();
                EmptyGroupList();

                this.comboBoxGroupOper.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Operation.Count; i++)
                {
                    this.comboBoxGroupOper.Items.Add(AppSett.Default.RLC_Operation[i]);
                }

                this.comboBoxGroupFunct.Items.Clear();
                for (i = 0; i < AppSett.Default.RLC_Function.Count; i++)
                {
                    this.comboBoxGroupFunct.Items.Add(AppSett.Default.RLC_Function[i]);
                }

                this.numericUpDownGroupInt.Minimum = AppSett.Default.RLC_Interval_Min;
                this.numericUpDownGroupInt.Maximum = AppSett.Default.RLC_Interval_Max;

                this.numericUpDownGroupTrig.Minimum = AppSett.Default.RLC_Trigger_Min;
                this.numericUpDownGroupTrig.Maximum = AppSett.Default.RLC_Trigger_Max;

                this.numericUpDownGroupGate.Minimum = AppSett.Default.RLC_Gate_Min;
                this.numericUpDownGroupGate.Maximum = AppSett.Default.RLC_Gate_Max;

                this.numericUpDownGroupSlave.Minimum = AppSett.Default.RLC_Diag_Min;
                this.numericUpDownGroupSlave.Maximum = AppSett.Default.RLC_Diag_Max;
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Initializes Group Signals
        /// </summary>
        public void InitializeGroupSignals()
        {
            try
            {
                int i;

                EmptyGroupSig();

                if (this.dataGridViewSignals.Columns.Count > 0)
                {
                    DataGridViewComboBoxColumn col;

                    col = this.dataGridViewSignals.Columns[0] as DataGridViewComboBoxColumn;
                    if (col != null)
                    {
                        col.DataSource = AppSett.Default.RLC_RegisterTypes;
                    }

                    col = this.dataGridViewSignals.Columns[2] as DataGridViewComboBoxColumn;
                    if ((col != null) && (AppSett.Default.ModSig_Types != null))
                    {
                        col.DataSource = AppSett.Default.ModSig_Types;
                    }

                    col = this.dataGridViewSignals.Columns[3] as DataGridViewComboBoxColumn;
                    if ((col != null) && (AppSett.Default.RLC_DataTypes != null))
                    {
                        col.DataSource = AppSett.Default.RLC_DataTypes;
                    }

                    col = this.dataGridViewSignals.Columns[5] as DataGridViewComboBoxColumn;
                    if ((col != null) && (this.AllBits != null))
                    {
                        col.DataSource = this.AllBits;
                    }

                    col = this.dataGridViewSignals.Columns[8] as DataGridViewComboBoxColumn;
                    if ((col != null) && (AppSett.Default.RLC_ConvTypes != null))
                    {
                        col.DataSource = AppSett.Default.RLC_ConvTypes;
                    }
                }
            }
            catch (Exception e) { }
        }
        
        #endregion

        #region Methods: Validate Functions

        /// <summary>
        /// Validates Card Parameters
        /// </summary>
        public bool ValidateCardPar()
        {
            bool ret = false;
            bool temp = false;
            bool validLinkStatReg = false;
            bool validCtrlStatReg = false;
            bool validExOnceReg = false;
            bool validDiagReg = false;

            try
            {
                if (this.rlcComm != null)
                {
                    List<long> registers = this.rlcComm.UsedModRegs;
                    int valueLinkStatReg = Convert.ToInt32(this.numericUpDownLinkStatReg.Value);
                    int valueCtrlStatReg = Convert.ToInt32(this.numericUpDownCtrlReg.Value);
                    int valueExOnceReg = Convert.ToInt32(this.numericUpDownExOnceReg.Value);
                    int valueDiagReg = Convert.ToInt32(this.numericUpDownDiagReg.Value);

                    if (registers != null)
                    {
                        // Link Stat Reg
                        if ((valueLinkStatReg != this.rlcComm.ParStatReg) && this.numericUpDownLinkStatReg.Enabled)
                        {
                            temp = registers.Contains(valueLinkStatReg);

                            if (temp)
                            {
                                validLinkStatReg = false;
                                this.numericUpDownLinkStatReg.Value = this.rlcComm.ParStatReg;
                                ShowErr(AppSett.Default.App_Errors_LinkStatReg, null);
                            }
                            else validLinkStatReg = true;
                        }
                        else validLinkStatReg = true;

                        // Ctrl Stat Reg
                        if ((valueCtrlStatReg != this.rlcComm.ParCtrlReg) && this.numericUpDownCtrlReg.Enabled)
                        {
                            temp = registers.Contains(valueCtrlStatReg);

                            if (temp)
                            {
                                validCtrlStatReg = false;
                                this.numericUpDownCtrlReg.Value = this.rlcComm.ParCtrlReg;
                                ShowErr(AppSett.Default.App_Errors_CtrlStatReg, null);
                            }
                            else validCtrlStatReg = true;
                        }
                        else validCtrlStatReg = true;

                        // Executed Once Reg
                        if ((valueExOnceReg != this.rlcComm.ParRedReg) && this.numericUpDownExOnceReg.Enabled)
                        {
                            temp = registers.Contains(valueExOnceReg);

                            if (temp)
                            {
                                validExOnceReg = false;
                                this.numericUpDownExOnceReg.Value = this.rlcComm.ParRedReg;
                                ShowErr(AppSett.Default.App_Errors_ExOnceReg, null);
                            }
                            else validExOnceReg = true;
                        }
                        else validExOnceReg = true;

                        // Diag Reg
                        if ((valueDiagReg != this.rlcComm.ParDiagReg) && this.numericUpDownDiagReg.Enabled)
                        {
                            temp = registers.Contains(valueDiagReg);

                            if (temp)
                            {
                                validDiagReg = false;
                                this.numericUpDownDiagReg.Value = this.rlcComm.ParDiagReg;
                                ShowErr(AppSett.Default.App_Errors_DiagReg, null);
                            }
                            else validDiagReg = true;
                        }
                        else validDiagReg = true;

                    } 
                }

                ret = validCtrlStatReg && validDiagReg && validExOnceReg && validLinkStatReg;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Validates Sel Group Parameters
        /// </summary>
        public bool ValidateSelGroupPar()
        {
            bool ret = false;
            bool temp = false;
            bool validName = false;
            bool validTriggReg = false;
            bool validGateReg = false;

            try
            {
                if ( (this.actGroup != null) && (this.rlcComm != null))
                {
                    List<long> registers = this.rlcComm.UsedModRegs;
                    List<string> groupNames = this.rlcComm.GroupsNames;
                    string valueName = textBoxGroupName.Text;
                    int valueTrigReg = Convert.ToInt32(this.numericUpDownGroupTrig.Value);
                    int valueGateReg = Convert.ToInt32(this.numericUpDownGroupGate.Value);

                    if ( (registers != null) && (groupNames != null))
                    {
                        // Name
                        if ( (valueName != this.actGroup.Name) && (this.textBoxGroupName.Enabled))
                        {
                            temp = groupNames.Contains(valueName);

                            if (temp)
                            {
                                validName = false;
                                this.textBoxGroupName.Text = this.actGroup.Name;
                                ShowErr(AppSett.Default.App_Errors_GroupName, null);
                            }
                            else validName = true;
                        }
                        else validName = true;

                        // Trigger Reg
                        if ((valueTrigReg != this.actGroup.Triggerreg) && (this.numericUpDownGroupTrig.Enabled))
                        {
                            temp = registers.Contains(valueTrigReg);

                            if (temp)
                            {
                                validTriggReg = false;
                                this.numericUpDownGroupTrig.Value = this.actGroup.Triggerreg;
                                ShowErr(AppSett.Default.App_Errors_TrigReg, null);
                            }
                            else validTriggReg = true;
                        }
                        else validTriggReg = true;

                        // Gate Reg
                        if ((valueGateReg != this.actGroup.Gatereg) && (this.numericUpDownGroupGate.Enabled))
                        {
                            temp = registers.Contains(valueGateReg);

                            if (temp)
                            {
                                validGateReg = false;
                                this.numericUpDownGroupGate.Value = this.actGroup.Gatereg;
                                ShowErr(AppSett.Default.App_Errors_GateReg, null);
                            }
                            else validGateReg = true;
                        }
                        else validGateReg = true;
                    }
                }

                ret = validName && validTriggReg && validGateReg;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Validates Register Type
        /// </summary>
        /// <param name="OldRegType">An old register type</param>
        /// <param name="NewRegType">An new register type</param>
        /// <param name="RegNumber">A register number</param>
        public bool ValidateRegType(ModSignal.SigRegTypes OldRegType, ModSignal.SigRegTypes NewRegType, long RegNumber)
        {
            bool ret = false;

            try
            {
                if ((OldRegType != null) && (NewRegType != null))
                {
                    List<long> freeRegs = this.AllFreeRegs;
                    long max = AppSett.Default.RLC_Registers_Max;
                    long temp;

                    if (freeRegs != null)
                    {
                        if (OldRegType == ModSignal.SigRegTypes.DTYPE)
                        {
                            if (NewRegType == ModSignal.SigRegTypes.DTYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.ITYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.FTYPE)
                            {
                                if (RegNumber == max) ret = false;
                                else ret = freeRegs.Contains((RegNumber + 1));
                            }
                            if (NewRegType == ModSignal.SigRegTypes.STYPE)
                            {
                                if (RegNumber >= (max-1)) ret = false;
                                else ret = freeRegs.Contains((RegNumber + 1)) && freeRegs.Contains((RegNumber + 2));
                            }
                        }
                        if (OldRegType == ModSignal.SigRegTypes.ITYPE)
                        {
                            if (NewRegType == ModSignal.SigRegTypes.DTYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.ITYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.FTYPE)
                            {
                                if (RegNumber == max) ret = false;
                                else ret = freeRegs.Contains((RegNumber + 1));
                            }
                            if (NewRegType == ModSignal.SigRegTypes.STYPE)
                            {
                                if (RegNumber >= (max - 1)) ret = false;
                                else ret = freeRegs.Contains((RegNumber + 1)) && freeRegs.Contains((RegNumber + 2));
                            }
                        }
                        if (OldRegType == ModSignal.SigRegTypes.FTYPE)
                        {
                            if (NewRegType == ModSignal.SigRegTypes.DTYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.ITYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.FTYPE) ret = true;
                            if (NewRegType == ModSignal.SigRegTypes.STYPE)
                            {
                                if (RegNumber == max) ret = false;
                                else ret = freeRegs.Contains((RegNumber + 1));
                            }
                        }
                        if (OldRegType == ModSignal.SigRegTypes.STYPE) ret = true;
                    }
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }


        #endregion

        #region Methods: Update Functions

        /// <summary>
        /// Updates Group Tree View
        /// </summary>
        public void UpdateGroupTree()
        {
            try
            {
                if (this.treeViewGroups != null)
                {
                    int i;

                    if (this.treeViewGroups.Nodes != null)
                    {
                        ModGroup tempGroup;
                        string tempName;

                        for (i = 0; i < this.treeViewGroups.Nodes.Count; i++)
                        {
                            tempGroup = this.treeViewGroups.Nodes[i].Tag as ModGroup;

                            if (tempGroup != null)
                            {
                                tempName = tempGroup.Index.ToString() + ":  " + tempGroup.Name;
                                this.treeViewGroups.Nodes[i].Text = tempName;
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        #endregion

        #region Methods: Remove Functions

        /// <summary>
        /// Removes actual selected group
        /// </summary>
        public void RemoveGroup()
        {
            try
            {
                if (this.actGroup != null)
                {
                    string mess = "Group (" + this.actGroup.Name + ") will be deleted.\n\r\n\rAre you sure?";

                    DialogResult res = MessageBox.Show(mess, "Delete Group", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (res == DialogResult.Yes)
                    {
                        if (this.rlcComm != null)
                        {
                            bool resBool = this.rlcComm.Remove(this.actGroup);

                            if (resBool)
                            {
                                ShowGroups();
                                EmptyGroupPar();
                                EmptyGroupSig();
                                this.groupBoxSelGroup.Text = AppSett.Default.App_SelGroup_GroupBox;
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        #endregion

        #region Methods: Add Functions

        /// <summary>
        /// Adds a new group
        /// </summary>
        public void AddGroup()
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.UpdateGroupsInd();

                    int newIndex = this.rlcComm.Groups.Count;
                    string newName = this.UniqueGroupName;
                    List<long> freeRegs = this.AllFreeRegs;

                    if ( (newName != null) && (freeRegs != null))
                    {
                        if (freeRegs.Count >= 3)
                        {
                            int triggerReg = AppSett.Default.ModGroup_Def_Trigg;
                            int gateReg = AppSett.Default.ModGroup_Def_Gate;

                            ModGroup newGroup = new ModGroup(newIndex, newName, "", ModGroup.GroupOperation.PERIODIC, AppSett.Default.ModGroup_Def_Int, triggerReg, gateReg, AppSett.Default.ModGroup_Def_Slave, ModGroup.GroupFunction.RIR);

                            if (newGroup != null)
                            {
                                bool res = this.rlcComm.Add(newGroup);

                                if (!res) ShowErr("Failed to add a new group!", null);

                                ShowGroups();
                                SelectLastGroup();

                            }
                            else ShowErr("Failed to create a new group!", null);
                        }
                        else ShowErr("Not enough free register to add a new group!", null);
                    }
                    else ShowErr("Failed to get a new name!", null);
                }
                else ShowErr("No file loaded!", null);
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        #endregion

        #region Get Functions

        /// <summary>
        /// Get Register Type from specified symbol
        /// </summary>
        /// <param name="Symbol">A specified symbol</param>
        public ModSignal.SigRegTypes GetRegType(string Symbol)
        {
            ModSignal.SigRegTypes ret = ModSignal.SigRegTypes.ITYPE;

            if (Symbol != null)
            {
                if (String.Compare(Symbol, AppSett.Default.ModSig_Char_DTYPE, true) == 0) ret = ModSignal.SigRegTypes.DTYPE;
                if (String.Compare(Symbol, AppSett.Default.ModSig_Char_ITYPE, true) == 0) ret = ModSignal.SigRegTypes.ITYPE;
                if (String.Compare(Symbol, AppSett.Default.ModSig_Char_FTYPE, true) == 0) ret = ModSignal.SigRegTypes.FTYPE;
                if (String.Compare(Symbol, AppSett.Default.ModSig_Char_STYPE, true) == 0) ret = ModSignal.SigRegTypes.STYPE;
            }

            return ret;
        }

        #endregion

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime Time1 = DateTime.Now;

            #region First Test

            ModAdd add = new ModAdd(65000, 3, ModAdd.ModLoc.InputRegisters);

            #endregion

            DateTime Time2 = DateTime.Now;

            TimeSpan diff1 = Time2 - Time1;
        }

        private void tempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.rlcComm = new ModComm("E:\\test.bin");
            this.rlcComm.LocDrop = 13;
            this.rlcComm.LocPCI = 1;
            this.rlcComm.LocBranch = 1;
            this.rlcComm.LocSlot = 4;
            this.rlcComm.HeaderAuthor = "Petr Prochazka";
            this.rlcComm.HeaderComp = "Valcon Int";
            this.rlcComm.HeaderDesc = "Test config file";
            this.rlcComm.HeaderEmail = "prochazka@valcon-int.com";
            this.rlcComm.HeaderName = "TestConfigFile";
            this.rlcComm.HeaderProject = "San Vittore";
            this.rlcComm.HeaderRev = "rev 1";
            this.rlcComm.Date = DateTime.Now;
            this.rlcComm.ParBaud = 19200;

            ModGroup group1 = new ModGroup(0, "Group 1", "Test group 1", ModGroup.GroupOperation.PERIODIC, 1500, 1910, 1920, 11, ModGroup.GroupFunction.RHR);
            ModGroup group2 = new ModGroup(1, "Group 2", "Test group 2", ModGroup.GroupOperation.TRIGGERED, 2000, 1930, 1940, 11, ModGroup.GroupFunction.RIR);
            ModGroup group3 = new ModGroup(2, "Group 3", "Test group 3", ModGroup.GroupOperation.GATED, 2000, 1950, 1960, 11, ModGroup.GroupFunction.FMC);

            ModPoint sig1 = new ModPoint(0, "Sig1", "Signal 1", ModPoint.ModDataType.INT16, ModGroup.GroupFunction.RHR, new ModReg(10, ModReg.ModRegType.DTYPE), new ModAdd(10, 1, ModAdd.ModLoc.HoldingRegisters), new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));
            ModPoint sig2 = new ModPoint(0, "Sig2", "Signal 2", ModPoint.ModDataType.INT16, ModGroup.GroupFunction.RHR, new ModReg(11, ModReg.ModRegType.DTYPE), new ModAdd(10, 3, ModAdd.ModLoc.HoldingRegisters), new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));
            ModPoint sig3 = new ModPoint(0, "Sig3", "Signal 3", ModPoint.ModDataType.INT32, ModGroup.GroupFunction.RHR, new ModReg(12, ModReg.ModRegType.FTYPE), new ModAdd(11, ModAdd.ModLoc.HoldingRegisters), new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));
            ModPoint sig4 = new ModPoint(0, "Sig4", "Signal 4", ModPoint.ModDataType.INT16, ModGroup.GroupFunction.RHR, new ModReg(14, ModReg.ModRegType.DTYPE), new ModAdd(10,2, ModAdd.ModLoc.HoldingRegisters), new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));
            ModPoint sig5 = new ModPoint(0, "Sig5", "Signal 5", ModPoint.ModDataType.INT32, ModGroup.GroupFunction.RHR, new ModReg(15, ModReg.ModRegType.FTYPE), new ModAdd(14,ModAdd.ModLoc.HoldingRegisters), new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));

            bool res1 = group1.Add(sig1);
            bool res2 = group1.Add(sig2);
            bool res3 = group1.Add(sig3);
            bool res4 = group2.Add(sig4);
            bool res5 = group2.Add(sig5);

            this.rlcComm.Groups.Add(group1);
            this.rlcComm.Groups.Add(group2);
            this.rlcComm.Groups.Add(group3);

            ShowHeader();
            ShowLinkPar();
            ShowCardPar();
            ShowGroups();

            //ModAdd add1 = new ModAdd(50, ModAdd.ModLoc.HoldingRegisters);
            //ModAdd add2 = new ModAdd(51, "1110000000000000",ModAdd.ModLoc.HoldingRegisters);
            //ModAdd add3 = new ModAdd(52, ModAdd.ModLoc.HoldingRegisters);
            //List<ModAdd> freeAdds = new List<ModAdd>();
            //freeAdds.Add(add1);
            //freeAdds.Add(add2);
            //freeAdds.Add(add3);

            //ModAdd test = new ModAdd(53,2, ModAdd.ModLoc.HoldingRegisters);
            //ModPoint point = new ModPoint(0, "Point 1", "Point 1", ModPoint.ModDataType.INT16, ModGroup.GroupFunction.RHR, new ModReg(0, ModReg.ModRegType.DTYPE), test, new ModConv(ModConv.ModConvType.NoConv, 0, 0, 0, 0, 0, 0));
            //int res = point.ChangeAddNum(51, freeAdds);
            

            //ModPoint testIn = new ModPoint(0,"Signal 1","Signal 1",ModPoint.ModDataType.INT16,ModGroup.GroupFunction.RIR, new ModReg(10,ModReg.ModRegType.ITYPE),new ModAdd(50,ModAdd.ModLoc.InputRegisters),new ModConv(ModConv.ModConvType.NoConv,0,0,0,0,0,0));
            //Stream fileIn = new FileStream("E:\\temp.bin", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            //BinaryWriter bw = new BinaryWriter(fileIn);
            //bool res1 = testIn.SerializeToStream(ref bw);
            //fileIn.Close();

            //Stream fileOut = new FileStream("E:\\temp.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            //BinaryReader br = new BinaryReader(fileOut);
            //ModPoint testOut = new ModPoint(ref br);
            //fileOut.Close();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EmptyAll();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tabPageText_Enter(object sender, EventArgs e)
        {
            ShowText();
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderName = this.textBoxName.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxDesc_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderDesc = this.textBoxDesc.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxProject_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderProject= this.textBoxProject.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxRev_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderRev = this.textBoxRev.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxComp_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderComp = this.textBoxComp.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxAuthor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderAuthor = this.textBoxAuthor.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxEmail_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.HeaderEmail = this.textBoxEmail.Text;
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownDrop_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.LocDrop= Convert.ToInt32(this.numericUpDownDrop.Value);
                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxLoc_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxLoc.SelectedItem.ToString();

                    this.rlcComm.UpdateLoc(temp);

                    this.state = AppState.FileNeedsSave;
                    ShowHeader();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxBaud_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxBaud.SelectedItem.ToString();

                    this.rlcComm.ParBaud = Convert.ToInt32(temp);

                    this.state = AppState.FileNeedsSave;
                    ShowLinkPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxDataBits_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxDataBits.SelectedItem.ToString();

                    this.rlcComm.ParDataBits = Convert.ToInt32(temp);

                    this.state = AppState.FileNeedsSave;
                    ShowLinkPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxParity_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxParity.SelectedItem.ToString();

                    if (String.Compare(temp,AppSett.Default.RLC_Parity[0],true) == 0)
                    {
                        this.rlcComm.ParParity = ModComm.CommParity.Odd;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_Parity[1], true) == 0)
                    {
                        this.rlcComm.ParParity = ModComm.CommParity.Even;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_Parity[2], true) == 0)
                    {
                        this.rlcComm.ParParity = ModComm.CommParity.None;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowLinkPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxStopBits_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxStopBits.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_StopBits[0], true) == 0)
                    {
                        this.rlcComm.ParStopBits = ModComm.CommStopBits.one;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_StopBits[1], true) == 0)
                    {
                        this.rlcComm.ParStopBits = ModComm.CommStopBits.oneAndHalf;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_StopBits[2], true) == 0)
                    {
                        this.rlcComm.ParStopBits = ModComm.CommStopBits.two;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowLinkPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxDuplex_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxDuplex.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_Duplex[0], true) == 0)
                    {
                        this.rlcComm.ParDuplex = ModComm.CommDuplex.half;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_Duplex[1], true) == 0)
                    {
                        this.rlcComm.ParDuplex = ModComm.CommDuplex.full;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowLinkPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxPlatform_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxPlatform.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_Platform[0], true) == 0)
                    {
                        this.rlcComm.ParPlatform = ModComm.CommPlatform.PC;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_Platform[1], true) == 0)
                    {
                        this.rlcComm.ParPlatform = ModComm.CommPlatform.QLC;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_Platform[2], true) == 0)
                    {
                        this.rlcComm.ParPlatform = ModComm.CommPlatform.RLC;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownRetries_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.ParRetries = Convert.ToInt32(this.numericUpDownRetries.Value);

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxFlowCtrl_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxFlowCtrl.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_FlowCtrl[0], true) == 0)
                    {
                        this.rlcComm.ParFlowCtrl = ModComm.CommFlowCtrl.on;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_FlowCtrl[1], true) == 0)
                    {
                        this.rlcComm.ParFlowCtrl = ModComm.CommFlowCtrl.off;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_FlowCtrl[2], true) == 0)
                    {
                        this.rlcComm.ParFlowCtrl = ModComm.CommFlowCtrl.rts_on_tx;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownMsgTout_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.ParMsgTout = Convert.ToInt32(this.numericUpDownMsgTout.Value);

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownIntMsgDel_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.ParInterMsgDel = Convert.ToInt32(this.numericUpDownIntMsgDel.Value);

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxSysLog_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxSysLog.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[0], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[0]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[1], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[1]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[2], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[2]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[3], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[3]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[4], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[4]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[5], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[5]);
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_SysLog[6], true) == 0)
                    {
                        this.rlcComm.ParSysLog = Convert.ToInt32(AppSett.Default.RLC_SysLog[6]);
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownStatHoldTime_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.ParStatHoldTime= Convert.ToInt32(this.numericUpDownStatHoldTime.Value);

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownWDTime_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    this.rlcComm.ParWdTime= Convert.ToInt32(this.numericUpDownWDTime.Value);

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownLinkStatReg_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    bool temp = ValidateCardPar();

                    if (temp)
                    {
                        this.rlcComm.ParStatReg = Convert.ToInt32(this.numericUpDownLinkStatReg.Value);

                        this.state = AppState.FileNeedsSave;

                        ShowCardPar();
                    }

                    
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownCtrlReg_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    bool temp = ValidateCardPar();

                    if (temp)
                    {
                        this.rlcComm.ParCtrlReg = Convert.ToInt32(this.numericUpDownCtrlReg.Value);

                        this.state = AppState.FileNeedsSave;
                        
                        ShowCardPar();
                    }    
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownExOnceReg_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    bool temp = ValidateCardPar();

                    if (temp)
                    {
                        this.rlcComm.ParRedReg = Convert.ToInt32(this.numericUpDownExOnceReg.Value);

                        this.state = AppState.FileNeedsSave;

                        ShowCardPar();
                    }    
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownDiagReg_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    bool temp = ValidateCardPar();

                    if (temp)
                    {
                        this.rlcComm.ParDiagReg= Convert.ToInt32(this.numericUpDownDiagReg.Value);

                        this.state = AppState.FileNeedsSave;

                        ShowCardPar();
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxBackupModeAct_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxBackupModeAct.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_BackupModeReact[0], true) == 0)
                    {
                        this.rlcComm.ParBackupMode = ModComm.CommBackupMode.never;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_BackupModeReact[1], true) == 0)
                    {
                        this.rlcComm.ParBackupMode = ModComm.CommBackupMode.mute;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_BackupModeReact[2], true) == 0)
                    {
                        this.rlcComm.ParBackupMode = ModComm.CommBackupMode.read_only;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxList_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.rlcComm != null)
                {
                    string temp = this.comboBoxList.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_List[0], true) == 0)
                    {
                        this.rlcComm.ParList = ModComm.CommList.off;
                    }

                    if (String.Compare(temp, AppSett.Default.RLC_List[1], true) == 0)
                    {
                        this.rlcComm.ParList = ModComm.CommList.on;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowCardPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void treeViewGroups_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                TreeView view = sender as TreeView;

                if (view != null)
                {
                    if (view.SelectedNode != null)
                    {
                        TreeNode temp = view.SelectedNode;

                        if (temp != null)
                        {
                            if (temp.Tag != null)
                            {
                                this.actGroup = temp.Tag as ModGroup;

                                if (this.actGroup != null)
                                {
                                    this.statusBarText.Text = temp.Text;
                                    this.buttonGroupRem.Enabled = true;
                                    this.groupBoxSelGroup.Text = AppSett.Default.App_SelGroup_GroupBox + " (" + this.actGroup.Name + ")";

                                    EmptyGroupPar();
                                    EmptyGroupSig();

                                    ShowGroupPar();
                                    ShowGroupSig();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }

            Cursor.Current = Cursors.Default;
        }

        private void textBoxGroupName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.textBoxGroupName.Enabled)
                {
                    bool temp = ValidateSelGroupPar();

                    if (temp)
                    {
                        this.actGroup.Name = this.textBoxGroupName.Text;

                        this.state = AppState.FileNeedsSave;

                        this.groupBoxSelGroup.Text = AppSett.Default.App_SelGroup_GroupBox + " (" + this.actGroup.Name + ")";

                        UpdateGroupTree();

                        ShowGroupPar();
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void textBoxGroupDesc_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.textBoxGroupDesc.Enabled)
                {
                    this.actGroup.Description = this.textBoxGroupDesc.Text;

                    this.state = AppState.FileNeedsSave;
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxGroupOper_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.comboBoxGroupOper.Enabled)
                {
                    string temp = this.comboBoxGroupOper.SelectedItem.ToString();
                    List<long> freeRegs = this.AllFreeRegs;
                    bool tempBool = false;
                    int tempTrigg = 0;
                    int tempGate = 0;

                    if (freeRegs != null)
                    {
                        if (String.Compare(temp, AppSett.Default.RLC_Operation[0], true) == 0)
                        {
                            this.actGroup.Operation = ModGroup.GroupOperation.PERIODIC;
                            this.numericUpDownGroupInt.Enabled = true;
                            this.numericUpDownGroupTrig.Enabled = false;
                            this.numericUpDownGroupGate.Enabled = false;
                        }

                        if (String.Compare(temp, AppSett.Default.RLC_Operation[1], true) == 0)
                        {
                            tempTrigg = this.actGroup.Triggerreg;
                            tempBool = freeRegs.Contains(tempTrigg);

                            if (!tempBool)
                            {
                                tempTrigg = Convert.ToInt32(freeRegs[freeRegs.Count - 1]);
                                this.actGroup.Triggerreg = tempTrigg;
                                this.numericUpDownGroupTrig.Value = tempTrigg;
                            }

                            this.actGroup.Operation = ModGroup.GroupOperation.TRIGGERED;
                            this.numericUpDownGroupInt.Enabled = false;
                            this.numericUpDownGroupTrig.Enabled = true;
                            this.numericUpDownGroupGate.Enabled = false;
                        }

                        if (String.Compare(temp, AppSett.Default.RLC_Operation[2], true) == 0)
                        {
                            tempGate = this.actGroup.Gatereg;
                            tempBool = freeRegs.Contains(tempGate);

                            if (!tempBool)
                            {
                                tempGate = Convert.ToInt32(freeRegs[freeRegs.Count - 1]);
                                this.actGroup.Gatereg = tempGate;
                                this.numericUpDownGroupGate.Value = tempGate;
                            }

                            this.actGroup.Operation = ModGroup.GroupOperation.GATED;
                            this.numericUpDownGroupInt.Enabled = false;
                            this.numericUpDownGroupTrig.Enabled = false;
                            this.numericUpDownGroupGate.Enabled = true;
                        }

                        this.state = AppState.FileNeedsSave;
                         
                    }
                    else ShowErr("Failed to change value!", null);

                    ShowGroupPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownGroupInt_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.numericUpDownGroupInt.Enabled)
                {
                    
                    this.actGroup.Interval = Convert.ToInt64(this.numericUpDownGroupInt.Value);

                    this.state = AppState.FileNeedsSave;

                    ShowGroupPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownGroupTrig_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.numericUpDownGroupTrig.Enabled)
                {
                    bool temp = ValidateSelGroupPar();

                    if (temp)
                    {
                        this.actGroup.Triggerreg = Convert.ToInt32(this.numericUpDownGroupTrig.Value);

                        this.state = AppState.FileNeedsSave;

                        ShowGroupPar();
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownGroupGate_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.numericUpDownGroupGate.Enabled)
                {
                    bool temp = ValidateSelGroupPar();

                    if (temp)
                    {
                        this.actGroup.Gatereg = Convert.ToInt32(this.numericUpDownGroupGate.Value);

                        this.state = AppState.FileNeedsSave;

                        ShowGroupPar();
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void numericUpDownGroupSlave_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.numericUpDownGroupSlave.Enabled)
                {

                    this.actGroup.Slave = Convert.ToInt32(this.numericUpDownGroupSlave.Value);

                    this.state = AppState.FileNeedsSave;

                    ShowGroupPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void comboBoxGroupFunct_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if ((this.actGroup != null) && this.comboBoxGroupFunct.Enabled)
                {
                    string temp = this.comboBoxGroupFunct.SelectedItem.ToString();

                    if (String.Compare(temp, AppSett.Default.RLC_Function[0], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.RCS;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[1], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.RIS;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[2], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.RHR;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[3], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.RIR;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[4], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.FSC;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[5], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.PSR;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[6], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.RES;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[7], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.FMC;
                    }
                    if (String.Compare(temp, AppSett.Default.RLC_Function[8], true) == 0)
                    {
                        this.actGroup.Function = ModGroup.GroupFunction.PMR;
                    }

                    this.state = AppState.FileNeedsSave;
                    ShowGroupPar();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void buttonGroupRem_Click(object sender, EventArgs e)
        {
            try
            {
                RemoveGroup();
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void treeViewGroups_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyData == Keys.Delete)
                {
                    RemoveGroup();
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void buttonGroupAdd_Click(object sender, EventArgs e)
        {
            try
            {
                AddGroup();
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void groupsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Add Group
            if (treeViewGroups.Enabled) this.addGroupToolStripMenuItem.Enabled = true;
            else this.addGroupToolStripMenuItem.Enabled = false;
            this.addGroupToolStripMenuItem.Text = AppSett.Default.App_MainMenu_AddGroup;

            // Remove Group
            if (treeViewGroups.Enabled && (this.actGroup != null))
            {
                this.removeGroupToolStripMenuItem.Text = AppSett.Default.App_MainMenu_RemGroup + " (" + this.actGroup.Name + ")";
                this.removeGroupToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.removeGroupToolStripMenuItem.Text = AppSett.Default.App_MainMenu_RemGroup;
                this.removeGroupToolStripMenuItem.Enabled = false;
            }
        }

        private void addGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.tabControlMain.SelectedIndex = 1;
                this.tabControlGroup.SelectedIndex = 0;
                AddGroup();
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void removeGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.tabControlMain.SelectedIndex = 1;
                this.tabControlGroup.SelectedIndex = 0;
                RemoveGroup();
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        private void dataGridViewSignals_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView table = sender as DataGridView;
                int ErrCode = 0;

                if (table != null)
                {
                    ModPoint sigMod = table.Rows[e.RowIndex].Tag as ModPoint;

                    if (sigMod != null)
                    {
                        int columnIndex = e.ColumnIndex;
                        int rowIndex = e.RowIndex;
                        bool res = false;
                        string columnValue = "";
                        double columnValDoub = 0;

                        // Register Type
                        if (columnIndex == 0)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Register Number
                        if (columnIndex == 1)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }
                        
                        // Signal Type
                        if (columnIndex == 2)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Data Type
                        if (columnIndex == 3)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Address
                        if (columnIndex == 4)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Bit
                        if (columnIndex == 5)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Tag
                        if (columnIndex == 6)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Description
                        if (columnIndex == 7)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                        }

                        // Conv Type
                        if (columnIndex == 8)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();


                        }

                        // C0
                        if (columnIndex == 9)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Change
                                sigMod.Conversion.Coeff1 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff1.ToString();
                            }
                        }

                        // C2
                        if (columnIndex == 10)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Call change function
                                sigMod.Conversion.Coeff2 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff2.ToString();
                            }
                        }

                        // C3
                        if (columnIndex == 11)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Call change function
                                sigMod.Conversion.Coeff3 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff3.ToString();
                            }
                        }

                        // C4
                        if (columnIndex == 12)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Call change function
                                sigMod.Conversion.Coeff4 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff4.ToString();
                            }
                        }

                        // C5
                        if (columnIndex == 13)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Call change function
                                sigMod.Conversion.Coeff5 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff5.ToString();
                            }
                        }

                        // C6
                        if (columnIndex == 14)
                        {
                            columnValue = table.Rows[rowIndex].Cells[columnIndex].Value.ToString();

                            try
                            {
                                columnValDoub = Convert.ToDouble(columnValue);

                                // Call change function
                                sigMod.Conversion.Coeff6 = columnValDoub;
                            }
                            catch (Exception Ex)
                            {
                                // Error Message
                                ShowErr("Wrong value!", null);

                                // Go back with value
                                table.Rows[rowIndex].Cells[columnIndex].Value = sigMod.Conversion.Coeff6.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ShowErr("An error has occured!", Ex);
            }
        }

        /// <summary>
        /// Change the conversion type
        /// </summary>
        /// <param name="Signal">A signal to be changed</param>
        /// <param name="NewConvType">A new conversion type</param>
        /// <param name="RowIndex">A row index</param>
        void ChangeConvType(ref ModPoint Signal, string NewConvType, int RowIndex)
        {
            try
            {
                if (Signal == null)
                {
                    ShowErr("Wrong call!", null);
                    return;
                }

                if (NewConvType == null)
                {
                    ShowErr("Wrong call!", null);
                    return;
                }

                int i;

                bool correct = false;

                for (i = 0; i < AppSett.Default.RLC_ConvTypes.Count; i++)
                { 
                    if(String.Compare(AppSett.Default.RLC_ConvTypes[i],NewConvType,true)==0)
                    {
                        correct = true;
                        break;
                    }
                }

                if(!correct)
                {
                    ShowErr("Wrong conversion type!", null);
                    return;
                }

                
            }
            catch (Exception e) { }
        }
    }
}
