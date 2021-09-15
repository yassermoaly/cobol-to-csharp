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
            Line = Line.Replace("ALL SPACES", "SPACES");
            var Matches = new Regex("MOVE[ ]+|([a-zA-Z-0-9]+|\".+\")[ ]+|TO[ ]+|[a-zA-Z-0-9]+").Matches(Line);
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
            if (NewTokens[0].Equals("MOVE") && NewTokens[2].Equals("TO"))
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
            for (int i = 3; i < Tokens.Length; i++)
                ConvertedLine.Append($"{NamingConverter.Convert(Tokens[i])} = ");
            ConvertedLine.Append($"{NamingConverter.Convert(Tokens[1])};");
            return ConvertedLine.ToString();           
        }
        
    }
}
