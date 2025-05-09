using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.CreditBureaus
{
    public enum CBBatchStatus : int
    {
        ///////////////////////НАШИ СТАТУСЫ
        /// <summary>
        /// Пакет в подготовке
        /// </summary>
        InProgress = 50,
        /// <summary>
        /// Пакет в подготовке
        /// </summary>
        Created = 100,
        /// <summary>
        /// Данные заполнены
        /// </summary>
        FulfillError = 150,
        /// <summary>
        /// Данные заполнены
        /// </summary>
        Fulfilled = 200,
        /// <summary>
        ///  Создан XML
        /// </summary>
        XMLCreated = 300,
        /// <summary>
        ///  Создан XML
        /// </summary>
        XMLCreationError = 350,
        /// <summary>
        /// Пакет отправлен
        /// </summary>
        Sent = 400,

        ///////////////////////СТАТУСЫ ПКБ
        /// <summary>
        /// Пакет был принят системой. Ожидается вставка в зеркальную базу данных.
        /// </summary>
        WaitingForImport = 519,
        /// <summary>
        /// Пакет импортируется в зеркальную базу данных.
        /// </summary>
        ImportMirror = 520,
        /// <summary>
        /// Пакет был передан в Зеркальную базу данных, ожидается проверка достоверности данных
        /// </summary>
        WaitingForCheck = 5200,
        
        /// <summary>
        /// Пакет импортируется в ЖИВУЮ базу данных.
        /// </summary>
        ImportLive = 524,
        /// <summary>
        /// Пакет содержит слишком много ошибок, чтобы быть обработанным, отказано системой.
        /// </summary>
        RejectedTooManyErrors = 525,
        /// <summary>
        /// Пакет ошибочный против схемы XML.
        /// </summary>
        RejectedBadXML = 527,
        /// <summary>
        /// Пакет успешно вставлен в ЖИВУЮ базу данных с предупреждающим сообщением
        /// </summary>
        AcceptedWithWarnings = 532,
        /// <summary>
        /// Пакет успешно вставлен в ЖИВУЮ базу данных.
        /// </summary>
        Accepted = 533,

        ///////////////////////СТАТУСЫ ГКБ
        /// <summary>
        /// Ошибка обработки (на сервере ГКБ)
        /// </summary>
        ErrorSCB = 898,
        /// <summary>
        /// Файл получен (на сервере ГКБ)
        /// </summary>
        SentSCB = 899,
        /// <summary>
        /// Файл подготовлен к обработке (на сервере ГКБ)
        /// </summary>
        PreparedForProcessing = 900,
        /// <summary>
        /// Начинается обработка (на сервере ГКБ)
        /// </summary>
        StartProcessing = 901,
        /// <summary>
        /// Файл находится в процессе обработки (на сервере ГКБ)
        /// </summary>
        Processing = 902,
        /// <summary>
        /// Файл обработан
        /// </summary>
        Processed = 903,
        /// <summary>
        /// Файл был отправлен вручную
        /// </summary>
        SentManually = 904
    }
}
