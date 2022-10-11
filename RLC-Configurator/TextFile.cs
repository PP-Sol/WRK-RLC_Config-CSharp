using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace RLC_Configurator
{
    public static class TextFile
    {

        /// <summary>
        /// Writes a new text file. If succeeds returns true otherwise false
        /// </summary>
        /// <param name="name">Filename</param>
        /// <param name="Cont">Content of textfile</param>
        /// <param name="EncCode">A code for encoding</param>
        public static bool writeFile(string name, ArrayList Cont, int EncCode)
        {
            bool ret = true;

            try
            {
                Encoding enc = TextFile.GetEncodingByCode(EncCode);

                if (enc != null)
                {
                    using (StreamWriter sw = new StreamWriter(name, false, enc))
                    {

                        for (int i = 0; i < Cont.Count; i++)
                        {
                            sw.WriteLine(Cont[i]);
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(name, false))
                    {

                        for (int i = 0; i < Cont.Count; i++)
                        {
                            sw.WriteLine(Cont[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Writes a new text file. If succeeds returns true otherwise false
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="Cont">Content of textfile</param>
        /// <param name="EncCode">A code for encoding</param>
        public static bool writeFile(string path, List<string> Cont, int EncCode)
        {
            bool ret = true;

            if (IsValidPath(path))
            {
                try
                {
                    Encoding enc = TextFile.GetEncodingByCode(EncCode);

                    if (enc != null)
                    {
                        using (StreamWriter file = new StreamWriter(path, false, enc))
                        {
                            foreach (string line in Cont) file.WriteLine(line);
                        }
                    }
                    else
                    {
                        using (StreamWriter file = new StreamWriter(path, false))
                        {
                            foreach (string line in Cont) file.WriteLine(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    ret = false;
                }
            }
            else ret = false;

            return ret;
        }

        /// <summary>
        /// Reads a specified text file as ArrayList. If succeeds returns content otherwise null
        /// </summary>
        /// <param name="name">Filename</param>
        /// <param name="EncCode">A code for encoding</param>
        public static ArrayList readFile(string name, int EncCode)
        {
            ArrayList arrText = new ArrayList();

            try
            {
                Encoding enc = TextFile.GetEncodingByCode(EncCode);
                StreamReader objReader;

                if (enc != null)
                {
                    objReader = new StreamReader(name, enc);
                }
                else
                {
                    objReader = new StreamReader(name, true);
                }

                string sLine = "";
                while (sLine != null)
                {

                    sLine = objReader.ReadLine();
                    if (sLine != null) arrText.Add(sLine);
                }
                objReader.Close();
            }
            catch (Exception e)
            {
                arrText = null;
            }

            return arrText;
        }

        /// <summary>
        /// Reads a specified text file as list of string. If succeeds returns content otherwise null
        /// </summary>
        /// <param name="name">Filename</param>
        /// <param name="EncCode">A code for encoding</param>
        public static List<string> readFile2(string name, int EncCode)
        {
            List<string> arrText = new List<string>();

            try
            {
                Encoding enc = TextFile.GetEncodingByCode(EncCode);
                StreamReader objReader;

                if (enc != null)
                {
                    objReader = new StreamReader(name, enc);
                }
                else
                {
                    objReader = new StreamReader(name, true);
                }

                string sLine = "";
                while (sLine != null)
                {

                    sLine = objReader.ReadLine();
                    if (sLine != null) arrText.Add(sLine);
                }
                objReader.Close();
            }
            catch (Exception e)
            {
                arrText = null;
            }

            return arrText;
        }

        /// <summary>
        /// Reads a specified text file as string. If succeeds returns content otherwise null
        /// </summary>
        /// <param name="name">Filename</param>
        /// <param name="EncCode">A code for encoding</param>
        public static string readFileStr(string name, int EncCode)
        {
            ArrayList arrText = new ArrayList();
            string vrat = "";
            int i;

            try
            {
                Encoding enc = TextFile.GetEncodingByCode(EncCode);
                StreamReader objReader;

                if (enc != null)
                {
                    objReader = new StreamReader(name, enc);
                }
                else
                {
                    objReader = new StreamReader(name, true);
                }

                string sLine = "";
                while (sLine != null)
                {

                    sLine = objReader.ReadLine();
                    if (sLine != null) arrText.Add(sLine);
                }
                objReader.Close();

                for (i = 0; i < arrText.Count; i++)
                {
                    vrat = vrat + arrText[i].ToString() + "\r\n";
                }
            }
            catch (Exception e)
            {
                vrat = null;
            }

            return vrat;
        }

        /// <summary>
        /// Returns total lines of the specified text file. If error occurs then returns -1
        /// </summary>
        /// <param name="name">Filename</param>
        public static long getNumLines(string name)
        {
            long ret = 0;

            try
            {
                ret = File.ReadAllLines(name).Length;
            }
            catch (Exception e)
            {
                ret = -1;
            }

            return ret;
        }


        /// <summary>
        /// Replace restricted chararacters from specified path by safe character
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="safe">Safe character</param>
        public static string makeCorrectFName(string path, char safe)
        {
            string ret = "";
            string dir;
            string fileName;
            char[] invDirChars = Path.GetInvalidPathChars();
            char[] invFileChars = Path.GetInvalidFileNameChars();
            char toReplBy;

            path = path.Replace('/', safe);

            if (!IsValidChar(safe)) toReplBy = '-';
            else toReplBy = safe;


            if (IsValidPath(path))   // Path is valid
            {
                ret = path;
            }
            else                   // Path is invalid - we are going to replace
            {
                foreach (char elem in invDirChars) path = path.Replace(elem, toReplBy);

                dir = Path.GetDirectoryName(path);
                fileName = Path.GetFileName(path);

                foreach (char elem in invFileChars) fileName = fileName.Replace(elem, toReplBy);

                ret = Path.Combine(dir, fileName);

            }


            return ret;
        }

        /// <summary>
        /// Returns whehter specified path is valid path or no
        /// </summary>
        /// <param name="path">Path</param>
        public static bool IsValidPath(string path)
        {
            bool ret = true;
            string dir = "";
            string fileName = "";
            char[] invDirChars = Path.GetInvalidPathChars();
            char[] invFileChars = Path.GetInvalidFileNameChars();

            try
            {
                Path.GetFullPath(path);
                dir = Path.GetDirectoryName(path);
                fileName = Path.GetFileName(path);
            }
            catch (Exception)
            {
                ret = false;
            }

            try
            {
                if (ret)
                {
                    foreach (char elem in invDirChars)
                    {
                        if (dir.Contains(elem))
                        {
                            ret = false;
                            break;
                        }
                    }
                }

                if (ret)
                {
                    foreach (char elem in invFileChars)
                    {
                        if (fileName.Contains(elem))
                        {
                            ret = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Returns whether specified char is valid for file path or not
        /// </summary>
        /// <param name="chr">Char to be checked</param>
        public static bool IsValidChar(char chr)
        {
            int i;
            bool ret = true;
            char[] invDirChars = Path.GetInvalidPathChars();
            char[] invFileChars = Path.GetInvalidFileNameChars();

            for (i = 0; i < invDirChars.Length; i++)
            {
                if (chr == invDirChars[i])
                {
                    ret = false;
                    break;
                }
            }

            if (ret)
            {
                for (i = 0; i < invFileChars.Length; i++)
                {
                    if (chr == invFileChars[i])
                    {
                        ret = false;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns all invalid characters for directory delimited by delimiter
        /// </summary>
        /// <param name="delim">Delimiter</param>
        public static string PrintDirInvChars(string delim)
        {
            string ret = "";
            char[] invDirChars = Path.GetInvalidPathChars();

            foreach (char elem in invDirChars) ret = ret + elem + delim;

            return ret;
        }

        /// <summary>
        /// Returns all invalid characters for file name delimited by delimiter
        /// </summary>
        /// <param name="delim">Delimiter</param>
        public static string PrintFileInvChars(string delim)
        {
            string ret = "";
            char[] invFileChars = Path.GetInvalidFileNameChars();

            foreach (char elem in invFileChars) ret = ret + elem + delim;

            return ret;
        }

        /// <summary>
        /// Creates folder with specified path. If succedes then returns true otherwise false
        /// </summary>
        /// <param name="path">Specified path</param>
        public static bool CreateFolder(string path)
        {
            bool ret = true;
            string dir = "";

            if (IsValidPath(path))
            {
                try
                {
                    dir = Path.GetDirectoryName(path);

                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
                catch (Exception e)
                {
                    ret = false;
                }

            }
            else ret = false;

            return ret;
        }

        /// <summary>
        /// Returns encoding linked to code
        /// </summary>
        /// <param name="Code">A code for encoding</param>
        public static Encoding GetEncodingByCode(int Code)
        {
            Encoding ret = null;

            try
            {
                switch (Code)
                {
                    case 0:
                        {
                            ret = null;
                            break;
                        }
                    case 1:
                        {
                            ret = new ASCIIEncoding();
                            break;
                        }
                    case 2:
                        {
                            ret = new UTF7Encoding(true);
                            break;
                        }
                    case 3:
                        {
                            ret = new UTF8Encoding(true);
                            break;
                        }
                    case 4:
                        {
                            ret = new UnicodeEncoding(false, true);
                            break;
                        }
                    case 5:
                        {
                            ret = new UnicodeEncoding(true, true);
                            break;
                        }
                    case 6:
                        {
                            ret = new UTF32Encoding(false, true);
                            break;
                        }
                    case 7:
                        {
                            ret = new UTF32Encoding(true, true);
                            break;
                        }
                    default:
                        {
                            ret = null;
                            break;
                        }
                }
            }
            catch (Exception e) { ret = null; }

            return ret;
        }
    }
}

