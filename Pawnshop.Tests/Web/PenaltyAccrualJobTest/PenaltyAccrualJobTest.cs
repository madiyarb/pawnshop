using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Penalty;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.PenaltyLimit;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Tests.Web.PenaltyAccrualJobTest
{
    [TestClass()]
    public class PenaltyAccrualJobTest
    {
        /*private ContractRepository _contractRepository;
        private ContractRateRepository _contractRateRepository;
        private IPenaltyRateService _penaltyRateService;
        private AccrualBaseService _accrualBaseService;
        private AccountSettingRepository _accountSettingRepository;

        public PenaltyAccrualJobTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test_20210914;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";
            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);

            ClientRepository _clientRepository = new ClientRepository(unitOfWork);
            LoanSubjectRepository _loanSubjectRepository = new LoanSubjectRepository(unitOfWork);
            LoanProductTypeRepository _loanProductTypeRepository = new LoanProductTypeRepository(unitOfWork);
            GroupRepository _groupRepository = new GroupRepository(unitOfWork);
            
            LoanPercentSettingInsuranceCompanyRepository insuranceCompanyRepository = new LoanPercentSettingInsuranceCompanyRepository(unitOfWork);
            LoanSettingRateRepository loanSettingRateRepository = new LoanSettingRateRepository(unitOfWork);
            ISessionContext sessionContext = new SessionContext();
            LoanPercentRepository _loanPercentRepository = new LoanPercentRepository(unitOfWork, insuranceCompanyRepository, loanSettingRateRepository, sessionContext);

            _contractRateRepository = new ContractRateRepository(unitOfWork);
            _contractRepository = new ContractRepository(unitOfWork, _loanPercentRepository, _clientRepository, _loanSubjectRepository, _loanProductTypeRepository, _groupRepository, _contractRateRepository);

            SqlMapper.AddTypeHandler(new JsonObjectHandler<PermissionListDefinition>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<GoldContractSpecific>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractActionData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<Configuration>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceData>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<ContractRefinanceConfig>());
            SqlMapper.AddTypeHandler(new JsonObjectHandler<InsuranceRequestData>());

            var accrualBaseRepository = new AccrualBaseRepository(unitOfWork);
            _accrualBaseService = new AccrualBaseService(accrualBaseRepository);

            _accountSettingRepository = new AccountSettingRepository(unitOfWork);
        }

        [TestMethod()]
        public void RunPenaltyAccrualJob()
        {
            var contract = _contractRepository.Get(361920);
        }

        [TestMethod()]
        public void FillOverdueAmounts()
        {
            List<PenaltyRates> calcPenaltyList = new List<PenaltyRates>()
            { 
                new PenaltyRates(new DateTime(2021, 10, 15), 0, 0.4M, false),
                new PenaltyRates(new DateTime(2021, 10, 16), 12566.54M, 0, false),
                new PenaltyRates(new DateTime(2021, 09, 15), 0, 0.5M, false),
                new PenaltyRates(new DateTime(2022, 01, 14), 0, 0.03M, false),
                new PenaltyRates(new DateTime(2022, 01, 20), 0, 0.6M, false),
                new PenaltyRates(new DateTime(2021, 08, 15), 11256M, 0, false),
                new PenaltyRates(new DateTime(2021, 07, 15), 0, 0.7M, false),
                new PenaltyRates(new DateTime(2021, 06, 15), 0, 0.8M, false)
            };

            calcPenaltyList = calcPenaltyList.OrderByDescending(x => x.Date).ToList();

            FillOverdueAmounts(calcPenaltyList);
            FillRates(calcPenaltyList);
        }

        private void FillOverdueAmounts(List<PenaltyRates> calcPenaltyList)
        {
            calcPenaltyList = calcPenaltyList.OrderByDescending(x => x.Date).ToList();

            decimal prevAmount = 0;
            foreach (var calcPenalty in calcPenaltyList)
            {
                if (calcPenalty.OverdueSum == 0)
                    calcPenalty.OverdueSum = prevAmount;
                else
                    prevAmount = calcPenalty.OverdueSum.Value;
            }
        }

        private void FillRates(List<PenaltyRates> calcPenaltyList)
        {
            var contractRates = calcPenaltyList.Where(x=>x.Rate!=0).ToList().OrderByDescending(x => x.Date);

            calcPenaltyList.ForEach(x=> { 
                if (x.Rate == 0)
                    x.Rate = contractRates.Where(r => r.Date < x.Date).First().Rate;
            });
        }

        [TestMethod()]
        public void JoinTwoLists()
        {
            List<PenaltyRates> calcPenaltyList = new List<PenaltyRates>()
            {
                new PenaltyRates(new DateTime(2021, 10, 15), 0, 0.4M, false),
                new PenaltyRates(new DateTime(2021, 09, 15), 0, 0.5M, false),
                new PenaltyRates(new DateTime(2022, 01, 14), 0, 0.03M, false),
                new PenaltyRates(new DateTime(2022, 01, 20), 0, 0.6M, false),
                new PenaltyRates(new DateTime(2021, 07, 15), 0, 0.7M, false),
                new PenaltyRates(new DateTime(2021, 06, 15), 0, 0.8M, false)
            };

            List<PenaltyRates> overdueList = new List<PenaltyRates>()
            {
                new PenaltyRates(new DateTime(2021, 10, 16), 12566.54M, 0, true),
                new PenaltyRates(new DateTime(2021, 08, 15), 11256M, 0, true),
                new PenaltyRates(new DateTime(2021, 09, 15), 11256M, 0, true),
                new PenaltyRates(new DateTime(2022, 01, 14), 11256M, 0, true),
                new PenaltyRates(new DateTime(2021, 06, 15), 11256M, 0, true)
            };

            ConcatListsWithoutDublicate(overdueList, calcPenaltyList);

            var result = calcPenaltyList.Concat(overdueList).ToList();


            IDictionary<AmountType, decimal> amounts = new ConcurrentDictionary<AmountType, decimal>();

            amounts[AmountType.PenaltyLimit] = 1;
            amounts[AmountType.PenaltyWriteOffByLimit] = 2;

            amounts.Clear();

        }

        private List<PenaltyRates> ConcatListsWithoutDublicate(List<PenaltyRates> mainList, List<PenaltyRates> conactList)
        {
            foreach (var concatElement in conactList)
            {
                if (!mainList.Contains(concatElement, new PenaltyRateComparer()))
                    mainList.Add(concatElement);
            }
            return mainList;
        }

        [TestMethod()]
        public void GetContractsForDecrease()
        {
            var date = new DateTime(2021, 09, 17);
            //var contract = _contractRepository.GetContractsForDecreasePenaltyRates(date);

            var contract = _contractRepository.Get(361920);

            var _accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;

            _accrualSettings.ForEach(x=> {                
                ContractRate contractRate = _contractRateRepository.FindRateOnDateByContractAndRateSettingId(contract.Id, (int)x.RateSettingId, date);
                if (contractRate.Rate > Constants.NBRK_PENALTY_RATE)
                {
                    List<ContractRate> contractRates = new List<ContractRate>() { new ContractRate { ContractId = contract.Id, 
                                                                                                     Date = date, 
                                                                                                     RateSettingId = (int)x.RateSettingId, 
                                                                                                     Rate = Constants.NBRK_PENALTY_RATE , 
                                                                                                     CreateDate = DateTime.Now, 
                                                                                                     AuthorId = Constants.ADMINISTRATOR_IDENTITY
                                                                                                    } 
                                                                                };
                    _contractRateRepository.DeleteAndInsert(contractRates);
                }
            });
        }

        [TestMethod()]
        public void GetActualPenaltyRateForContract()
        {
            var date = new DateTime(2021, 10, 10);
            var contract = _contractRepository.Get(361822);

            var _accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;
            _accrualSettings.ForEach(x => {
                decimal rate = GetActualPenaltyRate(contract, date, (int)x.RateSettingId);

                ContractRate contractRate = _contractRateRepository.FindRateOnDateByContractAndRateSettingId(contract.Id, (int)x.RateSettingId, date);
                if (contractRate is null)
                    throw new NullReferenceException($"Массив процентных ставок пустой по Договору {contract.Id}");

            });
        }

        private decimal GetActualPenaltyRate(Contract contract, DateTime date, int rateSettingId)
        {
            var currentContractRate = contract.ContractRates.Where(x => x.DeleteDate is null && x.Date <= date && x.RateSettingId == rateSettingId).OrderByDescending(x => x.Date).FirstOrDefault();
            
            if (currentContractRate is null)
                throw new NullReferenceException($"Не найдена ставка пени для Договора {contract.Id} действующая на дату {date:dd.MM.yyyy}");

            return currentContractRate.Rate;
        }
        */
    }
}
