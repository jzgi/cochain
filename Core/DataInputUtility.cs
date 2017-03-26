using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Greatbone.Core
{
    public static class DataInputUtility
    {
        ///
        /// Used in both client and server to parse received content into model.
        ///
        public static IDataInput ParseContent(string ctyp, byte[] buffer, int start, int count, Type typ = null)
        {
            if (string.IsNullOrEmpty(ctyp)) return null;

            if ("application/x-www-form-urlencoded".Equals(ctyp))
            {
                return new FormParse(buffer, count).Parse();
            }
            if (ctyp.StartsWith("multipart/form-data; boundary="))
            {
                return new FormMpParse(buffer, count, ctyp.Substring(30)).Parse();
            }
            if (ctyp.StartsWith("application/json"))
            {
                return new JsonParse(buffer, count).Parse();
            }
            if (ctyp.StartsWith("application/xml"))
            {
                return new XmlParse(buffer, count).Parse();
            }
            if (ctyp.StartsWith("text/plain"))
            {
                if (typ == typeof(JObj) || typ == typeof(JArr))
                {
                    return new JsonParse(buffer, count).Parse();
                }
                else if (typ == typeof(XElem))
                {
                    return new XmlParse(buffer, count).Parse();
                }
                else
                {
                    Text txt = new Text();
                    for (int i = 0; i < count; i++)
                    {
                        txt.Accept(buffer[i]);
                    }
                    return txt;
                }
            }
            return null;
        }

        public static T StringTo<T>(string v) where T : class, IDataInput
        {
            Type t = typeof(T);
            if (t == typeof(JArr) || t == typeof(JObj))
            {
                return new JsonParse(v).Parse() as T;
            }
            else if (t == typeof(XElem))
            {
                return new XmlParse(v).Parse() as T;
            }
            else if (t == typeof(Form))
            {
                return new FormParse(v).Parse() as T;
            }
            return null;
        }

        public static D StringToObject<D>(string v, int proj = 0) where D : IData, new()
        {
            JObj jo = (JObj)new JsonParse(v).Parse();
            return jo.ToObject<D>(proj);
        }

        public static D[] StringToArray<D>(string v, int proj = 0) where D : IData, new()
        {
            JArr ja = (JArr)new JsonParse(v).Parse();
            return ja.ToArray<D>(proj);
        }

        public static string ToString<D>(D v, int proj = 0) where D : IData
        {
            JsonContent cont = new JsonContent(false, true, 4 * 1024);
            cont.Put(null, v, proj);
            string str = cont.ToString();
            BufferUtility.Return(cont); // return buffer to pool
            return str;
        }

        public static string ToString<D>(D[] v, int proj = 0) where D : IData
        {
            JsonContent cont = new JsonContent(false, true, 4 * 1024);
            cont.Put(null, v, proj);
            string str = cont.ToString();
            BufferUtility.Return(cont); // return buffer to pool
            return str;
        }

        public static string ToString<D>(List<D> v, int proj = 0) where D : IData
        {
            JsonContent cont = new JsonContent(false, true, 4 * 1024);
            cont.Put(null, v, proj);
            string str = cont.ToString();
            BufferUtility.Return(cont); // return buffer to pool
            return str;
        }

        public static T FileTo<T>(string file) where T : class, IDataInput, new()
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);

                Type t = typeof(T);
                if (t == typeof(JArr) || t == typeof(JObj))
                {
                    return new JsonParse(bytes, bytes.Length).Parse() as T;
                }
                else if (t == typeof(XElem))
                {
                    return new XmlParse(bytes, bytes.Length).Parse() as T;
                }
                else if (t == typeof(Form))
                {
                    return new FormParse(bytes, bytes.Length).Parse() as T;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public static D FileToObject<D>(string file, int proj = 0) where D : IData, new()
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);
                JObj jo = (JObj)new JsonParse(bytes, bytes.Length).Parse();
                if (jo != null)
                {
                    return jo.ToObject<D>(proj);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return default(D);
        }

        public static D[] FileToArray<D>(string file, int proj = 0) where D : IData, new()
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);
                JArr ja = (JArr)new JsonParse(bytes, bytes.Length).Parse();
                if (ja != null)
                {
                    return ja.ToArray<D>(proj);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public static List<D> FileToList<D>(string file, int proj = 0) where D : IData, new()
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);
                JArr ja = (JArr)new JsonParse(bytes, bytes.Length).Parse();
                if (ja != null)
                {
                    return ja.ToList<D>(proj);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }
    }
}