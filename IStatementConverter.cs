using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public interface IStatementConverter
    {
        List<StatementType> StatementTypes { get; }
        string Convert(string Line,Paragraph Paragraph,List<Paragraph> Paragraphs,Dictionary<string,string> CobolVariablesDataTypes = null);
    }
}
