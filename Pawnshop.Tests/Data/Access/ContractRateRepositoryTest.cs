using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;

namespace Pawnshop.Tests.Data.Access
{
    [TestClass()]
    public class ContractRateRepositoryTest
    {
        /*private readonly ContractRateRepository _contractRateRepository;

        public ContractRateRepositoryTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210914;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";
            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            _contractRateRepository = new ContractRateRepository(unitOfWork);
        }

        [TestMethod()]
        public void TestContractRepository()
        {
            var contractId = 361786;
            var date = DateTime.Today;

            var contractRate = _contractRateRepository.FindRateOnDateByContractAndCode(contractId,
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT, date);

            Assert.IsNotNull(contractRate);

            contractRate = _contractRateRepository.FindRateOnDateByContractAndCodeWithoutBankRate(contractId,
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT, date);

            Assert.IsNotNull(contractRate);

            var contractRates = _contractRateRepository.List(new ListQuery(), new {ContractId = contractId});
            var contractRatesCount = _contractRateRepository.Count(new ListQuery(), new { ContractId = contractId });

            contractRates.ForEach(t => Assert.IsNotNull(t.RateSetting));

            Assert.IsTrue(contractRatesCount > 0);
        }*/
    }
}