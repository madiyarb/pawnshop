using System;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.ApplicationOnlineFileCodes;

namespace Pawnshop.Data.Models.ApplicationOnlineFiles
{
    [Table("ApplicationOnlineFiles")]
    public sealed class ApplicationOnlineFile
    {
        /// <summary>
        /// Идентификатор файла внутренний и в БД
        /// </summary>
        [ExplicitKey]
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
        public Guid PreviewId { get; set; }

        /// <summary>
        /// Описание 
        /// </summary>
        public string Description { get; set; }

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
        /// Идентификатор кода файла 
        /// </summary>
        public Guid ApplicationOnlineFileCodeId { get; set; }

        /// <summary>
        /// Детали о коде файла
        /// </summary>
        [Computed]
        public ApplicationOnlineFileCode ApplicationOnlineFileCode { get; set; }

        /// <summary>
        /// Дополнительное фото 
        /// </summary>
        public bool IsAdditionalPhoto { get; set; }

        /// <summary>
        /// Отправлять на оценку 
        /// </summary>
        public bool SendToEstimate { get; set; }

        /// <summary>
        /// Идентификатор файла в сервисе оценки
        /// </summary>
        public int? EstimationServiceFileId { get; set; }


        public ApplicationOnlineFile() { }

        public ApplicationOnlineFile(Guid id, Guid applicationId, Guid storageFileId, string type, string contentType,
            string previewId, Func<string> originalUrlBuilder, Guid applicationOnlineFileCodeId, bool isAdditionalPhoto = false,
            bool sendToEstimate = true, int? estimationServiceFileId = null)
        {
            Id = id;
            ApplicationId = applicationId;
            StorageFileId = storageFileId;
            Type = type;
            ContentType = contentType;
            Url = originalUrlBuilder.Invoke();
            ApplicationOnlineFileCodeId = applicationOnlineFileCodeId;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
            IsAdditionalPhoto = isAdditionalPhoto;
            SendToEstimate = sendToEstimate;
            EstimationServiceFileId = estimationServiceFileId;
        }

        public void Delete()
        {
            DeleteDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }
    }
}
