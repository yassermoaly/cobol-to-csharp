using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class StopRunStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.STOP_RUN });

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs, Dictionary<string, string> CobolVariablesDataTypes = null)
        {
            StringBuilder SB = new StringBuilder();
            SB.AppendLine("FullStack.Add(new Stack(\"END\", string.Empty));");
            SB.AppendLine("return FullStack;");
            return SB.ToString();
        }
    }
}
