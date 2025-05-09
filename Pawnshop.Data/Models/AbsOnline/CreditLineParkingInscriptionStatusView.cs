using System.Collections.Generic;

namespace Pawnshop.Data.Models.AbsOnline
{
    /// <summary>
    /// Статус парковки и ЧСИ КЛ
    /// </summary>
    public class CreditLineParkingInscriptionStatusView
    {
        /// <summary>
        /// Статус парковки
        /// </summary>
        public CarStatusView CarStatus { get; set; }

        /// <summary>
        /// Список статусов ЧСИ траншей КЛ
        /// </summary>
        public List<TrancheInscriptionStatus> TrancheInscriptionStatusList { get; set; } = new List<TrancheInscriptionStatus>();
    }
}
