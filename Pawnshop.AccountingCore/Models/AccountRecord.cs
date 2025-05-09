using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Выписка по счету
    /// </summary>
    public class AccountRecord : IEntity, ICreateLogged
    {
        public AccountRecord()
        {

        }

        public AccountRecord(IAccount account, decimal amount, decimal amountNC, int authorId, int businessOperationSettingId, int orderId, bool isDebit = false, DateTime? date = null, IAccount corrAccount = null, AccountRecord previousRecord = null, string reason = null)
        {
            if(account == null) throw new ArgumentNullException($"{nameof(account)} не может быть равен null");
            if(string.IsNullOrWhiteSpace(reason)) reason = $"Движение в {account.AccountNumber}";
            IsDebit = isDebit;//TODO:Инвертировать, в зависимости от наиболее часто используемого значения
            Reason = reason;
            AuthorId = authorId;
            CreateDate = DateTime.Now;
            AccountId = account.Id;
            Date = date ?? DateTime.Now;
            Amount = amount;
            AmountNC = amountNC;
            BusinessOperationSettingId = businessOperationSettingId;
            OrderId = orderId;
            
            if (previousRecord == null)
            {
                IncomingBalance = account.Balance;
                IncomingBalanceNC = account.BalanceNC;
            }
            else
            {
                IncomingBalance = previousRecord.OutgoingBalance;
                IncomingBalanceNC = previousRecord.OutgoingBalanceNC;
            }

            if (isDebit)
            {
                OutgoingBalance = IncomingBalance - amount;
                ///////
                OutgoingBalanceNC = IncomingBalanceNC - amountNC;
            }
            else
            {
                OutgoingBalance = IncomingBalance + amount;
                ///////
                OutgoingBalanceNC = IncomingBalanceNC + amountNC;
            }

            if (corrAccount != null) CorrAccountId = corrAccount.Id;

        }

        public void CheckBalance(AccountPlan plan, bool redBalanceAllowed, bool isMigration = false)
        {
            if (redBalanceAllowed || isMigration) return;

            if (plan.IsActive)
            {
                if (OutgoingBalance > 0) throw new AggregateException($"Остаток на счете \"{plan.Name}\"({plan.Code}, Id = {AccountId}) не может быть больше нуля (до операции = {IncomingBalance}, после = {OutgoingBalance})");
                if (OutgoingBalanceNC > 0) throw new AggregateException($"Остаток в национальной валюте на счете \"{plan.Name}\"({plan.Code}, Id = {AccountId}) не может быть больше нуля (до операции = {IncomingBalanceNC}, после = {OutgoingBalanceNC})");
            }
            else
            {
                if (OutgoingBalance < 0) throw new AggregateException($"Остаток на счете \"{plan.Name}\"({plan.Code}, Id = {AccountId}) не может быть меньше нуля (до операции = {IncomingBalance}, после = {OutgoingBalance})");
                if (OutgoingBalanceNC < 0) throw new AggregateException($"Остаток в национальной валюте на счете \"{plan.Name}\"({plan.Code}, Id = {AccountId}) не может быть меньше нуля (до операции = {IncomingBalanceNC}, после = {OutgoingBalanceNC})");
            }
        }
        
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Счет
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Счет-корреспондент
        /// </summary>
        public int? CorrAccountId { get; set; }

        /// <summary>
        /// Настройка бизнес-операции
        /// </summary>
        public int? BusinessOperationSettingId { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Сумма в национальной валюте
        /// </summary>
        public decimal AmountNC { get; set; }

        /// <summary>
        /// Входящий остаток
        /// </summary>
        public decimal IncomingBalance { get; set; }

        /// <summary>
        /// Входящий остаток в национальной валюте
        /// </summary>
        public decimal IncomingBalanceNC { get; set; }

        /// <summary>
        /// Входящий остаток
        /// </summary>
        public decimal OutgoingBalance { get; set; }

        /// <summary>
        /// Входящий остаток в национальной валюте
        /// </summary>
        public decimal OutgoingBalanceNC { get; set; }

        /// <summary>
        /// Дебетовый?
        /// </summary>
        public bool IsDebit { get; set; }

        /// <summary>
        /// Назначение платежа
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор ордера
        /// </summary>
        public int? OrderId { get; set; }
    }
}
