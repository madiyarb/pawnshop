using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Core;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class ContractDutyDiscount
    {
        /// <summary>
        /// Настройки выключения начисления процентов или штрафов
        /// </summary>
        public List<Discount> Discounts { get; set; } = new List<Discount>();

        /// <summary>
        /// Сообщение пользователю
        /// </summary>
        public string Message => BuildMessage();

        private string BuildMessage()
        {
            string noDiscountMessage = "Скидок не предусмотрено";
            if (Discounts == null) return noDiscountMessage;
            StringBuilder result = new StringBuilder(Discounts.Count > 0 ? "Подробная информация о скидках:\n" : noDiscountMessage);

            foreach (var discount in Discounts)
            {
                string tabs = "   ";
                if (discount.BlackoutId.HasValue) result.AppendLine($"Группа скидок: {discount.Blackout.Note}");

                if(discount.ContractDiscountId.HasValue) {
                    if (discount.ContractDiscount.IsTypical)
                    {
                        if(discount.ContractDiscount.PersonalDiscount.Blackout != null)
                        {
                            result.AppendLine($"Скидочная кампания: {discount.ContractDiscount.PersonalDiscount.Blackout.Note}");
                            tabs += "   ";
                        }
                        result.AppendLine($"{tabs}Персональная скидка: {discount.ContractDiscount.PersonalDiscount.Note}");
                    }
                    else
                    {
                        result.AppendLine($"{tabs}Персональная скидка по сумме: {discount.ContractDiscount.Note}");
                    }

                }
                tabs += "   ";
                foreach (var row in discount.Rows)
                {
                    switch (row.PaymentType)
                    {
                        case AmountType.Debt:
                            if (row.AddedCost != row.SubtractedCost)
                            {
                                result.AppendLine($@"{tabs} Основной долг: {row.OriginalCost}");
                                result.AppendLine($@"{tabs} {(row.AddedCost > row.SubtractedCost ? "Добавлена" : "Списана")} сумма основного долга {Math.Abs(row.AddedCost - row.SubtractedCost)}");

                            }
                            if (row.PercentAdjustment != 0)
                            {
                                result.AppendLine($@"{tabs} {(row.PercentAdjustment > 0 ? "Уменьшается" : "Увеличивается")} процентная ставка ОД на {Math.Abs(row.PercentAdjustment)}");
                            }
                            break;
                        case AmountType.Loan:
                            if (row.AddedCost != row.SubtractedCost)
                            {
                                result.AppendLine($@"{tabs} Проценты: {row.OriginalCost}");
                                result.AppendLine($@"{tabs} {(row.AddedCost > row.SubtractedCost ? "Добавлены" : "Списаны")} проценты{((row.AddedDays - row.SubtractedDays != 0) ? string.Concat(" за ", Math.Abs(row.AddedDays - row.SubtractedDays), " дней") : string.Empty)}: {Math.Abs(row.AddedCost - row.SubtractedCost)}");
                            }
                            if (row.PercentAdjustment != 0)
                            {
                                result.AppendLine($@"{tabs} {(row.PercentAdjustment > 0 ? "Уменьшается" : "Увеличивается")} процентная ставка пошлины на {Math.Abs(row.PercentAdjustment)}");
                            }
                            break;
                        case AmountType.Penalty:
                            if (row.AddedCost != row.SubtractedCost)
                            {
                                result.AppendLine($@"{tabs} Штрафы: {row.OriginalCost}");
                                result.AppendLine($@"{tabs} {(row.AddedCost > row.SubtractedCost ? "Добавлены" : "Списаны")} штрафы{((row.AddedDays - row.SubtractedDays != 0) ? string.Concat(" за ", Math.Abs(row.AddedDays - row.SubtractedDays), " дней") : string.Empty)}: {Math.Abs(row.AddedCost - row.SubtractedCost)}");
                            }
                            if (row.PercentAdjustment != 0)
                            {
                                result.AppendLine($@"{tabs} {(row.PercentAdjustment > 0 ? "Уменьшается" : "Увеличивается")} процентная ставка штрафа на {Math.Abs(row.PercentAdjustment)}");
                            }
                            break;
                        case AmountType.Duty:
                            if (row.AddedCost != row.SubtractedCost)
                            {
                                result.AppendLine($@"{tabs} Госпошлина: {row.OriginalCost}");
                                result.AppendLine($@"{tabs} {(row.AddedCost > row.SubtractedCost ? "Добавлена" : "Списана")} госпошлина {Math.Abs(row.AddedCost - row.SubtractedCost)}");
                            }
                            if (row.PercentAdjustment != 0)
                            {
                                result.AppendLine($@"{tabs} {(row.PercentAdjustment > 0 ? "Уменьшается" : "Увеличивается")} процентная ставка госпошлины на {Math.Abs(row.PercentAdjustment)}");
                            }
                            break;
                        default:
                            if (row.AddedCost != row.SubtractedCost)
                            {
                                
                                result.AppendLine($@"{tabs} {row.PaymentType.GetDisplayName()}: {row.OriginalCost}");
                                result.AppendLine($@"{tabs} {row.PaymentType.GetDisplayName()} {(row.AddedCost > row.SubtractedCost ? "увеличен" : "списан")} на {Math.Abs(row.AddedCost - row.SubtractedCost)}");
                            }
                            if (row.PercentAdjustment != 0)
                            {
                                result.AppendLine($@"{tabs} {(row.PercentAdjustment > 0 ? "Уменьшается" : "Увеличивается")} процентная ставка для {Enum.GetName(typeof(AmountType), row.PaymentType)} на {Math.Abs(row.PercentAdjustment)}");
                            }
                            break;
                    }
                    
                }
            }
            return result.ToString();
        }
    }    

    public class Discount : IEntity
    {
        public int Id { get; set; }

        public int? ActionId { get; set; }

        public int? BlackoutId { get; set; }
        public Blackout Blackout { get; set; }

        public int? ContractDiscountId { get; set; }
        public ContractDiscount ContractDiscount { get; set; }

        public decimal TotalAddedSum => Rows.Sum(x => x.AddedCost);
        public decimal TotalDiscountSum => Rows.Sum(x => x.SubtractedCost);
        public decimal TotalLoanPercentAdjustment => Rows.Where(x=>x.PaymentType== AmountType.Loan).Sum(x => x.PercentAdjustment);
        public decimal TotalPenaltyPercentAdjustment => Rows.Where(x => x.PaymentType == AmountType.Penalty).Sum(x => x.PercentAdjustment);
        public bool HasChanges => TotalAddedSum!=0 || TotalDiscountSum != 0 || TotalLoanPercentAdjustment != 0 || TotalPenaltyPercentAdjustment != 0;

        public List<DiscountRow> Rows { get; set; } = new List<DiscountRow>();
    }

    public class DiscountRow : IEntity
    {

        public int Id { get; set; }

        public int? DiscountId { get; set; }
        /// <summary>
        /// Тип погашения
        /// </summary>
        public AmountType PaymentType { get; set; }
        /// <summary>
        /// Сумма наценки
        /// </summary>
        public decimal AddedCost { get; set; } = 0;

        /// <summary>
        /// Сумма скидки
        /// </summary>
        public decimal SubtractedCost { get; set; } = 0;

        /// <summary>
        /// Сумма до скидки
        /// </summary>
        public decimal OriginalCost { get; set; } = 0;

        /// <summary>
        /// Добавленное количество дней
        /// </summary>
        public int AddedDays { get; set; } = 0;

        /// <summary>
        /// Списанное количество дней
        /// </summary>
        public int SubtractedDays { get; set; } = 0;

        /// <summary>
        /// Количество дней до списания
        /// </summary>
        public int OriginalDays { get; set; } = 0;

        /// <summary>
        /// Корректировка процентов для порожденного договора
        /// </summary>
        public decimal PercentAdjustment { get; set; } = 0;

        public int? OrderId { get; set; }

        public CashOrder Order { get; set; }
    }
}
