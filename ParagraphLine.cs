using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class ParagraphLine
    {
        public string Statement { get; set; }
        public int StartRowNo { get; set; }
        public int EndRowNo { get; set; }
    }
}
