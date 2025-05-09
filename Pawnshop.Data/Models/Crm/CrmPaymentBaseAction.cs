using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Notifications;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmPaymentBaseAction
    {
        public CrmPaymentBaseAction(
            Contracts.Contract contract,
            int categoryId,
            List<Notification> notifications,
            string defaultContact,
            decimal loanCostLeft,
            decimal loanPercentCost,
            decimal penaltyPercentCost,
            decimal prepayment,
            decimal buyoutAmount,
            decimal prolongAmount,
            int overdueContracts,
            CollectionContractStatus collectionStatus,
            string fincoreUrl)
        {
            _categoryId = categoryId;
            CrmPaymentId = contract.CrmPaymentId;
            ClientCrmId = contract.Client.CrmId.ToString();
            ContractStatus = Convert.ToString((int)contract.Status);
            ContractId = contract.Id;
            ContractNumber = contract.ContractNumber;
            ClientName = contract.Client.FullName;
            ClientIdentityNumber = contract.Client.IdentityNumber;
            ClientAddress =
                    contract.Client.Addresses?.OrderByDescending(x => x.CreateDate)
                        ?.FirstOrDefault(x => x.IsActual)?.FullPathRus ?? "Адрес не найден";
            ClientPhoneNumber = defaultContact;
            PercentPaymentType = contract.PercentPaymentType.GetDisplayName();
            LoanCost = contract.LoanCost.ToString();
            LoanCostLeft = loanCostLeft.ToString();
            LoanPercent = contract.LoanPercent;
            ContractDate = contract.ContractDate;
            MaturityDate = contract.MaturityDate;
            LoanPeriod = contract.LoanPeriod.ToString();
            LoanPercentCost = loanPercentCost.ToString();
            try
            {
                var collection = collectionStatus;
                ActiveOverdueContractCount = overdueContracts;
                if (collection != null && 
                    collection.CollectionStatusCode != Core.Constants.NOCOLLECTION_STATUS && 
                    collection.IsActive)
                {
                    DelayDayCount = (DateTime.Now.Date - collection.StartDelayDate.Date).Days.ToString();
                }
                else
                {
                    DelayDayCount = 0.ToString();
                }
            }
            catch(Exception ex)
            {
                DelayDayCount = ex.Message;
            }

            PenaltyPercentCost = penaltyPercentCost.ToString();
            IsTransferred = contract.ContractTransfers.Any(x => x.BackTransferDate==null) ? "Да" : "Нет";
            QuantityDelayedPaymentCount =
                    contract.PercentPaymentType == Pawnshop.AccountingCore.Models.PercentPaymentType.EndPeriod
                        ? "0"
                        : contract.PaymentSchedule.Count(x => x.Status == ScheduleStatus.Overdue)
                            .ToString();
            BranchDisplayName = contract.Branch.DisplayName;
            Prepayment = prepayment.ToString();
            try
            {
                PaymentDate = contract.PercentPaymentType == Pawnshop.AccountingCore.Models.PercentPaymentType.EndPeriod
                    ? contract.MaturityDate
                    : contract.PaymentSchedule.Where(x => !x.ActionId.HasValue)
                        .Min(x => x.Date);
            }
            catch (Exception)
            {
                PaymentDate = new DateTime(1970,1,1);
            }
            DelayDate = PaymentDate.AddDays(1);
            ExtraExpense = contract.Expenses.Where(x => !x.IsPayed).Sum(x => x.TotalCost);

            if (contract.CollateralType == CollateralType.Car && contract.Positions.FirstOrDefault()?.Position is Car car)
            {
                PositionMark = car.Mark;
                PositionModel = car.Model;
                PositionYear = car.ReleaseYear.ToString();
                PositionColor = car.Color;
                PositionNumber = car.TransportNumber;
                PositionStatusId = car.ParkingStatusId.ToString();
            }

            IsSVG = contract.Client.IsSVG ? "Да" : "Нет";
            if (notifications != null)
            {
                List<string> smsStatuses = new List<string>();
                foreach (var notification in notifications)
                {
                    if (notification?.NotificationReceiver?.Address != null)
                    {
                        NotificationReceiver receiver = notification.NotificationReceiver;
                        string sentAt = receiver.SentAt.HasValue ? receiver.SentAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "не отправлено";
                        string deliveredAt = receiver.DeliveredAt.HasValue ? receiver.DeliveredAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "не доставлено";
                        smsStatuses.Add($"{receiver.Address}, {sentAt}, {deliveredAt}, {notification.Message}, {notification.Status}");
                    }
                }

                if (smsStatuses.Count > 0)
                    SmsStatuses = string.Join('\n', smsStatuses);
            }

            BuyoutAmount = buyoutAmount.ToString();
            ProlongAmount = prolongAmount.ToString();
            ProlongAmountMinusExtra = (prolongAmount - ExtraExpense).ToString("N");
            Url = fincoreUrl + $"contracts/{ContractId}/";
        }

        protected int _categoryId { get; set; }

        /// <summary>
        /// Статус договора
        /// </summary>
        [JsonProperty("UF_CRM_1596830402459")]
        public string ContractStatus;

        /// <summary>
        /// Идентификатор сделки/договора
        /// </summary>
        [JsonIgnore]
        public int? CrmPaymentId { get; set; }

        /// <summary>
        /// Идентификатор контакта
        /// </summary>
        [JsonProperty("CONTACT_ID")]
        public string ClientCrmId { get; set; }

        /// <summary>
        /// Идентификатор договора (Contracts)
        /// </summary>
        [JsonIgnore]
        public int ContractId { get; set; }

        /// <summary>
        /// Номер договора во фронт-базе (Contracts)
        /// </summary>
        [JsonProperty("TITLE")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Номер договора во фронт-базе (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580272769360")]
        public string ContractNumberTitle => ContractNumber;

        /// <summary>
        /// ФИО Клиента (Clients)
        /// </summary>
        [JsonProperty("UF_CRM_1580272780542")]
        public string ClientName { get; set; }

        /// <summary>
        /// ИИН Клиента (Clients)
        /// </summary>
        [JsonProperty("UF_CRM_1580272843803")]
        public string ClientIdentityNumber { get; set; }

        /// <summary>
        /// Адрес Клиента (Clients)
        /// </summary>
        [JsonProperty("UF_CRM_1580272800719")]
        public string ClientAddress { get; set; }

        /// <summary>
        /// Телефонные номера Клиента (Clients)
        /// </summary>
        [JsonProperty("UF_CRM_1580272818164")]
        public string ClientPhoneNumber { get; set; }


        [JsonProperty("UF_CRM_1580272861911")]
        public string PercentPaymentType { get; set; }

        /// <summary>
        /// Ссуда (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580272874895")]
        public string LoanCost { get; set; }

        /// <summary>
        /// Остаток ссуды (?)
        /// </summary>
        [JsonProperty("UF_CRM_1580272892391")]
        public string LoanCostLeft { get; set; }

        /// <summary>
        /// Процент (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580272905702")]
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Дата выдачи (Contracts)
        /// </summary>
        [JsonIgnore]
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата выдачи (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580273033337")]
        public string ContractDateString => ContractDate.Date.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Дата выгрузки
        /// </summary>
        [JsonProperty("UF_CRM_1580273044021")]
        public string Today => DateTime.Now.Date.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Дата возврата (Contracts)
        /// </summary>
        [JsonIgnore]
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Кол-во дней (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580273054787")]
        public string LoanPeriod { get; set; }

        /// <summary>
        /// Сумма % (Contracts)
        /// </summary>
        [JsonProperty("UF_CRM_1580273064467")]
        public string LoanPercentCost { get; set; }

        /// <summary>
        /// Дней просрочки
        /// </summary>
        [JsonProperty("UF_CRM_1580273088076")]
        public string DelayDayCount { get; set; }

        /// <summary>
        /// Штраф
        /// </summary>
        [JsonProperty("UF_CRM_1580273096437")]
        public string PenaltyPercentCost { get; set; }

        /// <summary>
        /// Передан
        /// </summary>
        [JsonProperty("UF_CRM_1580273117969")]
        public string IsTransferred { get; set; }

        /// <summary>
        /// Марка машины
        /// </summary>
        [JsonProperty("UF_CRM_1580273132554")]
        public string PositionMark { get; set; }

        /// <summary>
        /// Модель машины
        /// </summary>
        [JsonProperty("UF_CRM_1580273138484")]
        public string PositionModel { get; set; }

        /// <summary>
        /// Год выпуска машины
        /// </summary>
        [JsonProperty("UF_CRM_1580273144739")]
        public string PositionYear { get; set; }

        /// <summary>
        /// Цвет машины
        /// </summary>
        [JsonProperty("UF_CRM_1580273150963")]
        public string PositionColor { get; set; }

        /// <summary>
        /// Номер машины
        /// </summary>
        [JsonProperty("UF_CRM_1580273164173")]
        public string PositionNumber { get; set; }

        /// <summary>
        /// Просрочено платежей
        /// </summary>
        [JsonProperty("UF_CRM_1580273175603")]
        public string QuantityDelayedPaymentCount { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        [JsonProperty("UF_CRM_1580273186608")]
        public string BranchDisplayName { get; set; }

        /// <summary>
        /// Авансом доступно
        /// </summary>
        [JsonProperty("UF_CRM_1580273199467")]
        public string Prepayment { get; set; }

        /// <summary>
        /// Дата предстоящего платежа
        /// </summary>
        [JsonIgnore]
        public DateTime PaymentDate { get; set; }


        /// <summary>
        /// Дата предстоящего платежа
        /// </summary>
        [JsonProperty("UF_CRM_1587951624172")]
        public string PaymentDateString => PaymentDate.Date.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Дата выхода на просрочку
        /// </summary>
        [JsonIgnore]
        public DateTime DelayDate { get; set; }

        /// <summary>
        /// Дата выхода на просрочку
        /// </summary>
        [JsonProperty("UF_CRM_1582176769332")]
        public string DelayDateString => DelayDate.Date.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Ссылка на договор
        /// </summary>
        [JsonProperty("UF_CRM_1588656970984")]
        public string Url;

        /// <summary>
        /// Дополнительные расходы
        /// </summary>
        [JsonProperty("UF_CRM_1600830682959")]
        public decimal ExtraExpense { get; set; }

        /// <summary>
        /// Статусы СМС
        /// </summary>
        [JsonProperty("UF_CRM_1610339285")]
        public string SmsStatuses { get; set; }

        /// <summary>
        /// Клиент - СУСН
        /// </summary>
        [JsonProperty("UF_CRM_1607324318")]
        public string IsSVG { get; set; }

        /// <summary>
        /// Статус стоянки
        [JsonProperty("UF_CRM_1607323504139")]
        public string PositionStatusId { get; set; }

        /// <summary>
        /// Сумма для продления
        [JsonProperty("UF_CRM_1610537012199")]
        public string ProlongAmount { get; set; }

        /// <summary>
        /// Сумма для выкупа
        /// </summary>
        [JsonProperty("UF_CRM_1580273105914")]
        public string BuyoutAmount { get; set; }

        /// <summary>
        /// Сумма для продления - аванс
        /// </summary>
        [JsonProperty("UF_CRM_1610537041128")]
        public string ProlongAmountMinusExtra { get; set; }

        /// <summary>
        /// Признак для обработки сделки роботом в битрикс (передавать 0 int)
        /// Чтобы статусы звонков обнулялись у сотрудников soft-collection(забирает договоры из стадии недозвон в воронке SOFT collection)
        /// </summary>
        [JsonProperty("UF_CRM_1611680648161")]
        public int ForCallRobot => 0;

        /// <summary>
        /// Количество просроченных договоров с одинаковым ClientId
        /// </summary>
        [JsonProperty("UF_CRM_1703493444")]
        public int ActiveOverdueContractCount { get; set; }

        [JsonProperty("UF_CRM_1593955440")]
        public int StartProcessAfterCreateOrUpdateDeal => 1;
    }
}