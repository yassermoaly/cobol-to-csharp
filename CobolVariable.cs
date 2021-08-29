using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class CobolVariable
    {

        static Regex RegexNumeric = new Regex(@"^(S9+|9+)((V9+)*|(\.9+)*)$");
        static Regex RegexAlphaNumberic = new Regex(@"^X+$");
        static Regex RegexAlphabetic = new Regex(@"^A+$");

        public CobolVariable()
        {
            Childs = new List<CobolVariable>();
        }

        public int RowNumber { get; set; }
        public CobolVariable Parent { get; set; }
        public string Raw { get; set; }
        public int Level { get; set; }
        private string _RawWithoutLevel = null;
        public string RawWithoutLevel
        {
            get
            {
                if(string.IsNullOrEmpty(_RawWithoutLevel) && !string.IsNullOrEmpty(Raw))
                    _RawWithoutLevel =  new Regex(@"^[\d/ ]+").Replace(Raw, string.Empty).Trim();

                return _RawWithoutLevel;
            }
        }
        public string RawName
        {
            get
            {
                if (string.IsNullOrEmpty(Raw))
                    return string.Empty;
                return new Regex("^[a-zA-Z][a-zA-Z0-9-]*").Matches(RawWithoutLevel).First().Value;
            }
        }
        public string RawDataType
        {
            get
            {
                if (string.IsNullOrEmpty(Raw))
                    return string.Empty;
                if (Childs.Count > 0)
                    return "class";

                return new Regex(@"PIC[ ]+.+?(?=(\.| ))").Match(Raw).Value.Replace("PIC",string.Empty).Trim();
            }
        }

        public string DataType
        {
            get
            {
                if (string.IsNullOrEmpty(Raw))
                    return string.Empty;
                string _RawDataType = RawDataType;
                if(_RawDataType == "class")
                {
                    return "class";
                }
                if (_RawDataType.StartsWith("9")|| _RawDataType.StartsWith("S9"))
                {
                    if (_RawDataType.ToLower().Contains("v") || _RawDataType.ToLower().Contains("."))
                        return "double";
                    return "long";
                }
                
                else if (_RawDataType.StartsWith("X") || _RawDataType.StartsWith("A"))
                {
                    return "string";
                }

                throw new Exception($"UnRecognized Data Type {_RawDataType}");
            }
        }
        public int Size
        {

            get
            {

                string _RawDataType = RawDataType;
                if (string.IsNullOrEmpty(_RawDataType)) return 0;
                if (_RawDataType.Equals("class"))
                {
                    return Childs.Sum(r => r.Size);
                }
               

                Match Match;
                while (true)
                {
                    Match = new Regex(@"(9|X|A)\([0-9]+\)").Match(_RawDataType);
                    if (Match.Captures.Count==0) break;
                    char RepeatedCharacter = Match.Value[0];

                    int Length = int.Parse(Match.Value.Substring(2, Match.Length - 3));

                    _RawDataType = $"{_RawDataType.Substring(0, Match.Index)}{String.Empty.PadLeft(Length, RepeatedCharacter)}{(Match.Index+Match.Length < RawDataType.Length ? _RawDataType.Substring(Match.Length + 1) : string.Empty)}";
                }


                if (RegexNumeric.IsMatch(_RawDataType) || RegexAlphaNumberic.IsMatch(_RawDataType) || RegexAlphabetic.IsMatch(_RawDataType))
                {
                    return _RawDataType.Replace("V", string.Empty).Replace("v", string.Empty).Length;
                }

                throw new Exception($"UnRecognized Data Type {_RawDataType}");                
            }
        }

       
        public List<CobolVariable> Childs { get; set; }


        public override string ToString()
        {
            if (RawName.Equals("FILLER")) return string.Empty;
            if (DataType.Equals("class")) return string.Empty;
            StringBuilder SB = new StringBuilder();
            SB.AppendLine($"        #region {RawName}");
            SB.AppendLine($"        private string _{NamingConverter.Convert(RawName)};");
            SB.AppendLine($"        public {DataType} {NamingConverter.Convert(RawName)}");
            SB.AppendLine($"        {{");
            SB.AppendLine($"            get");
            SB.AppendLine($"            {{");
            if(DataType == "string")
                SB.AppendLine($"                return _{NamingConverter.Convert(RawName)};");
            else
                SB.AppendLine($"                return {DataType}.Parse(_{NamingConverter.Convert(RawName)});");
            SB.AppendLine($"            }}");
            SB.AppendLine($"            set");
            SB.AppendLine($"            {{");
            SB.AppendLine($"                _{NamingConverter.Convert(RawName)}=value.ToString();");
            SB.AppendLine($"            }}");
            SB.AppendLine($"         }}");
            SB.AppendLine($"        #endregion");

            return SB.ToString();
        }
    }
}
