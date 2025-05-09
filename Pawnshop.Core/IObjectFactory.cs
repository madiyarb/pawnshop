using System;

namespace Pawnshop.Core
{
    public interface IObjectFactory
    {
        T Create<T>();
        object Create(Type type);
    }
}