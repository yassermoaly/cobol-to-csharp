using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class GoToStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.GOTO});

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            if (new Regex("^GO[ ]+TO[ ]+[a-zA-Z0-9-]+").IsMatch(Line))
            {
                string[] Tokens = Line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (Tokens.Length == 3)
                {
                    StringBuilder SB = new StringBuilder();
                    SB.AppendLine($"return {NamingConverter.Convert(Tokens[2].Replace(".",string.Empty))}(false);");
                    return SB.ToString();
                }
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
            
        }
    }
}
