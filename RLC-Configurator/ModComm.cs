using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

namespace RLC_Configurator
{
    public class ModComm
    {

        #region Objects: RegEx Patterns

        // Pattern for: (TYPE="VALUE" NAME="VALUE"
        const string C_Reg_Patt1 = "([12]{1,1})\\.([1-8]{1,1})\\.([1-8]{1,1})";

        #endregion
        
        #region Objects: Enumerators

        public enum CommPlatform
        {
            PC,
            QLC,
            RLC,
        }

        public enum CommParity
        {
            Odd,
            Even,
            None,
        }

        public enum CommFlowCtrl
        {
            on,
            off,
            rts_on_tx,
        }

        public enum CommDuplex
        {
            full,
            half,
        }

        public enum CommBackupMode
        {
            read_only,
            mute,
            never,
        }

        public enum CommList
        {
            on,
            off,
        }

        public enum CommStopBits
        {
            one,
            oneAndHalf,
            two,
        }

        #endregion
        
        #region Objects: General

        private string path;
        private string name;

        private string headerName;
        private string headerDesc;
        private string headerRev;
        private string headerAuthor;
        private string headerComp;
        private string headerEmail;
        private string headerProject;

        private int locDrop;
        private int locPCI;
        private int locBranch;
        private int locSlot;

        private CommPlatform parPlatform;
        private int parBaud;
        private int parDataBits;
        private CommParity parParity;
        private CommStopBits parStopBits;
        private int parRetries;
        private CommFlowCtrl parFlowCtrl;
        private CommDuplex parDuplex;
        private int parMsgTout;
        private int parInterMsgDel;
        private int parSysLog;
        private int parStatReg;
        private long parStatHoldTime;
        private int parWdTime;
        private int parCtrlReg;
        private int parRedReg;
        private CommBackupMode parBackupMode;
        private int parDiagReg;
        private CommList parList;

        private List<ModGroup> groups;

        #endregion


        #region Public Properties: General

        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string HeaderName
        {
            get { return this.headerName; }
            set { this.headerName = value; }
        }
        public string HeaderDesc
        {
            get { return this.headerDesc; }
            set { this.headerDesc = value; }
        }
        public string HeaderRev
        {
            get { return this.headerRev; }
            set { this.headerRev = value; }
        }
        public string HeaderAuthor
        {
            get { return this.headerAuthor; }
            set { this.headerAuthor = value; }
        }
        public string HeaderComp
        {
            get { return this.headerComp; }
            set { this.headerComp = value; }
        }
        public string HeaderEmail
        {
            get { return this.headerEmail; }
            set { this.headerEmail = value; }
        }
        public string HeaderProject
        {
            get { return this.headerProject; }
            set { this.headerProject = value; }
        }

        public int LocDrop
        {
            get { return this.locDrop; }
            set { this.locDrop = value; }
        }
        public int LocPCI
        {
            get { return this.locPCI; }
            set { this.locPCI = value; }
        }
        public int LocBranch
        {
            get { return this.locBranch; }
            set { this.locBranch = value; }
        }
        public int LocSlot
        {
            get { return this.locSlot; }
            set { this.locSlot = value; }
        }
        public string LocFull
        {
            get
            {
                string ret = null;

                if (this.IsDefinedPCI && this.IsDefinedBranch && this.IsDefinedSlot)
                {
                    ret = this.locPCI.ToString() + "." + this.locBranch.ToString() + "." + this.locSlot.ToString();
                }

                return ret;
            }
        }

        public string HWAddDec
        {
            get
            {
                string ret = null;
                bool found = false;
                int i;

                if (this.IsDefinedFullLoc)
                {
                    for (i = 0; i < AppSett.Default.RLC_HWLoc.Count; i++)
                    {
                        if (String.Compare(AppSett.Default.RLC_HWLoc[i], this.LocFull) == 0)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) ret = AppSett.Default.RLC_HWAddDec[i];
                }

                return ret;
            }
        }
        public string HWAddHex
        {
            get
            {
                string ret = null;
                bool found = false;
                int i;

                if (this.IsDefinedFullLoc)
                {
                    for (i = 0; i < AppSett.Default.RLC_HWLoc.Count; i++)
                    {
                        if (String.Compare(AppSett.Default.RLC_HWLoc[i], this.LocFull) == 0)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) ret = AppSett.Default.RLC_HWAddHex[i];
                }

                return ret;
            }
        }

