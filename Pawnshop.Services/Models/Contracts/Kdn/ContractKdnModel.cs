using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class ContractKdnModel
    {
        public bool IsKdnRequired { get; set; }
        public bool IsKdnPassed { get; set; }
        public List<string> KdnError { get; set; }
        public string GamblerError { get; set; }
        public bool IsFirstTransh { get; set; } = false;
        public decimal TotalAmount4Transh { get; set; } = 0;
        public List<ContractPaymentSchedule> VirtualPaymentSchedule { get; set; }
        public List<ContractKdnEstimatedIncomeModel> ContractKdnEstimatedIncomeModels { get; set; }
        public int CalcLogId { get; set; }
        public bool CalcKdn4Addition { get; set; }
        public Contract? ChildContract { get; set; }
        public decimal TotalAmount4TranshWithPremium { get; set; } = 0;
        public decimal TotalAspIncome { get; set; } = 0;
        public decimal TotalContractDebts { get; set; } = 0;
        public decimal TotalOtherClientPayments { get; set; } = 0;
        public decimal TotalIncome { get; set; } = 0;
        public decimal TotalDebt { get; set; } = 0;
        public decimal TotalFcbDebt { get; set; } = 0;
        public decimal Kdn { get; set; } = 0;
        public decimal KdnK4 { get; set; } = 0;
        public decimal TotalFamilyDebt { get; set; } = 0;
        public decimal TotalAllLoan { get; set; } = 0;
        public decimal EpDelta { get; set; } = 0;
        public decimal AvgPaymentToday { get; set; } = 0;
        public Contract Contract { get; set; }
        public decimal K1Income { get; set; } = 0;
        public decimal K2Income { get; set; } = 0;
        public decimal K3Income { get; set; } = 0;
        public decimal K4Income { get; set; } = 0;
    }
}
