using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian
{
    public class utils
    {
        static int FILE_EMPTY = 0;

        static public IEnumerable<FileInfo> GetFileList(string path, string filter)
        {
            DirectoryInfo d = new DirectoryInfo(path);

            var result = d.GetFiles("*."+ filter).ToList();

           // IEnumerable<FileInfo> result = Directory.Exists(path) ? System.IO.Directory.GetFiles(path, filter).Select(r => new FileInfo(r)) : null;

            //IEnumerable <FileInfo> result = System.IO.Directory.GetFiles(path, filter).Select(r => new FileInfo(r));

            return result;
        }


        public static bool FileIsEmpty(String path)
        {
            var file = new FileInfo(path);

            return (file.Length == FILE_EMPTY);
        }


        public static bool IsFileReady(String path)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Dictionary<string, string> GetFileContentbyKeyPair(string path)
        {

            Dictionary<string, string> ticketinfo = new Dictionary<string, string>();

            if (!FileIsEmpty(path) && IsFileReady(path))
            {
                List<string> content = File.ReadAllLines(path).ToList();

                foreach (string line in content)
                {
                    string[] value;
                    value = line.Split(new Char[] { '|' });

                    if ((value != null) && (value.Length > 0))
                    {
                        string key = value[0];
                        string data = value.Length > 1 ? value[1] : "";

                        ticketinfo.Add(key, data);
                    }
                }

                return ticketinfo;
            }
            else {
                return null;
            }
            
        }

        public static bool createfile(string fullpath,List<string> lines)
        {
            bool result = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullpath))
            {

                lines.ForEach(item => file.WriteLine(item));

                result = true;
            }

            return result;

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
