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
    public class ModReg:IComparable
    {
        
        #region Objects: Enumerators

        public enum ModRegType
        {
            DTYPE,
            ITYPE,
            FTYPE,
            STYPE,
        }

        #endregion

        #region Objects: Mandatory Items

        private int number;
        private ModRegType type;

        #endregion


        #region Public Properties: General

        public int Number
        {
            get { return this.number; }
            set { this.number = value; }
        }
        public string NumberStr
        {
            get
            {
                string ret = "";
                int i;

                long diff = AppSett.Default.ModSig_Reg_Length - this.number.ToString().Length;

                if (diff > 0)
                {
                    ret = this.number.ToString();

                    for (i = 0; i < diff; i++)
                    {
                        ret = "0" + ret;
                    }
                }
                else ret = this.number.ToString();

                return ret;
            }
        }
        public ModRegType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public int MinAbsAdd
        {
            get
            {
                int ret = 0;

                ret = AppSett.Default.RLC_Registers_Min;

                return ret;
            }
        }
        public int MaxAbsAdd
        {
            get
            {
                int ret = 0;

                ret = AppSett.Default.RLC_Registers_Max;

                return ret;
            }
        }
        public int MinCurrAdd
        {
            get
            {
                int ret = 0;

                ret = this.MinAbsAdd;

                return ret;
            }
        }
        public int MaxCurrAdd
        {
            get
            {
                int ret = 0;

                ret = this.MaxAbsAdd;

                if (this.type == ModRegType.FTYPE) ret = ret - 1;
                if (this.type == ModRegType.STYPE) ret = ret - 2;

                return ret;
            }
        }

        public string RegTypeChar
        {
            get
            {
                string ret = "";

                if (this.type == ModRegType.DTYPE) ret = AppSett.Default.ModSig_Char_DTYPE;
                if (this.type == ModRegType.FTYPE) ret = AppSett.Default.ModSig_Char_FTYPE;
                if (this.type == ModRegType.ITYPE) ret = AppSett.Default.ModSig_Char_ITYPE;
                if (this.type == ModRegType.STYPE) ret = AppSett.Default.ModSig_Char_STYPE;

                return ret;
            }
        }

        public string TextComp
        {
            get
            {
                string ret = "";

                if (this.IsValid)
                {
                    ret = this.RegTypeChar + this.NumberStr;
                }

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
                    ret.Add(this.number);

                    if ((this.type == ModRegType.FTYPE) && (this.number <= this.MaxCurrAdd)) ret.Add(this.number + 1);
                    if ((this.type == ModRegType.STYPE) && (this.number <= this.MaxCurrAdd))
                    {
                        ret.Add(this.number + 1);
                        ret.Add(this.number + 2);
                    }
                }

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsValid
        {
            get
            {
                bool ret = false;

                int min = this.MinCurrAdd;
                int max = this.MaxCurrAdd;

                ret = (this.number >= min) && (this.number <= max);

                return ret;
            }
        }

        public bool IsAtMax
        {
            get
            {
                bool ret = false;

                ret = this.number == MaxCurrAdd;

                return ret;
            }
        }

        public bool IsAvailToFTYPE
        {
            get
            {
                bool ret = false;

                if (this.IsValid)
                {
                    if (this.type == ModRegType.DTYPE)
                    {
                        ret = ((this.MaxAbsAdd - this.Number) >= 1);
                    }

                    if (this.type == ModRegType.ITYPE)
                    {
                        ret = ((this.MaxAbsAdd - this.Number) >= 1);
                    }

                    if (this.type == ModRegType.FTYPE)
                    {
                        ret = true;
                    }

                    if (this.type == ModRegType.STYPE)
                    {
                        ret = true;
                    }
                }

                return ret;
            }
        }
        public bool IsAvailToSTYPE
        {
            get
            {
                bool ret = false;

                if (this.IsValid)
                {
                    if (this.type == ModRegType.DTYPE)
                    {
                        ret = ((this.MaxAbsAdd - this.Number) >= 2);
                    }

                    if (this.type == ModRegType.ITYPE)
                    {
                        ret = ((this.MaxAbsAdd - this.Number) >= 2);
                    }

                    if (this.type == ModRegType.FTYPE)
                    {
                        ret = ((this.MaxAbsAdd - this.Number) >= 1);
                    }

                    if (this.type == ModRegType.STYPE)
                    {
                        ret = true;
                    }
                }

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors

        public ModReg(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempType = null;

                this.number = reader.ReadInt32();

                temp = reader.ReadBoolean();
                if (temp) tempType = reader.ReadString();

                if (tempType != null)
                {
                    if (String.Compare(tempType, ModRegType.DTYPE.ToString()) == 0) this.type = ModRegType.DTYPE;
                    if (String.Compare(tempType, ModRegType.ITYPE.ToString()) == 0) this.type = ModRegType.ITYPE;
                    if (String.Compare(tempType, ModRegType.FTYPE.ToString()) == 0) this.type = ModRegType.FTYPE;
                    if (String.Compare(tempType, ModRegType.STYPE.ToString()) == 0) this.type = ModRegType.STYPE;
                }
                else this.type = ModRegType.ITYPE;
            }
        }

        public ModReg(int Number, ModRegType Type)
        {
            Exception ex = new Exception("Invalid data!");

            this.number = Number;
            this.type = Type;

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
            bool numMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModReg p = obj as ModReg;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.number != null) && (p.number != null)) numMatch = this.number == p.number;

            // Return true if the fields match:
            return numMatch;
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int numHash = 0;

            numHash = this.number.GetHashCode();

            return (numHash);
        }

        /// <summary>
        /// Convert the parameter to specified string format
        /// </summary>
        public override string ToString()
        {
            string ret = "";

            if (this.TextComp != null) ret = this.TextComp;

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
            ModReg p = obj as ModReg;
            if ((System.Object)p == null) return ret;

            if (p.number > this.number) ret = -1;
            else
            {
                if (p.number < this.number) ret = 1;
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

                // Number
                writer.Write(this.number);

                // Type
                if (this.type != null)
                {
                    writer.Write(true);
                    writer.Write(this.type.ToString());
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
