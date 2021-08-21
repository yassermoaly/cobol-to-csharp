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

        public string Convert(string Line, Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            return $"//{Line}";
        }
    }
}
