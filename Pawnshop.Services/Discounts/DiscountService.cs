using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Discounts
{
    public class DiscountService : IDiscountService
    {
        private readonly ContractActionRepository _contractActionRepository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly DiscountRepository _discountRepository;
        private readonly BlackoutRepository _blackoutRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly DiscountRowRepository _discountRowRepository;
        public DiscountService(ContractActionRepository contractActionRepository, 
            ContractDiscountRepository contractDiscountRepository,
            BlackoutRepository blackoutRepository,
            DiscountRepository discountRepository,
            CashOrderRepository cashOrderRepository,
            DiscountRowRepository discountRowRepository)
        {
            _contractActionRepository = contractActionRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _blackoutRepository = blackoutRepository;
            _cashOrderRepository = cashOrderRepository;
            _discountRepository = discountRepository;
            _cashOrderRepository = cashOrderRepository;
            _discountRowRepository = discountRowRepository;
        }

        public List<Discount> GetByContractActionId(int contractActionId)
        {
            ContractAction contractAction = _contractActionRepository.Get(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие договора {contractActionId}");

            List<Discount> discounts = _discountRepository.GetListByContractActionId(contractAction.Id);
            if (discounts == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(discounts)} не будет null");;

            foreach (Discount discount in discounts)
            {
                if (discount == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(discount)} не будет null");

                if (!discount.ContractDiscountId.HasValue && !discount.BlackoutId.HasValue)
                    throw new PawnshopApplicationException($"Скидка {discount.Id} не содержит ни скидки, ни скидку при ЧС");

                if (discount.ContractDiscountId.HasValue)
                {
                    ContractDiscount contractDiscount = _contractDiscountRepository.GetOnlyDiscount(discount.ContractDiscountId.Value);
                    if (contractDiscount == null)
                        throw new PawnshopApplicationException($"Скидка договора {discount.ContractDiscountId.Value} не найден");

                    discount.ContractDiscount = contractDiscount;
                }

                if (discount.BlackoutId.HasValue)
                {
                    Blackout blackout = _blackoutRepository.Get(discount.BlackoutId.Value); 
                    if (blackout == null)
                        throw new PawnshopApplicationException($"Скидка при ЧС {discount.BlackoutId.Value} не найден");

                    discount.Blackout = blackout;
                }

                List<DiscountRow> discountRows = _discountRowRepository.GetByDiscountId(discount.Id);
                if (discountRows == null)
                    throw new PawnshopApplicationException("Ожидалось что {nameof(discountRows)} не будет null");

                foreach (DiscountRow discountRow in discountRows)
                { 
                    if(discountRow == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRow)} не будет null");

                    if (!discountRow.OrderId.HasValue)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRow.OrderId)} будет иметь значение");

                    CashOrder cashOrder = _cashOrderRepository.GetAsync(discountRow.OrderId.Value).Result;
                    if (cashOrder == null)
                        throw new PawnshopApplicationException($"Кассовый ордер {discountRow.OrderId.Value} не найден");

                    discountRow.Order = cashOrder;
                }

                discount.Rows = discountRows;
            }

            return discounts;
        }
    }
}
