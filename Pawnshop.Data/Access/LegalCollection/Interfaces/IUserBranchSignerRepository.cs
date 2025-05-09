using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Data.Access.LegalCollection
{
    public interface IUserBranchSignerRepository
    {
        public Task<UserBranchSigner> GetByBranch(int branchId);
    }
}