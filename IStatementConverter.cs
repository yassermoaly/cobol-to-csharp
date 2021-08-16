using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolParser
{
    public interface IStatementConverter
    {
        StatementType StatementType { get; }
        string Convert(string Line,Paragraph Paragraph);
    }
}
