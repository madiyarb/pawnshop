using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Web.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Tests.Web.CreateContractFromApplication
{
    /*
    [TestClass()]
    public class ContractFromApplicationTest
    {
        private ContractRepository _contractRepository;
        private LoanPercentRepository _loanPercentRepository;
        private ClientRepository _clientRepository;
        private LoanSubjectRepository _loanSubjectRepository;
        private LoanProductTypeRepository _loanProductTypeRepository;
        private GroupRepository _groupRepository;
        private LoanPercentSettingInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly ContractNumberCounterRepository _counterRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly OrganizationRepository _organizationRepository;
        private readonly MemberRepository _memberRepository;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly LoanSettingRateRepository _loanSettingRateRepository;
        private readonly ContractRateRepository _contractRateRepository;
        private readonly ApplicationDetailsRepository _applicationDetailsRepository;

        public ContractFromApplicationTest()
        {
            string connectionString = "Data Source=192.168.10.33,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210927;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            _insuranceCompanyRepository = new LoanPercentSettingInsuranceCompanyRepository(unitOfWork);
            _loanSettingRateRepository = new LoanSettingRateRepository(unitOfWork);
            _clientRepository = new ClientRepository(unitOfWork);
            _loanSubjectRepository = new LoanSubjectRepository(unitOfWork);
            _loanProductTypeRepository = new LoanProductTypeRepository(unitOfWork);
            _groupRepository = new GroupRepository(unitOfWork);
            _contractRateRepository = new ContractRateRepository(unitOfWork);
            _contractRepository = new ContractRepository(unitOfWork, _loanPercentRepository, _clientRepository, _loanSubjectRepository, _loanProductTypeRepository, _groupRepository, _contractRateRepository);
            _applicationRepository = new ApplicationRepository(unitOfWork);
            _counterRepository = new ContractNumberCounterRepository(unitOfWork);

            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<GoldContractSpecific>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<Configuration>());

            _organizationRepository = new OrganizationRepository(unitOfWork);
            _memberRepository = new MemberRepository(unitOfWork);
            _sessionContext = new SessionContext();
            _sessionContext.Init(456, "Abzal", 1, "TAS FINANCE GROUP", "1234567890", true, new string[] { "UserView", "UserManage" });//UserId = 456
            _branchContext = new BranchContext(_sessionContext, _organizationRepository, _memberRepository);
            _branchContext.Init(154);//BranchId=154

            _loanPercentRepository = new LoanPercentRepository(unitOfWork, _insuranceCompanyRepository, _loanSettingRateRepository, _sessionContext);
            _contractCheckRepository = new ContractCheckRepository(unitOfWork);

            _applicationDetailsRepository = new ApplicationDetailsRepository(unitOfWork);
        }

        [TestMethod()]
        public void CreateContract()
        {
            int applicationId = 5;
            var application = _applicationRepository.Get(applicationId);
            var contract = new Contract();

            contract.ClientId = application.ClientId;

            contract.LoanCost = 6040000;
            contract.EstimatedCost = application.EstimatedCost; // 8500000;//из заявки
            contract.OwnerId = _branchContext.Branch.Id; //154;
            contract.BranchId = _branchContext.Branch.Id; //154;
            contract.AuthorId = _sessionContext.UserId; //application.AuthorId;
            contract.ContractDate = DateTime.Now;
            contract.MaturityDate = DateTime.Now;
            contract.OriginalMaturityDate = DateTime.Now;
            contract.CollateralType = CollateralType.Car;//из Формы
            contract.PercentPaymentType = PercentPaymentType.Product; //из Фронта
            contract.ContractData = new ContractData() { Client = application.Client, PrepaymentCost = 0 };

            contract.ContractNumber = _counterRepository.Next(
                        contract.ContractDate.Year, _branchContext.Branch.Id,
                        _branchContext.Configuration.ContractSettings.NumberCode);

            var contractPosition = new ContractPosition()
            {
                PositionId = application.PositionId,
                Position = application.Position,
                PositionCount = 1,
                CategoryId = 1,
                LoanCost = contract.LoanCost,
                EstimatedCost = contract.EstimatedCost
            };
            contract.Positions.Add(contractPosition);

            contract.LoanPercent = 0.1247m; //из Фронта
            contract.LoanPercentCost = 7531.8800m; //из Фронта

            contract.SettingId = 1089;//Find для не из продукта - NULL, из продукта SettingId + ProductTypeId заполняем

            //contract.ContractTypeId = заполняем по setting

            //contract.ProductTypeId = 1;
            //contract.ContractTypeId = 12;
            //contract.PeriodTypeId = 2;
            //contract.LoanPurposeId = 7;
            //contract.MinimalInitialFee = 2550000;
            //contract.RequiredInitialFee = 2550000;
            //contract.PenaltyPercentCost = 231332.0000m;
            //contract.AttractionChannelId = 13;
            //contract.LoanPeriod = 0;

            //не получаем от Фронта, а запрашиваем сами
            var contractChecks = _contractCheckRepository.List(new ListQuery() { Page = null });

            contract.Checks.AddRange(contractChecks.Select(c => new ContractCheckValue()
            {
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                BeginDate = c.PeriodRequired ? contract.ContractDate : default,
                EndDate = c.PeriodRequired ? contract.ContractDate.AddYears(c.DefaultPeriodAddedInYears ?? 0) : default
            }).ToList());

            //данные о мерчанте (продавце)
            var loanSubjects = _loanSubjectRepository.List(new ListQuery() { Page = null });

            contract.Subjects = new List<ContractLoanSubject>();
            contract.Subjects.AddRange(loanSubjects.Where(s => s.Id == 1).Select(s => new ContractLoanSubject()
            {
                SubjectId = s.Id,
                AuthorId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                ClientId = 960 //Id клиентской карты Мерчанта
            }).ToList());

            _contractRepository.Insert(contract);

            contract.ContractRates = new List<ContractRate>() { new ContractRate { } };

        }

        [TestMethod()]
        public void GetApplicationList()
        {
            int? authorId = 456;
            var applications = _applicationRepository.List(new ListQuery() { Page = new Page() { Limit = 50 } }, new { AuthorId = authorId });
            Assert.AreEqual(applications.Count, 34);

            authorId = 167;
            applications = _applicationRepository.List(new ListQuery() { Page = new Page() { Limit = 50 } }, new { AuthorId = authorId });
            Assert.AreEqual(applications.Count, 34);

            var count = _applicationRepository.Count(new ListQuery() { Page = new Page() { Limit = 50 } }, new { AuthorId = authorId });
            Assert.AreEqual(count, 34);

            applications = _applicationRepository.List(new ListQuery() { Page = new Page() { Limit = 60 } });
            Assert.AreEqual(applications.Count, 51);

            applications = _applicationRepository.List(new ListQuery() { Filter = "1", Page = new Page() { Limit = 60 } });

        }

        [TestMethod()]
        public void InsertApplicationDetails()
        {
            var applicationDetails = new ApplicationDetails();
            applicationDetails.ApplicationId = 16;
            applicationDetails.ProdKind = ProdKind.Light;
            applicationDetails.InsuranceRequired = false;
            applicationDetails.ContractId = 41319; // 41197;

            _applicationDetailsRepository.Insert(applicationDetails);
        }

        [TestMethod()]
        public void GetApplicationDetails()
        {
            var applicationDetails = _applicationDetailsRepository.Get(16);
        }
    }*/
}
