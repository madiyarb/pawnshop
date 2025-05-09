using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Data.Access.LegalCollection
{
    public class UserBranchSignerRepository : RepositoryBase, IUserBranchSignerRepository
    {
        public UserBranchSignerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<UserBranchSigner> GetByBranch(int branchId)
        {
            var parameters = new { BranchId = branchId };
            var sqlQuery = @"
                SELECT *
                FROM UserBranchSigner
                WHERE BranchId = @BranchId";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<UserBranchSigner>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
    }
}