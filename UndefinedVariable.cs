using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public class UndefinedVariable
    {
        public UndefinedVariable()
        {
            Values = new HashSet<string>();
        }
        public string Name { get; set; }

        public ICollection<string> Values { get; set; }
    }
}
