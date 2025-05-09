using System;
using System.Security.Cryptography.X509Certificates;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Insurances
{
    /// <summary>
    /// Заявки в СК на оформление страховки
    /// </summary>
    public class InsurancePoliceRequest : IEntity
    {
        /// <summary>
        /// Идентификатор для BPM
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        public string ContractNumber { get; set; }
        /// <summary>
        /// Идентификатор СК
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        public Client InsuranceCompany { get; set; }
        /// <summary>
        /// Статус заявки
        /// </summary>
        public InsuranceRequestStatus Status { get; set; }
        /// <summary>
        /// Признак обязательности страхования
        /// </summary>
        public bool IsInsuranceRequired { get; set; }
        /// <summary>
        /// Пользователь, разрешивший выдачу без страхования
        /// </summary>
        public int? CancelledUserId { get; set; }
        public User CancelledUser { get; set; }
        /// <summary>
        /// Дата разрешения выдачи без страхования
        /// </summary>
        public DateTime? CancelDate { get; set; }
        /// <summary>
        /// Причина отмены страхования
        /// </summary>
        public string CancelReason { get; set; }
        /// <summary>
        /// Идентификатор онлайн-заявки
        /// </summary>
        public int? OnlineRequestId { get; set; }
        public InsuranceOnlineRequest OnlineRequest { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        public int AlgorithmVersion { get; set; }
        public InsuranceRequestData RequestData { get; set; }
        public bool InsurancePoliceExists { get; set; }
        public DateTime? CanceledDateByClient { get; set; }
        public string RequestDataBPM { get; set; }
    }
}