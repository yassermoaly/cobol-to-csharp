using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class MoveStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.MOVE });
        
        private string[] GetTokens(string Line)
        {
            Line = Line.RegexReplace("ALL[ ]+SPACES", "SPACES").RegexReplace("MOVE[ ]+ALL", "MOVE").Replace("MOVE ALL ZEROS", "MOVE ZEROS");
            var Matches = new Regex($"{"MOVE".RegexUpperLower()}[ ]+|([a-zA-Z-0-9]+|[Xx]*\".+\")[ ]+|{"TO".RegexUpperLower()}[ ]+|[a-zA-Z-0-9]+").Matches(Line);
            var MatchesT = new Regex($"{"MOVE".RegexUpperLower()}[ ]+|([a-zA-Z-0-9]+|\".+\")[ ]+|{"TO".RegexUpperLower()}[ ]+|[a-zA-Z-0-9]+").Matches(Line);
            string[] NewTokens = new string[Matches.Count];
            int i = 0;
            foreach (Match Match in Matches)
            {
                NewTokens[i++] = Match.Value.Trim();
            }
           
            string[] Tokens = Line.Replace(".", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (NewTokens.Length == Tokens.Length)
            {
                for (int si = 0; si < Tokens.Length; si++)
                {
                    if (Tokens[si] != NewTokens[si])
                    {
                        using (System.IO.StreamWriter WW=new System.IO.StreamWriter("Tokens-Diff.log"))
                        {
                            WW.WriteLine($"{"MOVE".RegexUpperLower()}[ ]+|([a-zA-Z-0-9]+|[Xx]\".+\")[ ]+|{"TO".RegexUpperLower()}[ ]+|[a-zA-Z-0-9]+");
                            WW.WriteLine($"{"MOVE".RegexUpperLower()}[ ]+|([a-zA-Z-0-9]+|\".+\")[ ]+|{"TO".RegexUpperLower()}[ ]+|[a-zA-Z-0-9]+");
                            WW.WriteLine(Line);
                        }
                        int asdasd = 10;
                    }
                }
            }
            else
            {
                int asdasd = 10;
            }
            if (NewTokens[0].ToUpper().Equals("MOVE") && NewTokens[2].ToUpper().Equals("TO"))
            {
                return NewTokens;
            }
            

            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
        }
        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            if(Line.Equals("MOVE X\"0A\"      TO LF01 LF02                     LF07 LF08 LF09 LF10 LF11 LF12 LF13 LF14 LF15 LF16 LF17 LF121 LF122 LF123 LF124 LF18 LF120."))
            {
                int x = 10;
            }
          
            StringBuilder ConvertedLine = new StringBuilder();
            string[] Tokens = GetTokens(Line);
            string SetValue = Tokens[1].ToUpper().StartsWith("X\"")?$"{Tokens[1].Substring(1)}.GetStringValueFromHexa()": Tokens[1];
            string BaseDateType = string.Empty;
            string ConvertFunction = string.Empty;
            string DataType = string.Empty;
            Dictionary<string, List<string>> SetVariablesWithDataTypes = new Dictionary<string, List<string>>();
            for (int i = 3; i < Tokens.Length; i++)
            {
                DataType = HelpingFunctions.GetDatatype(Tokens[i],CobolVariablesDataTypes);
                if (!SetVariablesWithDataTypes.ContainsKey(DataType))
                {
                    SetVariablesWithDataTypes.Add(DataType, new List<string>());
                }
                SetVariablesWithDataTypes[DataType].Add(Tokens[i]);
                //if (string.IsNullOrEmpty(BaseDateType))
                //{
                //    BaseDateType = DataType;
                //    switch (BaseDateType)
                //    {
                //        case "string":
                //            ConvertFunction = "ToString";
                //            break;
                //        case "long":
                //            ConvertFunction = "ToInt64";
                //            break;
                //        case "double":
                //            ConvertFunction = "ToDouble";
                //            break;
                //        default:
                //            DataType = HelpingFunctions.GetDatatype(Tokens[i], CobolVariablesDataTypes);
                //            throw new Exception("Unhandeled DataType");
                //    }
                //}
                //if (DataType != BaseDateType)
                //{
                //    ConvertedLine.Append($"Convert.{ConvertFunction}({NamingConverter.Convert(Tokens[i])}) = ");
                //}
                //else
                //{
                //    ConvertedLine.Append($"{NamingConverter.Convert(Tokens[i])} = ");
                //}
            }
            string BaseDataType = HelpingFunctions.GetDatatype(Tokens[1], CobolVariablesDataTypes);
            foreach (KeyValuePair<string,List<string>> SetVariablesWithDataType in SetVariablesWithDataTypes)
            {
                StringBuilder SBLine = new StringBuilder();
                foreach (var SetVariable in SetVariablesWithDataType.Value)
                {
                    SBLine.Append($"{NamingConverter.Convert(SetVariable)} = ");
                }
                if (BaseDataType != SetVariablesWithDataType.Key)
                {
                    switch (SetVariablesWithDataType.Key)
                    {
                        case "string":
                            ConvertFunction = "ToString";
                            break;
                        case "long":
                            ConvertFunction = "ToInt64";
                            break;
                        case "double":
                            ConvertFunction = "ToDouble";
                            break;
                        default:
                            throw new Exception("Unhandeled DataType");
                    }
                    SBLine.Append($"Convert.{ConvertFunction}({NamingConverter.Convert(SetValue)});");
                }
                else
                {
                    SBLine.Append($"{NamingConverter.Convert(SetValue)};");
                }

                ConvertedLine.AppendLine(SBLine.ToString());
            }
            //if (DataType != BaseDateType)
            //{
            //    ConvertedLine.Append($"Convert.{ConvertFunction}({NamingConverter.Convert(SetValue)});");
            //}
            //else
            //{
            //    ConvertedLine.Append($"{NamingConverter.Convert(SetValue)};");
            //}
            return ConvertedLine.ToString();           
        }
        
    }
}
