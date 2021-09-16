using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class CALLStatementConverter: IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.CALL });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string,string> CobolVariablesDataTypes = null)
        {
            StringBuilder SB = new StringBuilder();
            if(Line.Contains("\"TXCOMMIT\""))
            {
                SB.AppendLine($"#region {Line}");
                SB.AppendLine("if(!TXCOMMIT())return false;");
                SB.AppendLine($"#endregion");
                return SB.ToString();
            }
            else if (Line.Contains("\"TXROLLBACK\""))
            {
                SB.AppendLine($"#region {Line}");
                SB.AppendLine("if(!TXROLLBACK())return false;");
                SB.AppendLine($"#endregion");
                return SB.ToString();
            }
            return $"//{Line}";
        }
    }
}
