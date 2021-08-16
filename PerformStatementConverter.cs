using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class PerformStatementConverter : IStatementConverter
    {
        public StatementType StatementType => StatementType.PERFORM;

        public string Convert(string Line, Paragraph Paragraph,List<Paragraph> Paragraphs)
        {
            string[] Tokens = Line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            if (Line.Contains("THRU") && Tokens.Length==4)
            {
                string StartParagraph = Tokens[1];
                string EndParagraph = Tokens[3];
                return string.Empty;
            }
            else if (Tokens.Length == 2)
            {
                return $"//{Line}";
            }
            else
            {
                return $"//{Line}";
            }
        }
    }
}
