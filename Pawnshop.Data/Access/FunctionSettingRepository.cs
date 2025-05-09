using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.FunctionSetting;

namespace Pawnshop.Data.Access
{
    public class FunctionSettingRepository : RepositoryBase
    {
        public FunctionSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public FunctionSetting GetByCode(string code) =>
            UnitOfWork.Session.QueryFirstOrDefault<FunctionSetting>("SELECT * FROM FunctionSettings WHERE DeleteDate IS NULL AND Code = @code",
                new { code }, UnitOfWork.Transaction);
        
        public async Task<FunctionSetting> GetByCodeAsync(string code) =>
            await UnitOfWork.Session.QueryFirstOrDefaultAsync<FunctionSetting>(
                "SELECT * FROM FunctionSettings WHERE DeleteDate IS NULL AND Code = @code",
                new { code }, 
                UnitOfWork.Transaction);
    }
}
