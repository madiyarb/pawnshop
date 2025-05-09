using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFiles.Views
{
    public sealed class ApplicationOnlineFileView
    {
        /// <summary>
        /// Идентификатор файла внутренний и в БД
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор заявки 
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Идентификатор файла в хранилище ДРПП
        /// </summary>
        public Guid StorageFileId { get; set; }

        /// <summary>
        /// Имя файла в хранилище оценки
        /// </summary>
        public string EstimationStorageFileName { get; set; }

        /// <summary>
        /// Тип файла картинка, видео итд
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ContentType файла
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Идентификатор PreviewId 
        /// </summary>
        public Guid? PreviewId { get; set; }

        /// <summary>
        /// Url по которому файл доступен
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// Дата удаления файла 
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Код типа файла в хранилище ДРПП
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// Код типа файла в нашем хранилище
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Наименование у ДРПП
        /// </summary>
        public string StorageFileTitle { get; set; }

        /// <summary>
        /// Категория файлов TODO надо распросить че там за категория то ? 
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Дополнительный файл
        /// </summary>
        public bool IsAdditionalPhoto { get; set; }

        /// <summary>
        /// Отправлять на оценку 
        /// </summary>
        public bool SendToEstimate { get; set; }

        public Guid ApplicationOnlineFileCodeId { get; set; }
    }
}
