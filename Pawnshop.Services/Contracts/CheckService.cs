using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Contracts
{
    public class CheckService : IContractActionCheckService, IContractCheckService //TODO: доделать реализацию (листы, геты, удаления и сохранения)
    {
        private readonly ContractActionCheckRepository _contractActionCheckRepository;

        public CheckService(ContractActionCheckRepository contractActionCheckRepository)
        {
            _contractActionCheckRepository = contractActionCheckRepository;
        }

        /// <summary>
        /// Проверка списка признаков, предлагаемых менеджеру для проверки
        /// </summary>
        /// <param name="action">Действие по договору</param>
        public void ContractActionCheck(ContractAction action)
        {
            var checkList = _contractActionCheckRepository.Find(new ListQuery() { Page = null }, new { action.ActionType, action.PayTypeId });

            if (action.ActionType == ContractActionType.Addition ||
                action.ActionType == ContractActionType.PartialPayment)
            {
                //подтверждения для действий, которые делают порожденное подписание
                var signChecks = _contractActionCheckRepository.Find(new Core.Queries.ListQuery() { Page = null },
                        new { ActionType = ContractActionType.Sign, action.PayTypeId })
                    .Where(w => !action.Checks.Any(x => w.Id == x.Id));

                checkList.AddRange(signChecks);
            }

            if (action.Checks == null)
            {
                action.Checks = new List<ContractActionCheckValue>();
            }

            var notChecked = checkList.Where(w => !action.Checks.Any(x => w.Id == x.CheckId)).ToList();

            List<string> errors = new List<string>();
            notChecked.ForEach(item => {
                errors.Add($"Признак \"{item.Name}\" не найден");
            });

            action.Checks.ForEach(item =>
            {
                if (!item.Value) errors.Add($"Признак \"{checkList.Where(x => x.Id == item.CheckId).FirstOrDefault().Name}\" не подтвержден");
            });

            if (errors.Count > 0)
            {
                throw new PawnshopApplicationException(errors.ToArray());
            }
        }
    }
}
