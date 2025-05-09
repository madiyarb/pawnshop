using System;
using Autofac;

namespace Pawnshop.Core.Impl
{
    public class ObjectFactory: IObjectFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public ObjectFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public T Create<T>()
        {
            return _lifetimeScope.Resolve<T>();
        }

        public object Create(Type type)
        {
            return _lifetimeScope.Resolve(type);
        }
    }
}