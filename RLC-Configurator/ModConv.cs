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
    public class ModConv
    {
        
        #region Objects: Enumerators

        public enum ModConvType
        {
            NoConv,
            Linear,
            FifthOrdPoly,
            SquareRoot,
            Exp,
            SquareRootFifthPoly,
        }

        #endregion

        #region Objects: Mandatory Items

        private ModConvType type;
        private double coeff1;
        private double coeff2;
        private double coeff3;
        private double coeff4;
        private double coeff5;
        private double coeff6;

        #endregion


        #region Public Properties: General

        public ModConvType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        public double Coeff1
        {
            get { return this.coeff1; }
            set { this.coeff1 = value; }
        }
        public double Coeff2
        {
            get { return this.coeff2; }
            set { this.coeff2 = value; }
        }
        public double Coeff3
        {
            get { return this.coeff3; }
            set { this.coeff3 = value; }
        }
        public double Coeff4
        {
            get { return this.coeff4; }
            set { this.coeff4 = value; }
        }
        public double Coeff5
        {
            get { return this.coeff5; }
            set { this.coeff5 = value; }
        }
        public double Coeff6
        {
            get { return this.coeff6; }
            set { this.coeff6 = value; }
        }

        public int ConvNum
        {
            get
            {
                int ret = 0;

                if (this.type == ModConvType.NoConv) ret = 0;
                if (this.type == ModConvType.Linear) ret = 1;
                if (this.type == ModConvType.FifthOrdPoly) ret = 2;
                if (this.type == ModConvType.SquareRoot) ret = 3;
                if (this.type == ModConvType.Exp) ret = 4;
                if (this.type == ModConvType.SquareRootFifthPoly) ret = 5;

                return ret;
            }
        }

        public string TextConvType
        {
            get
            {
                string ret = "";

                ret = ret + AppSett.Default.ModSig_ConvType_Text + " ";
                ret = ret + this.ConvNum.ToString();

                return ret;
            }
        }
        public string TextConv1
        {
            get
            {
                string ret = "";

                if (this.IsConv1Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C0_Text + "=";
                    ret = ret + this.coeff1.ToString("F2", CultureInfo.InvariantCulture);
                    
                }

                return ret;
            }
        }
        public string TextConv2
        {
            get
            {
                string ret = "";

                if (this.IsConv2Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C2_Text + "=";
                    ret = ret + this.coeff2.ToString("F2", CultureInfo.InvariantCulture);
                }

                return ret;
            }
        }
        public string TextConv3
        {
            get
            {
                string ret = "";

                if (this.IsConv3Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C3_Text + "=";
                    ret = ret + this.coeff3.ToString("F2", CultureInfo.InvariantCulture);
                }

                return ret;
            }
        }
        public string TextConv4
        {
            get
            {
                string ret = "";

                if (this.IsConv4Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C4_Text + "=";
                    ret = ret + this.coeff4.ToString("F2", CultureInfo.InvariantCulture);
                }

                return ret;
            }
        }
        public string TextConv5
        {
            get
            {
                string ret = "";

                if (this.IsConv5Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C5_Text + "=";
                    ret = ret + this.coeff5.ToString("F2", CultureInfo.InvariantCulture);
                }

                return ret;
            }
        }
        public string TextConv6
        {
            get
            {
                string ret = "";

                if (this.IsConv6Pos)
                {
                    ret = ret + AppSett.Default.ModSig_C6_Text + "=";
                    ret = ret + this.coeff6.ToString("F2", CultureInfo.InvariantCulture);
                }

                return ret;
            }
        }
        public string TextComp
        {
            get
            {
                string ret = "";

                if (this.TextConvType != null) ret = ret + this.TextConvType;

                if ((this.TextConv1 != null) && (this.IsConv1Pos)) ret = ret + " " + this.TextConv1;
                if ((this.TextConv2 != null) && (this.IsConv2Pos)) ret = ret + " " + this.TextConv2;
                if ((this.TextConv3 != null) && (this.IsConv3Pos)) ret = ret + " " + this.TextConv3;
                if ((this.TextConv4 != null) && (this.IsConv4Pos)) ret = ret + " " + this.TextConv4;
                if ((this.TextConv5 != null) && (this.IsConv5Pos)) ret = ret + " " + this.TextConv5;
                if ((this.TextConv6 != null) && (this.IsConv6Pos)) ret = ret + " " + this.TextConv6;

                return ret;
            }
        }

        #endregion

        #region Public Properties: States

        public bool IsConv1Pos
        {
            get
            {
                bool ret = false;

                ret = this.ConvNum > 0;

                return ret;
            }
        }
        public bool IsConv2Pos
        {
            get
            {
                bool ret = false;

                ret = this.ConvNum > 0;

                return ret;
            }
        }
        public bool IsConv3Pos
        {
            get
            {
                bool ret = false;

                ret = this.ConvNum > 1;

                return ret;
            }
        }
        public bool IsConv4Pos
        {
            get
            {
                bool ret = false;

                ret = (this.ConvNum == 2) || (this.ConvNum == 5);

                return ret;
            }
        }
        public bool IsConv5Pos
        {
            get
            {
                bool ret = false;

                ret = (this.ConvNum == 2) || (this.ConvNum == 5);

                return ret;
            }
        }
        public bool IsConv6Pos
        {
            get
            {
                bool ret = false;

                ret = (this.ConvNum == 2) || (this.ConvNum == 5);

                return ret;
            }
        }

        #endregion


        #region Methods: Constructors
        
        public ModConv(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;
                byte[] tempBytes;
                int bytesNum = 0;
                int objNum = 0;
                int i;
                string tempType = null;

                this.coeff1 = reader.ReadDouble();
                this.coeff2 = reader.ReadDouble();
                this.coeff3 = reader.ReadDouble();
                this.coeff4 = reader.ReadDouble();
                this.coeff5 = reader.ReadDouble();
                this.coeff6 = reader.ReadDouble();

                temp = reader.ReadBoolean();
                if (temp) tempType = reader.ReadString();

                if (tempType != null)
                {
                    if (String.Compare(tempType, ModConvType.NoConv.ToString()) == 0) this.type = ModConvType.NoConv;
                    if (String.Compare(tempType, ModConvType.Exp.ToString()) == 0) this.type = ModConvType.Exp;
                    if (String.Compare(tempType, ModConvType.FifthOrdPoly.ToString()) == 0) this.type = ModConvType.FifthOrdPoly;
                    if (String.Compare(tempType, ModConvType.Linear.ToString()) == 0) this.type = ModConvType.Linear;
                    if (String.Compare(tempType, ModConvType.SquareRoot.ToString()) == 0) this.type = ModConvType.SquareRoot;
                    if (String.Compare(tempType, ModConvType.SquareRootFifthPoly.ToString()) == 0) this.type = ModConvType.SquareRootFifthPoly;
                }
                else this.type = ModConvType.NoConv;
            }
        }

        public ModConv(ModConvType Type, double C1, double C2, double C3, double C4, double C5, double C6)
        {
            this.type = Type;
            this.coeff1 = C1;
            this.coeff2 = C2;
            this.coeff3 = C3;
            this.coeff4 = C4;
            this.coeff5 = C5;
            this.coeff6 = C6;
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

                // Coeff 1
                writer.Write(this.coeff1);

                // Coeff 2
                writer.Write(this.coeff2);

                // Coeff 3
                writer.Write(this.coeff3);

                // Coeff 4
                writer.Write(this.coeff4);

                // Coeff 5
                writer.Write(this.coeff5);

                // Coeff 6
                writer.Write(this.coeff6);

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

        /// <summary>
        /// Calculates output value for specified input using this conversion
        /// </summary>
        /// <param name="X">A specified input</param>
        public double Calc(float X)
        {
            double ret = 0;

            try
            {
                if (this.type == ModConvType.NoConv) ret = X;
                if (this.type == ModConvType.Linear) ret = (this.coeff1*X) + this.coeff2;
                if (this.type == ModConvType.FifthOrdPoly)
                {
                    ret = this.coeff1 + this.coeff2*X + this.coeff3*X*X + this.coeff4*X*X*X + this.coeff5*X*X*X*X + this.coeff6*X*X*X*X*X;
                }
                if (this.type == ModConvType.SquareRoot)
                {
                    ret = this.coeff1*(Math.Sqrt(X+this.coeff2)) + this.coeff3;
                }
                if (this.type == ModConvType.Exp)
                {
                    ret = this.coeff1 * (Math.Exp(X*this.coeff2)) + this.coeff3;
                }
                if (this.type == ModConvType.SquareRootFifthPoly)
                {
                    ret = Math.Sqrt(this.coeff1 + this.coeff2 * X + this.coeff3 * X * X + this.coeff4 * X * X * X + this.coeff5 * X * X * X * X + this.coeff6 * X * X * X * X * X);
                }
            }
            catch (Exception e) { ret = 0; }

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
            bool typeMatch = false;
            bool c1Match = false;
            bool c2Match = false;
            bool c3Match = false;
            bool c4Match = false;
            bool c5Match = false;
            bool c6Match = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ModConv p = obj as ModConv;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.type != null) && (p.type != null)) typeMatch = this.type == p.type;
            if ((this.coeff1 != null) && (p.coeff1 != null)) c1Match = this.coeff1 == p.coeff1;
            if ((this.coeff2 != null) && (p.coeff2 != null)) c2Match = this.coeff2 == p.coeff2;
            if ((this.coeff3 != null) && (p.coeff3 != null)) c3Match = this.coeff3 == p.coeff3;
            if ((this.coeff4 != null) && (p.coeff4 != null)) c4Match = this.coeff4 == p.coeff4;
            if ((this.coeff5 != null) && (p.coeff5 != null)) c5Match = this.coeff5 == p.coeff5;
            if ((this.coeff6 != null) && (p.coeff6 != null)) c6Match = this.coeff6 == p.coeff6;

            // Return true if the fields match:
            return (typeMatch && c1Match && c2Match && c3Match && c4Match && c5Match && c6Match);
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int typeHash = 0;
            int c1Hash = 0;
            int c2Hash = 0;
            int c3Hash = 0;
            int c4Hash = 0;
            int c5Hash = 0;
            int c6Hash = 0;

            typeHash = this.type.GetHashCode();
            c1Hash = this.coeff1.GetHashCode();
            c2Hash = this.coeff2.GetHashCode();
            c3Hash = this.coeff3.GetHashCode();
            c4Hash = this.coeff4.GetHashCode();
            c5Hash = this.coeff5.GetHashCode();
            c6Hash = this.coeff6.GetHashCode();

            return (typeHash ^ c1Hash ^ c2Hash ^ c3Hash ^ c4Hash ^ c5Hash ^ c6Hash);
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


        #endregion

    }
}
