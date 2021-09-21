using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class DisplayStatementConverter: IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.DISPLAY });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            return string.Empty;
            //if (Line.EndsWith(".")) Line = Line.Substring(0, Line.Length - 1);
            //return $"{Line.Replace("'", "\"").Replace(".",string.Empty).Replace("DISPLAY", "Console.WriteLine(")});";
        }        
    }
}
