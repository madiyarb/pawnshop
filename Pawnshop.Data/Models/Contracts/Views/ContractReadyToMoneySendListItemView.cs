using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractReadyToMoneySendListItemView
    {
        /// <summary>
        /// Дата договора
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// ФИО Клиента
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Название продукта
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Процентная ставка
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Сумма договора
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Дата возврата 
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public ContractStatus StatusId { get; set; }

        public string Status => StatusId.GetDisplayName();

        /// <summary>
        /// Название филилала 
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Имя автора 
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Номер Авто
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// ИИН
        /// </summary>
        public string IdentityNumber { get; set; }

        public int ContractId { get; set; }

        public int Id { get; set; }

        /// <summary>
        /// Признак отправки залога на регистрацию
        /// </summary>
        public bool EncumbranceRegistered { get; set; }

        /// <summary>
        /// Идентификатор способы выдачи
        /// </summary>
        public int? PayTypeId { get; set; }

        /// <summary>
        /// Наименование способы выдачи
        /// </summary>
        public string PayTypeName { get; set; }
    }
}
