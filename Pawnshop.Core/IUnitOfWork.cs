using System;
using System.Data;

namespace Pawnshop.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Session { get; }

        IDbTransaction Transaction { get; }

        IDbTransaction BeginTransaction(IsolationLevel il = IsolationLevel.ReadCommitted);
    }
}