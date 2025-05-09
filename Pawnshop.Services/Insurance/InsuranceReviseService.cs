using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using System.Collections.Generic;
using System.Linq;
using System;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Services.Insurance
{
    public class InsuranceReviseService : BaseService<InsuranceRevise>, IInsuranceReviseService
    {
        private readonly IRepository<InsuranceRevise> _insuranceReviseRepository;
        private readonly InsurancePolicyRepository _insurancePolicyRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly ISessionContext _sessionContext;
        private readonly IEventLog _eventLog;
        private readonly DateTime newAgencyFeesDate = new DateTime(2023, 6, 30);

        public InsuranceReviseService(IRepository<InsuranceRevise> insuranceReviseRepository,
                                      InsurancePolicyRepository insurancePolicyRepository,
                                      ClientRepository clientRepository,
                                      ContractRepository contractRepository,
                                      GroupRepository groupRepository,
                                      ISessionContext sessionContext,
                                      IEventLog eventLog
            ) : base(insuranceReviseRepository)
        {
            _insuranceReviseRepository = insuranceReviseRepository;
            _insurancePolicyRepository = insurancePolicyRepository;
            _clientRepository = clientRepository;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _sessionContext = sessionContext;
            _eventLog = eventLog;
        }

        public InsuranceRevise CreateInsuranceRevise(InsuranceReviseRequest req)
        {
            InsuranceRevise revise = new InsuranceRevise();
            try
            {
                List<InsuranceReviseRow> rows = new List<InsuranceReviseRow>();
                List<InsurancePolicy> policiesFromFile = new List<InsurancePolicy>();
                List<InsurancePolicy> policies = _insurancePolicyRepository.GetListByPeriod(req.beginDate,req.endDate);
                if (policies == null || policies.Count == 0) throw new PawnshopApplicationException("На данным датам нету полючов в Финкоре");

                (int, decimal) returned = GetinsurancePoliciesFromFile(req.file, policiesFromFile);
                if (policiesFromFile == null || policiesFromFile.Count == 0) throw new PawnshopApplicationException("После обработки файла, подходящих данных нету");
                revise.Rows = rows;
                revise.CreateDate = DateTime.Now;
                revise.AutorId = _sessionContext.UserId;
                revise.AutorName = _sessionContext.UserName;
                revise.Period = @$"{req.beginDate.ToString("dd/MM/yyyy")} - {req.endDate.ToString("dd/MM/yyyy")}";
                revise.Status = 10;

                //Расторженные
                revise.ReturnPolicies = returned.Item1;
                revise.ReturnAgencyFees = returned.Item2;

                //Страховая компаниия
                Client client = _clientRepository.GetOnlyClient(req.insuranceCompanyId);
                revise.InsuranceCompanyId = req.insuranceCompanyId;
                revise.InsuranceCompanyName = client.FullName;

                //Количество полисов
                revise.TotalInsurancePoliciesFinCore = policies.Count;
                revise.TotalInsurancePoliciesInsuranceCompany = policiesFromFile.Count;

                //Итоговая страховая сумма
                revise.TotalInsuranceAmountFinCore = policies.Sum(x => x.EsbdAmount==0 ? x.InsuranceAmount : x.EsbdAmount);
                revise.TotalInsuranceAmountInsuranceCompany = policiesFromFile.Sum(x => x.InsuranceAmount);

                //Страховая премия
                revise.TotalSurchargeAmountFinCore = policies.Sum(x => x.SurchargeAmount);
                revise.TotalSurchargeAmountInsuranceCompany = policiesFromFile.Sum(x => x.SurchargeAmount);

                //Агентское вознаграждение
                revise.TotalAgencyFeesFinCore = policies.Sum(x => x.EsbdAmount == 0 ? x.InsuranceAmount : x.EsbdAmount);
                revise.TotalAgencyFeesInsuranceCompany = policiesFromFile.Sum(x => x.InsurancePremium);

                base.Save(revise);

                foreach (InsurancePolicy policy in policies)
                {
                    InsurancePolicy policyFromFile = policiesFromFile.Where(x => x.PoliceNumber == policy.PoliceNumber).FirstOrDefault();

                    InsuranceReviseRow row = new InsuranceReviseRow();
                    row.InsuranceReviseId = revise.Id;
                    row.CreateDate = DateTime.Now;
                    CheckPolicyWithFile(row, policy, policyFromFile);

                    revise.Rows.Add(row);
                }

                var newPoliciesFromFile = policiesFromFile.Where(x => x.Id != -1).ToList();
                foreach (InsurancePolicy policyFromFile in newPoliciesFromFile)
                {

                    InsuranceReviseRow row = new InsuranceReviseRow();
                    row.InsuranceReviseId = revise.Id;
                    row.CreateDate = DateTime.Now;

                    var canceledPolicy = _insurancePolicyRepository.GetDeleted(policyFromFile.PoliceNumber);
                    if (canceledPolicy == null)
                    {
                        row.InsurancePolicyNumber = policyFromFile.PoliceNumber;
                        row.Status = 90;
                        row.Message = @$"Данный номер полиса: {policyFromFile.PoliceNumber} отсутствует в данных FinCore ";
                        revise.Rows.Add(row);
                    }
                    else
                    {
                        CheckPolicyWithFile(row, canceledPolicy, policyFromFile);

                        revise.Rows.Add(row);
                    }
                }

                _insuranceReviseRepository.Update(revise);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException("Что то пошло не так при сверке");
            }
            return revise;
        }

        private void CheckPolicyWithFile(InsuranceReviseRow row, InsurancePolicy policy, InsurancePolicy policyFromFile)
        {
            try
            {
                row.InsurancePolicyId = policy.Id;
                row.InsurancePolicyNumber = policy.PoliceNumber;
                row.InsuranceStartDate = policy.StartDate;
                row.InsuranceEndDate = policy.EndDate;
                row.SurchargeAmount = policy.SurchargeAmount;
                row.AgencyFees = policy.StartDate.Date < newAgencyFeesDate ? policy.SurchargeAmount * (decimal)0.9 : policy.SurchargeAmount * (decimal)0.93;
                row.InsuranceAmount = policy.EsbdAmount == 0 ? policy.InsuranceAmount : policy.EsbdAmount;

                if (policy.ContractId != null)
                {
                    Contract policyContract = _contractRepository.GetOnlyContract((int)policy.ContractId);
                    if (policyContract != null)
                    {
                        Client policyClient = _clientRepository.GetOnlyClient(policyContract.ClientId);
                        Group policyBranch = _groupRepository.Get(policyContract.BranchId);
                        row.ClientId = policyClient.Id;
                        row.ClientFullName = policyClient.FullName;
                        row.ClientIdentityNumber = policyClient.IdentityNumber;
                        row.BranchId = policyContract.BranchId;
                        row.BranchName = policyBranch.DisplayName;
                    }
                }

                if (policyFromFile == null)
                {
                    row.Status = 80;
                    row.Message = @$"Данный номер полиса: {policy.PoliceNumber} отсутствует в отчете с СК";
                }
                else
                {
                    policyFromFile.Id = -1;
                    if (policy.StartDate.Date != policyFromFile.StartDate.Date)
                    {
                        row.Status = 20;
                        row.Message = @$"В отчете с СК в поле 'Дата начала действия договора' указано: {policyFromFile.StartDate.ToString("dd/MM/yyyy")}";
                    }
                    else if (policy.EndDate.Date != policyFromFile.EndDate.Date)
                    {
                        row.Status = 30;
                        row.Message = @$"В отчете с СК в поле 'Дата окончания действия договора' указано: {policyFromFile.EndDate.ToString("dd/MM/yyyy")}";
                    }
                    else if (row.SurchargeAmount != policyFromFile.SurchargeAmount)
                    {
                        row.Status = 40;
                        row.Message = @$"В отчете с СК в поле 'Страховая премия' указано: {policyFromFile.SurchargeAmount}";
                    }
                    else if (row.AgencyFees != policyFromFile.InsurancePremium)
                    {
                        row.Status = 50;
                        row.Message = @$"В отчете с СК в поле 'Агентское вознаграждение' указано: {policyFromFile.InsurancePremium}";
                    }
                    else if (row.InsuranceAmount != policyFromFile.InsuranceAmount)
                    {
                        row.Status = 60;
                        row.Message = @$"В отчете с СК в поле 'Итоговая страховая сумма' указано: {policyFromFile.InsuranceAmount}";
                    }
                    else
                    {
                        if (policy.ContractId != null)
                            row.Status = 10;
                        else row.Status = 11;
                    }
                }
            }
            catch (Exception)
            {
                throw new PawnshopApplicationException(@$"Ошибка при сверке полиcа {policy.PoliceNumber}");
            }

        }

        private (int,decimal) GetinsurancePoliciesFromFile(IFormFile file, List<InsurancePolicy> policiesFromFile)
        {
            try
            {
                int returnPolicies = 0;
                decimal returnAgencyFees = 0;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    ExcelWorksheet returned = package.Workbook.Worksheets[1];
                    int returnedRowCount = returned.Dimension.Rows;

                    returnPolicies = int.Parse(returned.Cells[returnedRowCount - 1, 1].Value?.ToString());
                    returnAgencyFees = decimal.Parse(returned.Cells[returnedRowCount, 17].Value?.ToString());

                    ExcelWorksheet report = package.Workbook.Worksheets[0];
                    int reportRowCount = report.Dimension.Rows;

                    for (int row = 3; row < reportRowCount; row++)
                    {
                        InsurancePolicy policy = new InsurancePolicy();
                        policy.PoliceNumber = report.Cells[row, 6].Value?.ToString();
                        policy.StartDate = DateTime.ParseExact(report.Cells[row, 9].Value?.ToString(), "dd/MM/yyyy", null);
                        policy.EndDate = DateTime.ParseExact(report.Cells[row, 10].Value?.ToString(), "dd/MM/yyyy", null);
                        policy.InsuranceAmount = Decimal.Round(decimal.Parse(report.Cells[row, 11].Value?.ToString()), 2);
                        policy.SurchargeAmount = Decimal.Round(decimal.Parse(report.Cells[row, 12].Value?.ToString()), 2);
                        policy.InsurancePremium = Decimal.Round(decimal.Parse(report.Cells[row, 17].Value?.ToString()), 2);

                        policiesFromFile.Add(policy);
                    }

                }
                return (returnPolicies, returnAgencyFees);
            }
            catch (Exception)
            {
                throw new PawnshopApplicationException("Ошибка в файле");
            }

        }
        
    }
}