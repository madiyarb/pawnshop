using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Договор для загрузки в Mintos
    /// </summary>
    public class MintosContractUploadModel
    {

        public MintosContractUploadModel(Contract contract, decimal interestRate, string currency, decimal exchangeRate = 1, Contract parentContract = null, ClientContact defaultClientContact = null)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            exchangeRate = Math.Round(exchangeRate, 6);
            loan = new MintosLoan(contract, interestRate, currency, exchangeRate, parentContract);
            client = new MintosClient(contract.Client, defaultClientContact?.Address);
            if (contract.CollateralType == CollateralType.Car)
            {
                pledge = new MintosPledgeCar(contract.Positions.FirstOrDefault().Position as Car, contract, exchangeRate);
            }
            else throw new ArgumentOutOfRangeException($@"Значение CollateralType={contract.CollateralType} вне диапазона. MintosContractUploadModel.");

            apr_calculation_data = new MintosAprCalculationData(contract, exchangeRate);

            payment_schedule = CalculateScheduleForInvestors(contract, interestRate, exchangeRate, loan.loan_amount_assigned_to_mintos);
            //TODO: Доделать реализацию расписания инвесторам.
        }

        /// <summary>
        /// Договор
        /// </summary>
        public MintosLoan loan { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public MintosClient client { get; set; }
        
        /// <summary>
        /// Залог
        /// </summary>
        public object pledge { get; set; }

        /// <summary>
        /// Расписание оплаты для инвесторов
        /// </summary>
        public List<MintosInvestorScheduleItem> payment_schedule { get; set; }

        /// <summary>
        /// Документы по договору
        /// </summary>
        public List<MintosDocument> documents { get; set; } = new List<MintosDocument>();

        /// <summary>
        /// Данные для расчёта ГЭСВ(APR)
        /// </summary>
        public MintosAprCalculationData apr_calculation_data  { get; set; }

        private List<MintosInvestorScheduleItem> CalculateScheduleForInvestors(Contract contract, decimal interestRate, decimal exchangeRate, decimal loanAmount)
        {
            var schedule = new List<MintosInvestorScheduleItem>();
            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
            {
                var i = 1;
                decimal prevDebtLeft = -1;
                decimal debt = loanAmount;
                DateTime prevDate = DateTime.Today;
                foreach (var scheduleItem in contract.PaymentSchedule.Where(x=>x.Date.Date>=DateTime.Today.Date && x.Status != ScheduleStatus.Payed && x.Status != ScheduleStatus.Canceled))
                {
                    var item = new MintosInvestorScheduleItem();
                        item.number = i;
                        item.date = scheduleItem.Date.ToString("yyyy-MM-dd");
                        item.principal_amount = Math.Round((scheduleItem.DebtCost) * exchangeRate,2);
                        item.total_remaining_principal = prevDebtLeft == -1 ? loanAmount : Math.Round(prevDebtLeft * exchangeRate, 2);
                        item.interest_amount =
                            Math.Round(
                                (item.total_remaining_principal * (interestRate / 100) / 360 *
                                 (scheduleItem.Date - prevDate.Date).Days));
                    
                    debt -= Math.Round((scheduleItem.DebtCost) * exchangeRate, 2);
                    item.sum = item.principal_amount + item.interest_amount;
                    prevDate = scheduleItem.Date;
                    prevDebtLeft = scheduleItem.DebtLeft;
                    i++;
                    schedule.Add(item);
                }

                if (debt != 0)
                {
                    var lastSchedule = schedule.OrderBy(x => x.number).LastOrDefault();
                    schedule.Remove(lastSchedule);

                    var scheduleItem =
                        contract.PaymentSchedule.Where(x => x.Date.Date == DateTime.Parse(lastSchedule.date)).FirstOrDefault();
                    lastSchedule.principal_amount += debt;
                    lastSchedule.interest_amount =
                        Math.Round(
                            (lastSchedule.total_remaining_principal * (interestRate / 100) / 360 *
                             (scheduleItem.Date - contract.PaymentSchedule.Where(x=>x.Date.Date<scheduleItem.Date).OrderByDescending(z=>z.Id).FirstOrDefault().Date).Days));
                    lastSchedule.sum = lastSchedule.principal_amount + lastSchedule.interest_amount;
                    lastSchedule.total_remaining_principal = lastSchedule.principal_amount;
                    schedule.Add(lastSchedule);
                }
            }
            else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                DateTime prevDate = DateTime.Today;
                var prolongCount = contract.Actions.Where(x => x.ActionType == ContractActionType.Prolong).Count();
                //if (prolongCount > 0)
                //{
                //    var i = 1;
                //    foreach (var action in contract.Actions.Where(x => x.ActionType == ContractActionType.Prolong))
                //    {
                //        var prolongItem = new MintosInvestorScheduleItem()
                //        {
                //            number = i,
                //            date = action.Date.ToString("yyyy-MM-dd"),
                //            principal_amount = 0,
                //            total_remaining_principal = Math.Round(contract.LoanCost * exchangeRate,2),
                //            interest_amount = Math.Round((contract.LoanCost * (interestRate / 100) / 360 * (contract.MaturityDate - prevDate.Date).Days) * exchangeRate,2)
                //        };
                //        prolongItem.sum = prolongItem.principal_amount + prolongItem.interest_amount;
                //        schedule.Add(prolongItem);
                //        i++;
                //    }
                //}

                var item = new MintosInvestorScheduleItem()
                {
                    number = 1,
                    date = contract.MaturityDate.ToString("yyyy-MM-dd"),
                    principal_amount = loanAmount,
                    total_remaining_principal = loanAmount,
                    interest_amount = Math.Round((contract.LoanCost * (interestRate / 100) / 360 * (contract.MaturityDate - prevDate.Date).Days) * exchangeRate,2)
                };
                item.sum = item.principal_amount + item.interest_amount;
                schedule.Add(item);
            }
            else throw new ArgumentOutOfRangeException($@"Значение PercentPaymentType={contract.PercentPaymentType} вне диапазона. MintosContractUploadModel.");

            return schedule;
        }
    }
}
