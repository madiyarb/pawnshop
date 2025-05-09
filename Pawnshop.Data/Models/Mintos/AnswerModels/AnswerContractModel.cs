using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos.UploadModels;

namespace Pawnshop.Data.Models.Mintos.AnswerModels
{
    public class AnswerContractModel
    {
        public AnswerContractDataModel data { get; set; }

        public MintosContract ConvertToMintosContract(Currency defaultCurrency, MintosUploadQueue contractToUpload, MintosConfig config)
        {
            var contract = new MintosContract();

            contract.ContractCurrencyId = defaultCurrency.Id;
            contract.ContractCurrency = defaultCurrency;
            contract.ContractId = contractToUpload.ContractId;
            contract.MintosCurrencyId = contractToUpload.CurrencyId;
            contract.MintosCurrency = contractToUpload.Currency;
            contract.OrganizationId = config.OrganizationId;
            contract.ExchangeRate = config.Currency.ExchangeRate;
            contract.InvestorInterestRate = Parse(data.loan.interest_rate_percent);
            contract.MintosId = data.loan.mintos_id;
            contract.MintosPublicId = data.loan.public_id;
            contract.MintosStatus = data.loan.status;
            contract.UploadDate = contractToUpload.UploadDate ?? (contractToUpload.CreateDate.Date < DateTime.Now.Date ? DateTime.Parse(data.loan.mintos_issue_date.date) : DateTime.Now);
            contract.LoanCost = Parse(data.loan.loan_amount);
            contract.LoanCostAssigned = Parse(data.loan.loan_amount_assigned_to_mintos);
            contract.FinalPaymentDate = DateTime.Parse(data.loan.final_payment_date.date);

            foreach (var item in data.payment_schedule)
            {
                try
                {
                    contract.PaymentSchedule.Add(item.ConvertToSaved(contractToUpload.ContractId));
                }
                catch (Exception e)
                {
                    throw new PawnshopApplicationException($"Ошибка \"{e.Message}\" при попытке распарса графика mintos {item.number} от {item.date}");
                }
            }

            return contract;
        }

        private decimal Parse(string s)
        {
            s = s.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            return decimal.Parse(s, NumberStyles.Any,
                CultureInfo.InvariantCulture);
        }
    }
}
