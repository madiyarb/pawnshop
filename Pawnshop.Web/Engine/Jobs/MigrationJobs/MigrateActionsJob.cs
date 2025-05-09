using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Services.Migrate;

namespace Pawnshop.Web.Engine.Jobs.MigrationJobs
{
    public class MigrateActionsJob
    {
        private readonly MigrateContractActionService _migrateContractActionService;
        private readonly ContractRepository _contractRepository;

        public MigrateActionsJob(MigrateContractActionService migrateContractActionService, ContractRepository contractRepository)
        {
            _migrateContractActionService = migrateContractActionService;
            _contractRepository = contractRepository;
        }

        public void Migrate(int contractId)
        {
            var contract = _contractRepository.Get(contractId);

            _migrateContractActionService.MigrateActions(contract);
        }
    }
}
