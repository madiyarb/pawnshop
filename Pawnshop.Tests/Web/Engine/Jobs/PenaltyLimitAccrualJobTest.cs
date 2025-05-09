using System;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine.Jobs;

namespace Pawnshop.Tests.Web.Engine.Jobs
{
    [TestClass()]
    public class PenaltyLimitAccrualJobTest
    {
        /*private readonly ContractRateRepository _contractRateRepository;
        private readonly ContractRepository _contractRepository;

        public PenaltyLimitAccrualJobTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210914;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";
            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            _contractRateRepository = new ContractRateRepository(unitOfWork);

            ClientRepository _clientRepository = new ClientRepository(unitOfWork);
            LoanSubjectRepository _loanSubjectRepository = new LoanSubjectRepository(unitOfWork);
            LoanProductTypeRepository _loanProductTypeRepository = new LoanProductTypeRepository(unitOfWork);
            GroupRepository _groupRepository = new GroupRepository(unitOfWork);

            LoanPercentSettingInsuranceCompanyRepository insuranceCompanyRepository = new LoanPercentSettingInsuranceCompanyRepository(unitOfWork);
            LoanSettingRateRepository loanSettingRateRepository = new LoanSettingRateRepository(unitOfWork);
            ISessionContext sessionContext = new SessionContext();
            LoanPercentRepository _loanPercentRepository = new LoanPercentRepository(unitOfWork, insuranceCompanyRepository, loanSettingRateRepository, sessionContext);

            _contractRepository = new ContractRepository(unitOfWork, _loanPercentRepository, _clientRepository, _loanSubjectRepository, _loanProductTypeRepository, _groupRepository, _contractRateRepository);

            SqlMapper.AddTypeHandler(new JsonObjectHandler<PermissionListDefinition>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<GoldContractSpecific>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractActionData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<Configuration>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractRefinanceConfig>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceRequestData>());
        }

        [TestMethod()]
        public void Execute()
        {
            var date = DateTime.Today;
            var newAndAdditionContracts = _contractRepository.GetContractsOnDateForPenaltyAccrual(date);

            Assert.IsTrue(newAndAdditionContracts.Count > 0);

            var partialPaymentContracts = _contractRepository.GetContractsByParentContractDateForPenaltyAccrual(date);

            Assert.IsTrue(partialPaymentContracts.Count > 0);
        }*/

    }
}