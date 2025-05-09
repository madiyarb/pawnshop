using System.Data;

namespace Pawnshop.Core.Impl
{
    public class RepositoryBase : IRepository
    {
        protected IUnitOfWork UnitOfWork { get; }

        public RepositoryBase(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public IDbTransaction BeginTransaction()
        {
            return UnitOfWork.BeginTransaction();
        }
    }
}