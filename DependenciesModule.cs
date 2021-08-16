using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CobolParser
{
    public class DependenciesModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStatementConverter>().To<MoveStatementConverter>();
        }
    }
}
