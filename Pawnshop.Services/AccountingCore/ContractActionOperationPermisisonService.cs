using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.AccountingCore
{
    public class ContractActionOperationPermisisonService : IContractActionOperationPermisisonService
    {
        private readonly Dictionary<ContractActionType, Func<ContractAction, bool>> _canMakePayOperationsActionTypesWithCallbacks = new Dictionary<ContractActionType, Func<ContractAction, bool>>
        {
        };

        private readonly HashSet<ContractActionType> _cantMakePayOperationsPreviousActionActionTypesDict = new HashSet<ContractActionType>
        {
            ContractActionType.PartialPayment
        };

        private readonly HashSet<ContractActionType> _registerEncumbranceAllowedActionTypes = new HashSet<ContractActionType>
        {
            ContractActionType.Sign,
            ContractActionType.Buyout,
            ContractActionType.BuyoutRestructuringCred,
            ContractActionType.CreditLineClose
        };

        private readonly HashSet<ContractActionType> _extraExpensesPaymentAllowedActionTypes = new HashSet<ContractActionType>
        {
            ContractActionType.PartialPayment,
            ContractActionType.Addition,
            ContractActionType.MonthlyPayment,
            ContractActionType.Buyout,
            ContractActionType.BuyoutRestructuringCred,
            ContractActionType.Prolong
        };

        private readonly HashSet<ContractActionType> _certainExtraExpensesPaymentAllowedActionTypes = new HashSet<ContractActionType>
        {
            ContractActionType.Payment,
            ContractActionType.Addition
        };

        private readonly HashSet<ContractActionType> _doNotValidateContractActionRowIntegrityActionTypes = new HashSet<ContractActionType>
        {
            ContractActionType.Prepayment,
            ContractActionType.PrepaymentReturn,
            ContractActionType.Addition,
            ContractActionType.Sign,
            ContractActionType.PartialPayment
        };

        private readonly HashSet<ContractActionType> _doNotValidateContractActionRowAndDiscounts = new HashSet<ContractActionType>
        {
            ContractActionType.InterestAccrual,
            ContractActionType.MoveToOverdue,
            ContractActionType.PenaltyAccrual,
            ContractActionType.PartialPayment,
            ContractActionType.InterestAccrualOnOverdueDebt,
            ContractActionType.Selling
        };

        private readonly HashSet<string> _certainExtraExpensesAdditionRestrictedTypes = new HashSet<string>
        {
            Constants.TH_EXPENSES_INSCRIPTIONS,
            Constants.TH_EXPENSES_STATE_DUTY
        };

        public ContractActionOperationPermisisonService(PayTypeRepository payTypeRepository) 
        {
            _canMakePayOperationsActionTypesWithCallbacks[ContractActionType.Sign] = ca => { return true; };
            _canMakePayOperationsActionTypesWithCallbacks[ContractActionType.PrepaymentReturn] = ca => CanMakePayOperationsRequiredIBANPayType(ca, payTypeRepository);
        }
        public bool CanPayExtraExpenses(ContractActionType contractActionType)
        {
            bool canPayExtraExpenses = _extraExpensesPaymentAllowedActionTypes.Contains(contractActionType);
            return canPayExtraExpenses;
        }

        public bool CanPayCertainExtraExpenses(ContractActionType contractActionType)
        {
            return _certainExtraExpensesPaymentAllowedActionTypes.Contains(contractActionType);
        }

        public bool CanPayEncumbrance(ContractActionType contractActionType)
        {
            return _registerEncumbranceAllowedActionTypes.Contains(contractActionType);
        }

        public bool CanMakePayOperations(ContractAction contractAction)
        {
            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            ContractActionType contractActionType = contractAction.ActionType;
            Func<ContractAction, bool> callback;
            if (_canMakePayOperationsActionTypesWithCallbacks.TryGetValue(contractActionType, out callback))
            {
                if (callback != null)
                    return callback(contractAction);

                return false;
            }

            bool canMakePayOperations = false;
            ContractActionType? previousActionContractActionType = contractAction.PreviousContractActionType;
            if (previousActionContractActionType.HasValue)
                canMakePayOperations = !_cantMakePayOperationsPreviousActionActionTypesDict.Contains(previousActionContractActionType.Value);

            return canMakePayOperations;
        }

        public bool DoNotValidateActionsRowsIntegrity(ContractActionType contractActionType)
        {
            return _doNotValidateContractActionRowIntegrityActionTypes.Contains(contractActionType);
        }

        public bool DoNotValidateActionsRowsAndDiscounts(ContractActionType contractActionType)
        {
            return _doNotValidateContractActionRowAndDiscounts.Contains(contractActionType);
        }

        private static bool CanMakePayOperationsRequiredIBANPayType(ContractAction action, PayTypeRepository payTypeRepository)
        {
            if (payTypeRepository == null)
                throw new ArgumentNullException(nameof(payTypeRepository));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.PayTypeId.HasValue)
            {
                PayType payType = payTypeRepository.Get(action.PayTypeId.Value);
                if (payType != null)
                    return payType.OperationCode == Constants.PAY_OPERATION_IBAN;
            }

            return false;
        }

        public bool RestrictedExtraExpensesForAddition(string typeHierarchyCode)
        {
            bool RestrictedExtraExpensesForAddition = _certainExtraExpensesAdditionRestrictedTypes.Contains(typeHierarchyCode);
            return RestrictedExtraExpensesForAddition;
        }

    }
}
