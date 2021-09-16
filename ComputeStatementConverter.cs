using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class ComputeStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.COMPUTE });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            Line = Line.Trim();
            if (Line.EndsWith("."))
                Line.Remove(Line.Length - 1);
            return $"{Line.RegexReplace("COMPUTE", string.Empty).Trim()};";
            //foreach (Match Match in new Regex(@"([/*+-=][ ]*(([a-zA-Z-]+)|([0-9]+)|([0-9]*\.[0-9]+)))").Matches(Line))
            //{
            //    int x = 100;
            //}
            //return Line.Replace("*", "//");
        }
        
    }
}
