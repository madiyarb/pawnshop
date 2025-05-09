using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.ApplicationOnlineInsurances;
using Pawnshop.Data.Models.ApplicationOnlineInsurances.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlineInsuranceRepository : RepositoryBase
    {
        public ApplicationOnlineInsuranceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task<ApplicationOnlineInsurance> GetByApplicationId(Guid applicationId)
        {
            return (await UnitOfWork.Session.QueryAsync<ApplicationOnlineInsurance>
            (@"Select * from ApplicationOnlineInsurances where ApplicationOnlineId = @applicationId and DeleteDate is null",
                new { applicationId }, UnitOfWork.Transaction)).FirstOrDefault();
        }

        public async Task<ApplicationOnlineInsuranceView> GetViewByApplicationId(Guid applicationId)
        {
            var insurance = (await UnitOfWork.Session.QueryAsync<ApplicationOnlineInsuranceView>
            (@"Select * from ApplicationOnlineInsurances where ApplicationOnlineId = @applicationId and DeleteDate is null",
                new { applicationId }, UnitOfWork.Transaction)).FirstOrDefault();
            if (insurance != null)
                insurance.FillStatus();
            return insurance;
        }
        public async Task<ApplicationOnlineInsurance> Get(Guid id)
        {
            return await UnitOfWork.Session.GetAsync<ApplicationOnlineInsurance>(id);
        }

        public async Task Insert(ApplicationOnlineInsurance insurance)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(insurance, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<bool> Update(ApplicationOnlineInsurance insurance)
        {
            using (var transaction = BeginTransaction())
            {
                var result =  await UnitOfWork.Session.UpdateAsync(insurance, UnitOfWork.Transaction);
                transaction.Commit();
                return result;
            }
        }


    }
}
