using Newtonsoft.Json;
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

        static Regex RegexNumeric = new Regex(@"^([Ss]9+|9+)(([Vv]9*)*|(\.9*)*)$");
        static Regex RegexAlphaNumberic = new Regex(@"^[Xx]+$");
        static Regex RegexAlphabetic = new Regex(@"^[Aa]+$");



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
                
                if (string.IsNullOrEmpty(_RawWithoutLevel) && !string.IsNullOrEmpty(Raw))
                {                  
                    Match LastDigitMatch = new Regex(@"\d+[ ]+[a-zA-Z]").Matches($" {Raw.Replace("/", string.Empty)}").First();
                    _RawWithoutLevel = Raw.Substring(LastDigitMatch.Index + LastDigitMatch.Length-2).Trim();                    
                }

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

                return new Regex($@"{"PIC".RegexUpperLower()}[ ]+.+?(?=(\.| ))").Match(Raw).Value.RegexReplace("PIC",string.Empty).Trim().Replace("Z","9").Replace("B", "9");
            }
        }
        public bool IsSigned
        {
            get
            {
                return RawDataType.StartsWith("[Ss]9");
            }
        }
        private string TransformSize(string _RawDataType)
        {
            Match Match;
            while (true)
            {
                Match = new Regex(@"(9|[Xx]|[Aa])\([0-9]+\)").Match(_RawDataType);
                if (Match.Captures.Count == 0) break;
                char RepeatedCharacter = Match.Value[0];

                int Length = int.Parse(Match.Value.Substring(2, Match.Length - 3));

                _RawDataType = $"{_RawDataType.Substring(0, Match.Index)}{String.Empty.PadLeft(Length, RepeatedCharacter)}{((Match.Index + Match.Length) < _RawDataType.Length ? _RawDataType.Substring(Match.Index + Match.Length) : string.Empty)}";
            }
            return _RawDataType;
        }
        public int SizePrePoint
        {
            get
            {
                string _RawDataType = TransformSize(RawDataType).ToLower();
                char pointChar = 'v';
                if (!_RawDataType.ToLower().Contains(pointChar)) pointChar = '.';
                int index = _RawDataType.IndexOf(pointChar);
                if (index >= 0)
                    return index;
                return _RawDataType.Length;
            }
        }
        public int SizePostPoint
        {
            get
            {
                string _RawDataType = TransformSize(RawDataType).ToLower();
                char pointChar = 'v';
                if (!_RawDataType.Contains(pointChar)) pointChar = '.';
                int index = _RawDataType.IndexOf(pointChar);
                if (index >= 0)
                    return _RawDataType.Length - (index+1);
                return 0;
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
                if (_RawDataType.StartsWith("9")|| _RawDataType.ToLower().StartsWith("s9"))
                {
                    if (!_RawDataType.ToLower().EndsWith("v") && (_RawDataType.ToLower().Contains("v") || _RawDataType.ToLower().Contains(".")))
                        return "double";
                    return "long";
                }
                
                else if (_RawDataType.ToLower().StartsWith("x") || _RawDataType.ToLower().StartsWith("a"))
                {
                    return "string";
                }

                throw new Exception($"UnRecognized Data Type {_RawDataType}");
            }
        }
        public string PropertyDataType
        {
            get
            {
                

                switch (DataType)
                {
                    case "class":
                        if (!string.IsNullOrEmpty(REDEFINENAME))
                        {
                            return RedefineVariable.PropertyDataType;
                        }
                        return "string";
                    default:
                        return DataType;
                }                
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
                    return Childs.Where(r => string.IsNullOrEmpty(r.REDEFINENAME)).Sum(r => r.Size);
                }

                _RawDataType = TransformSize(_RawDataType);               

                if (RegexNumeric.IsMatch(_RawDataType) || RegexAlphaNumberic.IsMatch(_RawDataType) || RegexAlphabetic.IsMatch(_RawDataType))
                {
                    return _RawDataType.Replace("V", string.Empty).Replace("v", string.Empty).Length;
                }
                _RawDataType = RawDataType;
                throw new Exception($"UnRecognized Data Type {_RawDataType}");                
            }
        }

       
        public List<CobolVariable> Childs { get; set; }

        private string _REDEFINENAME = null;
        public string REDEFINENAME
        {
            get
            {
                if (_REDEFINENAME == null)
                {
                    Match REDEFINESMatch = new Regex($"{"REDEFINES".RegexUpperLower()}[ ]+[a-zA-Z0-9-]+").Match(Raw);
                    _REDEFINENAME =  REDEFINESMatch.Success ? REDEFINESMatch.Value.ToString().RegexReplace("REDEFINES", string.Empty).Trim() : string.Empty;
                }

                return _REDEFINENAME;
            }
        }
        public CobolVariable RedefineVariable
        {
            get
            {
                if (!string.IsNullOrEmpty(REDEFINENAME))
                {
                    return Parent.Childs.First(r => r.RawName == REDEFINENAME);
                    //CobolVariable RootVariable = Parent;
                    //while (true)
                    //{
                    //    if (RootVariable.Parent == null)
                    //        break;
                    //    RootVariable = RootVariable.Parent;
                    //}
                    //return FindVariableByName(RootVariable.Childs, REDEFINENAME);
                }
                return null;
            }
        }
        private CobolVariable FindVariableByName(List<CobolVariable> Variables, string VariableName)
        {
            foreach (var Variable in Variables)
            {
                if (Variable.RawName == VariableName)
                    return Variable;

                if (Variable.Childs.Count > 0)
                    return FindVariableByName(Variable.Childs, VariableName);
            }

            return null;
        }
        private CobolVariable GetRedefinedAs(List<CobolVariable> Variables, string VariableName)
        {
            foreach (var Variable in Variables)
            {
                if (Variable.REDEFINENAME == VariableName)
                    return Variable;

                if (Variable.Childs.Count > 0)
                    GetRedefinedAs(Variable.Childs, VariableName);
            }

            return null;
        }
        private CobolVariable _RedefinedAs = null;
        public CobolVariable RedefinedAs
        {
            get
            {
                if (_RedefinedAs == null)
                {
                    CobolVariable RootVariable = Parent;
                    while (true)
                    {
                        if (RootVariable.Parent == null)
                            break;
                        RootVariable = RootVariable.Parent;
                    }

                    _RedefinedAs = GetRedefinedAs(RootVariable.Childs, RawName);
                }
                return _RedefinedAs;
            }
        }
        public bool IsString
        {
            get
            {
                return DataType.Equals("string") || DataType.Equals("class");
            }
        }

        public int StartPosition
        {
            get
            {

                if (string.IsNullOrEmpty(REDEFINENAME))
                {
                    int pos = IsNull(Parent) ? 0 : Parent.StartPosition;
                    foreach (var child in Parent.Childs)
                    {
                        if (child == this) break;
                        if (string.IsNullOrEmpty(child.REDEFINENAME))
                            pos += child.Size;
                    }
                    return pos;
                }
                return RedefineVariable.StartPosition;               
            }
        }
        private bool IsNull(CobolVariable Variable)
        {
            return Variable == null || string.IsNullOrEmpty(Variable.Raw);
        }
        public string GetSetPropertyName
        {
            get
            {
            
                CobolVariable CobolVariable = this;
                while (true)
                {
                    if (string.IsNullOrEmpty(CobolVariable.REDEFINENAME) && IsNull(CobolVariable.Parent))
                        break;
                    if (!string.IsNullOrEmpty(CobolVariable.REDEFINENAME))
                        CobolVariable = CobolVariable.RedefineVariable;
                    else
                        CobolVariable = CobolVariable.Parent;
                }
                return $"_{NamingConverter.Convert(CobolVariable.RawName)}";
                //if (IsNull(Parent))
                //{
                //    if(string.IsNullOrEmpty(REDEFINENAME))
                //        return $"_{NamingConverter.Convert(RawName)}";
                   
                //    return $"{NamingConverter.Convert(RedefineVariable.RawName)}";

                //}
                //else
                //{
                //    if (!string.IsNullOrEmpty(REDEFINENAME))
                //    {
                //        return $"{NamingConverter.Convert(RedefineVariable.RawName)}";
                //    }
                //    return $"{NamingConverter.Convert(Parent.RawName)}";
                //}
                
            }
        }
        public override string ToString()
        {
            string[] ExecludedVraibles = Config.Values["ExecludedVariables"].Split(',');
            if (ExecludedVraibles.Contains(RawName)) return string.Empty;
            StringBuilder SB = new StringBuilder();
            SB.AppendLine($"        #region {RawName}");
            string ParameterName = NamingConverter.Convert(RawName);

            if (string.IsNullOrEmpty(REDEFINENAME) && IsNull(Parent))
            {
                SB.AppendLine($"        private string _{ParameterName};");
            }
            SB.AppendLine($"        public {PropertyDataType} {ParameterName}");
            SB.AppendLine($"        {{");
            SB.AppendLine($"            get");
            SB.AppendLine($"            {{");
            switch (PropertyDataType)
            {
                case "string":
                    SB.AppendLine($"                return {GetSetPropertyName}.GetStringValue({StartPosition},{Size});");
                    break;
                case "long":
                    SB.AppendLine($"                return {GetSetPropertyName}.GetLongValue({StartPosition},{Size},{IsSigned.ToString().ToLower()});");
                    break;
                case "double":
                    SB.AppendLine($"                return {GetSetPropertyName}.GetDoubleValue({StartPosition},{SizePrePoint},{SizePostPoint},{IsSigned.ToString().ToLower()});");
                    break;              
            }
        
            SB.AppendLine($"            }}");
            SB.AppendLine($"            set");
            SB.AppendLine($"            {{");
            switch (PropertyDataType)
            {
                case "string":
                    SB.AppendLine($"                 {GetSetPropertyName} = {GetSetPropertyName}.SetStringValue(value, {StartPosition}, {Size});");
                    break;
                case "long":
                    SB.AppendLine($"                 {GetSetPropertyName} = {GetSetPropertyName}.SetLongValue(value, {StartPosition}, {Size},{IsSigned.ToString().ToLower()});");
                    break;
                case "double":
                    SB.AppendLine($"                 {GetSetPropertyName} = {GetSetPropertyName}.SetDoubleValue(value, {StartPosition}, {SizePrePoint}, {SizePostPoint},{IsSigned.ToString().ToLower()});");
                    break;
            }
                   
            SB.AppendLine($"            }}");
            SB.AppendLine($"         }}");


           
            SB.AppendLine($"        #endregion");

            return SB.ToString();
        }

        public List<CobolVariable> GetBaseChilds()
        {
            List<CobolVariable> result = new List<CobolVariable>();
            foreach (var Child in Childs)
            {
                if (string.IsNullOrEmpty(Child.REDEFINENAME))
                {
                    if(Child.DataType == "class")
                    {
                        result.AddRange(Child.GetBaseChilds());
                    }
                    else
                    {
                        result.Add(Child);
                    }
                }
            }
            return result;
        }
    }
}
