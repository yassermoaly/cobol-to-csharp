using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public interface IStatementConverter
    {
        StatementType StatementType { get; }
        string Convert(string Line,Paragraph Paragraph,List<Paragraph> Paragraphs);
    }
}
