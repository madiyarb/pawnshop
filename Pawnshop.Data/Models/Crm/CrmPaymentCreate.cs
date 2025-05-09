using Newtonsoft.Json;
using Pawnshop.Data.Models.Collection;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmPaymentCreate : CrmPaymentBaseAction
    {
        public CrmPaymentCreate(
            Contracts.Contract contract,
            int categoryId,
            string defaultContact,
            decimal loanCostLeft,
            decimal loanPercentCost,
            decimal penaltyPercentCost,
            decimal prepayment,
            decimal buyoutAmount,
            decimal prolongAmount,
            int overdueContracts,
            CollectionContractStatus collectionStatus,
            CrmStatus currentStatus,
            string prodUrl
            ) 
            : base(
                contract,
                categoryId,
                null,
                defaultContact,
                loanCostLeft,
                loanPercentCost,
                penaltyPercentCost,
                prepayment,
                buyoutAmount,
                prolongAmount,
                overdueContracts,
                collectionStatus,
                prodUrl)
        {
            CrmStage = currentStatus.GetCrmStage(_categoryId);
        }

        /// <summary>
        /// Идентификатор категории
        /// </summary>
        [JsonProperty("CATEGORY_ID")]
        public string CategoryId => _categoryId.ToString();

        /// <summary>
        /// Статус сделки
        /// </summary>
        [JsonProperty("STAGE_ID")]
        public string CrmStage;
    }
}
