using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Mintos.UploadModels;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosContract : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор договора в Mintos
        /// </summary>
        public int MintosId { get; set; }
        
        /// <summary>
        /// Идентификатор организации
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Номер договора в Mintos
        /// </summary>
        public string MintosPublicId { get; set; }

        /// <summary>
        /// Процент инвестора
        /// </summary>
        public decimal InvestorInterestRate { get; set; }

        /// <summary>
        /// Сумма договора в валюте Mintos
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Сумма договора к инвестированию в валюте Mintos
        /// </summary>
        public decimal LoanCostAssigned { get; set; }

        /// <summary>
        /// Статус договора
        /// Description of status values:
        /// Decision - new loan request for Loan Originator has been received, but the loan is not processed to active status;  
        /// Payout - sub-status between decision and active.If a loan doesn't match internal validation criteria, it usually stays in this status while it is processed manually;   
        /// Active - loan is listed in mintos platform; 
        /// Finished - loan has been bought back or all payments have been received;    
        /// Declined - manually declined new loan request(for example incorrect data);  
        /// </summary>
        public string MintosStatus { get; set; }

        /// <summary>
        /// Валюта договора
        /// </summary>
        public int ContractCurrencyId { get; set; }
        public Currency ContractCurrency { get; set; }

        /// <summary>
        /// Валюта договора в Mintos
        /// </summary>
        public int MintosCurrencyId { get; set; }
        public Currency MintosCurrency { get; set; }

        /// <summary>
        /// Курс обмена для операций по договору
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Дата закрытия/выкупа договора(ожидаемая)
        /// </summary>
        public DateTime FinalPaymentDate { get; set; }

        /// <summary>
        /// Дата выгрузки договора
        /// </summary>
        public DateTime UploadDate { get; set; }

        /// <summary>
        /// Дата удаления договора
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public List<MintosInvestorPaymentScheduleItem> PaymentSchedule = new List<MintosInvestorPaymentScheduleItem>();

        public List<MintosContractAction> FindNotUploadedActions(List<ContractAction>  actions, DateTime createDate)
        {
            var actionsToEnqueue = actions.Where(x => x.Date > createDate && (x.ActionType == ContractActionType.Prolong || x.ActionType == ContractActionType.Buyout || x.ActionType == ContractActionType.PartialBuyout || x.ActionType == ContractActionType.PartialPayment || x.ActionType == ContractActionType.MonthlyPayment || x.ActionType == ContractActionType.Addition || x.ActionType == ContractActionType.Refinance));

            return actionsToEnqueue.Select(action => new MintosContractAction(action)).ToList();
        }
    }
}
