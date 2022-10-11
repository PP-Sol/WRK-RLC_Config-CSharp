using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.IO;
using System.Text.RegularExpressions;

namespace RLC_Configurator
{
    public class ModGroup:IComparable
    {
        
        #region Objects: Enumerators

        public enum GroupOperation
        {
            PERIODIC,
            TRIGGERED,
            GATED,
        }

        public enum GroupFunction
        {
            RCS,
            RIS,
            RHR,
            RIR,
            FSC,
            PSR,
            RES,
            FMC,
            PMR,
        }

        public enum GroupStorage
        {
            Coils,
            Inputs,
            InputRegs,
            HoldingRegs,
        }

        public enum GroupSigDir
        {
            Input,
            Output,
        }

        #endregion

        #region Objects: Mandatory Items

        private int index;
        private string name;
        private string description;
        private GroupOperation operation;
        private long interval;
        private int triggerreg;
        private int gatereg;
        private int slave;
        private GroupFunction function;

        private List<ModPoint> signals;

        #endregion


        #region Public Properties: General

        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
        public GroupOperation Operation
        {
            get { return this.operation; }
            set { this.operation = value; }
        }
        public long Interval
        {
            get { return this.interval; }
            set { this.interval = value; }
        }
        public int Triggerreg
        {
            get { return this.triggerreg; }
            set { this.triggerreg = value; }
        }
        public int Gatereg
        {
            get { return this.gatereg; }
            set { this.gatereg = value; }
        }
        public int Slave
        {
            get { return this.slave; }
            set { this.slave = value; }
        }
        public GroupFunction Function
        {
            get { return this.function; }
            set { this.function = value; }
        }

        public List<ModPoint> Signals
        {
            get { return this.signals; }
        }

        public string TextShort
        {
            get
            {
                string ret = "";

                ret = ret + "Index=" + this.index.ToString() + " ";
                ret = ret + "Name=" + AppSett.Default.ModGroup_Apos + this.name + AppSett.Default.ModGroup_Apos + " ";
                ret = ret + "Slave=" + this.slave.ToString() + " ";
                ret = ret + "Function=" + this.function.ToString() + " ";
                ret = ret + "Signals=" + this.signals.Count.ToString();

                return ret;
            }
        }
        public string TextGroup
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = ret + AppSett.Default.ModGroup_Group_Text + " = " + AppSett.Default.ModGroup_Apos + this.name + AppSett.Default.ModGroup_Apos; 
                }

