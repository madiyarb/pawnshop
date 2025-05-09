using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Pawnshop.Core.Impl.Transactions;

namespace Pawnshop.Core.Impl
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionWrapper _transaction;

        public UnitOfWork(string connectionString)
        {
            Session = new SqlConnection(connectionString);
        }

        public void Init()
        {
            Session.Open();
        }

        public IDbConnection Session { get; private set; }
        public IDbTransaction Transaction => _transaction?.InternalTransaction;

        public IDbTransaction BeginTransaction(IsolationLevel il = IsolationLevel.ReadCommitted)
        {
            if (Session.State == ConnectionState.Broken || Session.State == ConnectionState.Closed)
            {
                Session.Open();
            }
            if (_transaction == null)
            {
                _transaction = new TransactionWrapper(Session.BeginTransaction(il));
                _transaction.Disposing += OnTransactionDisposing;

                return _transaction;
            }

            return new TransactionStub(Session, il);
        }

        private void OnTransactionDisposing(TransactionWrapper sender)
        {
            sender.Disposing -= OnTransactionDisposing;
            sender = null;

            _transaction = null;
        }

        public void Dispose()
        {
            Session?.Close();
            Session?.Dispose();
            Session = null;
        }
    }
}