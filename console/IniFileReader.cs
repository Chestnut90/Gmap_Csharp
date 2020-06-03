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
    public class IniFileReader
    {
        public IniFileReader(string fileName)
        {
            this.FileName = fileName;
        }

        public IniFileReader() { }

        public string FileName { get; set; }
        
        private Dictionary<string, Dictionary<string, string>> Sections { get; set; }

        private List<string> Split(string input, string pattern)
        {
            return Regex.Split(input, pattern).ToList();
        }

        private const string pattern_section = @"\[(.*?)\]";
        private const string pattern_key_value = @"\s*(.+?)\s*=\s*(.+)";//\s*(;.*)*";//\s*(;*)";
        private const string pattern_comment = @"\s*(.*)\s*\;(.+)";

        public string GetValue(string section, string key)
        {
            if(this.Sections is null)
            {
                throw new Exception("Do read first.");
            }

            Dictionary<string, string> temp;
            if(!Sections.TryGetValue(section, out temp))
            {
                throw new Exception($"Wrong section." +
                    $"\n file name : {this.FileName}" +
                    $"\n section : {section}");
            }

            string result;
            if(!temp.TryGetValue(key, out result))
            {
                throw new Exception($"Wrong key in section." +
                                    $"\n file name : {this.FileName}" +
                                    $"\n section : {section}" +
                                    $"\n key : {key}");
            }

            return result;
        }

        public bool Read(string fileName)
        {
            Sections = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
                {
                    string all = reader.ReadToEnd();
                    List<string> data = Regex.Split(all, "\r\n").ToList();
                    bool isSection = false;
                    Dictionary<string, string> current = new Dictionary<string, string>();

                    foreach (var d in data)
                    {
                        string line = d.Trim();
                        if (line.Equals(string.Empty))
                        {
                            // white space 
                            continue;
                        }
                        if (line.ElementAt(0).Equals(';'))
                        {
                            // comment
                            continue;
                        }

                        // search section
                        var matchSection = Regex.Match(line, pattern_section);

                        if (matchSection.Success)
                        {
                            isSection = true;
                            string section = matchSection.Groups[1].ToString();
                            if (section.Equals(string.Empty))
                            {
                                throw new Exception($"Section field is empty." +
                                    $"\nInput line : {line}");
                            }

                            current = new Dictionary<string, string>();
                            Sections.Add(section, current);
                            continue;
                        }

                        // search key and value
                        var matchKeyValue = Regex.Match(line, pattern_key_value);
                        if (matchKeyValue.Success)
                        {
                            if (!isSection)
                            {
                                throw new Exception($"No section field in key and value string." +
                                    $"\nInput line : {line}");
                            }
                            string key = matchKeyValue.Groups[1].ToString();
                            if (key.Trim().Equals(string.Empty))
                            {
                                throw new Exception($"Key is empty." +
                                    $"\nInput line : {line}");
                            }

                            string value = matchKeyValue.Groups[2].ToString();

                            var matchValueComment = Regex.Match(value, pattern_comment);
                            if (matchValueComment.Success)
                            {
                                value = matchValueComment.Groups[1].ToString();
                            }

                            if (value.Trim().Equals(string.Empty))
                            {
                                throw new Exception($"Value is empty." +
                                    $"\nInput line : {line}");
                            }

                            current.Add(key, value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
                return false;
            }
            return true;
        }

    }
}
