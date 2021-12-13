using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor.Directory
{
    public class utils
    {

        static public IEnumerable<FileInfo> GetFileList(string path, string filter)
        {

            IEnumerable<FileInfo> result = System.IO.Directory.GetFiles(path, filter).ToList().Select(r => new FileInfo(r));

            return result;
        }


        public static bool IsFileReady(String path)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

    public class uData {
        private string _label;
        private object _value;



        public string Label { get => _label; set => _label = value; }
        public object Value { get => _value; set => this._value = value; }

        public uData() { 
        }

        public uData(string label, object value)
        {
            _label = label;
            _value = value;
        }

    }
}
