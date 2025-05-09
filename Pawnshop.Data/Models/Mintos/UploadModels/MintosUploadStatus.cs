using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos.UploadModels
{
    public enum MintosUploadStatus : short
    {
        /// <summary>
        /// Ожидает выгрузки
        /// </summary>
        Await,

        /// <summary>
        /// Успешно выгружен
        /// </summary>
        Success,

        /// <summary>
        /// Отменён
        /// </summary>
        Canceled,

        /// <summary>
        /// Ошибка выгрузки
        /// </summary>
        Error,

        /// <summary>
        /// Статус договора в Mintos не принимает оплаты
        /// </summary>
        Declined
    }
}
