using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolParser
{
    public static class StatementConverterFactory
    {
        public static IStatementConverter CreateInstance(Statement Statement)
        {
            var kernel = new StandardKernel(new DependenciesModule());
            var IStatementConverters = kernel.Get<List<IStatementConverter>>();
            return IStatementConverters.First(r => r.StatementType == Statement.StatementType);
        }
    }
}