                return ret;
            }
        }
        public string TextOper
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = ret + AppSett.Default.ModGroup_Oper_Text + " " + this.operation.ToString().ToLower();
                }

                return ret;
            }
        }
        public string TextInt
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    if (this.operation == GroupOperation.GATED) ret = ret + AppSett.Default.ModGroup_Gate_Text + " " + this.gatereg.ToString();
                    if (this.operation == GroupOperation.PERIODIC) ret = ret + AppSett.Default.ModGroup_Int_Text + " " + this.interval.ToString();
                    if (this.operation == GroupOperation.TRIGGERED) ret = ret + AppSett.Default.ModGroup_Trigg_Text + " " + this.triggerreg.ToString();
                }

                return ret;
            }
        }
        public string TextSlave
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = ret + AppSett.Default.ModGroup_Slave_Text + " " + this.slave.ToString().ToLower();
                }

                return ret;
            }
        }
        public string TextFunct
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = ret + AppSett.Default.ModGroup_Funct_Text + " " + this.function.ToString();
                }

                return ret;
            }
        }
        public List<string> TextComp
        {
            get
            {
                List<string> ret = new List<string>();

                if (this.IsValid)
                {
                    if(this.description != null ) ret.Add(AppSett.Default.RLC_Comment_Char + " " + this.description);

                    ret.Add(this.TextGroup);
                    ret.Add(this.TextOper);
                    ret.Add(this.TextInt);
                    ret.Add(this.TextSlave);
                    ret.Add(this.TextFunct);

                    ret.Add("");

                    foreach (var elem in this.signals)
                    {
                        ret.Add(elem.TextComp);
                    }
                }

                return ret;
            }
        }

        public GroupStorage ModStorage
        {
            get
            {
                GroupStorage ret = GroupStorage.InputRegs;

                if ((this.function == GroupFunction.RCS) || (this.function == GroupFunction.FSC) || (this.function == GroupFunction.FMC))
                {
                    ret = GroupStorage.Coils;
                }
                if (this.function == GroupFunction.RIS)
                {
                    ret = GroupStorage.Inputs;
                }
                if (this.function == GroupFunction.RIR)
                {
                    ret = GroupStorage.InputRegs;
                }
                if ((this.function == GroupFunction.RHR) || (this.function == GroupFunction.PSR) || (this.function == GroupFunction.PMR))
                {
                    ret = GroupStorage.HoldingRegs;
                }

                return ret;
            }
        }
        public GroupSigDir SigDirection
        {
            get
            {
                GroupSigDir ret = GroupSigDir.Input;

                if ((this.function == GroupFunction.FSC) || (this.function == GroupFunction.PSR) || (this.function == GroupFunction.FMC) || (this.function == GroupFunction.PMR))
                {
                    ret = GroupSigDir.Output;
                }
                else ret = GroupSigDir.Input;

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
                    if (this.operation == GroupOperation.TRIGGERED) ret.Add(this.triggerreg);
                    if (this.operation == GroupOperation.GATED) ret.Add(this.gatereg);

                    foreach (var elem in this.signals) ret.AddRange(elem.UsedModRegs);

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
                ModAdd add1;
                ModAdd add2;
                bool exists = false;

                if (this.IsValid)
                {
                    if (this.signals != null)
                    {
                        foreach (var elem in this.signals)
                        {
                            if (elem.Is16bits)
                            {
                                if (elem.UsedModAdds.Count >= 1)
                                {
                                    add1 = elem.UsedModAdds[0];

                                    if (add1 != null)
                                    {
                                        exists = ret.Contains(add1);

                                        if (exists)
                                        {
                                            add2 = ret.Find(x => x.Equals(add1));

                                            if (add2 != null) add2.UpdateBitMask(add1.BitMask, true);
                                        }
                                        else ret.Add(add1);
                                    }
                                }
                            }
                            else
                            {
                                if (elem.UsedModAdds.Count >= 2)
                                {
                                    add1 = elem.UsedModAdds[0];
                                    add2 = elem.UsedModAdds[1];
                                    
                                    if ((add1 != null) && (add2 != null))
                                    {
                                        exists = ret.Contains(add1);
                                        if (!exists) ret.Add(add1);

                                        exists = ret.Contains(add2);
                                        if (!exists) ret.Add(add2);
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

        public bool IsValidIndex
        {
            get
            {
                bool ret = false;

                ret = this.index >= 0;

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
        public bool IsValidInterval
        {
            get
            {
                bool ret = false;
                int min = 0;
                int max = 3600000;

                ret = (this.interval >= min) && (this.interval <= max);

                return ret;
            }
        }
        public bool IsValidTriggerreq
        {
            get
            {
                bool ret = false;
                int min = 0;
                int max = 2047;

                ret = (this.triggerreg >= min) && (this.triggerreg <= max);

                return ret;
            }
        }
        public bool IsValidGatereg
        {
            get
            {
                bool ret = false;
                int min = 0;
                int max = 2047;

                ret = (this.gatereg >= min) && (this.gatereg <= max);

                return ret;
            }
        }
        public bool IsValidSlave
        {
            get
            {
                bool ret = false;
                int min = 1;
                int max = 255;

                ret = (this.slave >= min) && (this.slave <= max);

                return ret;
            }
        }
        public bool IsValidSigList
        {
            get
            {
                bool ret = false;

                ret = this.signals != null;

                return ret;
            }
        }
        public bool IsValid
        {
            get
            {
                bool ret = false;

                ret = this.IsValidGatereg && this.IsValidIndex && this.IsValidInterval && this.IsValidName && this.IsValidSlave && this.IsValidTriggerreq && this.IsValidSigList;

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors

        public ModGroup(int Index, string Name, string Description, GroupOperation Operation, long Interval, int Triggerreg, int Gatereg, int Slave, GroupFunction Function)
        {
            this.index = Index;
            this.name = Name;
            this.description = Description;
            this.operation = Operation;
            this.interval = Interval;
            this.triggerreg = Triggerreg;
            this.gatereg = Gatereg;
            this.slave = Slave;
            this.function = Function;

            this.signals = new List<ModPoint>();

            if (!this.IsValid) throw new Exception("Invalid input data!");
        }

        public ModGroup(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempString;
                string tempOper = null;
                string tempFunct = null;

                this.index = reader.ReadInt32();

                this.interval = reader.ReadInt64();

                this.triggerreg = reader.ReadInt32();

                this.gatereg= reader.ReadInt32();

                this.slave = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) this.name = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.description = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempOper = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempFunct = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.signals = new List<ModPoint>();

                    objNum = reader.ReadInt32();

                    for (i = 0; i < objNum; i++)
                    {
                        this.signals.Add(new ModPoint(ref reader));
                    }
                }

                if (tempOper != null)
                {
                    if (String.Compare(tempOper, GroupOperation.GATED.ToString()) == 0) this.operation = GroupOperation.GATED;
                    if (String.Compare(tempOper, GroupOperation.PERIODIC.ToString()) == 0) this.operation = GroupOperation.PERIODIC;
                    if (String.Compare(tempOper, GroupOperation.TRIGGERED.ToString()) == 0) this.operation = GroupOperation.TRIGGERED;
                }
                else this.operation = GroupOperation.PERIODIC;

                if (tempFunct != null)
                {
                    if (String.Compare(tempFunct, GroupFunction.FMC.ToString()) == 0) this.function = GroupFunction.FMC;
                    if (String.Compare(tempFunct, GroupFunction.FSC.ToString()) == 0) this.function = GroupFunction.FSC;
                    if (String.Compare(tempFunct, GroupFunction.PMR.ToString()) == 0) this.function = GroupFunction.PMR;
                    if (String.Compare(tempFunct, GroupFunction.PSR.ToString()) == 0) this.function = GroupFunction.PSR;
                    if (String.Compare(tempFunct, GroupFunction.RCS.ToString()) == 0) this.function = GroupFunction.RCS;
                    if (String.Compare(tempFunct, GroupFunction.RES.ToString()) == 0) this.function = GroupFunction.RES;
                    if (String.Compare(tempFunct, GroupFunction.RHR.ToString()) == 0) this.function = GroupFunction.RHR;
                    if (String.Compare(tempFunct, GroupFunction.RIR.ToString()) == 0) this.function = GroupFunction.RIR;
                    if (String.Compare(tempFunct, GroupFunction.RIS.ToString()) == 0) this.function = GroupFunction.RIS;
                }
                else this.function = GroupFunction.RHR;
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

                // Index
                writer.Write(this.index);

                // Interval
                writer.Write(this.interval);

                // Trigger Reg
                writer.Write(this.triggerreg);

                // Gate Reg
                writer.Write(this.gatereg);

                // Slave
                writer.Write(this.slave);

                // Name
                if (this.name != null)
                {
                    writer.Write(true);
                    writer.Write(this.name.ToString());
                }
                else writer.Write(false);

                // Description
                if (this.description != null)
                {
                    writer.Write(true);
                    writer.Write(this.description.ToString());
                }
                else writer.Write(false);

                // Operation
                if (this.operation != null)
                {
                    writer.Write(true);
                    writer.Write(this.operation.ToString());
                }
                else writer.Write(false);

                // Function
                if (this.function != null)
                {
                    writer.Write(true);
                    writer.Write(this.function.ToString());
                }
                else writer.Write(false);


                // Signals
                if (this.signals != null)
                {
                    writer.Write(true);

                    writer.Write(this.signals.Count);

                    foreach (var elem in this.signals)
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
        /// Validates specified modbus signal
        /// </summary>
        /// <param name="Signal">A specified signal</param>
        public bool ValidateSignal(ModPoint Signal)
        {
            bool ret = false;
            bool sigDir = false;
            bool noRegConflict = false;
            bool modAddValid = false;
            List<long> groupRegs;
            List<long> sigRegs;
            List<long> intersectRegs;

            try
            {
                if (Signal != null)
                {
                    #region Signal Direction

                    if (this.SigDirection == GroupSigDir.Input)
                    {
                        if ((Signal.SigType == ModPoint.SigTypes.AI) || (Signal.SigType == ModPoint.SigTypes.DI) || (Signal.SigType == ModPoint.SigTypes.PI))
                        {
                            sigDir = true;
                        }
                        else sigDir = false;
                    }
                    else
                    {
                        if ((Signal.SigType == ModPoint.SigTypes.AO) || (Signal.SigType == ModPoint.SigTypes.DO) || (Signal.SigType == ModPoint.SigTypes.PO))
                        {
                            sigDir = true;
                        }
                        else sigDir = false;
                    } 
                    
                    #endregion

                    #region Registers Conflict

                    groupRegs = this.UsedModRegs;
                    sigRegs = Signal.UsedModRegs;

                    if ( (groupRegs != null) && (sigRegs != null) )
                    {
                        intersectRegs = groupRegs.Intersect(sigRegs).ToList<long>();

                        if (intersectRegs != null)
                        {
                            if (intersectRegs.Count == 0) noRegConflict = true;
                            else noRegConflict = false;
                        }
                    }

                    #endregion

                    #region Modbus Address Validation

                    if (Signal.Address.IsBinaryType)
                    {
                        if (Signal.Register.Type == ModReg.ModRegType.DTYPE) modAddValid = true;
                        else modAddValid = false;
                    }
                    else modAddValid = true;

                    #endregion

                    ret = sigDir && noRegConflict && modAddValid; 
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Adds specified modbus signal
        /// </summary>
        /// <param name="Signal">A specified signal</param>
        public bool Add(ModPoint Signal)
        {
            bool ret = false;

            try
            {
                if (Signal != null)
                {
                    ret = ValidateSignal(Signal);

                    if (ret)
                    {
                        ret = !(this.signals.Contains(Signal));

                        if (ret)
                        {
                            Signal.GroupIndex = this.index;
                            this.signals.Add(Signal);
                        }
                    }
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
            bool indexMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModGroup p = obj as ModGroup;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.index != null) && (p.index != null)) indexMatch = this.index == p.index;

            // Return true if the fields match:
            return indexMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int indexHash = 0;

            if (this.index != null) indexHash = this.index.GetHashCode();

            return (indexHash);
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

        /// <summary>
        /// Returns comparison of object and instance
        /// </summary>
        /// <param name="obj">obj</param>
        public int CompareTo(System.Object obj)
        {
            int ret = -1;

            // If parameter is null return -1.
            if (obj == null) return ret;

            // If parameter cannot be cast to Point return -1.
            ModGroup p = obj as ModGroup;
            if ((System.Object)p == null) return ret;

            if (p.index > this.index) ret = -1;
            else
            {
                if (p.index < this.index) ret = 1;
                else ret = 0;
            }

            return ret;
        }

        #endregion


    }
}
