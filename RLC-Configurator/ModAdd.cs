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
    public class ModAdd:IComparable
    {

        #region Objects: Enumerators

        public enum ModLoc
        {
            Coils,
            Inputs,
            InputRegisters,
            HoldingRegisters,
        }

        #endregion
        
        #region Objects: Mandatory Items

        private int address;
        private int bitMask;
        private ModLoc location;
        
        #endregion


        #region Public Properties: General

        public int Address
        {
            get { return this.address; }
            set { this.address = value; }
        }
        public string AddressStr
        {
            get
            {
                string ret = "";
                string maxStr = this.MaxAddStr;
                string addStr = this.address.ToString();
                int i;

                if( (maxStr != null) && (addStr != null) )
                {
                    int diff = maxStr.Length - addStr.Length;

                    if (diff > 0)
                    {
                        ret = addStr;

                        for (i = 0; i < diff; i++)
                        {
                            ret = "0" + ret;
                        }
                    }
                    else ret = addStr;
                }

                return ret;
            }
        }

        public int AddressLoc
        {
            get { return (this.address+1); }
        }
        public string AddressLocStr
        {
            get
            {
                string ret = "";
                string maxStr = this.MaxAddStr;
                string addStr = this.AddressLoc.ToString();
                int i;

                if ((maxStr != null) && (addStr != null))
                {
                    int diff = maxStr.Length - addStr.Length;

                    if (diff > 0)
                    {
                        ret = addStr;

                        for (i = 0; i < diff; i++)
                        {
                            ret = "0" + ret;
                        }
                    }
                    else ret = addStr;
                }

                return ret;
            }
        }

        public string BitMask
        {
            get
            {
                string ret = "";

                if (this.IsValidBit)
                {
                    foreach (var elem in this.Bits)
                    {
                        if (elem) ret = ret + AppSett.Default.App_Symbol_True;
                        else ret = ret + AppSett.Default.App_Symbol_False;
                    }
                }

                return ret;
            }
        }
        public string BitMaskInv
        {
            get
            {
                string ret = "";

                if (this.IsValidBit)
                {
                    foreach (var elem in this.Bits)
                    {
                        if (elem) ret = ret + AppSett.Default.App_Symbol_False;
                        else ret = ret + AppSett.Default.App_Symbol_True;
                    }
                }

                return ret;
            }
        }
        public string BitMaskAllBits
        {
            get
            {
                string ret = "";
                int i;

                for (i = MinBit; i < (MaxBit + 1); i++)
                {
                    ret = ret + AppSett.Default.App_Symbol_True;
                }
                
                return ret;
            }
        }

        public ModLoc Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        public int MinAdd
        {
            get { return AppSett.Default.RLC_ModAdd_Min; }
        }
        public int MaxAdd
        {
            get { return AppSett.Default.RLC_ModAdd_Max; }
        }
        public string MaxAddStr
        {
            get
            {
                string ret = "";

                ret = this.MaxAdd.ToString();

                return ret;
            }
        }

        public int MinBit
        {
            get { return AppSett.Default.RLC_ModAddBit_Min; }
        }
        public int MaxBit
        {
            get { return AppSett.Default.RLC_ModAddBit_Max; }
        }

        public bool Bit0
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 0));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit1
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 1));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit2
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 2));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit3
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 3));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit4
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 4));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit5
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 5));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit6
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 6));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit7
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 7));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit8
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 8));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit9
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 9));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit10
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 10));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit11
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 11));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit12
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 12));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit13
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 13));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit14
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 14));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }
        public bool Bit15
        {
            get
            {
                bool ret = false;
                int mask = Convert.ToInt32(Math.Pow(2, 15));

                if (this.IsValidBit)
                {
                    if ((this.bitMask & mask) == mask) ret = true;
                }

                return ret;
            }
        }

        public List<bool> Bits
        {
            get
            {
                List<bool> ret = new List<bool>();

                if (this.IsValidBit)
                {
                    ret.Add(this.Bit0);
                    ret.Add(this.Bit1);
                    ret.Add(this.Bit2);
                    ret.Add(this.Bit3);
                    ret.Add(this.Bit4);
                    ret.Add(this.Bit5);
                    ret.Add(this.Bit6);
                    ret.Add(this.Bit7);
                    ret.Add(this.Bit8);
                    ret.Add(this.Bit9);
                    ret.Add(this.Bit10);
                    ret.Add(this.Bit11);
                    ret.Add(this.Bit12);
                    ret.Add(this.Bit13);
                    ret.Add(this.Bit14);
                    ret.Add(this.Bit15);
                }

                return ret;
            }
        }

        public int FirstSetBit
        {
            get
            {
                int ret = -1;

                if (!this.AllBitsUsed)
                {
                    int i;

                    for (i = 0; i < this.Bits.Count; i++)
                    {
                        if (this.Bits[i])
                        {
                            ret = i;
                            break;
                        }
                    }
                }

                return ret;
            }
        }

        public string LocCode
        {
            get
            {
                string ret = "";

                if (this.location == ModLoc.Coils) ret = AppSett.Default.RLC_Code_Coils;
                if (this.location == ModLoc.Inputs) ret = AppSett.Default.RLC_Code_Inputs;
                if (this.location == ModLoc.InputRegisters) ret = AppSett.Default.RLC_Code_IR;
                if (this.location == ModLoc.HoldingRegisters) ret = AppSett.Default.RLC_Code_HR;

                return ret;
            }
        }
        public string LocPrefix
        {
            get
            {
                string ret = "";

                if (this.location == ModLoc.Coils) ret = AppSett.Default.RLC_Prefix_Coils;
                if (this.location == ModLoc.Inputs) ret = AppSett.Default.RLC_Prefix_Inputs;
                if (this.location == ModLoc.InputRegisters) ret = AppSett.Default.RLC_Prefix_IR;
                if (this.location == ModLoc.HoldingRegisters) ret = AppSett.Default.RLC_Prefix_HR;

                return ret;
            }
        }
        public string LocAdd
        {
            get
            {
                string ret = "";

                if (this.LocCode != null) ret = ret + this.LocCode;
                if (this.LocPrefix != null) ret = ret + this.LocPrefix;
                if (this.AddressLocStr != null) ret = ret + this.AddressLocStr;

                return ret;
            }
        }
        public string LocComp
        {
            get
            {
                string ret = "";

                if (this.LocAdd != null) ret = ret + this.LocAdd;

                ret = ret + " ";

                if (this.TextBits != null) ret = ret + this.TextBits;

                return ret;
            }
        }
        public string LocComb
        {
            get
            {
                string ret = "";

                if ((!this.AllBitsUsed) && (this.FirstSetBit >= 0) && this.AreBitsAvail) ret = this.LocAdd + "." + this.FirstSetBit.ToString();
                else ret = this.LocAdd;

                return ret;
            }
        }

        public string TextAdd
        {
            get
            {
                string ret = "";

                ret = this.AddressStr;

                return ret;
            }
        }
        public string TextBits
        {
            get
            {
                string ret = "";

                if (this.AreBitsAvail)
                {
                    ret = ret + "[";

                    ret = ret + this.BitMask;

                    ret = ret + "]"; 
                }

                return ret;
            }
        }
        public string TextComp
        {
            get
            {
                string ret = "";

                ret = this.TextAdd + " " + this.TextBits;

                return ret;
            }
        }
        public string TextComb
        {
            get
            {
                string ret = "";

                if ((!this.AllBitsUsed) && (this.FirstSetBit >= 0) && this.AreBitsAvail) ret = this.TextAdd + "." + this.FirstSetBit.ToString();
                else ret = this.TextAdd;

                return ret;
            }
        }

        public int NumOfSetBits
        {
            get
            {
                int ret = -1;

                if (this.IsValid)
                {
                    ret = 0;

                    foreach (var elem in this.Bits)
                    {
                        if (elem) ret++;
                    }
                }

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsValidAdd
        {
            get
            {
                bool ret = false;

                int min = this.MinAdd;
                int max = this.MaxAdd;

                ret = (this.address >= min) && (this.address <= max);

                return ret;
            }
        }
        public bool IsValidBit
        {
            get
            {
                bool ret = false;

                double min = AppSett.Default.RLC_BitMask_Min;
                double max = Math.Pow(2,(this.MaxBit+1));

                ret = (this.bitMask >= min) && (this.bitMask <= max);

                return ret;
            }
        }
        public bool IsValid
        {
            get
            {
                bool ret = false;

                ret = this.IsValidAdd && this.IsValidBit;

                return ret;
            }
        }

        public bool AllBitsUsed
        {
            get
            {
                bool ret = false;

                string actMask = this.BitMask;
                string allMask = this.BitMaskAllBits;

                if( (actMask != null) && (allMask != null) )
                {
                    ret = String.Compare(actMask,allMask) == 0;
                }

                return ret;
            }
        }

        public bool IsAddMax
        {
            get
            {
                bool ret = false;

                ret = (this.address == this.MaxAdd);

                return ret;
            }
        }

        public bool AreBitsAvail
        {
            get
            {
                bool ret = false;

                ret = (this.location == ModLoc.HoldingRegisters) || (this.location == ModLoc.InputRegisters);

                return ret;
            }
        }

        public bool IsWritePos
        {
            get
            {
                bool ret = false;

                ret = (this.location == ModLoc.HoldingRegisters) || (this.location == ModLoc.Coils);

                return ret;
            }
        }

        public bool IsBinaryType
        {
            get
            {
                bool ret = false;

                if (this.IsValid)
                {
                    if ((this.location == ModLoc.Coils) || (this.location == ModLoc.Inputs)) ret = true;
                    else ret = this.NumOfSetBits == 1;
                }

                return ret;
            }
        }
        public bool IsAnalogType
        {
            get
            {
                bool ret = false;

                if (this.IsValid)
                {
                    if ((this.location == ModLoc.Coils) || (this.location == ModLoc.Inputs)) ret = false;
                    else ret = this.AllBitsUsed;
                }

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors

        public ModAdd(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempLocation = null;

                this.address = reader.ReadInt32();
                this.bitMask = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) tempLocation = reader.ReadString();

                if (tempLocation != null)
                {
                    if (String.Compare(tempLocation, ModLoc.Coils.ToString()) == 0) this.location = ModLoc.Coils;
                    if (String.Compare(tempLocation, ModLoc.HoldingRegisters.ToString()) == 0) this.location= ModLoc.HoldingRegisters;
                    if (String.Compare(tempLocation, ModLoc.InputRegisters.ToString()) == 0) this.location = ModLoc.InputRegisters;
                    if (String.Compare(tempLocation, ModLoc.Inputs.ToString()) == 0) this.location = ModLoc.Inputs;
                }
                else this.location = ModLoc.InputRegisters;
            }
        }

        public ModAdd(int Address, ModLoc Location)
        {
            Exception ex = new Exception("Invalid data!");

            this.address = Address;

            this.bitMask = AppSett.Default.RLC_BitMask_Max;

            this.location = Location;

            if (!this.IsValid) throw ex;
        }

        public ModAdd(int Address, string BitMask, ModLoc Location)
        {
            Exception ex = new Exception("Invalid data!");
            bool res = false;

            this.address = Address;

            this.location = Location;

            if (this.AreBitsAvail)
            {
                this.bitMask = AppSett.Default.RLC_BitMask_Min;
                res = UpdateBitMask(BitMask, false);
            }
            else
            {
                this.bitMask = AppSett.Default.RLC_BitMask_Max;
                res = true;
            }

            if ((!res) || (!this.IsValid)) throw ex;
        }

        public ModAdd(int Address, List<bool> Bits, ModLoc Location)
        {
            Exception ex = new Exception("Invalid data!");
            bool res = false;
            
            if (Bits != null) throw ex;

            this.address = Address;

            this.location = Location;

            if (this.AreBitsAvail)
            {
                this.bitMask = AppSett.Default.RLC_BitMask_Min;
                res = UpdateBitMask(Bits, false);
            }
            else
            {
                this.bitMask = AppSett.Default.RLC_BitMask_Max;
                res = true;
            }

            if ((!res) || (!this.IsValid)) throw ex;
        }

        public ModAdd(int Address, int SetBit, ModLoc Location)
        {
            Exception ex = new Exception("Invalid data!");

            int mask = Convert.ToInt32(Math.Pow(2, SetBit));

            this.address = Address;

            this.location = Location;

            if(this.AreBitsAvail) this.bitMask = AppSett.Default.RLC_BitMask_Min | mask;
            else this.bitMask = AppSett.Default.RLC_BitMask_Max;

            if (!this.IsValid) throw ex;
        }

        #endregion


        #region Methods: Overrides and Implemented Interfaces

        /// <summary>
        /// Returns whether specified object equals this object or not
        /// </summary>
        /// <param name="obj">Object for equality checking</param>
        public override bool Equals(object obj)
        {
            bool addMatch = false;
            bool bitsMatch = false;
            bool locMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModAdd p = obj as ModAdd;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.address != null) && (p.address != null)) addMatch = this.address == p.address;
            if ((this.bitMask != null) && (p.bitMask != null)) bitsMatch = this.bitMask == p.bitMask;
            if ((this.location != null) && (p.location!= null)) locMatch = this.location == p.location;

            // Return true if the fields match:
            return addMatch && locMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int addHash = 0;
            int bitHash = 0;
            int locHash = 0;

            addHash = this.address.GetHashCode();
            bitHash = this.bitMask.GetHashCode();
            locHash = this.location.GetHashCode();

            return (addHash ^ locHash);
        }

        /// <summary>
        /// Convert the parameter to specified string format
        /// </summary>
        public override string ToString()
        {
            string ret = "";

            if (this.LocComp!= null) ret = this.LocComp;
            
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
            ModAdd p = obj as ModAdd;
            if ((System.Object)p == null) return ret;

            if (p.address > this.address) ret = -1;
            else
            {
                if (p.address < this.address) ret = 1;
                else ret = 0;
            }

            return ret;
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

                // Address
                writer.Write(this.address);

                // BitMask
                writer.Write(this.bitMask);

                // Location
                if (this.location!= null)
                {
                    writer.Write(true);
                    writer.Write(this.location.ToString());
                }
                else writer.Write(false);

                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Updates bit mask
        /// </summary>
        /// <param name="BitMask">A bit mask</param>
        public bool UpdateBitMask(string BitMask, bool LeaveOld)
        {
            bool ret = false;

            try
            {
                if (BitMask != null)
                {
                    int len = BitMask.Length;
                    int i;
                    bool valid;
                    bool isTrue;
                    bool isFalse;
                    int mask = 0;

                    if (len == 16)
                    {
                        if(!LeaveOld) this.bitMask = 0;

                        for (i = 0; i < len; i++)
                        {
                            isTrue  = (String.Compare(BitMask[i].ToString(), AppSett.Default.App_Symbol_True) == 0);
                            isFalse = (String.Compare(BitMask[i].ToString(), AppSett.Default.App_Symbol_False) == 0);

                            valid = isTrue || isFalse;

                            if (!valid) return false;

                            if (isTrue)
                            {
                                mask = Convert.ToInt32(Math.Pow(2,i));
                                this.bitMask = this.bitMask | mask;
                            }
                        }

                        ret = true;
                    }
                    else return false;
                }
            }
            catch (Exception Ex) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Updates bit mask
        /// </summary>
        /// <param name="Bits">A bits</param>
        public bool UpdateBitMask(List<bool> Bits, bool LeaveOld)
        {
            bool ret = false;

            try
            {
                if (Bits != null)
                {
                    int len = Bits.Count;
                    int i;
                    int mask = 0;

                    if (len == 16)
                    {
                        if (!LeaveOld) this.bitMask = 0;

                        for (i = 0; i < len; i++)
                        {

                            if (Bits[i])
                            {
                                mask = Convert.ToInt32(Math.Pow(2, i));
                                this.bitMask = this.bitMask | mask;
                            }
                        }

                        ret = true;
                    }
                    else return false;
                }
            }
            catch (Exception Ex) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Gets specified bit
        /// </summary>
        /// <param name="Bit">A specified bit</param>
        public bool IsBitSet(int Bit)
        {
            bool ret = false;

            try
            {
                if ((Bit >= 0) && (Bit <= 15)) ret = this.Bits[Bit];
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Sets specified bit
        /// </summary>
        /// <param name="Bit">A specified bit</param>
        /// <param name="LeaveOld">A specifier whether to reset other bits or not</param>
        public bool SetBit(int Bit, bool LeaveOld)
        {
            bool ret = false;

            try
            {
                if ((Bit >= 0) && (Bit <= 15) && this.AreBitsAvail)
                {
                    int mask = Convert.ToInt32(Math.Pow(2, Bit));
                    
                    if (LeaveOld)
                    {
                        this.bitMask = this.bitMask | mask;
                        ret = true;
                    }
                    else
                    {
                        this.bitMask = AppSett.Default.RLC_BitMask_Min | mask;
                        ret = true;
                    }
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Sets all bits
        /// </summary>
        public bool SetAllBits()
        {
            bool ret = false;

            try
            {
                if (this.IsValid)
                {
                    this.bitMask = AppSett.Default.RLC_BitMask_Max;
                    ret = true;
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        /// <summary>
        /// Sets new address
        /// </summary>
        /// <param name="Add">A new address</param>
        public bool SetAdd(int Add)
        {
            bool ret = false;

            try
            {
                int min = this.MinAdd;
                int max = this.MaxAdd;

                if (this.IsValid)
                { 
                    if( (Add >= min) && (Add <= max) )
                    {
                        this.address = Add;
                        ret = true;
                    }
                }
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        #endregion

    }
}
