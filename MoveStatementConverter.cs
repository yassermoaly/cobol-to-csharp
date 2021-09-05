using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class MoveStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.MOVE });

        private string[] GetTokens(string Line)
        {
            Line = Line.Replace("ALL SPACES", "SPACES");
            string[] Tokens = Line.Replace(".", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (Tokens[0].Equals("MOVE") && Tokens[2].Equals("TO"))
            {
                return Tokens;
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
