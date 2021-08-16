using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class Statement
    {
        public Paragraph Paragraph { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
        public string Raw { get; set; }
        public string Converted
        {
            get
            {
                return StatementConverterFactory.CreateInstance(this).Convert(Raw, Paragraph, Paragraphs);
            }
        }
        public int RowNo { get; set; }
        public StatementType StatementType { get; set; }
    }
}
