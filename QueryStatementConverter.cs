using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class QueryStatementConverter : IStatementConverter
    {
        public List<StatementType> StatementTypes => new List<StatementType>(new StatementType[] { StatementType.QUERY });

        public string Convert(string Line,Paragraph Paragraph, List<Paragraph> Paragraphs)
        {
            return string.Empty;
        }
    }
}