        public CommPlatform ParPlatform
        {
            get { return this.parPlatform; }
            set { this.parPlatform = value; }
        }
        public int ParBaud
        {
            get { return this.parBaud; }
            set { this.parBaud = value; }
        }
        public int ParDataBits
        {
            get { return this.parDataBits; }
            set { this.parDataBits = value; }
        }
        public CommParity ParParity
        {
            get { return this.parParity; }
            set { this.parParity = value; }
        }
        public CommStopBits ParStopBits
        {
            get { return this.parStopBits; }
            set { this.parStopBits = value; }
        }
        public int ParRetries
        {
            get { return this.parRetries; }
            set { this.parRetries = value; }
        }
        public CommFlowCtrl ParFlowCtrl
        {
            get { return this.parFlowCtrl; }
            set { this.parFlowCtrl = value; }
        }
        public CommDuplex ParDuplex
        {
            get { return this.parDuplex; }
            set { this.parDuplex = value; }
        }
        public int ParMsgTout
        {
            get { return this.parMsgTout; }
            set { this.parMsgTout = value; }
        }
        public int ParInterMsgDel
        {
            get { return this.parInterMsgDel; }
            set { this.parInterMsgDel = value; }
        }
        public int ParSysLog
        {
            get { return this.parSysLog; }
            set { this.parSysLog = value; }
        }
        public int ParStatReg
        {
            get { return this.parStatReg; }
            set { this.parStatReg = value; }
        }
        public long ParStatHoldTime
        {
            get { return this.parStatHoldTime; }
            set { this.parStatHoldTime = value; }
        }
        public int ParWdTime
        {
            get { return this.parWdTime; }
            set { this.parWdTime = value; }
        }
        public int ParCtrlReg
        {
            get { return this.parCtrlReg; }
            set { this.parCtrlReg = value; }
        }
        public int ParRedReg
        {
            get { return this.parRedReg; }
            set { this.parRedReg = value; }
        }
        public CommBackupMode ParBackupMode
        {
            get { return this.parBackupMode; }
            set { this.parBackupMode = value; }
        }
        public int ParDiagReg
        {
            get { return this.parDiagReg; }
            set { this.parDiagReg = value; }
        }
        public CommList ParList
        {
            get { return this.parList; }
            set { this.parList = value; }
        }

        public List<ModGroup> Groups
        {
            get { return this.groups; }
            set { this.groups = value; }
        }

        public List<string> GroupsNames
        {
            get
            {
                List<string> ret = new List<string>();

                if (this.groups != null)
                {
                    foreach (var elem in this.groups) ret.Add(elem.Name);
                }

                return ret;
            }
        }

        public DateTime Date { get; set; }

        public int TextHeadMax
        {
            get
            { 
                int ret = 0;

                if (AppSett.Default.ModComm_HeadAuth_Text.Length > ret) ret = AppSett.Default.ModComm_HeadAuth_Text.Length;
                if (AppSett.Default.ModComm_HeadComp_Text.Length > ret) ret = AppSett.Default.ModComm_HeadComp_Text.Length;
                if (AppSett.Default.ModComm_HeadDate_Text.Length > ret) ret = AppSett.Default.ModComm_HeadDate_Text.Length;
                if (AppSett.Default.ModComm_HeadHW_Text.Length > ret) ret = AppSett.Default.ModComm_HeadHW_Text.Length;
                if (AppSett.Default.ModComm_HeadName_Text.Length > ret) ret = AppSett.Default.ModComm_HeadName_Text.Length;
                if (AppSett.Default.ModComm_HeadProject_Text.Length > ret) ret = AppSett.Default.ModComm_HeadProject_Text.Length;
                if (AppSett.Default.ModComm_HeadRev_Text.Length > ret) ret = AppSett.Default.ModComm_HeadRev_Text.Length;

                return ret;
            }
        }
        public string TextShort
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = "Name=" + this.name + " Groups=" + this.groups.Count;
                }

