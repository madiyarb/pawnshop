using Autofac;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Options;

namespace Pawnshop.Core
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SessionContext>()
                .As<ISessionContext>().InstancePerLifetimeScope();

            builder.RegisterType<ObjectFactory>()
                .As<IObjectFactory>();

            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<EnviromentAccessOptions>>().Value;
                var uow = new UnitOfWork(options.DatabaseConnectionString);
                uow.Init();

                return uow;
            }).As<IUnitOfWork>().InstancePerLifetimeScope();
        }
    }
}