using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Insurance.InsuranceCompanies;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Pawnshop.Tests.Web.InsurancePolicyServiceTest
{
    /*
    [TestClass()]
    public class InsurancePolicyServiceTest
    {
        IInsurancePolicyService _insurancePolicyService;
        ContractRepository _contractRepository;
        ClientRepository _clientRepository;

        public InsurancePolicyServiceTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210518;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            InsurancePolicyRepository _insurancePolicyRepository = new InsurancePolicyRepository(unitOfWork);
            InsuranceCompanyServiceFactory _insuranceCompanyServiceFactory = new InsuranceCompanyServiceFactory();

            OuterServiceSettingRepository _outerServiceSettingRepository = new OuterServiceSettingRepository(unitOfWork);

            _insurancePolicyService = new InsurancePolicyService(_insurancePolicyRepository, _insuranceCompanyServiceFactory, _outerServiceSettingRepository);

            _contractRepository = new ContractRepository(unitOfWork);
            _clientRepository = new ClientRepository(unitOfWork);
        }

        [TestMethod()]
        public void RegisterPolicy()
        {
            InsurancePoliceRequest insurancePoliceRequest = new InsurancePoliceRequest();
            insurancePoliceRequest.InsuranceCompanyId = 78135;
            insurancePoliceRequest.InsuranceCompany = new Client { IdentityNumber = "140940003807" };
            //insurancePoliceRequest.Contract = _contractRepository.Get(354754);

            var clientId = 75118; //MICRO_ENTREPRENEUR_NOT_REGISTERED
            int contractId = 362828;

            var validateFIO = IsClientForValidateFIO(clientId, contractId);            
            
            _insurancePolicyService.RegisterPolicy(insurancePoliceRequest);
        }

        private bool IsClientForValidateFIO(int clientId, int contractId)
        {
            var client = _clientRepository.Get(clientId);

            var contract = _contractRepository.GetOnlyContract(contractId);

            return true;
        }
    }
    */
}
