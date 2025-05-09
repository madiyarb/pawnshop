using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Contracts.Expenses
{
    /// <summary>
    /// Расходы договора
    /// </summary> 
    public class ContractExpense : IEntity, ILoggableToEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary> 
        public int Id { get; set; }

        /// <summary>
        /// Дата
        /// </summary> 
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Идентификатор типа расхода
        /// </summary> 
        public int ExpenseId { get; set; }

        /// <summary>
        /// Тип расхода
        /// </summary>
        public Expense Expense { get; set; } 


        /// <summary>
        /// Идентификатор договора
        /// </summary> 
        public int ContractId { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary> 
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Взято с клиента
        /// </summary> 
        public decimal TotalLeft { get; set; }

        /// <summary>
        /// Причина
        /// </summary> 
        public string Reason { get; set; }

        /// <summary>
        /// Примечание
        /// </summary> 
        public string Name { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>  
        public string Note { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary> 
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Порожденный перевод
        /// </summary>
        public int? RemittanceId { get; set; }

        /// <summary>
        /// Оплачено
        /// </summary>
        public bool IsPayed { get; set; }

        /// <summary>   
        /// Идентификатор сотрудника
        /// </summary>
        public int UserId { get; set; }

        public List<ContractExpenseRow> ContractExpenseRows { get; set; } = new List<ContractExpenseRow>();

        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}
