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

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            if (new Regex("^GO[ ]+TO[ ]+[a-zA-Z0-9-]+").IsMatch(Line))
            {
                StringBuilder SB = new StringBuilder();
                SB.AppendLine($"#region {Line}");
                string[] Tokens = Line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (Tokens.Length == 3)
                {                    
                    SB.AppendLine($"return {NamingConverter.Convert(Tokens[2].Replace(".",string.Empty))}(false,true);");                   
                }
                SB.AppendLine($"#endregion");
                return SB.ToString();
            }
            throw new Exception($"Invalid {StatementTypes.First().ToString()} Statement, {Line}");
            
        }
    }
}
