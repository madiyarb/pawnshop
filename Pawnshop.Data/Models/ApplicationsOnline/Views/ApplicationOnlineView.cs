using System.Collections.Generic;
using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnlineCar.Views;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineView
    {
        /// <summary>
        /// Кем создан
        /// </summary>
        public string CreationAuthor { get; set; }

        /// <summary>
        /// Идентификатор автора создавшего заявку
        /// </summary>
        public int CreationAuthorId { get; set; }

        /// <summary>
        /// Идентификатор автора последнего изменившего заявку
        /// </summary>
        public int LastChangeAuthorId { get; set; }

        /// <summary>
        /// Кем изменен
        /// </summary>
        public string LastChangeAuthor { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Статус идентификатор
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Статус код
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Человекочитаемый номер 
        /// </summary>
        public string ApplicationNumber { get; set; }

        /// <summary>
        /// Сумма заявки
        /// </summary>
        public decimal ApplicationAmount { get; set; }
        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int LoanTerm { get; set; }

        /// <summary>
        /// Стадия
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// Идентификатор продукта
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Продукт
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Аккаунт
        /// </summary>
        public string MobileAccountUserName { get; set; }

        /// <summary>
        /// Источник заявки
        /// </summary>
        public string ApplicationSource { get; set; }

        /// <summary>
        /// Код партнера
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Цель кредита
        /// </summary>
        public int? LoanPurposeId { get; set; }

        /// <summary>
        /// Сфера деятельности бизнеса
        /// </summary>
        public int? BusinessLoanPurposeId { get; set; }

        /// <summary>
        /// Идентификатор для физических лиц (ОКЭД)
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? OkedForIndividualsPurposeId { get; set; }

        /// <summary>
        /// Идентификатор целевой цели кредита
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? TargetPurposeId { get; set; }

        /// <summary>
        /// Канал привлечения
        /// </summary>
        public int? AttractionChannelId { get; set; }

        /// <summary>
        /// Займы для рефинансирования
        /// </summary>
        public List<int> RefinancesContractIds { get; set; }

        /// <summary>
        /// Идентификатор клиента 
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Привязка к Филиалу
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Платежная информация
        /// </summary>
        public ApplicationOnlineClientRequisiteView PaymentInfo { get; set; }

        /// <summary>
        /// Работа
        /// </summary>
        public ApplicationOnlineWorkView WorkInfo { get; set; }

        /// <summary>
        /// Данные об автомобиле
        /// </summary>
        public ApplicationOnlineCarView CarInfo { get; set; }

        /// <summary>
        /// Контакты
        /// </summary>
        public ApplicationOnlineClientContactsView ClientContactsInfo { get; set; }

        /// <summary>
        /// Документы
        /// </summary>
        public List<ApplicationOnlineClientDocumentView> Documents { get; set; }

        /// <summary>
        /// Идентификатор машины и залога
        /// </summary>
        public Guid ApplicationOnlinePositionId { get; set; }

        /// <summary>
        /// Максимально доступный срок займа 
        /// </summary>
        public int? MaximumAvailableLoanTerm { get; set; }

        /// <summary>
        /// Похожесть лица с фото на лицо с документа
        /// </summary>
        public decimal? Similarity { get; set; }

        /// <summary>
        /// Идентификатор менеджера отвечающего за заявку
        /// </summary>
        public int? ResponsibleManagerId { get; set; }

        /// <summary>
        /// ФИО менеджера отвечающего за заявку 
        /// </summary>
        public string ResponsibleManagerName { get; set; }

        /// <summary>
        /// Идентификатор кредитной линии
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Количество месяцев до окончания КЛ (нужно для дискрета)
        /// </summary>
        public int MaxPeriodInMonths { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Дата первого платежа
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }

        /// <summary>
        /// Комментарий 
        /// </summary>
        public string RejectReasonComment { get; set; }

        /// <summary>
        /// Тип рефинанс добор базовый
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Тип рефинанс добор базовый
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Причина отказа
        /// </summary>
        public string RejectReason { get; set; }

        /// <summary>
        /// Возможность изменить дату первого платежа.
        /// </summary>
        public bool CanEditFirstPaymentDate { get; set; }

        public string ContractNumber { get; set; }

        /// <summary>
        /// BranchId договора 
        /// </summary>
        public int? ContractBranchId { get; set; }

        /// <summary>
        /// BranchId договора 
        /// </summary>
        public string ContractBranchName { get; set; }

        /// <summary>
        /// Ответственный верификатор
        /// </summary>
        public int? ResponsibleVerificatorId { get; set; }

        /// <summary>
        /// Ответственный верификатор
        /// </summary>
        public string ResponsibleVerificatorName { get; set; }

        /// <summary>
        /// Ответственный администратор
        /// </summary>
        public int? ResponsibleAdminId { get; set; }

        /// <summary>
        /// Ответственный администратор
        /// </summary>
        public string ResponsibleAdminName { get; set; }

        /// <summary>
        /// Признак отправки залога на регистрацию
        /// </summary>
        public bool EncumbranceRegistered { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        public string ApplicationPartnerCode { get; set; }


        public void FillStatus()
        {
            ApplicationOnlineStatus applicationOnlineStatus;
            if (Enum.TryParse(StatusCode, out applicationOnlineStatus))
            {
                Status = applicationOnlineStatus.GetDisplayName();
                StatusId = (int)applicationOnlineStatus;
            }

            ApplicationOnlineType applicationOnlineType;

            if (Enum.TryParse(Type, out applicationOnlineType))
            {
                TypeName = applicationOnlineType.GetDisplayName();
            }
        }
    }
}
