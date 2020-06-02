using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace console
{
    public class IniReading
    {
        public IniReading(string fileName)
        {
            this.FileName = fileName;
        }

        public IniReading() { }

        public string FileName { get; set; }
        
        private Dictionary<string, Dictionary<string, string>> maps { get; set; }

        private List<string> Split(string input, string pattern)
        {
            return Regex.Split(input, pattern).ToList();
        }

        public Dictionary<string, Dictionary<string, string>> GetAll(string fileName)
        {
            Dictionary<string, Dictionary<string, string>> dic = new Dictionary<string, Dictionary<string, string>>();

            using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
            {
                string all = reader.ReadToEnd();
                Debug.WriteLine(all);

                List<string> data = Regex.Split(all, "\r\n").ToList();
                bool isSection = false;
                Dictionary<string, string> current;

                foreach (var d in data)
                {
                    if (d.Equals(string.Empty))
                    {
                        continue;
                    }

                    string sectionPattern = @"\[(.*?)\]";
                    var match = Regex.Match(d, sectionPattern);
                    if(match.Success)
                    {
                        string section = match.Groups[1].ToString();

                    }

                    if (d.ElementAt(0) == '[' & d.ElementAt(d.Length -1) == ']')
                    {
                        //current
                        current = new Dictionary<string, string>();

                    }
                    else
                    {
                        isSection = false;
                    }

                    
                }

            }

            return dic;
        }

    }
}
