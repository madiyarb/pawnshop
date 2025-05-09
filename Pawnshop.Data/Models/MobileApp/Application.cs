using System;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.MobileApp
{
    public class Application : IEntity
    {
        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int ClientId { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public Client Client { get; set; }
        /// <summary>
        /// Авто
        /// </summary>
        public int PositionId { get; set; }
        /// <summary>
        /// Авто
        /// </summary>
        public Position Position { get; set; }
        /// <summary>
        /// Идентификатор заявки в базе мобильного приложения
        /// </summary>
        public int AppId { get; set; }
        /// <summary>
        /// Идентификатор продавца
        /// </summary>
        public int? ApplicationMerchantId { get; set; }
        /// <summary>
        /// Продовец
        /// </summary>
        public ApplicationMerchant? ApplicationMerchant { get; set; }
        /// <summary>
        /// Дата создания заявки
        /// </summary>
        public DateTime ApplicationDate { get; set; }
        /// <summary>
        /// Сумма оценки
        /// </summary>
        public int EstimatedCost { get; set; }
        /// <summary>
        /// Первоначальный взнос
        /// </summary>
        public int? PrePayment { get; set; }
        /// <summary>
        /// Желаемая сумма
        /// </summary>
        public int RequestedSum { get; set; }
        /// <summary>
        /// Идентификатор пользователя создавшего заявку
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Сумма из реестра должников 
        /// </summary>
        public int DebtorsRegisterSum { get; set; }
        /// <summary>
        /// Сумма light лимита
        /// </summary>
        public int? LightCost { get; set; }
        /// <summary>
        /// Сумма turbo лимита
        /// </summary>
        public int? TurboCost { get; set; }
        /// <summary>
        /// Сумма motor лимита
        /// </summary>
        public int? MotorCost { get; set; }
        /// <summary>
        /// Сумма лимита
        /// </summary>
        public int? LimitSum { get; set; }
        /// <summary>
        /// Статус заявки
        /// </summary>
        public  ApplicationStatus Status { get; set; }
        /// <summary>
        /// Гарантия менеджера
        /// </summary>
        public bool ManagerGuarentee { get; set; }
        /// <summary>
        ///  Без права вождения
        /// </summary>
        public bool WithoutDriving { get; set; }
        /// <summary>
        ///  Признак рефинансирования (добора)
        /// </summary>
        public bool IsAddition { get; set; }
        /// <summary>
        /// Ссылка на родительский договор
        /// </summary>
        private int? parentContractId = null;
        public int? ParentContractId
        {
            get { return parentContractId; }
            set { parentContractId = value == 0 || value is null ? null : value; }
        }
        /// <summary>
        /// Идентификатор заявки в Битриксе
        /// </summary>
        public int? BitrixId { get; set; }
        /// <summary>
        /// Признак автокредита
        /// </summary>
        public int? IsAutocredit { get; set; }
        /// <summary>
        /// Перечисления признаков договора для заявки
        /// </summary>
        public ContractClass? ContractClass { get; set; }
    }
}