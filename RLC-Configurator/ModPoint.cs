using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RLC_Configurator
{
    public class ModPoint:IComparable
    {
        
        #region Objects: Enumerators

        public enum ModDataType
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

        private int groupIndex;
        private string tagName;
        private string description;
        private ModDataType dataType;
        private ModGroup.GroupFunction function;
        private ModReg register;
        private ModAdd address;
        private ModConv conversion;

        #endregion


        #region Public Properties: General

        public int GroupIndex
        {
            get { return this.groupIndex; }
            set { this.groupIndex = value; }
        }
        public string TagName
        {
            get { return this.tagName; }
        }
        public string Description
        {
            get { return this.description; }
        }
        public ModDataType DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }
        public ModGroup.GroupFunction Function
        {
            get { return this.function; }
        }
        public ModReg Register
        {
            get { return this.register; }
        }
        public ModAdd Address
        {
            get { return this.address; }
        }
        public ModConv Conversion
        {
            get { return this.conversion; }
        }

        // Error Codes:
        // 0 = No Error
        // 1 = Point is not valid
        // 2 = Register type is D and the modbus address is not binary
        // 3 = Register type is I/F/S and the modbus address is not analog
        // 4 = Signal direction is output and modbus storage cannot be written
        // 5 = The modbus storage type is not equeal to storage type determined by the function
        // 6 = The modbus address is binary and the conversion is not X=Y
        public int ErrCode
        {
            get
            {
                int ret = 0;

                if (this.IsValid)
                {
                    #region Register Type vs Modbus Storage Type

                    if (this.register.Type == ModReg.ModRegType.DTYPE)
                    {
                        if (!this.address.IsBinaryType) return 2;
                    }
                    if (this.register.Type == ModReg.ModRegType.ITYPE)
                    {
                        if (!this.address.IsAnalogType) return 3;
                    }
                    if (this.register.Type == ModReg.ModRegType.FTYPE)
                    {
                        if (!this.address.IsAnalogType) return 3;
                    }
                    if (this.register.Type == ModReg.ModRegType.STYPE)
                    {
                        if (!this.address.IsAnalogType) return 3;
                    }

                    #endregion

                    #region Signal Direction vs Modbus Storage Type

                    if (this.SigDirection == ModGroup.GroupSigDir.Output)
                    {
                        if (!this.address.IsWritePos) return 4;
                    }

                    #endregion

                    #region Function Type vs Modbus Storage Type

                    if (this.FunctionAddLoc != this.address.Location) return 5;

                    #endregion

                    #region Conversion Type vs Modbus Storage Type

                    if ((this.address.IsBinaryType) && (this.conversion.Type != ModConv.ModConvType.NoConv)) return 6;

                    #endregion
                }
                else ret = 1;

                return ret;
            }
        }
        public string ErrCodeDesc
        {
            get
            {
                string ret = "";

                int code = this.ErrCode;

                if (code <= (AppSett.Default.ModPoint_ErrCode_Desc.Count - 1)) ret = AppSett.Default.ModPoint_ErrCode_Desc[code];

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
        public ModAdd.ModLoc FunctionAddLoc
        {
            get
            {
                ModAdd.ModLoc ret = ModAdd.ModLoc.Inputs;

                if ((this.function == ModGroup.GroupFunction.RCS) || (this.function == ModGroup.GroupFunction.FSC) || (this.function == ModGroup.GroupFunction.FMC))
                {
                    ret = ModAdd.ModLoc.Coils;
                }

                if (this.function == ModGroup.GroupFunction.RIS) ret = ModAdd.ModLoc.Inputs;

                if (this.function == ModGroup.GroupFunction.RIR) ret = ModAdd.ModLoc.InputRegisters;

                if ((this.function == ModGroup.GroupFunction.RHR) || (this.function == ModGroup.GroupFunction.PSR) || (this.function == ModGroup.GroupFunction.PMR))
                {
                    ret = ModAdd.ModLoc.HoldingRegisters;
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
                ret = ret + "Reg=" + this.register.TextComp + " ";
                ret = ret + "Address=" + this.address.LocComb + " ";
                ret = ret + "Function=" + this.function.ToString();

                return ret;
            }
        }
        public string TextComp
        {
            get
            {
                string ret = "";

                ret = ret + this.register.TextComp + " ";
                ret = ret + this.address.TextComb + " ";
                ret = ret + this.dataType.ToString() + " ";
                ret = ret + this.conversion.TextComp + " ";

                ret = ret + AppSett.Default.RLC_Comment_Char + " ";
                ret = ret + this.address.LocComb + " ";
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
                    ret = this.register.UsedModRegs;

                    if (ret == null) ret = new List<long>();
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
                        ret.Add(this.address);
                    }
                    else
                    {
                        ret.Add(this.address);

                        if (!this.address.IsAddMax) ret.Add(new ModAdd((this.address.Address + 1), this.address.Location));

                    }
                }

                return ret;
            }
        }

        public SigTypes SigType
        {
            get
            {
                SigTypes ret = SigTypes.AI;

                if (this.SigDirection == ModGroup.GroupSigDir.Input)
                {
                    if (this.Function == ModGroup.GroupFunction.RCS)
                    {
                        ret = SigTypes.DI;
                    }
                    if (this.Function == ModGroup.GroupFunction.RHR)
                    {
                        if (this.address.IsBinaryType)
                        {
                            ret = SigTypes.DI;
                        }
                        else
                        {
                            if (this.register.Type == ModReg.ModRegType.DTYPE) ret = SigTypes.DI;
                            if (this.register.Type == ModReg.ModRegType.ITYPE) ret = SigTypes.PI;
                            if (this.register.Type == ModReg.ModRegType.FTYPE) ret = SigTypes.AI;
                            if (this.register.Type == ModReg.ModRegType.STYPE) ret = SigTypes.AI;
                        }
                    }
                    if (this.Function == ModGroup.GroupFunction.RES)
                    {
                        ret = SigTypes.PI;
                    }
                    if (this.Function == ModGroup.GroupFunction.RIR)
                    {
                        if (this.address.IsBinaryType)
                        {
                            ret = SigTypes.DI;
                        }
                        else
                        {
                            if (this.register.Type == ModReg.ModRegType.DTYPE) ret = SigTypes.DI;
                            if (this.register.Type == ModReg.ModRegType.ITYPE) ret = SigTypes.PI;
                            if (this.register.Type == ModReg.ModRegType.FTYPE) ret = SigTypes.AI;
                            if (this.register.Type == ModReg.ModRegType.STYPE) ret = SigTypes.AI;
                        }
                    }
                    if (this.Function == ModGroup.GroupFunction.RIS)
                    {
                        ret = SigTypes.DI;
                    }
                }
                else
                { 
                
                }

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsValidGroup
        {
            get
            {
                bool ret = false;

                ret = this.groupIndex >= 0;

                return ret;
            }
        }
        public bool IsValidTag
        {
            get
            {
                bool ret = false;

                ret = this.tagName != null;

                return ret;
            }
        }
        public bool IsValidDesc
        {
            get
            {
                bool ret = false;

                ret = this.description != null;

                return ret;
            }
        }
        public bool IsValidReg
        {
            get
            {
                bool ret = false;

                if (this.register != null)
                {
                    ret = this.register.IsValid;
                }

                return ret;
            }
        }
        public bool IsValidAdd
        {
            get
            {
                bool ret = false;

                if (this.address != null)
                {
                    ret = this.address.IsValid;
                }

                return ret;
            }
        }
        public bool IsValidConv
        {
            get
            {
                bool ret = false;

                ret = this.conversion != null;

                return ret;
            }
        }
        public bool IsValid
        {
            get
            {
                bool ret = false;

                ret = this.IsValidAdd && this.IsValidConv && this.IsValidDesc && this.IsValidGroup && this.IsValidReg && this.IsValidTag;

                return ret;
            }
        }
        public bool IsValidConf
        {
            get
            {
                bool ret = false;
                bool validRegAdd = false;

                if (this.IsValid)
                {
                    if (this.register.Type == ModReg.ModRegType.DTYPE)
                    { 
                        if( (this.address.Location == ModAdd.ModLoc.Coils) || (this.address.Location == ModAdd.ModLoc.Inputs)) validRegAdd = true;
                        else
                        {
                            
                        }
                    }
                    if (this.register.Type == ModReg.ModRegType.ITYPE)
                    {

                    }
                    if (this.register.Type == ModReg.ModRegType.FTYPE)
                    {

                    }
                    if (this.register.Type == ModReg.ModRegType.STYPE)
                    {

                    }
                
                }

                return ret;
            }
        }

        public bool HasValidConfig
        {
            get
            {
                bool ret = false;

                int code = this.ErrCode;

                ret = code == 0;

                return ret;
            }
        }

        public bool Is16bits
        {
            get
            {
                bool ret = false;

                ret = this.Is16bitFunct(this.dataType);

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

        public ModPoint(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempString;
                string tempDataType = null;
                string tempFunction = null;

                this.groupIndex = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) this.tagName = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.description = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempDataType = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) tempFunction = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.register = new ModReg(ref reader);
                }

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.address = new ModAdd(ref reader);
                }

                temp = reader.ReadBoolean();
                if (temp)
                {
                    this.conversion = new ModConv(ref reader);
                }

                if (tempDataType != null)
                {
                    if (String.Compare(tempDataType, ModDataType.BCD4.ToString()) == 0) this.dataType = ModDataType.BCD4;
                    if (String.Compare(tempDataType, ModDataType.BCD8.ToString()) == 0) this.dataType = ModDataType.BCD8;
                    if (String.Compare(tempDataType, ModDataType.FLOARREV.ToString()) == 0) this.dataType = ModDataType.FLOARREV;
                    if (String.Compare(tempDataType, ModDataType.FLOAT.ToString()) == 0) this.dataType = ModDataType.FLOAT;
                    if (String.Compare(tempDataType, ModDataType.FLOAT_BS.ToString()) == 0) this.dataType = ModDataType.FLOAT_BS;
                    if (String.Compare(tempDataType, ModDataType.FLOAT_WS_BS.ToString()) == 0) this.dataType = ModDataType.FLOAT_WS_BS;
                    if (String.Compare(tempDataType, ModDataType.INT16.ToString()) == 0) this.dataType = ModDataType.INT16;
                    if (String.Compare(tempDataType, ModDataType.INT32.ToString()) == 0) this.dataType = ModDataType.INT32;
                    if (String.Compare(tempDataType, ModDataType.INT32_BS.ToString()) == 0) this.dataType = ModDataType.INT32_BS;
                    if (String.Compare(tempDataType, ModDataType.INT32_WS_BS.ToString()) == 0) this.dataType = ModDataType.INT32_WS_BS;
                    if (String.Compare(tempDataType, ModDataType.INT32REV.ToString()) == 0) this.dataType = ModDataType.INT32REV;
                    if (String.Compare(tempDataType, ModDataType.UINT16.ToString()) == 0) this.dataType = ModDataType.UINT16;
                    if (String.Compare(tempDataType, ModDataType.UINT32.ToString()) == 0) this.dataType = ModDataType.UINT32;
                    if (String.Compare(tempDataType, ModDataType.UINT32_BS.ToString()) == 0) this.dataType = ModDataType.UINT32_BS;
                    if (String.Compare(tempDataType, ModDataType.UINT32_WS_BS.ToString()) == 0) this.dataType = ModDataType.UINT32_WS_BS;
                    if (String.Compare(tempDataType, ModDataType.UINT32REV.ToString()) == 0) this.dataType = ModDataType.UINT32REV;
                }
                else this.dataType = ModDataType.INT16;

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

        public ModPoint(int GroupIndex, string TagName, string Description, ModDataType DataType, ModGroup.GroupFunction Function, ModReg Reg, ModAdd Add, ModConv Conv)
        {
            Exception ex1 = new Exception("Invalid input data!");

            this.groupIndex = GroupIndex;
            this.tagName = TagName;
            this.description = Description;
            this.dataType = DataType;
            this.function = Function;
            this.register = Reg;
            this.address = Add;
            this.conversion = Conv;

            if (!this.IsValid) throw ex1;

            int code = this.ErrCode;

            if (code != 0)
            {
                Exception ex2 = new Exception(this.ErrCodeDesc);
                throw ex2;
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

                // Group Index
                writer.Write(this.groupIndex);

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

                // Data Type
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

                // Register
                if (this.register != null)
                {
                    writer.Write(true);

                    tempBytes = this.register.SerializeToStream(ref writer);

                    if (!tempBytes) throw new Exception("Serialization error!");
                }
                else writer.Write(false);

                // Modbus Address
                if (this.address != null)
                {
                    writer.Write(true);

                    tempBytes = this.address.SerializeToStream(ref writer);

                    if (!tempBytes) throw new Exception("Serialization error!");
                }
                else writer.Write(false);

                // Conversion
                if (this.conversion != null)
                {
                    writer.Write(true);

                    tempBytes = this.conversion.SerializeToStream(ref writer);

                    if (!tempBytes) throw new Exception("Serialization error!");
                }
                else writer.Write(false);

                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Returns whether specified data type is 16bits
        /// </summary>
        /// <param name="DataType">A specified data type</param>
        public bool Is16bitFunct(ModDataType DataType)
        {
            bool ret = false;

            ret = (DataType == ModDataType.BCD4) || (DataType == ModDataType.INT16) || (DataType == ModDataType.UINT16);

            return ret;
        }

        #endregion

        #region Methods: Change Functions

        /// <summary>
        /// Changes the group index to new
        /// </summary>
        /// <param name="NewIndex">A new group index</param>
        public bool ChangeGroupIndex(int NewIndex)
        {
            bool ret = false;

            try
            {
                this.groupIndex = NewIndex;
                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Changes the tag name
        /// </summary>
        /// <param name="NewTagName">A new tag name</param>
        public bool ChangeTagName(string NewTagName)
        {
            bool ret = false;

            try
            {
                if (NewTagName != null)
                {
                    this.tagName= NewTagName;
                    ret = true; 
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Changes the description
        /// </summary>
        /// <param name="NewDesc">A new description</param>
        public bool ChangeDesc(string NewDesc)
        {
            bool ret = false;

            try
            {
                if (NewDesc != null)
                {
                    this.description = NewDesc;
                    ret = true;
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Changes the conversion
        /// </summary>
        /// <param name="NewConv">A new conversion</param>
        public bool ChangeConv(ModConv NewConv)
        {
            bool ret = false;

            try
            {
                if (NewConv != null)
                {
                    this.conversion = NewConv;
                    ret = true;
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Changes the address bit
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: The Modbus Location does not support bit addresses
        ///  2: Set of the bit(s) was not successful
        ///  3: The new bit is not free
        ///  4: Change to Binary Type - the register type is not binary
        ///  5: Change to Analog Type - the register type is not analog
        ///  6: New bits are not free
        /// </remarks>
        /// <param name="NewSetBit">A new bit to be set</param>
        /// <param name="FreeAdds">A list of free addresses</param>
        public int ChangeAddBit(int NewSetBit, List<ModAdd> FreeAdds)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;

            try
            {
                if (FreeAdds != null)
                {
                    if (this.address.AreBitsAvail)
                    {
                        ModAdd temp;

                        bool inRange = (NewSetBit >= 0) && (NewSetBit <= 15);

                        #region CASE 1: Change of the bit

                        if (inRange && this.address.IsBinaryType)
                        {
                            temp = FreeAdds.Find(x => x.Equals(this.address));

                            if (temp != null)
                            {
                                isSet = temp.IsBitSet(NewSetBit);

                                if (!isSet) return 3;
                                else
                                {
                                    res = this.address.SetBit(NewSetBit, false);

                                    if (res) return 0;
                                    else return 2;
                                }
                            }
                            else return 3;
                        }

                        #endregion

                        #region CASE 2: Change from the Analog Type to Binary Type

                        if (inRange && !this.address.IsBinaryType)
                        {
                            if (this.register.Type == ModReg.ModRegType.DTYPE)
                            {
                                res = this.address.SetBit(NewSetBit, false);

                                if (res) return 0;
                                else return 2;
                            }
                            else return 4;
                        }

                        #endregion

                        #region CASE 3: Change from Binary Type to Analog Type

                        if (!inRange && this.address.IsBinaryType)
                        {
                            if (this.register.Type != ModReg.ModRegType.DTYPE)
                            {
                                temp = FreeAdds.Find(x => x.Equals(this.address));

                                if (temp != null)
                                {
                                    isSet = String.Compare(temp.BitMask, this.address.BitMaskInv, true) == 0;

                                    if (isSet)
                                    {
                                        res = this.address.SetAllBits();

                                        if (res) return 0;
                                        else return 2;
                                    }
                                    else return 5;
                                }
                                else return 5;
                            }
                            else return 4;
                        }

                        #endregion

                        #region CASE 4: Change from Analog Type to Analog Type

                        if (!inRange && !this.address.IsBinaryType)
                        {
                            return 0;
                        }

                        #endregion
                    }
                    else ret = 1; 
                }
            }
            catch (Exception e) { ret = -1; }

            return ret;
        }

        /// <summary>
        /// Changes the address number
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: Invalid new address
        ///  2: The new address is not free
        ///  3: The set of new address was not successful
        /// </remarks>
        /// <param name="NewSetNum">A new address to be set</param>
        /// <param name="FreeAdds">A list of free addresses</param>
        public int ChangeAddNum(int NewSetNum, List<ModAdd> FreeAdds)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;

            try
            {
                int min = this.address.MinAdd;
                int max = this.address.MaxAdd;

                if (FreeAdds != null)
                {
                    if ((NewSetNum >= min) && (NewSetNum <= max))
                    {
                        ModAdd temp;
                        ModAdd newAdd;

                        #region CASE 1: Binary Type Address

                        if (this.address.IsBinaryType)
                        {
                            newAdd = new ModAdd(NewSetNum, this.address.BitMask, this.address.Location);
                            temp = FreeAdds.Find(x => x.Equals(newAdd));

                            if (temp != null)
                            {
                                isSet = temp.IsBitSet(this.address.FirstSetBit);

                                if (!isSet) return 2;
                                else
                                {
                                    res = this.address.SetAdd(NewSetNum);

                                    if (res) return 0;
                                    else return 3;
                                }
                            }
                            else return 2;
                        }

                        #endregion

                        #region CASE 2: Analog Type Address

                        if (this.address.IsAnalogType)
                        {
                            newAdd = new ModAdd(NewSetNum, this.address.BitMask, this.address.Location);
                            temp = FreeAdds.Find(x => x.Equals(newAdd));

                            if (temp != null)
                            {
                                isSet = temp.AllBitsUsed;

                                if (!isSet) return 2;
                                else
                                {
                                    res = this.address.SetAdd(NewSetNum);

                                    if (res) return 0;
                                    else return 3;
                                }
                            }
                            else return 2;
                        }

                        #endregion
                    }
                    else return 1; 
                }
            }
            catch (Exception e) { ret = -1; }

            return ret;
        }

        /// <summary>
        /// Changes the location
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: Address of new location is not free
        ///  2: Cannot change location to analog type. Register type is D
        ///  3: Cannot change location to binary type. Register type is not D
        ///  4: No free address to change to
        /// </remarks>
        /// <param name="NewLoc">A new address location</param>
        /// <param name="FreeAdds">A list of free addresses</param>
        /// <param name="Auto">A switch to specify whether automatically change address when old address is not free</param>
        public int ChangeAddLoc(ModAdd.ModLoc NewLoc, List<ModAdd> FreeAdds, bool Auto)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;
            ModAdd tempAdd;
            List<ModAdd> selAdds;

            try
            {
                if (FreeAdds != null)
                {
                    #region CASE 1: Inputs changing to Inputs
                    
                    if( (this.address.Location == ModAdd.ModLoc.Inputs) && (NewLoc == ModAdd.ModLoc.Inputs) )
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 2: Inputs changing to Coils

                    if ((this.address.Location == ModAdd.ModLoc.Inputs) && (NewLoc == ModAdd.ModLoc.Coils))
                    {
                        tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Coils);
                        res = FreeAdds.Contains(tempAdd);

                        if (res)
                        {
                            this.address.Location = ModAdd.ModLoc.Coils;
                            return 0;
                        }
                        else
                        {
                            if (Auto)
                            {
                                selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Coils));

                                if (selAdds != null)
                                {
                                    if (selAdds.Count > 0)
                                    {
                                        tempAdd = selAdds[0];

                                        if (tempAdd != null)
                                        {
                                            this.address.Address = tempAdd.Address;
                                            this.address.Location = tempAdd.Location;
                                            return 0;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                            else return 1;
                        }
                    }

                    #endregion

                    #region CASE 3: Inputs changing to Input Registers

                    if ((this.address.Location == ModAdd.ModLoc.Inputs) && (NewLoc == ModAdd.ModLoc.InputRegisters))
                    {
                        if (this.register.Type != ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.InputRegisters);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.InputRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.InputRegisters));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 2;
                    }

                    #endregion

                    #region CASE 4: Inputs changing to Holding Registers

                    if ((this.address.Location == ModAdd.ModLoc.Inputs) && (NewLoc == ModAdd.ModLoc.HoldingRegisters))
                    {
                        if (this.register.Type != ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.HoldingRegisters);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.HoldingRegisters));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 2;
                    }

                    #endregion

                    #region CASE 5: Coils changing to Inputs

                    if ((this.address.Location == ModAdd.ModLoc.Coils) && (NewLoc == ModAdd.ModLoc.Inputs))
                    {
                        tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Inputs);
                        res = FreeAdds.Contains(tempAdd);

                        if (res)
                        {
                            this.address.Location = ModAdd.ModLoc.Coils;
                            return 0;
                        }
                        else
                        {
                            if (Auto)
                            {
                                selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Inputs));

                                if (selAdds != null)
                                {
                                    if (selAdds.Count > 0)
                                    {
                                        tempAdd = selAdds[0];

                                        if (tempAdd != null)
                                        {
                                            this.address.Address = tempAdd.Address;
                                            this.address.Location = tempAdd.Location;
                                            return 0;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                            else return 1;
                        }
                    }

                    #endregion

                    #region CASE 6: Coils changing to Coils

                    if ((this.address.Location == ModAdd.ModLoc.Coils) && (NewLoc == ModAdd.ModLoc.Coils))
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 7: Coils changing to Input Registers

                    if ((this.address.Location == ModAdd.ModLoc.Coils) && (NewLoc == ModAdd.ModLoc.InputRegisters))
                    {
                        if (this.register.Type != ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.InputRegisters);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.InputRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.InputRegisters));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 2;
                    }

                    #endregion

                    #region CASE 8: Coils changing to Holding Registers

                    if ((this.address.Location == ModAdd.ModLoc.Coils) && (NewLoc == ModAdd.ModLoc.HoldingRegisters))
                    {
                        if (this.register.Type != ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.HoldingRegisters);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.HoldingRegisters));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 2;
                    }

                    #endregion

                    #region CASE 9: Input Registers to Inputs

                    if ((this.address.Location == ModAdd.ModLoc.InputRegisters) && (NewLoc == ModAdd.ModLoc.Inputs))
                    {
                        if (this.register.Type == ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Inputs);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.Inputs;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Inputs));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 3;
                    }

                    #endregion

                    #region CASE 10: Input Registers to Coils

                    if ((this.address.Location == ModAdd.ModLoc.InputRegisters) && (NewLoc == ModAdd.ModLoc.Coils))
                    {
                        if (this.register.Type == ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Coils);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.Coils;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Coils));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 3;
                    }

                    #endregion

                    #region CASE 11: Input Registers to Input Registers

                    if ((this.address.Location == ModAdd.ModLoc.InputRegisters) && (NewLoc == ModAdd.ModLoc.InputRegisters))
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 12: Input Registers to Holding Registers

                    if ((this.address.Location == ModAdd.ModLoc.InputRegisters) && (NewLoc == ModAdd.ModLoc.HoldingRegisters))
                    {
                        if (this.address.IsBinaryType)
                        {
                            tempAdd = FreeAdds.Find(x => ((x.Address == this.address.Address) && (x.Location == this.address.Location)));

                            if (tempAdd != null)
                            {
                                res = tempAdd.IsBitSet(this.address.FirstSetBit);

                                if (res)
                                {
                                    this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                    return 0;
                                }
                                else
                                {
                                    if (Auto)
                                    {
                                        this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                        this.address.SetBit(tempAdd.FirstSetBit, false);
                                        return 0;
                                    }
                                    else return 1;
                                }
                            }
                            else
                            {
                                if (Auto)
                                {
                                    if (FreeAdds.Count > 0)
                                    {
                                        this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                        this.address.Address = FreeAdds[0].Address;
                                        this.address.SetBit(FreeAdds[0].FirstSetBit, false);
                                        return 0;
                                    }
                                    else return 4;
                                }
                                else return 1;
                            }
                        }
                        else
                        {
                            tempAdd = FreeAdds.Find(x => ((x.Address == this.address.Address) && (x.Location == this.address.Location)));

                            if (tempAdd != null)
                            {
                                this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    if (FreeAdds.Count > 0)
                                    {
                                        this.address.Location = ModAdd.ModLoc.HoldingRegisters;
                                        this.address.Address = FreeAdds[0].Address;
                                        return 0;
                                    }
                                    else return 4;
                                }
                                else return 1;
                            }
                        }
                    }

                    #endregion

                    #region CASE 13: Holding Registers to Inputs

                    if ((this.address.Location == ModAdd.ModLoc.HoldingRegisters) && (NewLoc == ModAdd.ModLoc.Inputs))
                    {
                        if (this.register.Type == ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Inputs);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.Inputs;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Inputs));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 3;
                    }

                    #endregion

                    #region CASE 14: Holding Registers to Coils

                    if ((this.address.Location == ModAdd.ModLoc.HoldingRegisters) && (NewLoc == ModAdd.ModLoc.Coils))
                    {
                        if (this.register.Type == ModReg.ModRegType.DTYPE)
                        {
                            tempAdd = new ModAdd(this.address.Address, ModAdd.ModLoc.Coils);
                            res = FreeAdds.Contains(tempAdd);

                            if (res)
                            {
                                this.address.Location = ModAdd.ModLoc.Coils;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    selAdds = FreeAdds.FindAll(x => (x.Location == ModAdd.ModLoc.Coils));

                                    if (selAdds != null)
                                    {
                                        if (selAdds.Count > 0)
                                        {
                                            tempAdd = selAdds[0];

                                            if (tempAdd != null)
                                            {
                                                this.address.Address = tempAdd.Address;
                                                this.address.Location = tempAdd.Location;
                                                return 0;
                                            }
                                            else return 1;
                                        }
                                        else return 1;
                                    }
                                    else return 1;
                                }
                                else return 1;
                            }
                        }
                        else return 3;
                    }

                    #endregion

                    #region CASE 15: Holding Registers to Input Registers

                    if ((this.address.Location == ModAdd.ModLoc.HoldingRegisters) && (NewLoc == ModAdd.ModLoc.InputRegisters))
                    {
                        if (this.address.IsBinaryType)
                        {
                            tempAdd = FreeAdds.Find(x => ((x.Address == this.address.Address) && (x.Location == this.address.Location)));

                            if (tempAdd != null)
                            {
                                res = tempAdd.IsBitSet(this.address.FirstSetBit);

                                if (res)
                                {
                                    this.address.Location = ModAdd.ModLoc.InputRegisters;
                                    return 0;
                                }
                                else
                                {
                                    if (Auto)
                                    {
                                        this.address.Location = ModAdd.ModLoc.InputRegisters;
                                        this.address.SetBit(tempAdd.FirstSetBit, false);
                                        return 0;
                                    }
                                    else return 1;
                                }
                            }
                            else
                            {
                                if (Auto)
                                {
                                    if (FreeAdds.Count > 0)
                                    {
                                        this.address.Location = ModAdd.ModLoc.InputRegisters;
                                        this.address.Address = FreeAdds[0].Address;
                                        this.address.SetBit(FreeAdds[0].FirstSetBit, false);
                                        return 0;
                                    }
                                    else return 4;
                                }
                                else return 1;
                            }
                        }
                        else
                        {
                            tempAdd = FreeAdds.Find(x => ((x.Address == this.address.Address) && (x.Location == this.address.Location)));

                            if (tempAdd != null)
                            {
                                this.address.Location = ModAdd.ModLoc.InputRegisters;
                                return 0;
                            }
                            else
                            {
                                if (Auto)
                                {
                                    if (FreeAdds.Count > 0)
                                    {
                                        this.address.Location = ModAdd.ModLoc.InputRegisters;
                                        this.address.Address = FreeAdds[0].Address;
                                        return 0;
                                    }
                                    else return 4;
                                }
                                else return 1;
                            }
                        }
                    }

                    #endregion

                    #region CASE 16: Holding Registers to Holding Registers

                    if ((this.address.Location == ModAdd.ModLoc.HoldingRegisters) && (NewLoc == ModAdd.ModLoc.HoldingRegisters))
                    {
                        return 0;
                    }

                    #endregion
                }
            }
            catch (Exception e) { ret = -1; }

            return ret;
        }

        /// <summary>
        /// Changes the register number
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: Invalid new register
        ///  2: The new register is not free
        /// </remarks>
        /// <param name="NewSetNum">A new register to be set</param>
        /// <param name="FreeRegs">A list of free registers</param>
        public int ChangeRegNum(int NewSetNum, List<ModReg> FreeRegs)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;
            ModReg tempReg;

            try
            {
                int min = this.register.MinCurrAdd;
                int max = this.register.MaxCurrAdd;

                if (FreeRegs != null)
                {
                    if ((NewSetNum >= min) && (NewSetNum <= max))
                    {
                        tempReg = FreeRegs.Find(x => x.Number.Equals(NewSetNum));

                        if (tempReg != null)
                        {
                            this.register.Number = NewSetNum;
                            return 0;
                        }
                        else return 2;
                    }
                    else return 1;
                }
                else return -1;
            }
            catch (Exception e) { ret = -1; }

            return ret;
        }

        /// <summary>
        /// Changes the register type
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: Cannot switch to specified type - used register numbers exceed maximum
        /// </remarks>
        /// <param name="NewType">A new register type</param>
        /// <param name="FreeRegs">A list of free registers</param>
        /// <param name="Auto">A switch to specify whether automatically change address when old address is not free</param>
        public int ChangeRegType(ModReg.ModRegType NewType, List<ModReg> FreeRegs, bool Auto)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;
            ModReg tempReg;
            List<ModReg> selRegs;

            try
            {
                if (FreeRegs != null)
                {
                    #region CASE 1: DTYPE to DTYPE

                    if ((this.register.Type == ModReg.ModRegType.DTYPE) && (NewType == ModReg.ModRegType.DTYPE))
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 2: DTYPE to ITYPE

                    if ((this.register.Type == ModReg.ModRegType.DTYPE) && (NewType == ModReg.ModRegType.ITYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 3: DTYPE to FTYPE

                    if ((this.register.Type == ModReg.ModRegType.DTYPE) && (NewType == ModReg.ModRegType.FTYPE))
                    {
                        if (this.register.IsAvailToFTYPE)
                        {
                            this.register.Type = NewType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 4: DTYPE to STYPE

                    if ((this.register.Type == ModReg.ModRegType.DTYPE) && (NewType == ModReg.ModRegType.STYPE))
                    {
                        if (this.register.IsAvailToSTYPE)
                        {
                            this.register.Type = NewType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 5: ITYPE to DTYPE

                    if ((this.register.Type == ModReg.ModRegType.ITYPE) && (NewType == ModReg.ModRegType.DTYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 6: ITYPE to ITYPE

                    if ((this.register.Type == ModReg.ModRegType.ITYPE) && (NewType == ModReg.ModRegType.ITYPE))
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 7: ITYPE to FTYPE

                    if ((this.register.Type == ModReg.ModRegType.ITYPE) && (NewType == ModReg.ModRegType.FTYPE))
                    {
                        if (this.register.IsAvailToFTYPE)
                        {
                            this.register.Type = NewType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 8: ITYPE to STYPE

                    if ((this.register.Type == ModReg.ModRegType.ITYPE) && (NewType == ModReg.ModRegType.STYPE))
                    {
                        if (this.register.IsAvailToSTYPE)
                        {
                            this.register.Type = NewType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 9: FTYPE to DTYPE

                    if ((this.register.Type == ModReg.ModRegType.FTYPE) && (NewType == ModReg.ModRegType.DTYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 10: FTYPE to ITYPE

                    if ((this.register.Type == ModReg.ModRegType.FTYPE) && (NewType == ModReg.ModRegType.ITYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 11: FTYPE to FTYPE

                    if ((this.register.Type == ModReg.ModRegType.FTYPE) && (NewType == ModReg.ModRegType.FTYPE))
                    {
                        return 0;
                    }

                    #endregion

                    #region CASE 12: FTYPE to STYPE

                    if ((this.register.Type == ModReg.ModRegType.FTYPE) && (NewType == ModReg.ModRegType.STYPE))
                    {
                        if (this.register.IsAvailToSTYPE)
                        {
                            this.register.Type = NewType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 13: STYPE to DTYPE

                    if ((this.register.Type == ModReg.ModRegType.STYPE) && (NewType == ModReg.ModRegType.DTYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 14: STYPE to ITYPE

                    if ((this.register.Type == ModReg.ModRegType.STYPE) && (NewType == ModReg.ModRegType.ITYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 15: STYPE to FTYPE

                    if ((this.register.Type == ModReg.ModRegType.STYPE) && (NewType == ModReg.ModRegType.FTYPE))
                    {
                        this.register.Type = NewType;
                        return 0;
                    }

                    #endregion

                    #region CASE 16: STYPE to STYPE

                    if ((this.register.Type == ModReg.ModRegType.STYPE) && (NewType == ModReg.ModRegType.STYPE))
                    {
                        return 0;
                    }

                    #endregion
                }
            }
            catch (Exception e) { ret = -1; }

            return ret;
        }

        /// <summary>
        /// Changes the data type
        /// </summary>
        /// <remarks>
        /// -1: Unhandled exception
        ///  0: No Errors
        ///  1: The address slots needed are not free
        /// </remarks>
        /// <param name="NewDataType">A new data type</param>
        /// <param name="FreeAdds">A list of free addresses</param>
        /// <param name="Auto">A switch to specify whether automatically change address when old address is not free</param>
        public int ChangeDataType(ModDataType NewDataType, List<ModAdd> FreeAdds, bool Auto)
        {
            int ret = -1;
            bool res = false;
            bool isSet = false;
            ModAdd tempAdd;
            List<ModAdd> selAdds;

            try
            {
                if (FreeAdds != null)
                {
                    bool isNew16bit = this.Is16bitFunct(NewDataType);
                    
                    #region CASE 1: 16bits to 16bits

                    if ((this.Is16bits) && (isNew16bit))
                    {
                        this.DataType = NewDataType;                        
                        return 0;
                    }

                    #endregion

                    #region CASE 2: 16bits to 32bits

                    if ((this.Is16bits) && (!isNew16bit))
                    {
                        tempAdd = FreeAdds.Find(x => (x.Address == (this.Address.Address+1) ));

                        if (tempAdd != null)
                        {
                            this.DataType = NewDataType;
                            return 0;
                        }
                        else return 1;
                    }

                    #endregion

                    #region CASE 3: 32bits to 32bits

                    if ((this.Is32bits) && (!isNew16bit))
                    {
                        this.DataType = NewDataType;
                        return 0;
                    }

                    #endregion

                    #region CASE 4: 32bits to 16bits

                    if ((this.Is32bits) && (isNew16bit))
                    {
                        this.DataType = NewDataType;
                        return 0;
                    }

                    #endregion
                }
                else ret = -1;
            }
            catch (Exception e) { ret = -1; }

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
            bool regMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModPoint p = obj as ModPoint;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.register != null) && (p.register != null)) regMatch = this.register == p.register;

            // Return true if the fields match:
            return regMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int regHash = 0;

            if (this.register != null) regHash = this.register.GetHashCode();

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
            ModPoint p = obj as ModPoint;
            if ((System.Object)p == null) return ret;

            if (p.register.Number > this.register.Number) ret = -1;
            else
            {
                if (p.register.Number < this.register.Number) ret = 1;
                else ret = 0;
            }

            return ret;
        }

        #endregion
    }
}
