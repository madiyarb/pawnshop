using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.Contracts;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddExpenceCommandHandler : IRequestHandler<AddExpenceCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ContractRepository _contractRepository;
        private readonly IContractExpenseOperationService _expenceService;
        private readonly ExpenseRepository _expenseRepository;

        public AddExpenceCommandHandler(IMediator mediator, IContractExpenseOperationService expenceService, ContractRepository contractRepository, ExpenseRepository expenseRepository)
        {
            _mediator = mediator;
            _expenceService = expenceService;
            _contractRepository = contractRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<bool> Handle(AddExpenceCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var contract = await _contractRepository.GetOnlyContractAsync(request.ContractId);

                var expence = await _expenseRepository.GetByCodeAsync(request.ExpenceCode);
                ContractExpense contractExpence = new ContractExpense()
                {
                    ContractId = request.ContractId,
                    Date = DateTime.Now,
                    ExpenseId = expence.Id,
                    IsPayed = false,
                    Name = expence.Name,
                    Note = request.Note,
                    Reason = $"{expence.Name} по договору займа № {contract.ContractNumber} от {contract.ContractDate.Date}",
                    TotalCost = request.Cost,
                    UserId = request.AuthorId
                };

                if (contract.ContractClass == Data.Models.Contracts.ContractClass.Tranche)
                {
                    var creditLineId = await _contractRepository.GetCreditLineByTrancheId(contract.Id);
                    if (creditLineId != 0)
                    {
                        contractExpence.ContractId = creditLineId;
                    }
                }

                await _expenceService.RegisterAsync(contractExpence, request.AuthorId, request.BranchId, forcePrepaymentReturn: false, orderStatus: OrderStatus.WaitingForConfirmation);

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(contractExpence.Id),
                    LogNotification = request.GetLogNotifications($"Расход по договору {request.ContractId} сохранен")
                };
                await _mediator.Publish(notification);

                return true;
            }
            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
