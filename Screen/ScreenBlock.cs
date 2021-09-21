using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp.Screen
{
    public class ScreenBlock
    {
        public string BindName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Raw { get; set; }

        List<ScreenBlock> _Childs = null;
        public List<ScreenBlock> Childs
        {
            get
            {
                if (_Childs == null)
                {
                    _Childs = ScreenBlock.ExtractFromText(Raw, new string[] { "FIELD", "VARIABLE", "MESSAGE" });
                }

                return _Childs;
            }
        }

        public string GetProperyValue(string PropertyName)
        {
            string Property = Raw.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(r=>r.Trim()).FirstOrDefault(r => r.Split('=').Length > 0 && r.Split('=')[0].Trim() == PropertyName);
            return Property!=null && Property.Contains("=")?Property.Split('=')[1].Replace("\"",string.Empty).Trim():string.Empty;
        }
        public string SFTYPE
        {
            get
            {
                return GetProperyValue("SFTYPE");
            }
        }
        public string LOCATION
        {
            get
            {
                return GetProperyValue("LOCATION");
            }
        }
        public string SIZE
        {
            get
            {
                return GetProperyValue("SIZE");
            }
        }
        public string VALUE
        {
            get
            {
                return GetProperyValue("VALUE");
            }
        }
        public bool IsLabel
        {
            get
            {
                return string.IsNullOrEmpty(INPUT) && string.IsNullOrEmpty(OUTPUT);
            }
        }
        public string INPUT
        {
            get
            {
                return GetProperyValue("INPUT");
            }
        }
        public string JUSTIFY
        {
            get
            {
                return GetProperyValue("JUSTIFY");
            }
        }
        public string OUTPUT
        {
            get
            {
                return GetProperyValue("OUTPUT");
            }
        }

        public double X
        {
            get
            {
                if (LOCATION.Contains(','))
                    return double.Parse(LOCATION.Split(',')[1].Trim());
                return 0;
            }
        }
        public double Y
        {
            get
            {
                if(LOCATION.Contains(','))
                    return double.Parse(LOCATION.Split(',')[0].Trim());
                return 0;
            }
        }

        public static List<ScreenBlock> ExtractFromText(string Text,string[] TagNames)
        {
            List<ScreenBlock> ScreenBlocks = new List<ScreenBlock>();
            #region Extract Screen Blocks
            foreach (Match Match in new Regex($"({string.Join('|',TagNames)})[ ]+([a-zA-Z0-9-]+[ ]+)*{{").Matches(Text))
            {
                string[] Tokens = Match.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();

                int count = 1;
                int index = Match.Index + Match.Length;
                StringBuilder SB = new StringBuilder();
                while (count != 0)
                {
                    if (Text[index] == '{') count++;
                    else if (Text[index] == '}') count--;
                    if (count == 0) break;
                    SB.Append(Text[index]);
                    index++;
                }
                ScreenBlocks.Add(new ScreenBlock()
                {
                    Type = Tokens[0],
                    Name = Tokens[1],
                    Raw = SB.ToString()
                });
            }
            #endregion

            return ScreenBlocks;
        }


        public Dictionary<int,double> RowMappings
        {
            get
            {
                if(Type == "SCREEN")
                {
                    Dictionary<int, double> result = new Dictionary<int, double>();
                    List<double> YPositions  = Childs.Where(r => r.Type == "FIELD").Select(r => r.Y).Distinct().OrderBy(r => r).ToList();
                    for (int i = 0; i < YPositions.Count; i++)
                    {
                        result.Add(i, YPositions[i]);
                    }
                    return result;
                }
                return null;
            }
        }
    }
}
