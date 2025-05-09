using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;
using Pawnshop.Services.Contracts.LoanFinancePlans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.ContractServiceTest
{
    [TestClass()]
    public class LoanFinancePlanServiceTest
    {
        /*
        LoanFinancePlanRepository _loanFinancePlanRepository;

        public LoanFinancePlanServiceTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            ISessionContext sessionContext = new SessionContext();
            _loanFinancePlanRepository = new LoanFinancePlanRepository(unitOfWork, sessionContext);
        }

        [TestMethod()]
        public void GetListTest()
        {
            var contractId = 349863;
            List<LoanFinancePlan> loanFinancePlans = _loanFinancePlanRepository.List(new ListQuery(), new { ContractId = contractId });

        }
        */
    }
}
