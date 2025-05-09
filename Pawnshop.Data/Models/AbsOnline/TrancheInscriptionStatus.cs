using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    /// <summary>
    /// Статус ЧСИ транша
    /// </summary>
    public class TrancheInscriptionStatus
    {
        /// <summary>
        /// Параметр шины <b><u>contract_id</u></b> (номер контракта)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>id</u></b> (идентификатор займа)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Параметр шины <b><u>inscription_status</u></b> (статус ЧСИ)
        /// </summary>
        public string InscriptionStatus { get; set; }

        /// <summary>
        /// Параметр шины <b><u>inscription_status_code</u></b> (код статуса ЧСИ)
        /// </summary>
        public string InscriptionStatusCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>update_date</u></b> (дата обновления статуса)
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
