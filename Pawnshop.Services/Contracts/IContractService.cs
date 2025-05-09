using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Vehicle;
using static Pawnshop.Services.Contracts.LoanFinancePlans.LoanFinancePlanSerivce;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Pawnshop.Services.Insurance;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Restructuring;

namespace Pawnshop.Services.Contracts
{
    public interface IContractService : IService<Contract>
    {
        Contract Get(int id, DateTime? date = null);
        Contract Find(ContractFilter filter);
        Contract Save(Contract model);
        void Delete(int id);
        ListModel<Contract> ListWithCount(ListQueryModel<ContractFilter> listQuery);
        List<Contract> List(ContractFilter filter);
        int Count(ContractFilter filter);
        Task<List<Contract>> FindForProcessingAsync(int clientId);
        Task<List<Contract>> FindCreditLinesForProcessing(int clientId);
        decimal GetPrepaymentBalance(int contractId, DateTime? date = null);
        decimal GetReceivableOnlinePaymentBalance(int contractId, DateTime? date = null);
        decimal GetExtraExpensesCost(int contractId, DateTime? date = null);
        List<ContractPaymentSchedule> GetOnlyPaymentSchedule(int ContractId);
        decimal GetAccountBalance(int contractId, DateTime? date = null);
        decimal GetOverdueAccountBalance(int contractId, DateTime? date = null);
        decimal GetProfitBalance(int contractId, DateTime? date = null);
        decimal GetOverdueProfitBalance(int contractId, DateTime? date = null);
        decimal GetPenyAccountBalance(int contractId, DateTime? date = null);
        decimal GetPenyProfitBalance(int contractId, DateTime? date = null);
        decimal GetContractAccountBalance(int contractId, string accountSettingCode, DateTime? date = null);
        decimal GetTotalDue(int contractId, DateTime? date = null);
        decimal GetDepoMerchantBalance(int contractId, DateTime? date = null);
        List<Contract> GetContractsByPaymentScheduleFilter(DateTime? fromDate, DateTime? endDate, IEnumerable<ContractStatus> contractStatuses, IEnumerable<PercentPaymentType> neededPercentPaymentTypes = null);
        void AllowContractPrepaymentReturn(Contract contract);
        decimal GetDebtAndDebtOverdueBalanceForClient(int clientId, string productCode);
        LoanPercentSetting GetProductSettings(int settingId);
        decimal GetAvailableBalanceForDAMU(AvailBalanceRequest availBalancerequest);
        Contract GetOnlyContract(int contractId, bool withoutCheckContract = false);
        void ContractStatusUpdate(int contractId, ContractStatus status);
        LoanPercentSetting GetSettingForContract(Contract contract, decimal? loanCost = null);
        decimal GetPenaltyLimitBalance(int contractId, DateTime? date = null);
        List<Contract> GetContractsForDecreasePenaltyRates(DateTime? date);
        decimal GetChildContractCost(int contractId, DateTime actionDate, decimal actionCost);
        string GenerateContractNumber(DateTime contractDate, int branchId, string numberCode);
        decimal GetPreApprovedAmount(int modelId, int releaseYear);
        Boolean AreAllRelatedContractsBoughtOut(int contractId);
        ContractModel ChangeForPensioner(ContractModel model);
        void SetBuyoutReasonForContractAction(Contract contract);
        void CheckNumberOfPositionsForContractCarCollateralType(Contract contract);
        void CheckPositionsForDuplicates(Contract contract);
        void CheckFirstPositionCollateralTypeSameAsContractCollateralType(Contract contract);
        void CheckPositionLiquidity(Contract contract);
        void CheckMaxPossibleContractPeriod(Contract contract);
        List<Contract> GetActiveContractsByClientId(int clientId);
        List<Contract> GetActiveContractsByClientId(int clientId, int contractId);
        Contract FillPositions4Contract(int contractId);
        int GetPaymentsCount(Contract contract);
        void CheckSchedule(Contract contract, bool isChangeCD = false);
        PositionDetails GetContractPositionDetail(ContractPosition contractPosition);
        LoanPercentSetting GetSettingForContract(Contract contract, int? settingId);
        Task FillCollateralCostForContractPositions(Contract contract);
        void FillPositionContractNumbers(Contract contract);
        void FillPositionSubjectsAndHasPledge(Contract contract);
        List<ContractPosition> FillPositionContractNumbers(Contract contract, List<ContractPosition> positions);
        Task<int> GetCreditLineId(int contractId);
        Task<decimal> GetCreditLineLimit(int contractId);
        Task<int> GetActiveTranchesCount(int trancheContractId);
        Task CalculateAPR(Contract contract);
        decimal CalculateAPRAfterRestructuring(CalculateAPRModel model);
        Task<List<Contract>> GetAllSignedTranches(int creditLineContractId);
        Task<List<Contract>> GetAllTranchesAsync(int creditLineContractId);
        IList<ContractTrancheInfo> GetTranches(int creditLineId);
        Task<List<ContractPosition>> GetPositionsByContractIdAsync(int contractId);
        List<ContractPosition> GetPositionsByContractId(int contractId);
        Task<Contract> GetNonCreditLineByNumberAsync(string contractNumber);
        Task<Contract> GetCreditLineByNumberAsync(string contractNumber);
        AnnuityType GetAnnuityType(Contract contract);
        Task<IEnumerable<Contract>> GetContractsByVinAsync(string vin);
        IList<ContractBalance> GetBalances(IList<int> contractIds);
        Task<IEnumerable<ContractBalance>> GetBalancesAsync(IList<int> contractIds);
        Task<IEnumerable<Contract>> GetListForOnlineByIinAsync(string iin);
        Task<IEnumerable<Contract>> GetHistoryForOnlineByIinAsync(string iin);
        Task<bool> HasPartialPaymentAsync(int id);
        Task<int> GetTranchesCount(int creditLineContractId);
        Task<string> GetWaitPayTypeOperationCode(int contractId);
        Task<IEnumerable<Contract>> GetTrancheListForOnlineByCreditLineIdAsync(int creditLineId);
        bool IsOnline(int contractId);
        Task CreditLineFillConsolidateSchedule(Contract contract, bool includeOffBalance = true);
        Task CheckAndChangeStatusForRealtyContractsOnSave(Contract contract);
        Task<Contract> GetAsync(int id, DateTime? date = null);
        Task<Contract> GetOnlyContractAsync(int contractId, bool withoutCheckContract = false);
        List<int> GetOffbalanceAdditionContractIds(DateTime inscriptionOnOrAfter, int? branchId = null);
        Task<IEnumerable<Contract>> FindListByIdentityNumberAsync(string iin);
        Task<Contract> CreateFirstTranche(Contract creditLine, FirstTranche firstTranche, int authorId,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractKdnService contractKdnService,
            bool isMobApp = false);
        bool ReissueAutocredit(ReissueCarModel model);
        bool CheckBlackListOnActionType(Contract contract, ContractActionType actionType);
        bool CheckCategoryLimitSum(Contract contract, decimal costSum, bool categoryChanged = true, decimal? additionalLimit = null);
        bool ChangeCategory(ContractAction action, Contract contract, decimal costSum, decimal? additionalLimit = null);
        bool CancelChangeCategory(Contract contract, ContractAction contractAction);
        int GetPeriodTypeId(DateTime maturityDate);
        DateTime? GetNextPaymentDateByCreditLineId(int creditLineId);
        Task<PaymentAmount> GetPaymentAmount(int id, Contract contract = null);
        Task<ContractBalance> GetCreditLineTotalBalance(int creditLineId, Contract contract = null);
        DebtInfo GetDebtInfoByCreditLine(int creditLineId);
        public (bool isCategoryChange, bool checkLeftLoanCostForLight, bool checkLeftLoanCostForMotor, decimal maxSumByAnaliticsCategory) ChangeCategoryForCredilLineData(
            Application application, bool isCategoryMotor, ProdKind prodKind, decimal applicationAdditionalLimit, decimal debt, int settingId);
        public bool IsContractBusinessPurpose(Contract contract);
        public void ValidateBusinessLoanPurpose(Contract contract);
        public void SaveContractExpertOpinionData(int contractId);
        (bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn) GetContractSettings(int contractId);
        bool IsKDNPassedForOffline(int contractId);
        Task IsCanEditFirstPaymentDate(Contract contract);
        Task<bool> CarHasClientAsync(int contractId);
        Task<bool> IncompleteActionExistsAsync(int contractId);
        Task<bool> HasExpenses(int contractId);
        public Task<ContractBalance> GetBalance(int contractId);
        Task UpdatePeriodType(int periodTypeId, int contractId);
        Task<DateTime?> GetNearestTranchePaymentDateOfCreditLine(int creditLineId);
    }
}
