using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System.Linq;
using Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Services.HardCollection.Query
{
    public class GetContractOnlyQueryHandler : IRequestHandler<GetContractOnlyQuery, ContractDataOnly>
    {
        private readonly ContractRepository _contractRepository;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly IMediator _mediator;
        public GetContractOnlyQueryHandler(ContractRepository contractRepository, 
            CollectionStatusRepository collectionStatusRepository, 
            IMediator mediator,
            ContractAdditionalInfoRepository contractAdditionalInfoRepository)
        {
            _contractRepository = contractRepository;
            _collectionStatusRepository = collectionStatusRepository;
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _mediator = mediator;
        }

        public async Task<ContractDataOnly> Handle(GetContractOnlyQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                var contract = await _contractRepository.GetOnlyContractAsync(query.ContractId);
                var contractBalance = (await _contractRepository.GetBalancesAsync(new List<int>() { query.ContractId })).FirstOrDefault();
                var delayDays = await _collectionStatusRepository.GetDelayDaysAsync(query.ContractId);
                var collectionStatus = await _collectionStatusRepository.GetByContractIdAsync(query.ContractId);
                var contractAdditionalInfo = await _contractAdditionalInfoRepository.GetAsync(query.ContractId);

                var contractModel = new HardCollectionContract
                {
                    ContractNumber = contract.ContractNumber,
                    ContractId = query.ContractId,
                    ClientId = contract.ClientId,
                    ContractStatus = collectionStatus.CollectionStatusCode,
                    ContractDate = contract.ContractDate,
                    MaturityDate = contract.MaturityDate,
                    LoanPeriod = contract.LoanPeriod,
                    LoanAmount = contractBalance.AccountAmount,
                    ProfitAmount = contractBalance.ProfitAmount,
                    OverdueProfitAmount = contractBalance.OverdueProfitAmount,
                    ProlongAmount = contractBalance.ProfitAmount + contractBalance.OverdueProfitAmount + contractBalance.PenyAmount,
                    OverdueAccountAmount = contractBalance.OverdueAccountAmount,
                    PenyAmount = contractBalance.PenyAmount,
                    PrepaymentBalance = contractBalance.PrepaymentBalance,
                    ExpenseAmount = contractBalance.ExpenseAmount,
                    CurrentDebt = contractBalance.CurrentDebt,
                    TotalRedemptionAmount = contractBalance.TotalRedemptionAmount,
                    BranchId = contract.BranchId,
                    SelectedBranchId = contractAdditionalInfo?.SelectedBranchId,
                    DelayDays = delayDays
                };

                var model = new ContractDataOnly()
                {
                    Contract = contractModel
                };;

                return model;
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = query.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed, !query.IsJobWorking, false)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
