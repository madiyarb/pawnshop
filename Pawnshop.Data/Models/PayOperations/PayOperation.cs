using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.PayOperations
{
    public class PayOperation : IEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        public string Number { get; set; }

        public List<PayOperationAction> Actions { get; set; } = new List<PayOperationAction>();

        public DateTime? ExecuteDate { get; set; }
        public PayOperationStatus Status { get; set; }

        /// <summary>
        /// Вид оплаты
        /// </summary>
        [RequiredId(ErrorMessage = "Вид оплаты не заполнен")]
        public int PayTypeId { get; set; }

        /// <summary>
        /// Вид оплаты
        /// </summary>
        public PayType PayType { get; set; }

        /// <summary>
        /// Вид реквизита
        /// </summary>
        [RequiredId(ErrorMessage = "Вид реквизита не заполнен")]
        public int RequisiteTypeId { get; set; }

        /// <summary>
        /// Реквизит
        /// </summary>
        public int? RequisiteId { get; set; }

        /// <summary>
        /// Реквизит
        /// </summary>
        public ClientRequisite Requisite { get; set; }

        /// <summary>
        /// Название операции
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public Client Client { get; set; } 

        /// <summary>
        /// Договор
        /// </summary>
        public int? ContractId { get; set; }
        
        /// <summary>
        /// Действие по договору
        /// </summary>
        public ContractAction Action { get; set; }
        public int? ActionId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public List<FileRow> Files { get; set; } = new List<FileRow>();
        public List<CashOrder> Orders { get; set; } = new List<CashOrder>();

        public decimal TotalCost => Orders.Sum(x => x.OrderType == OrderType.CashOut 
        || (x.OrderType == OrderType.Memorial && x.OperationId.HasValue) ? x.OrderCost : -x.OrderCost);
    }
}
