using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public class ContractStatusHistoryService : IContractStatusHistoryService
    {
        private readonly ContractStatusHistoryRepository _contractStatusHistoryRepository;

        public ContractStatusHistoryService(ContractStatusHistoryRepository contractStatusHistoryRepository)
        {
            _contractStatusHistoryRepository = contractStatusHistoryRepository;
        }

        public async Task<ContractStatusHistory> SaveToStatusChangeHistory(int contractId, ContractStatus status, DateTime date, int userId, bool enforceSavingToHistory = false)
        {
            var historyItem = new ContractStatusHistory();
            historyItem.ContractId = contractId;
            historyItem.Status = status;
            historyItem.Date = date.Date;
            historyItem.UserId = userId;
            historyItem.AuthorId = userId;
            historyItem.CreateDate = DateTime.Now;

            //если не было смены статуса с последней смены статуса, то не сохраняем в историю 
            var lastHistoryItem = await _contractStatusHistoryRepository.GetLastStatusHistoryItemForContract(contractId);
            
            //ситуации, при которых обязательно сохранение в истории статуса, даже если сам статус не поменялся (например, согласование договора) 
            if(!enforceSavingToHistory)
                if (lastHistoryItem?.Status == status)
                    return lastHistoryItem;

            _contractStatusHistoryRepository.Insert( historyItem );
            return historyItem;
        }
    }
}
