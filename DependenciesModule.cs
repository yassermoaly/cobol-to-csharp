using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CobolToCSharp
{
    public class DependenciesModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStatementConverter>().To<MoveStatementConverter>();
            Bind<IStatementConverter>().To<PerformStatementConverter>();
            Bind<IStatementConverter>().To<GoToStatementConverter>();
            Bind<IStatementConverter>().To<IfStatementConverter>();
            Bind<IStatementConverter>().To<QueryStatementConverter>();

            Bind<IStatementConverter>().To<CommentStatementConverter>();
            Bind<IStatementConverter>().To<ElseStatementConverter>();
            Bind<IStatementConverter>().To<BeginBlockStatementConverter>();
            Bind<IStatementConverter>().To<EndBlockStatementConverter>();

            Bind<IStatementConverter>().To<ADDStatementConverter>();
            Bind<IStatementConverter>().To<SUBTRACTStatementConverter>();
            Bind<IStatementConverter>().To<MULTIPLYStatementConverter>();
            Bind<IStatementConverter>().To<DIVIDEStatementConverter>();

            Bind<IStatementConverter>().To<CALLStatementConverter>();
            Bind<IStatementConverter>().To<DisplayStatementConverter>();

            Bind<IStatementConverter>().To<EXITProgramStatementConverter>();
            Bind<IStatementConverter>().To<ComputeStatementConverter>();
            Bind<IStatementConverter>().To<StopRunStatementConverter>();

            Bind<IStatementConverter>().To<InspectStatementConverter>();
            Bind<IStatementConverter>().To<AcceptStatementConverter>();
        }
    }
}
