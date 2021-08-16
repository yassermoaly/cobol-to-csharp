using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class GoToStatementConverter : IStatementConverter
    {
        public StatementType StatementType => StatementType.GOTO;

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            string[] Tokens = Line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (Tokens.Length == 3)
            {
                StringBuilder SB = new StringBuilder();
                SB.AppendLine("PerformStack.Clear();");
                SB.AppendLine($"goto {NamingConverter.Convert(Tokens[2])}");
                return SB.ToString();
            }
            throw new Exception($"Invalid {StatementType.ToString()} Statement, {Line}");
            
        }
    }
}
