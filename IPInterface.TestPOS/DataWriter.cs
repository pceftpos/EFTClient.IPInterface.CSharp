using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public interface IWriter
    {
        bool Save<T>(T items, string filename);
        bool Load<T>(string filename, out T items);
    }

    public class JsonWriter : IWriter
    {

        string GetPath(string filename)
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{System.IO.Path.DirectorySeparatorChar}PC-EFTPOS{System.IO.Path.DirectorySeparatorChar}IPInterface.TestPOS";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return $"{path}{System.IO.Path.DirectorySeparatorChar}{filename}";
        }

        public bool Load<T>(string filename, out T items)
        {
            try
            {
                var contents = File.ReadAllText(GetPath(filename));
                items = (T)JsonConvert.DeserializeObject(contents, typeof(T));
                return (items != null);
            }
            catch
            {
                items = default(T);
            }

            return false;
        }

        public bool Save<T>(T items, string filename)
        {
            try
            {
                var json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(GetPath(filename), json);
                return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
