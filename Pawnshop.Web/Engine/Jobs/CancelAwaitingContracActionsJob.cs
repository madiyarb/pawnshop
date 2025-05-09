using Hangfire;
using Microsoft.Extensions.Logging;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CancelAwaitingContracActionsJob
    {
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractService _contractService;
        private readonly ILogger<CancelAwaitingContracActionsJob> _logger;

        public CancelAwaitingContracActionsJob(IContractActionService contractActionService,
            IContractActionOperationService contractActionOperationService, 
            IContractService contractService,
            ILogger<CancelAwaitingContracActionsJob> logger)
        {
            _contractActionService = contractActionService;
            _contractActionOperationService = contractActionOperationService;
            _contractService = contractService;
            _logger = logger;
        }

        [DisableConcurrentExecution(10 * 60)]
        public async Task Execute()
        {
            _logger.LogInformation("Start CancelAwaitingContracActionsJob");
            try
            {
                var awaitingForApproveContractActions = await _contractActionService.GetAllAwaitingForApproveActions();
                _logger.LogInformation($"Found {awaitingForApproveContractActions.Count} awaitingForApproveContractActions");
                foreach (var action in awaitingForApproveContractActions)
                {
                    var contract = _contractService.Get(action.ContractId);
                    using var transaction = _contractActionService.BeginContractActionTransaction();
                    {
                        await _contractActionOperationService.Cancel(action.Id, 1, contract.BranchId, false, true);
                        transaction.Commit();
                    }
                }

                var awaitingForCancelContractActions = await _contractActionService.GetAllAwaitingForCancelActions();
                _logger.LogInformation($"Found {awaitingForCancelContractActions.Count} awaitingForCancelContractActions");
                foreach (var action in awaitingForCancelContractActions)
                {
                    var contract = _contractService.Get(action.ContractId);
                    _contractActionOperationService.UndoCancel(action.Id, 1, contract.BranchId);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error in CancelAwaitingContracActionsJob. {ex.Message}");
            }
            _logger.LogInformation("Finish CancelAwaitingContracActionsJob");
        }
    }
}
