using Pawnshop.Core;
using Pawnshop.Data.Models.Restructuring;
using System;

namespace Pawnshop.Data.Models.ClientDeferments
{
    public class ClientDeferment : IEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// id клиента
        /// </summary>
        public int ClientId { get; set; }
        /// <summary>
        /// id контракта
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// статус реструктуризации контракта
        /// </summary>
        public bool IsRestructured { get; set; }
        /// <summary>
        /// Cтатус Реструктуризации
        /// New - 0
        /// Frozen - 10
        /// Restructured - 20
        /// </summary>
        public RestructuringStatusEnum Status { get; set; }

        /// <summary>
        /// статус клиента (служит? находится в режиме ЧС?)
        /// </summary>
        public bool RecruitStatus { get; set; }
        /// <summary>
        /// тип отсрочки
        /// </summary>
        public int DefermentTypeId { get; set; }
        /// <summary>
        /// дата начала действия отсрочки
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// дата окончания действия отсрочки
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// дата удаления отсрочки
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// дата создания отсрочки
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// дата обновления отсрочки
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
