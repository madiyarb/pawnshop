using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    public interface IContractPaymentScheduleService
    {
        void Delete(int scheduleId, int authorId);
        void Save(List<ContractPaymentSchedule> schedule, int contractId, int authorId, bool logDelete = true);
        ContractPaymentSchedule Get(int id);
        List<ContractPaymentSchedule> GetListByContractId(int contractId, bool withoutCheckContract = false);
        ContractPaymentScheduleRevision GetRevision(int id, int revisionId);
        Task<List<ContractPaymentScheduleVersion>> GetScheduleVersions(int ContractId);
        Task<List<ContractPaymentScheduleVersion>> GetScheduleVersionsWithoutChangedControlDate(int ContractId);
        Task<List<RestructuredContractPaymentSchedule>> GetScheduleByAction(int ActionId);
        Task<List<ContractPaymentSchedule>> GetScheduleByActionForChangeDate(int ActionId);
		Task<List<ContractPaymentSchedule>> GetScheduleByHistory(int HistoryId);
		Task UpdateActionIdForPartialPayment(int ActionId, DateTime ActionDate, int ContractId);
        Task UpdateActionIdForPartialPaymentUnpaid(int ActionId, DateTime ActionDate, int ContractId, decimal penaltyCost, bool isEndPeriod);
        Task<ContractPaymentSchedule> GetUnpaidSchedule(int ContractId);
        Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterPartialPayment(int ActionId, int ContractId);
        Task<List<ContractPaymentSchedule>> GetScheduleRowsBeforePartialPayment(int ContractId);
        Task<int> InsertContractPaymentScheduleHistory(int ContractId, int ActionId, int Status);
        Task InsertContractPaymentScheduleHistoryItems(int ContractPaymentScheduleHistoryId, ContractPaymentSchedule item);
        Task UpdateContractPaymentScheduleHistoryStatus(int ContractId, int ActionId, int Status);
        Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterLastPartialPayment(int ContractId, int ActionId);
        Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleRow(int ContractId, int ActionId);
        Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleHistoryRow(int ContractId, int ActionId);
        Task RollbackScheduleToPreviousPartialPayment(int ContractId, int ActionId, decimal PartialPaymentCost);
        Task<ContractPaymentSchedule> GetNextPaymentSchedule(int ContractId, bool nowPeriodPayment = false);
        Task<decimal> GetAverageMonthlyPaymentAsync(int contractId);
        bool IsNeedUpdatePaymentSchedule(List<ContractPaymentSchedule> paymentSchedule, int contractId);
        void UpdateFirstPaymentInfo(int contractId, Contract contract = null);
        Task DeleteContractPaymentScheduleHistory(int ContractId, int ActionId);
        Task<Contract> SaveBuilderByControlDate(ContractPaymentScheduleUpdateModel updateModel);
    }
}
