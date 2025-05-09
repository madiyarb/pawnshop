using Pawnshop.Web.Models.ClientsAdditionalContacts;
using System.Collections.Generic;
using System;

namespace Pawnshop.Web.Models.ApplicationOnline
{
    public sealed class ApplicationOnlineCreateBinding
    {

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }
        
        /// <summary>
        /// Сумма займа
        /// </summary>
        public decimal LoanCost { get; set; }
        
        /// <summary>
        /// Идентификатор продукта(дискрет, ануитет итд)
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Страховка
        /// </summary>
        public bool? Insurance { get; set; }
        /// <summary>
        /// Дата первого платежа
        /// </summary>
        public DateTime FirstPaymentDate { get; set; } //FirstPaymentDate

        /// <summary>
        /// Сумма доходов указанная клиентом
        /// </summary>
        public decimal? IncomeAmount { get; set; }

        /// <summary>
        /// Сумма расходов указанная клиентом
        /// </summary>
        public decimal? ExpenseAmount { get; set; }

        /// <summary>
        /// Идентификатор машины в случае если это добор 
        /// </summary>
        public int? CarId { get; set; }

        /// <summary>
        /// Идентификатор списка файлов в хранилище ДРПП
        /// </summary>
        public int ListId { get; set; }

        /// <summary>
        /// Идентификатор кредитной линии
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Авто
        /// </summary>
        public ApplicationOnlineCreateCarBinding Car { get; set; }

        /// <summary>
        /// Контакты
        /// </summary>
        public List<ClientsAdditionalContactsFromMobile> Contacts { get; set; }

        /// <summary>
        /// Файлы
        /// </summary>
        public List<ApplicationOnlineFileBinding> Files { get; set; } = new List<ApplicationOnlineFileBinding>();

        /// <summary>
        /// Код партнёра 
        /// </summary>
        public string PartnerCode { get; set; }
    }
}
