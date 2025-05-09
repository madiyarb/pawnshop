using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApplicationDetails : IEntity
    {
        /// <summary>
        /// Идентификатор детализации
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Идентификатор Договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Вид выбранной программы кредитования
        /// </summary>
        public ProdKind ProdKind { get; set; }

        /// <summary>
        /// Признак необходимости нового страхового полиса
        /// </summary>
        public bool InsuranceRequired { get; set; } = false;

        /// <summary>
        /// Список одобренных сумм из Заявки
        /// </summary>
        public List<LimitAmount> MaxLimitAmounts { get; set; }

        /// <summary>
        /// Категория из родительского Договора
        /// </summary>
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Сумма добора
        /// </summary>
        public decimal AdditionAmount { get; set; }

        /// <summary>
        /// Id дисконта для смены категории
        /// </summary>
        public int? PersonalDiscountId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Доп сумма сверх одобренных из Заявки
        /// </summary>
        public decimal? OverIssueAmount { get; set; }

        /// <summary>
        /// Признак выдачи траншами: 1-ый транш на погашение долга у другого кредитора
        /// </summary>
        public bool IsFirstTransh { get; set; } = false;

        /// <summary>
        /// Общая сумма договора
        /// </summary>
        public decimal? TotalAmount4AllTransh { get; set; }
    }
}
