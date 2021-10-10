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
        public Dictionary<string,string> CobolVariablesDataTypes { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
        public string Raw { get; set; }
        private string _Converted { get; set; }
        public string Converted
        {
            get
            {
                if(string.IsNullOrEmpty(_Converted))
                    _Converted = StatementConverterFactory.CreateInstance(this).Convert(Raw, Paragraph, Paragraphs, CobolVariablesDataTypes);                
                return _Converted;
            }
        }
        public int RowNo { get; set; }
        public StatementType StatementType { get; set; }
    }
}
