using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.IO;

namespace RLC_Configurator
{
    [Serializable]
    public class Param : ISerializable
    {

        #region Objects: General

        string name;
        string val;
        string delim;
        bool quatMarks;
        string type;

        #endregion


        #region Public Properties: General
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public string Val
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
            }
        }
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        public string Delim
        {
            get
            {
                return delim;
            }
            set
            {
                delim = value;
            }
        }
        public bool QuatMarks
        {
            get
            {
                return quatMarks;
            }
            set
            {
                quatMarks = value;
            }
        }
        #endregion


        #region Methods: Constructors

        /// <summary>
        /// Creates parameter with specified name and value
        /// </summary>
        /// <param name="Name">Name of the parameter</param>
        /// <param name="Val">Value of the parameter</param>
        public Param(string Name, string Val, string Type)
        {
            this.name = Name;
            this.val = Val;
            this.delim = "=";
            this.quatMarks = true;
            this.type = Type;
        }

        /// <summary>
        /// Creates parameter with specified name and value
        /// </summary>
        /// <param name="Name">Name of the parameter</param>
        /// <param name="Val">Value of the parameter</param>
        /// <param name="Delim">Delimiter of the parameter</param>
        public Param(string Name, string Val, string Type, string Delim)
        {
            this.name = Name;
            this.val = Val;
            this.delim = Delim;
            this.quatMarks = true;
            this.type = Type;
            //this.lineNum = LineNum;
        }

        /// <summary>
        /// Creates parameter with specified name and value
        /// </summary>
        /// <param name="Name">Name of the parameter</param>
        /// <param name="Val">Value of the parameter</param>
        /// <param name="Delim">Delimiter of the parameter</param>
        /// <param name="QuatMarks">Indicates whether the value should be inside quation marks or not</param>
        public Param(string Name, string Val, string Type, string Delim, bool QuatMarks)
        {
            this.name = Name;
            this.val = Val;
            this.delim = Delim;
            this.quatMarks = QuatMarks;
            this.type = Type;
        }

        protected Param(SerializationInfo info, StreamingContext context)
        {
            this.name = info.GetString("ObjName");
            this.val = info.GetString("ObjValue");
            this.delim = info.GetString("ObjDelim");
            this.quatMarks = info.GetBoolean("ObjQuat");
            this.type = info.GetString("ObjType");
        }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]

        public Param(byte[] bytes)
        {
            using (MemoryStream m = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    bool temp = false;

                    temp = reader.ReadBoolean();
                    if (temp) this.name = reader.ReadString();

                    temp = reader.ReadBoolean();
                    if (temp) this.val = reader.ReadString();

                    temp = reader.ReadBoolean();
                    if (temp) this.delim = reader.ReadString();

                    temp = reader.ReadBoolean();
                    if (temp) this.type = reader.ReadString();

                    this.quatMarks = reader.ReadBoolean();
                }
            }
        }

        public Param(ref BinaryReader reader)
        {
            if (reader != null)
            {
                bool temp = false;

                temp = reader.ReadBoolean();
                if (temp) this.name = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.val = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.delim = reader.ReadString();

                temp = reader.ReadBoolean();
                if (temp) this.type = reader.ReadString();

                this.quatMarks = reader.ReadBoolean();
            }
        }

        #endregion

        #region Methods: Overrides and Implemented Interfaces

        /// <summary>
        /// Convert the parameter to specified string format
        /// </summary>
        public override string ToString()
        {
            string vrat = "";

            if (this.quatMarks)
            {
                vrat = this.name + this.delim + "\"" + this.val + "\"";
            }
            else
            {
                vrat = this.name + this.delim + this.val;
            }

            return vrat;
        }

        /// <summary>
        /// Returns whether specified object equals this object or not
        /// </summary>
        /// <param name="obj">Object for equality checking</param>
        public override bool Equals(System.Object obj)
        {
            bool nameMatch = false;
            bool typeMatch = false;
            //bool lineNumMatch = false;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Param p = obj as Param;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Field matches
            if ((this.name != null) && (p.name != null)) nameMatch = String.Compare(this.name, p.name, true) == 0;
            if ((this.type != null) && (p.type != null)) typeMatch = String.Compare(this.type, p.type, true) == 0;
            //lineNumMatch = p.lineNum == this.lineNum;

            // Return true if the fields match:
            return ((nameMatch) && (typeMatch));
        }

        /// <summary>
        /// Returns hash code of the line interval
        /// </summary>
        public override int GetHashCode()
        {
            int nameHash = 0;
            int typeHash = 0;

            if (this.name != null) nameHash = this.name.GetHashCode();
            if (this.type != null) typeHash = this.type.GetHashCode();

            return (nameHash ^ typeHash);
        }

        /// <summary>
        /// Gets object's data during serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="obj">Streaming context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ObjName", this.name);
            info.AddValue("ObjValue", this.val);
            info.AddValue("ObjDelim", this.delim);
            info.AddValue("ObjQuat", this.quatMarks);
            info.AddValue("ObjType", this.type);
        }

        #endregion

        #region Methods: General

        /// <summary>
        /// Converts the object to array of bytes
        /// </summary>
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    if (this.name != null)
                    {
                        writer.Write(true);
                        writer.Write(this.name);
                    }
                    else writer.Write(false);

                    if (this.val != null)
                    {
                        writer.Write(true);
                        writer.Write(this.val);
                    }
                    else writer.Write(false);

                    if (this.delim != null)
                    {
                        writer.Write(true);
                        writer.Write(this.delim);
                    }
                    else writer.Write(false);

                    if (this.type != null)
                    {
                        writer.Write(true);
                        writer.Write(this.type);
                    }
                    else writer.Write(false);

                    writer.Write(this.quatMarks);
                }
                return m.ToArray();
            }
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
                if (this.name != null)
                {
                    writer.Write(true);
                    writer.Write(this.name);
                }
                else writer.Write(false);

                if (this.val != null)
                {
                    writer.Write(true);
                    writer.Write(this.val);
                }
                else writer.Write(false);

                if (this.delim != null)
                {
                    writer.Write(true);
                    writer.Write(this.delim);
                }
                else writer.Write(false);

                if (this.type != null)
                {
                    writer.Write(true);
                    writer.Write(this.type);
                }
                else writer.Write(false);

                writer.Write(this.quatMarks);

                ret = true;
            }
            catch (Exception e) { ret = false; }

            return ret;
        }

        #endregion

    }
}
