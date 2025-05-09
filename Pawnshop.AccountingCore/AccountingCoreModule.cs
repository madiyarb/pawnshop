using System;
using Autofac;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.AccountingCore
{
    public class AccountingCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<Type>().AsSelf();
            builder.RegisterType<BusinessOperation>().AsSelf();
            builder.RegisterType<BusinessOperationSetting>().AsSelf();
            builder.RegisterType<Account>()
                .As<IAccount>().AsImplementedInterfaces();
        }
    }
}