                return ret;
            }
        }
        public string TextDesc
        {
            get
            {
                string ret = "";

                ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadDesc_Text + " = ";
                
                if (this.headerDesc != null)
                {
                    ret = ret + this.headerDesc;
                }

                return ret;
            }
        }
        public string TextName
        {
            get
            {
                string ret = "";

                if (this.headerName != null)
                {
                    ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadName_Text + " = ";

                    ret = ret + this.headerName;
                }

                return ret;
            }
        }
        public string TextProject
        {
            get
            {
                string ret = "";

                if (this.headerProject != null)
                {
                    ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadProject_Text + " = ";

                    ret = ret + this.headerProject;
                }

                return ret;
            }
        }
        public string TextRev
        {
            get
            {
                string ret = "";

                if (this.headerRev != null)
                {
                    ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadRev_Text + " = ";

                    ret = ret + this.headerRev;
                }

                return ret;
            }
        }
        public string TextComp
        {
            get
            {
                string ret = "";

                if (this.headerComp != null)
                {
                    ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadComp_Text + " = ";

                    ret = ret + this.headerComp;
                }

                return ret;
            }
        }
        public string TextDate
        {
            get
            {
                string ret = "";

                ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadDate_Text + " = ";
                    
                if (this.Date != null)
                {
                    ret = ret + this.Date.Year.ToString() + "/" + this.Date.Month.ToString() + "/" + this.Date.Day.ToString();
                }

                return ret;
            }
        }
        public string TextHW
        {
            get
            {
                string ret = "";

                ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadHW_Text + " = ";

                if (this.IsDefinedDrop) ret = ret + AppSett.Default.ModComm_Head_Drop + " " + this.locDrop.ToString();

                if (this.IsDefinedFullLoc)
                {
                    ret = ret + ", " + AppSett.Default.ModComm_Head_PCI + " " + this.locPCI.ToString();
                    ret = ret + ", " + AppSett.Default.ModComm_Head_Branch + " " + this.locBranch.ToString();
                    ret = ret + ", " + AppSett.Default.ModComm_Head_Slot + " " + this.locSlot.ToString();
                }

                if (this.HWAddHex != null) ret = ret + " - " + AppSett.Default.ModComm_Head_HWAdd + " " + this.HWAddHex;

                if (this.HWAddDec != null) ret = ret + " (" + this.HWAddDec + ")";

                return ret;
            }
        }
        public string TextAuth
        {
            get
            {
                string ret = "";

                ret = AppSett.Default.RLC_Comment_Char + " " + AppSett.Default.ModComm_HeadAuth_Text + " = ";

                if (this.headerAuthor != null)
                {
                    ret = ret + this.headerAuthor;
                }

                if (this.headerEmail != null)
                {
                    ret = ret + " (" + this.headerEmail + ")";
                }

                return ret;
            }
        }
        public List<string> TextHeadFull
        {
            get
            {
                List<string> ret = new List<string>();

                ret.Add(this.TextName);
                ret.Add(this.TextDesc);
                ret.Add(this.TextProject);
                ret.Add(this.TextRev);
                ret.Add(this.TextHW);
                ret.Add(this.TextComp);
                ret.Add(this.TextAuth);
                ret.Add(this.TextDate);

                return ret;
            }
        }

        public string TextParPlatform
        {
            get
            {
                string ret = "";

                if (this.parPlatform != null) ret = AppSett.Default.ModComm_Text_Platform + " = " + this.parPlatform.ToString();

                return ret;
            }
        }
        public string TextParBaud
        {
            get
            {
                string ret = "";

                if (this.parBaud != null) ret = AppSett.Default.ModComm_Text_Baud + " = " + this.parBaud.ToString();

                return ret;
            }
        }
        public string TextParDataBits
        {
            get
            {
                string ret = "";

                if (this.parDataBits != null) ret = AppSett.Default.ModComm_Text_DataBits + " = " + this.parDataBits.ToString();

                return ret;
            }
        }
        public string TextParParity
        {
            get
            {
                string ret = "";

                if (this.parParity!= null) ret = AppSett.Default.ModComm_Text_Parity + " = " + this.parParity.ToString();

                return ret;
            }
        }
        public string TextParStopBits
        {
            get
            {
                string ret = "";

                if (this.parStopBits == CommStopBits.one) ret = AppSett.Default.ModComm_Text_StopBits + " = " + AppSett.Default.RLC_StopBits[0];
                if (this.parStopBits == CommStopBits.oneAndHalf) ret = AppSett.Default.ModComm_Text_StopBits + " = " + AppSett.Default.RLC_StopBits[1];
                if (this.parStopBits == CommStopBits.two) ret = AppSett.Default.ModComm_Text_StopBits + " = " + AppSett.Default.RLC_StopBits[2];

                return ret;
            }
        }
        public string TextParRetries
        {
            get
            {
                string ret = "";

                if (this.parRetries!= null) ret = AppSett.Default.ModComm_Text_Retries + " = " + this.parRetries.ToString();

                return ret;
            }
        }
        public string TextParFlowCtrl
        {
            get
            {
                string ret = "";

                if (this.parFlowCtrl != null) ret = AppSett.Default.ModComm_Text_FlowCtrl + " = " + this.parFlowCtrl.ToString();

                return ret;
            }
        }
        public string TextParDuplex
        {
            get
            {
                string ret = "";

                if (this.parDuplex != null) ret = AppSett.Default.ModComm_Text_Duplex + " = " + this.parDuplex.ToString();

                return ret;
            }
        }
        public string TextParMsgTout
        {
            get
            {
                string ret = "";

                if (this.parMsgTout != null) ret = AppSett.Default.ModComm_Text_MsgTout + " = " + this.parMsgTout.ToString();

                return ret;
            }
        }
        public string TextParInterMsgDel
        {
            get
            {
                string ret = "";

                if (this.parInterMsgDel!= null) ret = AppSett.Default.ModComm_Text_InterMsgDel + " = " + this.parInterMsgDel.ToString();

                return ret;
            }
        }
        public string TextParSysLog
        {
            get
            {
                string ret = "";

                if (this.parSysLog != null) ret = AppSett.Default.ModComm_Text_SysLog+ " = " + this.parSysLog.ToString();

                return ret;
            }
        }
        public string TextParLinkStatReg
        {
            get
            {
                string ret = "";

                if (this.parStatReg != null) ret = AppSett.Default.ModComm_Text_LinkStatReg + " = " + this.parStatReg.ToString();

                return ret;
            }
        }
        public string TextParStatHoldTime
        {
            get
            {
                string ret = "";

                if (this.parStatHoldTime != null) ret = AppSett.Default.ModComm_Text_StatHoldTime + " = " + this.parStatHoldTime.ToString();

                return ret;
            }
        }
        public string TextParWDTime
        {
            get
            {
                string ret = "";

                if (this.parWdTime != null) ret = AppSett.Default.ModComm_Text_WDTime + " = " + this.parWdTime.ToString();

                return ret;
            }
        }
        public string TextParCtrlReg
        {
            get
            {
                string ret = "";

                if (this.parCtrlReg != null) ret = AppSett.Default.ModComm_Text_CtrlReg + " = " + this.parCtrlReg.ToString();

                return ret;
            }
        }
        public string TextParRedReg
        {
            get
            {
                string ret = "";

                if (this.parRedReg != null) ret = AppSett.Default.ModComm_Text_RedReg + " = " + this.parRedReg.ToString();

                return ret;
            }
        }
        public string TextParBackupMode
        {
            get
            {
                string ret = "";

                if (this.parBackupMode != null) ret = AppSett.Default.ModComm_Text_BackupMode + " = " + this.parBackupMode.ToString();

                return ret;
            }
        }
        public string TextParDiagReg
        {
            get
            {
                string ret = "";

                if (this.parDiagReg != null) ret = AppSett.Default.ModComm_Text_DiagReg + " = " + this.parDiagReg.ToString();

                return ret;
            }
        }
        public string TextParList
        {
            get
            {
                string ret = "";

                if (this.parList != null) ret = AppSett.Default.ModComm_Text_List + " = " + this.parList.ToString();

                return ret;
            }
        }
        public List<string> TextParFull
        {
            get
            {
                List<string> ret = new List<string>();

                ret.Add(this.TextParPlatform);
                ret.Add(this.TextParBaud);
                ret.Add(this.TextParDataBits);
                ret.Add(this.TextParParity);
                ret.Add(this.TextParStopBits);
                ret.Add(this.TextParRetries);
                ret.Add(this.TextParFlowCtrl);
                ret.Add(this.TextParDuplex);
                ret.Add(this.TextParMsgTout);
                ret.Add(this.TextParInterMsgDel);
                ret.Add(this.TextParSysLog);
                ret.Add(this.TextParLinkStatReg);
                ret.Add(this.TextParStatHoldTime);
                ret.Add(this.TextParWDTime);
                ret.Add(this.TextParCtrlReg);
                ret.Add(this.TextParRedReg);
                ret.Add(this.TextParBackupMode);
                ret.Add(this.TextParDiagReg);
                ret.Add(this.TextParList);

                return ret;
            }
        }

        public List<string> TextGroupsFull
        {
            get
            {
                List<string> ret = new List<string>();
                List<string> temp = new List<string>();

                if (this.groups != null)
                {
                    foreach (var elem in this.groups)
                    {
                        temp = elem.TextComp;

                        if (temp != null)
                        {
                            ret.AddRange(temp);
                            ret.Add("");
                        }
                    }
                }

                return ret;
            }
        }

        public List<string> TextFull
        {
            get
            {
                List<string> ret = new List<string>();

                if (this.TextHeadFull != null) ret.AddRange(this.TextHeadFull);

                ret.Add("");

                if (this.TextParFull != null) ret.AddRange(this.TextParFull);

                ret.Add("");
                ret.Add("");

                if (this.TextGroupsFull != null) ret.AddRange(this.TextGroupsFull);

                return ret;
            }
        }

        public List<long> UsedModRegs
        {
            get
            {
                List<long> ret = new List<long>();

                if (this.IsValid)
                {
                    ret.Add(this.parCtrlReg);
                    ret.Add(this.parDiagReg);
                    ret.Add(this.parRedReg);
                    ret.Add(this.parStatReg);

                    foreach (var elem in this.groups) ret.AddRange(elem.UsedModRegs);

                    ret = ret.Distinct().ToList<long>();
                }

                return ret;
            }
        }
        public List<ModAdd> UsedModAdds
        {
            get
            {
                List<ModAdd> ret = new List<ModAdd>();
                List<ModAdd> temp;
                List<ModAdd> selExObject;
                List<ModAdd> selNExObject;
                ModAdd add1;
                bool exists = false;

                if (this.IsValid)
                {
                    if (this.groups != null)
                    {
                        foreach (var elem in this.groups)
                        {
                            temp = elem.UsedModAdds;

                            if (temp != null)
                            {
                                selExObject = temp.Intersect(ret).ToList<ModAdd>();
                                selNExObject = temp.Except(ret).ToList<ModAdd>();

                                if (selNExObject != null) ret.AddRange(selNExObject);

                                if (selExObject != null)
                                {
                                    foreach (var elemInner in selExObject)
                                    {
                                        add1 = ret.Find(x => x.Equals(elemInner));

                                        if (add1 != null)
                                        {
                                            add1.UpdateBitMask(elemInner.BitMask, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsValidPath
        {
            get
            {
                bool ret = false;

                ret = this.path != null;

                return ret;
            }
        }
        public bool IsValidName
        {
            get
            {
                bool ret = false;

                ret = this.name != null;

                return ret;
            }
        }
        public bool IsValidHeadName
        {
            get
            {
                bool ret = false;

                ret = this.headerName != null;

                return ret;
            }
        }
        public bool IsValidHeadDesc
        {
            get
            {
                bool ret = false;

                ret = this.headerDesc != null;

                return ret;
            }
        }
        public bool IsValidHeadRev
        {
            get
            {
                bool ret = false;

                ret = this.headerRev != null;

                return ret;
            }
        }
        public bool IsValidHeadAuthor
        {
            get
            {
                bool ret = false;

                ret = this.headerAuthor != null;

                return ret;
            }
        }
        public bool IsValidHeadComp
        {
            get
            {
                bool ret = false;

                ret = this.headerComp != null;

                return ret;
            }
        }
        public bool IsValidHeadEmail
        {
            get
            {
                bool ret = false;

                ret = this.headerEmail != null;

                return ret;
            }
        }
        public bool IsValidHeadProject
        {
            get
            {
                bool ret = false;

                ret = this.headerProject != null;

                return ret;
            }
        }
        public bool IsValidLocDrop
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Drop_Min;
                int max = AppSett.Default.RLC_Drop_Max;

                ret = (this.locDrop >= min) && (this.locDrop <= max);

                return ret;
            }
        }
        public bool IsValidLocPCI
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_PCI_Min;
                int max = AppSett.Default.RLC_PCI_Max;

                ret = (this.locPCI >= min) && (this.locPCI <= max);

                return ret;
            }
        }
        public bool IsValidLocBranch
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Branch_Min;
                int max = AppSett.Default.RLC_Branch_Max;

                ret = (this.locBranch >= min) && (this.locBranch <= max);

                return ret;
            }
        }
        public bool IsValidLocSlot
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Slot_Min;
                int max = AppSett.Default.RLC_Slot_Max;

                ret = (this.locSlot >= min) && (this.locSlot <= max);

                return ret;
            }
        }
        public bool IsValidParBaud
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Baud_Min;
                int max = AppSett.Default.RLC_Baud_Max;

                ret = (this.parBaud >= min) && (this.parBaud <= max);

                return ret;
            }
        }
        public bool IsValidParDataBits
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_DataBits_Min;
                int max = AppSett.Default.RLC_DataBits_Max;

                ret = (this.parDataBits >= min) && (this.parDataBits <= max);

                return ret;
            }
        }
        public bool IsValidParStopBits
        {
            get
            {
                bool ret = false;

                ret = this.parStopBits != null;

                return ret;
            }
        }
        public bool IsValidParRetries
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Retries_Min;
                int max = AppSett.Default.RLC_Retries_Max;

                ret = (this.parRetries >= min) && (this.parRetries <= max);

                return ret;
            }
        }
        public bool IsValidParMsgTout
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_MsgTout_Min;
                int max = AppSett.Default.RLC_MsgTout_Max;

                ret = (this.parMsgTout >= min) && (this.parMsgTout <= max);

                return ret;
            }
        }
        public bool IsValidParInterMsgDel
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_InterMsgDel_Min;
                int max = AppSett.Default.RLC_InterMsgDel_Max;

                ret = (this.parInterMsgDel >= min) && (this.parInterMsgDel <= max);

                return ret;
            }
        }
        public bool IsValidParSysLog
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_SysLog_Min;
                int max = AppSett.Default.RLC_SysLog_Max;

                ret = (this.parSysLog >= min) && (this.parSysLog <= max);

                return ret;
            }
        }
        public bool IsValidStatReg
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_LinkStatReg_Min;
                int max = AppSett.Default.RLC_LinkStatReg_Max;

                ret = (this.parStatReg >= min) && (this.parStatReg <= max);

                return ret;
            }
        }
        public bool IsValidParStatHoldTime
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_StatHoldTime_Min;
                long max = AppSett.Default.RLC_StatHoldTime_Max;

                ret = (this.parStatHoldTime >= min) && (this.parStatHoldTime <= max);

                return ret;
            }
        }
        public bool IsValidParWDTime
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_WDTime_Min;
                int max = AppSett.Default.RLC_WDTime_Max;

                ret = (this.parWdTime >= min) && (this.parWdTime <= max);

                return ret;
            }
        }
        public bool IsValidParCtrlReg
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_CtrlReg_Min;
                int max = AppSett.Default.RLC_CtrlReg_Max;

                ret = (this.parCtrlReg >= min) && (this.parCtrlReg <= max);

                return ret;
            }
        }
        public bool IsValidParRedReg
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_ExOnceReg_Min;
                int max = AppSett.Default.RLC_ExOnceReg_Max;

                ret = (this.parRedReg >= min) && (this.parRedReg <= max);

                return ret;
            }
        }
        public bool IsValidParDiagReg
        {
            get
            {
                bool ret = false;
                int min = AppSett.Default.RLC_Diag_Min;
                int max = AppSett.Default.RLC_Diag_Max;

                ret = (this.parDiagReg >= min) && (this.parDiagReg <= max);

                return ret;
            }
        }
        public bool IsValidGroups
        {
            get
            {
                bool ret = false;

                ret = this.groups != null;

                return ret;
            }
        }

        public bool IsValid
        {
            get
            {
                bool ret = false;

                ret = this.IsValidGroups && this.IsValidHeadAuthor && this.IsValidHeadComp && this.IsValidHeadDesc && this.IsValidHeadEmail;
                ret = ret && this.IsValidHeadName && this.IsValidHeadProject && this.IsValidHeadRev && this.IsValidLocBranch && this.IsValidLocDrop;
                ret = ret && this.IsValidLocPCI && this.IsValidLocSlot && this.IsValidName && this.IsValidParBaud && this.IsValidParCtrlReg;
                ret = ret && this.IsValidParDataBits && this.IsValidParDiagReg && this.IsValidParInterMsgDel && this.IsValidParMsgTout;
                ret = ret && this.IsValidParRedReg && this.IsValidParRetries && this.IsValidParStatHoldTime && this.IsValidParStopBits;
                ret = ret && this.IsValidParSysLog && this.IsValidParWDTime && this.IsValidPath && this.IsValidStatReg;

                return ret;
            }
        }

        public bool IsDefinedDrop
        {
            get { return this.locDrop > AppSett.Default.RLC_Drop_Min; }
        }
        public bool IsDefinedPCI
        {
            get { return this.locPCI > AppSett.Default.RLC_PCI_Min; }
        }
        public bool IsDefinedBranch
        {
            get { return this.locBranch > AppSett.Default.RLC_Branch_Min; }
        }
        public bool IsDefinedSlot
        {
            get { return this.locSlot > AppSett.Default.RLC_Slot_Min; }
        }
        public bool IsDefinedFullLoc
        {
            get { return (this.LocFull != null); }
        }

        #endregion


        #region Methods: Constructors

        public ModComm(string Path)
        {
            if (Path == null) throw new Exception("Invalid input data!");

            this.path = Path;
            this.name = System.IO.Path.GetFileNameWithoutExtension(this.path);

            this.headerName = "";
            this.headerDesc = "";
            this.headerRev = "";
            this.headerAuthor = "";
            this.headerComp = "";
            this.headerEmail = "";
            this.headerProject = "";

            this.locDrop = AppSett.Default.ModComm_Def_LocDrop;
            this.locPCI = AppSett.Default.ModComm_Def_LocPCI;
            this.locBranch = AppSett.Default.ModComm_Def_LocBranch;
            this.locSlot = AppSett.Default.ModComm_Def_LocSlot;

            this.parPlatform = CommPlatform.RLC;
            this.parBaud = AppSett.Default.ModComm_Def_Baud;
            this.parDataBits = AppSett.Default.ModComm_Def_DataBits;
            this.parParity = CommParity.Even;
            this.parStopBits = CommStopBits.one;
            this.parRetries = AppSett.Default.ModComm_Def_Retries;
            this.parFlowCtrl = CommFlowCtrl.rts_on_tx;
            this.parDuplex = CommDuplex.half;
            this.parMsgTout = AppSett.Default.ModComm_Def_MsgTout;
            this.parInterMsgDel = AppSett.Default.ModComm_Def_InterMsgDel;
            this.parSysLog = AppSett.Default.ModComm_Def_SysLog;
            this.parStatReg = AppSett.Default.ModComm_Def_StatReg;
            this.parStatHoldTime = AppSett.Default.ModComm_Def_StatHoldTime;
            this.parWdTime = AppSett.Default.ModComm_Def_WDTime;
            this.parCtrlReg = AppSett.Default.ModComm_Def_CtrlReg;
            this.parRedReg = AppSett.Default.ModComm_Def_RedReg;
            this.parBackupMode = CommBackupMode.never;
            this.parDiagReg = AppSett.Default.ModComm_Def_DiagReg;
            this.parList = CommList.off;

            this.groups = new List<ModGroup>();
        }

        public ModComm(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempString;
                string tempPlatform = null;
                string tempParity = null;
                string tempFlowCtrl = null;
                string tempDuplex = null;
                string tempBackupMode = null;
                string tempList = null;
                string tempStopBits = null;

                this.locDrop = reader.ReadInt32();

                this.locPCI = reader.ReadInt32();

                this.locBranch = reader.ReadInt32();

                this.locSlot = reader.ReadInt32();

                this.parBaud = reader.ReadInt32();

                this.parDataBits = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) tempStopBits = reader.ReadString();

                this.parRetries = reader.ReadInt32();

                this.parMsgTout = reader.ReadInt32();

                this.parInterMsgDel = reader.ReadInt32();

                this.parSysLog = reader.ReadInt32();

                this.parStatReg = reader.ReadInt32();

                this.parStatHoldTime = reader.ReadInt64();

                this.parWdTime = reader.ReadInt32();

                this.parCtrlReg = reader.ReadInt32();

                this.parRedReg = reader.ReadInt32();

                this.parDiagReg = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) this.path = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.name = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerName = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerDesc = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerRev = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerAuthor = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerComp = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerEmail = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.headerProject = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempPlatform = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempParity = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempFlowCtrl = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempDuplex = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempBackupMode = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempList = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.groups = new List<ModGroup>();

                    objNum = reader.ReadInt32();

                    for (i = 0; i < objNum; i++)
                    {
                        this.groups.Add(new ModGroup(ref reader));
                    }
                }

                if (tempPlatform != null)
                {
                    if (String.Compare(tempPlatform, CommPlatform.PC.ToString()) == 0) this.parPlatform = CommPlatform.PC;
                    if (String.Compare(tempPlatform, CommPlatform.QLC.ToString()) == 0) this.parPlatform = CommPlatform.QLC;
                    if (String.Compare(tempPlatform, CommPlatform.RLC.ToString()) == 0) this.parPlatform = CommPlatform.RLC;
                }
                else this.parPlatform = CommPlatform.RLC;

                if (tempParity != null)
                {
                    if (String.Compare(tempParity, CommParity.Even.ToString()) == 0) this.parParity = CommParity.Even;
                    if (String.Compare(tempParity, CommParity.None.ToString()) == 0) this.parParity = CommParity.None;
                    if (String.Compare(tempParity, CommParity.Odd.ToString()) == 0) this.parParity = CommParity.Odd;
                }
                else this.parParity = CommParity.Even;

                if (tempFlowCtrl != null)
                {
                    if (String.Compare(tempFlowCtrl, CommFlowCtrl.rts_on_tx.ToString()) == 0) this.parFlowCtrl = CommFlowCtrl.rts_on_tx;
                    if (String.Compare(tempFlowCtrl, CommFlowCtrl.off.ToString()) == 0) this.parFlowCtrl = CommFlowCtrl.off;
                    if (String.Compare(tempFlowCtrl, CommFlowCtrl.on.ToString()) == 0) this.parFlowCtrl = CommFlowCtrl.on;
                }
                else this.parFlowCtrl = CommFlowCtrl.rts_on_tx;

                if (tempDuplex != null)
                {
                    if (String.Compare(tempDuplex, CommDuplex.half.ToString()) == 0) this.parDuplex = CommDuplex.half;
                    if (String.Compare(tempDuplex, CommDuplex.full.ToString()) == 0) this.parDuplex = CommDuplex.full;
                }
                else this.parDuplex = CommDuplex.half;

                if (tempBackupMode != null)
                {
                    if (String.Compare(tempBackupMode, CommBackupMode.never.ToString()) == 0) this.parBackupMode = CommBackupMode.never;
                    if (String.Compare(tempBackupMode, CommBackupMode.mute.ToString()) == 0) this.parBackupMode = CommBackupMode.mute;
                    if (String.Compare(tempBackupMode, CommBackupMode.read_only.ToString()) == 0) this.parBackupMode = CommBackupMode.read_only;
                }
                else this.parBackupMode = CommBackupMode.never;

                if (tempList != null)
                {
                    if (String.Compare(tempList, CommList.off.ToString()) == 0) this.parList = CommList.off;
                    if (String.Compare(tempList, CommList.on.ToString()) == 0) this.parList = CommList.on;
                }
                else this.parList = CommList.off;

                if (tempStopBits != null)
                {
                    if (String.Compare(tempStopBits, CommStopBits.one.ToString()) == 0) this.parStopBits = CommStopBits.one;
                    if (String.Compare(tempStopBits, CommStopBits.oneAndHalf.ToString()) == 0) this.parStopBits = CommStopBits.oneAndHalf;
                    if (String.Compare(tempStopBits, CommStopBits.two.ToString()) == 0) this.parStopBits = CommStopBits.two;
                }
                else this.parStopBits = CommStopBits.one;
            }
        }

        #endregion


        #region Methods: General

        /// <summary>
        /// Converts the object to array of bytes
        /// </summary>
        /// <param name="writer">Binary stream to write to</param>
        public bool SerializeToStream(ref BinaryWriter writer)
        {
            bool ret = false;

            try
            {
                bool tempBytes;

                // Location Drop
                writer.Write(this.locDrop);

                // Location PCI
                writer.Write(this.locPCI);

                // Location Branch
                writer.Write(this.locBranch);

                // Location Slot
                writer.Write(this.locSlot);

                // Location Baud
                writer.Write(this.parBaud);

                // Location Data Bits
                writer.Write(this.parDataBits);

                // Location Stop Bits
                writer.Write(this.parStopBits.ToString());

                // Location Retries
                writer.Write(this.parRetries);

                // Location Msg Timeout
                writer.Write(this.parMsgTout);

                // Location Inter Msg Delay
                writer.Write(this.parInterMsgDel);

                // Location SysLog
                writer.Write(this.parSysLog);

                // Location Stat Reg
                writer.Write(this.parStatReg);

                // Location Stat Hold Time
                writer.Write(this.parStatHoldTime);

                // Location WD Time
                writer.Write(this.parWdTime);

                // Location Ctrl Reg
                writer.Write(this.parCtrlReg);

                // Location Red Reg
                writer.Write(this.parRedReg);

                // Location Diag Reg
                writer.Write(this.parDiagReg);

                // Path
                if (this.path != null)
                {
                    writer.Write(true);
                    writer.Write(this.path);
                }
                else writer.Write(false);

                // Name
                if (this.name != null)
                {
                    writer.Write(true);
                    writer.Write(this.name);
                }
                else writer.Write(false);

                // Header Name
                if (this.headerName != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerName);
                }
                else writer.Write(false);

                // Header Desc
                if (this.headerDesc != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerDesc);
                }
                else writer.Write(false);

                // Header Rev
                if (this.headerRev != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerRev);
                }
                else writer.Write(false);

                // Header Author
                if (this.headerAuthor != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerAuthor);
                }
                else writer.Write(false);

                // Header Comp
                if (this.headerComp != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerComp);
                }
                else writer.Write(false);

                // Header Email
                if (this.headerEmail != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerEmail);
                }
                else writer.Write(false);

                // Header Project
                if (this.headerProject != null)
                {
                    writer.Write(true);
                    writer.Write(this.headerProject);
                }
                else writer.Write(false);

                // Platform
                if (this.parPlatform != null)
                {
                    writer.Write(true);
                    writer.Write(this.parPlatform.ToString());
                }
                else writer.Write(false);

                // Parity
                if (this.parParity != null)
                {
                    writer.Write(true);
                    writer.Write(this.parParity.ToString());
                }
                else writer.Write(false);

                // Flow Ctrl
                if (this.parFlowCtrl!= null)
                {
                    writer.Write(true);
                    writer.Write(this.parFlowCtrl.ToString());
                }
                else writer.Write(false);

                // Duplex
                if (this.parDuplex != null)
                {
                    writer.Write(true);
                    writer.Write(this.parDuplex.ToString());
                }
                else writer.Write(false);

                // Backup Mode
                if (this.parBackupMode!= null)
                {
                    writer.Write(true);
                    writer.Write(this.parBackupMode.ToString());
                }
                else writer.Write(false);

                // List
                if (this.parList != null)
                {
                    writer.Write(true);
                    writer.Write(this.parList.ToString());
                }
                else writer.Write(false);

                // Groups
                if (this.groups != null)
                {
                    writer.Write(true);

                    writer.Write(this.groups.Count);

                    foreach (var elem in this.groups)
                    {
                        tempBytes = elem.SerializeToStream(ref writer);

                        if (!tempBytes) throw new Exception("Serialization error!");
                    }
                }
                else writer.Write(false);

                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Makes specified indent
        /// </summary>
        /// <param name="Ind">A specified indent</param>
        public string MakeIndent(int Ind)
        {
            string ret = "";
            int i;

            if (Ind > 0)
            {
                for (i = 0; i < Ind; i++) ret = ret + " ";
            }

            return ret;
        }

        /// <summary>
        /// Validates specified modbus group
        /// </summary>
        /// <param name="Group">A specified group</param>
        public bool ValidateGroup(ModGroup Group)
        {
            bool ret = false;
            bool validMaxNumber = false;
            bool validRegs = false;
            List<long> groupRegs;
            List<long> allRegs;
            List<long> intersectRegs;

            try
            {
                if (Group != null)
                {
                    if (this.groups.Count < AppSett.Default.RLC_GroupNum_Max) validMaxNumber = true;
                    else validMaxNumber = false;

                    allRegs = this.UsedModRegs;
                    groupRegs = Group.UsedModRegs;

                    if ((allRegs != null) && (groupRegs != null))
                    {
                        intersectRegs = allRegs.Intersect(groupRegs).ToList<long>();

                        if (intersectRegs != null)
                        {
                            if (intersectRegs.Count == 0) validRegs = true;
                            else validRegs = false;
                        }
                    }

                    ret = validMaxNumber && validRegs;
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Adds specified modbus group
        /// </summary>
        /// <param name="Group">A specified group</param>
        public bool Add(ModGroup Group)
        {
            bool ret = false;

            try
            {
                if (Group != null)
                {
                    ret = ValidateGroup(Group);

                    if (ret)
                    {
                        ret = !(this.groups.Contains(Group));

                        if (ret)
                        {
                            this.groups.Add(Group);
                        }
                    }
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Updates comm location
        /// </summary>
        /// <param name="Location">A specified location</param>
        public void UpdateLoc(string Location)
        {
            try
            {
                if (Location != null)
                {
                    Regex reg = new Regex(C_Reg_Patt1);
                    Match regMatch = reg.Match(Location);

                    if (regMatch.Success)
                    {
                        this.locPCI = Convert.ToInt32(regMatch.Groups[1].ToString());
                        this.locBranch = Convert.ToInt32(regMatch.Groups[2].ToString());
                        this.locSlot = Convert.ToInt32(regMatch.Groups[3].ToString());
                    }
                }
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Updates groups indexes
        /// </summary>
        public void UpdateGroupsInd()
        {
            try
            {
                if (this.groups != null)
                {
                    int i;
                    this.groups.Sort();

                    for (i = 0; i < this.groups.Count; i++) this.groups[i].Index = i;
                }
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// Removes specified modbus group
        /// </summary>
        /// <param name="Group">A specified group</param>
        public bool Remove(ModGroup Group)
        {
            bool ret = false;

            try
            {
                if (Group != null)
                {
                    ret = this.groups.Remove(Group);
                    UpdateGroupsInd();
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        #endregion

        #region Methods: Overrides and Implemented Interfaces

        /// <summary>
        /// Returns whether specified object equals this object or not
        /// </summary>
        /// <param name="obj">Object for equality checking</param>
        public override bool Equals(object obj)
        {
            bool pathMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModComm p = obj as ModComm;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.path != null) && (p.path != null)) pathMatch = String.Compare(this.path,p.path,true)==0;

            // Return true if the fields match:
            return pathMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int pathHash = 0;

            if (this.path != null) pathHash = this.path.GetHashCode();

            return (pathHash);
        }

        /// <summary>
        /// Convert the parameter to specified string format
        /// </summary>
        public override string ToString()
        {
            string ret = "";

            if (this.TextShort != null) ret = this.TextShort;

            return ret;
        }

        #endregion

    }
}
