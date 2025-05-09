using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Insurance
{
    public class InsuranceCompanySettingService : IInsuranceCompanySettingService
    {
        private readonly LoanPercentSettingInsuranceCompanyRepository _loanPercentSettingInsuranceCompanyRepository;

        public InsuranceCompanySettingService(LoanPercentSettingInsuranceCompanyRepository loanPercentSettingInsuranceCompanyRepository)
        {
            _loanPercentSettingInsuranceCompanyRepository = loanPercentSettingInsuranceCompanyRepository;
        }

        public LoanPercentSettingInsuranceCompany Save(LoanPercentSettingInsuranceCompany model, int userId)
        {
            using (var transaction = _loanPercentSettingInsuranceCompanyRepository.BeginTransaction())
            {
                model.AuthorId = userId;
                if (model.Id > 0)
                {
                    _loanPercentSettingInsuranceCompanyRepository.UpdateChangeableFileds(model);
                }
                else
                {
                    _loanPercentSettingInsuranceCompanyRepository.Insert(model);
                }

                transaction.Commit();
            }

            return model;
        }

        public ListModel<LoanPercentSettingInsuranceCompany> List(ListQueryModel<InsuranceSettingQueryModel> listQuery)
        {
            return new ListModel<LoanPercentSettingInsuranceCompany>
            {
                List = _loanPercentSettingInsuranceCompanyRepository.List(listQuery, listQuery.Model),
                Count = _loanPercentSettingInsuranceCompanyRepository.Count(listQuery, listQuery.Model)
            };
        }

        public LoanPercentSettingInsuranceCompany Card(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(id)} не может быть null для Deposit.Card");
            
            var insuranceCompanySetting = _loanPercentSettingInsuranceCompanyRepository.Get(id);
            if (insuranceCompanySetting is null)
                throw new PawnshopApplicationException($"Настройки для страховой компаний с Id {id} не найдено");

            return insuranceCompanySetting;
        }

        public List<LoanPercentSettingInsuranceCompany> InsuranceCompaniesList()
        {
            return _loanPercentSettingInsuranceCompanyRepository.InsuranceCompaniesList();
        }
    }
}
