using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class AutocreditContractCancelRepository : RepositoryBase
    {
        public AutocreditContractCancelRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<int> Find()
        {
            return UnitOfWork.Session.Query<int>($@"
                SELECT Id FROM Contracts
                WHERE ProductTypeId = 1 AND Status = 5 AND DeleteDate IS NULL").ToList();
        }

        public void CancelContract(int contractId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                UPDATE Contracts SET Status = 25 WHERE Id = @contractId",
                new { contractId }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
