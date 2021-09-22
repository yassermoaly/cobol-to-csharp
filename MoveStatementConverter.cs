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
            Line = Line.RegexReplace("ALL SPACES", "SPACES");
            var Matches = new Regex($"{"MOVE".RegexUpperLower()}[ ]+|([a-zA-Z-0-9]+|\".+\")[ ]+|{"TO".RegexUpperLower()}[ ]+|[a-zA-Z-0-9]+").Matches(Line);
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
          
            StringBuilder ConvertedLine = new StringBuilder();
            string[] Tokens = GetTokens(Line);
            string SetValue = Tokens[1];
            string BaseDateType = string.Empty;
            string ConvertFunction = string.Empty;
            string DataType = string.Empty;
            for (int i = 3; i < Tokens.Length; i++)
            {
                DataType = HelpingFunctions.GetDatatype(Tokens[i],CobolVariablesDataTypes);
                if (string.IsNullOrEmpty(BaseDateType))
                {
                    BaseDateType = DataType;
                    switch (BaseDateType)
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
                }
                if (DataType != BaseDateType)
                {
                    ConvertedLine.Append($"Convert.{ConvertFunction}({NamingConverter.Convert(Tokens[i])}) = ");
                }
                else
                {
                    ConvertedLine.Append($"{NamingConverter.Convert(Tokens[i])} = ");
                }
            }
            DataType = HelpingFunctions.GetDatatype(Tokens[1], CobolVariablesDataTypes);
            if (DataType != BaseDateType)
            {
                ConvertedLine.Append($"Convert.{ConvertFunction}({NamingConverter.Convert(Tokens[1])});");
            }
            else
            {
                ConvertedLine.Append($"{NamingConverter.Convert(Tokens[1])};");
            }
            return ConvertedLine.ToString();           
        }
        
    }
}
