using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class CobolVariable
    {       

        public CobolVariable()
        {
            Childs = new List<CobolVariable>();
        }

        public int RowNumber { get; set; }
        public CobolVariable Parent { get; set; }
        public string Raw { get; set; }
        public int Level { get; set; }
        private string _RawWithoutLevel = null;
        public string RawWithoutLevel
        {
            get
            {
                if(string.IsNullOrEmpty(_RawWithoutLevel) && !string.IsNullOrEmpty(Raw))
                    _RawWithoutLevel =  new Regex(@"^[\d ]+").Replace(Raw, string.Empty).Trim();

                return _RawWithoutLevel;
            }
        }
        public string RawName
        {
            get
            {
                if (string.IsNullOrEmpty(Raw))
                    return string.Empty;
                return new Regex("^[a-zA-Z][a-zA-Z0-9-]*").Matches(RawWithoutLevel).First().Value;
            }
        }
        public string RawDataType
        {
            get
            {
                if (string.IsNullOrEmpty(Raw))
                    return string.Empty;
                if (Childs.Count > 0)
                    return "Class";

                return new Regex(@"PIC[ ]+.+?(?=(\.| ))").Match(Raw).Value.Replace("PIC",string.Empty).Trim();
            }
        }

        public List<CobolVariable> Childs { get; set; }


        public override string ToString()
        {
            return $"Row#:{RowNumber}, Name:{RawName}, RawDataType:{RawDataType}, Level:{Level}"; 
        }
    }
}
