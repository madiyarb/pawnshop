using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.ContractServiceTest
{
    [TestClass()]
    public class ContractStatusUpdateTest
    {
        /*private ContractRepository _contractRepository;

        public ContractStatusUpdateTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            _contractRepository = new ContractRepository(unitOfWork);

            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<GoldContractSpecific>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<Configuration>());
        }

        [TestMethod()]
        public void updateStatus()
        {
            var contract = _contractRepository.GetOnlyContract(354726);
            _contractRepository.ContractStatusUpdate(contract.Id, ContractStatus.Disposed);

            _contractRepository.ContractStatusUpdate(contract.Id, ContractStatus.SoldOut);
        }*/
    }
}
