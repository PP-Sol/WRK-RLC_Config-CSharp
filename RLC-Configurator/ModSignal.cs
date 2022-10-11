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
    public class ModSignal:IComparable
    {
        
        #region Objects: Enumerators
        
        public enum SigRegTypes
        {
            DTYPE,
            ITYPE,
            FTYPE,
            STYPE,
        }

        public enum SigDataTypes
        {
            INT16,
            UINT16,
            INT32,
            INT32REV,
            INT32_BS,
            INT32_WS_BS,
            UINT32,
            UINT32REV,
            UINT32_BS,
            UINT32_WS_BS,
            BCD4,
            BCD8,
            FLOAT,
            FLOARREV,
            FLOAT_BS,
            FLOAT_WS_BS,
        }

        public enum SigTypes
        {
            AI,
            AO,
            DI,
            DO,
            PI,
            PO,
        } 
        
        #endregion
           
        #region Objects: Mandatory Items

        private SigRegTypes regType;
        private SigDataTypes dataType;
        private ModGroup.GroupFunction function;
        private long regNumber;
        private int groupIndex;
        private ModAdd modAddress;
        
        #endregion

        #region Objects: Optional Items

        private string tagName;
        private string description;
        private int convType;
        private float conv0;
        private float conv2;
        private float conv3;
        private float conv4;
        private float conv5;
        private float conv6;

        #endregion


        #region Public Properties: General

        public SigRegTypes RegType
        {
            get
            {
                return this.regType;    
            }
            set { this.regType = value; }
        }
        public SigDataTypes DataType
        {
            get
            {
                return this.dataType;
            }
        }
        public SigTypes SigType
        {
            get
            {
                return SigTypes.AI;
            }
        }
        public long RegNumber
        {
            get
            {
                return this.regNumber;
            }
        }
        public int GroupIndex
        {
            get
            {
                return this.groupIndex;
            }
            set
            {
                this.groupIndex = value;
            }
        }
        public ModAdd ModAddress
        {
            get
            {
                return this.modAddress;
            }
        }
        public ModGroup.GroupFunction Function
        {
            get { return this.function; }
            set { this.function = value; }
        }

        public string RegNumberString
        {
            get
            {
                string ret = "";
                int i;

                long diff = AppSett.Default.ModSig_Reg_Length - this.regNumber.ToString().Length;

                if (diff > 0)
                {
                    ret = this.regNumber.ToString();

                    for (i = 0; i < diff; i++)
                    {
                        ret = "0" + ret;
                    }
                }
                else ret = this.regNumber.ToString();

                return ret;
            }
        }
        public string ModAddressString
        {
            get
            {
                string ret = "";

                ret = ret + this.modAddress.TextComb;

                return ret;
            }
        }

        public string TagName
        {
            get
            {
                return this.tagName;
            }
            set
            {
                this.tagName = value;
            }
        }
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }
        public int ConvType
        {
            get
            {
                return this.convType;
            }
            set
            {
                this.convType = value;
            }
        }
        public float Conv0
        {
            get
            {
                return this.conv0;
            }
            set
            {
                this.conv0 = value;
            }
        }
        public float Conv2
        {
            get
            {
                return this.conv2;
            }
            set
            {
                this.conv2 = value;
            }
        }
        public float Conv3
        {
            get
            {
                return this.conv3;
            }
            set
            {
                this.conv3 = value;
            }
        }
        public float Conv4
        {
            get
            {
                return this.conv4;
            }
            set
            {
                this.conv4 = value;
            }
        }
        public float Conv5
        {
            get
            {
                return this.conv5;
            }
            set
            {
                this.conv5 = value;
            }
        }
        public float Conv6
        {
            get
            {
                return this.conv6;
            }
            set
            {
                this.conv6 = value;
            }
        }

        public string RegTypeChar
        {
            get
            {
                string ret = "";

                if (this.regType == SigRegTypes.DTYPE) ret = AppSett.Default.ModSig_Char_DTYPE;
                if (this.regType == SigRegTypes.FTYPE) ret = AppSett.Default.ModSig_Char_FTYPE;
                if (this.regType == SigRegTypes.ITYPE) ret = AppSett.Default.ModSig_Char_ITYPE;
                if (this.regType == SigRegTypes.STYPE) ret = AppSett.Default.ModSig_Char_STYPE;

                return ret;
            }
        }
        public string TextReg
        {
            get
            {
                string ret = "";

                ret = ret + AppSett.Default.ModSig_Reg_Text + " ";
                ret = ret + this.RegTypeChar + this.RegNumberString;

                ret = ret + " ";

                return ret;
            }
        }
        public string TextAdd
        {
            get
            {
                string ret = "";

                ret = ret + AppSett.Default.ModSig_Add_Text + " ";
                ret = ret + this.ModAddressString;
                ret = ret + " ";

                return ret;
            }
        }
        public string TextDataType
        {
            get
            {
                string ret = "";

                ret = ret + AppSett.Default.ModSig_DataType_Text + " ";
                ret = ret + this.dataType.ToString().ToLower();
                ret = ret + " ";

                return ret;
            }
        }
        public string TextConvType
        {
            get
            {
                string ret = "";

                ret = ret + AppSett.Default.ModSig_ConvType_Text + " ";
                ret = ret + this.convType.ToString();
                ret = ret + " ";

                return ret;
            }
        }
        public string TextConv0
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C0_Text + " ";
                    ret = ret + this.conv0.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextConv2
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C2_Text + " ";
                    ret = ret + this.conv2.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextConv3
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C3_Text + " ";
                    ret = ret + this.conv3.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextConv4
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C4_Text + " ";
                    ret = ret + this.conv4.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextConv5
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C5_Text + " ";
                    ret = ret + this.conv5.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextConv6
        {
            get
            {
                string ret = "";

                if (this.convType > 0)
                {
                    ret = ret + AppSett.Default.ModSig_C6_Text + " ";
                    ret = ret + this.conv6.ToString();
                    ret = ret + " ";
                }

                return ret;
            }
        }
        public string TextShort
        {
            get
            {
                string ret = "";

                ret = ret + "Group=" + this.groupIndex.ToString() + " ";
                ret = ret + "Reg=" + this.RegTypeChar + this.RegNumberString + " ";
                if(this.IsBitDefined) ret = ret + "Address=" + this.ModAddress.TextComb + " ";
                else ret = ret + "Address=" + this.ModAddressString + " ";
                ret = ret + "Type=" + this.SigType.ToString();

                return ret;
            }
        }
        public string TextComp
        {
            get
            {
                string ret = "";

                ret = ret + this.TextReg;
                ret = ret + this.TextAdd;
                ret = ret + this.TextDataType;
                ret = ret + this.TextConvType;
                ret = ret + this.TextConv0;
                ret = ret + this.TextConv2;
                ret = ret + this.TextConv3;
                ret = ret + this.TextConv4;
                ret = ret + this.TextConv5;
                ret = ret + this.TextConv6;

                ret = ret + AppSett.Default.RLC_Comment_Char + " ";
                if (this.tagName != null) ret = ret + this.tagName + " ";
                if (this.description != null) ret = ret + this.description;

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
                    ret.Add(this.regNumber);

                    if (this.regType == SigRegTypes.FTYPE)
                    {
                        ret.Add(this.regNumber + 1);
                    }
                    if (this.regType == SigRegTypes.STYPE)
                    {
                        ret.Add(this.regNumber + 1);
                        ret.Add(this.regNumber + 2);
                    }
                }

                return ret;
            }
        }
        public List<ModAdd> UsedModAdds
        {
            get
            {
                List<ModAdd> ret = new List<ModAdd>();

                if (this.IsValid)
                {
                    if (this.Is16bits)
                    {
                        if (this.IsBitDefined) ret.Add(this.modAddress);
                        else ret.Add(new ModAdd(this.modAddress.Address, ModAdd.ModLoc.HoldingRegisters));
                    }
                    else
                    {
                        ret.Add(new ModAdd(this.modAddress.Address, ModAdd.ModLoc.HoldingRegisters));

                        if (!this.modAddress.IsAddMax) ret.Add(new ModAdd((this.modAddress.Address + 1), ModAdd.ModLoc.HoldingRegisters));
                    }
                }

                return ret;
            }
        }

        public ModGroup.GroupSigDir SigDirection
        {
            get
            {
                ModGroup.GroupSigDir ret = ModGroup.GroupSigDir.Input;

                if ((this.function == ModGroup.GroupFunction.FSC) || (this.function == ModGroup.GroupFunction.PSR) || (this.function == ModGroup.GroupFunction.FMC) || (this.function == ModGroup.GroupFunction.PMR))
                {
                    ret = ModGroup.GroupSigDir.Output;
                }
                else ret = ModGroup.GroupSigDir.Input;

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsValidRegNumber
        {
            get
            {
                bool ret = false;
                int min = 0;
                int max = 2045;

                if (this.regType == SigRegTypes.DTYPE) max = 2047;
                if (this.regType == SigRegTypes.ITYPE) max = 2047;
                if (this.regType == SigRegTypes.FTYPE) max = 2046;
                if (this.regType == SigRegTypes.STYPE) max = 2045;

                ret = (this.regNumber >= min) && (this.regNumber <= max);

                return ret;
            }
        }
        public bool IsValidGroupIndex
        {
            get
            {
                bool ret = false;
                int min = 0;

                ret = (this.groupIndex >= min);

                return ret;
            }
        }
        public bool IsValidModAddress
        {
            get
            {
                bool ret = false;

                ret = this.modAddress.IsValid;

                return ret;
            }
        }
        public bool IsValidConvType
        {
            get
            {
                bool ret = false;
                int min = 0;
                int max = 5;

                ret = (this.convType >= min) && (this.convType <= max);

                return ret;
            }
        }
        public bool IsValid
        {
            get
            {
                bool ret = false;

                ret = this.IsValidConvType && this.IsValidGroupIndex && this.IsValidModAddress && this.IsValidRegNumber;

                return ret;
            }
        }

        public bool IsBitDefined
        {
            get
            {
                bool ret = false;

                int pos = this.modAddress.FirstSetBit;

                ret = (!this.modAddress.AllBitsUsed) && (pos >=0);

                return ret;
            }
        }

        public bool IsConv0Pos
        {
            get
            {
                bool ret = false;

                ret = this.convType > 0;

                return ret;
            }
        }
        public bool IsConv2Pos
        {
            get
            {
                bool ret = false;

                ret = this.convType > 0;

                return ret;
            }
        }
        public bool IsConv3Pos
        {
            get
            {
                bool ret = false;

                ret = this.convType > 1;

                return ret;
            }
        }
        public bool IsConv4Pos
        {
            get
            {
                bool ret = false;

                ret = (this.convType == 2) || (this.convType == 5);

                return ret;
            }
        }
        public bool IsConv5Pos
        {
            get
            {
                bool ret = false;

                ret = (this.convType == 2) || (this.convType == 5);

                return ret;
            }
        }
        public bool IsConv6Pos
        {
            get
            {
                bool ret = false;

                ret = (this.convType == 2) || (this.convType == 5);

                return ret;
            }
        }

        public bool Is16bits
        {
            get
            {
                bool ret = false;

                ret = (this.dataType == SigDataTypes.BCD4) || (this.dataType == SigDataTypes.INT16) || (this.dataType == SigDataTypes.UINT16);

                return ret;
            }
        }
        public bool Is32bits
        {
            get
            {
                bool ret = false;

                ret = !this.Is16bits;

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors

        public ModSignal(SigRegTypes RegType, SigDataTypes DataType,ModGroup.GroupFunction Function, long RegNumber, int GroupIndex, ModAdd ModAddress, string TagName, string Desc)
        {
            this.regType = RegType;
            this.dataType = DataType;
            this.function = Function;
            this.regNumber = RegNumber;
            this.groupIndex = GroupIndex;
            this.modAddress = ModAddress;

            this.tagName = TagName;
            this.description = Desc;
            this.convType = 0;
            this.conv0 = 0;
            this.conv2 = 0;
            this.conv3 = 0;
            this.conv4 = 0;
            this.conv5 = 0;
            this.conv6 = 0;

            if (!this.IsValid) throw new Exception("Invalid input data!");
        }

        public ModSignal(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempString;
                string tempRegType = null;
                string tempDataType = null;
                string tempFunction = null;

                this.regNumber = reader.ReadInt64();

                this.groupIndex = reader.ReadInt32();

                this.convType = reader.ReadInt32();

                this.conv0 = reader.ReadSingle();

                this.conv2 = reader.ReadSingle();

                this.conv3 = reader.ReadSingle();

                this.conv4 = reader.ReadSingle();

                this.conv5 = reader.ReadSingle();

                this.conv6 = reader.ReadSingle();

                temp = reader.ReadBoolean();
                if (temp) this.tagName = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.description = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempRegType = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempDataType = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempFunction = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.modAddress = new ModAdd(ref reader);
                }

                if (tempRegType != null)
                {
                    if (String.Compare(tempRegType, SigRegTypes.DTYPE.ToString()) == 0) this.regType = SigRegTypes.DTYPE;
                    if (String.Compare(tempRegType, SigRegTypes.FTYPE.ToString()) == 0) this.regType = SigRegTypes.FTYPE;
                    if (String.Compare(tempRegType, SigRegTypes.ITYPE.ToString()) == 0) this.regType = SigRegTypes.ITYPE;
                    if (String.Compare(tempRegType, SigRegTypes.STYPE.ToString()) == 0) this.regType = SigRegTypes.STYPE;
                }
                else this.regType = SigRegTypes.ITYPE;

                if (tempDataType != null)
                {
                    if (String.Compare(tempDataType, SigDataTypes.BCD4.ToString()) == 0) this.dataType = SigDataTypes.BCD4;
                    if (String.Compare(tempDataType, SigDataTypes.BCD8.ToString()) == 0) this.dataType = SigDataTypes.BCD8;
                    if (String.Compare(tempDataType, SigDataTypes.FLOARREV.ToString()) == 0) this.dataType = SigDataTypes.FLOARREV;
                    if (String.Compare(tempDataType, SigDataTypes.FLOAT.ToString()) == 0) this.dataType = SigDataTypes.FLOAT;
                    if (String.Compare(tempDataType, SigDataTypes.FLOAT_BS.ToString()) == 0) this.dataType = SigDataTypes.FLOAT_BS;
                    if (String.Compare(tempDataType, SigDataTypes.FLOAT_WS_BS.ToString()) == 0) this.dataType = SigDataTypes.FLOAT_WS_BS;
                    if (String.Compare(tempDataType, SigDataTypes.INT16.ToString()) == 0) this.dataType = SigDataTypes.INT16;
                    if (String.Compare(tempDataType, SigDataTypes.INT32.ToString()) == 0) this.dataType = SigDataTypes.INT32;
                    if (String.Compare(tempDataType, SigDataTypes.INT32_BS.ToString()) == 0) this.dataType = SigDataTypes.INT32_BS;
                    if (String.Compare(tempDataType, SigDataTypes.INT32_WS_BS.ToString()) == 0) this.dataType = SigDataTypes.INT32_WS_BS;
                    if (String.Compare(tempDataType, SigDataTypes.INT32REV.ToString()) == 0) this.dataType = SigDataTypes.INT32REV;
                    if (String.Compare(tempDataType, SigDataTypes.UINT16.ToString()) == 0) this.dataType = SigDataTypes.UINT16;
                    if (String.Compare(tempDataType, SigDataTypes.UINT32.ToString()) == 0) this.dataType = SigDataTypes.UINT32;
                    if (String.Compare(tempDataType, SigDataTypes.UINT32_BS.ToString()) == 0) this.dataType = SigDataTypes.UINT32_BS;
                    if (String.Compare(tempDataType, SigDataTypes.UINT32_WS_BS.ToString()) == 0) this.dataType = SigDataTypes.UINT32_WS_BS;
                    if (String.Compare(tempDataType, SigDataTypes.UINT32REV.ToString()) == 0) this.dataType = SigDataTypes.UINT32REV;
                }
                else this.dataType = SigDataTypes.INT16;

                if (tempFunction != null)
                {
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.FMC.ToString()) == 0) this.function = ModGroup.GroupFunction.FMC;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.FSC.ToString()) == 0) this.function = ModGroup.GroupFunction.FSC;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.PMR.ToString()) == 0) this.function = ModGroup.GroupFunction.PMR;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.PSR.ToString()) == 0) this.function = ModGroup.GroupFunction.PSR;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.RCS.ToString()) == 0) this.function = ModGroup.GroupFunction.RCS;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.RES.ToString()) == 0) this.function = ModGroup.GroupFunction.RES;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.RHR.ToString()) == 0) this.function = ModGroup.GroupFunction.RHR;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.RIR.ToString()) == 0) this.function = ModGroup.GroupFunction.RIR;
                    if (String.Compare(tempFunction, ModGroup.GroupFunction.RIS.ToString()) == 0) this.function = ModGroup.GroupFunction.RIS;
                }
                else this.function = ModGroup.GroupFunction.RIR;
            }
        }

        #endregion


        #region Methods: Overrides and Implemented Interfaces

        /// <summary>
        /// Returns whether specified object equals this object or not
        /// </summary>
        /// <param name="obj">Object for equality checking</param>
        public override bool Equals(object obj)
        {
            bool regMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModSignal p = obj as ModSignal;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.regNumber != null) && (p.regNumber != null)) regMatch = this.regNumber == p.regNumber;

            // Return true if the fields match:
            return regMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int regHash = 0;

            if (this.regNumber != null) regHash = this.regNumber.GetHashCode();

            return (regHash);
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
            ModSignal p = obj as ModSignal;
            if ((System.Object)p == null) return ret;

            if (p.regNumber > this.regNumber) ret = -1;
            else
            {
                if (p.regNumber < this.regNumber) ret = 1;
                else ret = 0;
            }

            return ret;
        }

        #endregion

        #region Methods: General

        /// <summary>
        /// Sets conversion coefficients according the conversion type and maximum/minimum x and y
        /// </summary>
        /// <param name="ConvType">A conversion type</param>
        /// <param name="Xmin">An input minimum</param>
        /// <param name="Xmax">An input maximum</param>
        /// <param name="Ymin">An output minimum</param>
        /// <param name="Ymax">An output maximum</param>
        bool SetConversion(int ConvType, float Xmin, float Xmax, float Ymin, float Ymax)
        {
            bool ret = false;

            switch (ConvType)
            {
                case 1:
                    {

                        break;
                    }

                case 2:
                    {

                        break;
                    }
                
                case 3:
                    {

                        break;
                    }

                case 4:
                    {

                        break;
                    }

                case 5:
                    {

                        break;
                    }

                default:
                    {

                        break;
                    }
            }

            return ret;
        }

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

                // Register
                writer.Write(this.regNumber);

                // Group Index
                writer.Write(this.groupIndex);

                // Conversion Type
                writer.Write(this.convType);

                // Conversion Coefficient 0
                writer.Write(this.conv0);

                // Conversion Coefficient 2
                writer.Write(this.conv2);

                // Conversion Coefficient 3
                writer.Write(this.conv3);

                // Conversion Coefficient 4
                writer.Write(this.conv4);

                // Conversion Coefficient 5
                writer.Write(this.conv5);

                // Conversion Coefficient 6
                writer.Write(this.conv6);

                // TagName
                if (this.tagName != null)
                {
                    writer.Write(true);
                    writer.Write(this.tagName);
                }
                else writer.Write(false);

                // Description
                if (this.description != null)
                {
                    writer.Write(true);
                    writer.Write(this.description);
                }
                else writer.Write(false);

                // RegType
                if (this.regType != null)
                {
                    writer.Write(true);
                    writer.Write(this.regType.ToString());
                }
                else writer.Write(false);

                // DataType
                if (this.dataType != null)
                {
                    writer.Write(true);
                    writer.Write(this.dataType.ToString());
                }
                else writer.Write(false);

                // Function
                if (this.function != null)
                {
                    writer.Write(true);
                    writer.Write(this.function.ToString());
                }
                else writer.Write(false);

                // Modbus Address
                if (this.modAddress != null)
                {
                    writer.Write(true);

                    tempBytes = this.modAddress.SerializeToStream(ref writer);

                    if (!tempBytes) throw new Exception("Serialization error!");
                }
                else writer.Write(false);

                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        #endregion
    }
}
