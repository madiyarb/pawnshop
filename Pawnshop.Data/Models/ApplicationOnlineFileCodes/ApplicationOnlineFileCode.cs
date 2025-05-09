using System;

namespace Pawnshop.Data.Models.ApplicationOnlineFileCodes
{
    public sealed class ApplicationOnlineFileCode
    {
        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public Guid Id { get; set; }
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
        /// Идентификатор кода файла в сервисе оценки
        /// </summary>
        public int? EstimationServiceCodeId { get; set; }
        /// <summary>
        /// Название файла отображаемое в ЦОИД
        /// </summary>
        public int? NpckTitleId { get; set; }
        public string NpckTitle { get; set; }


        public ApplicationOnlineFileCode()
        {

        }

        public ApplicationOnlineFileCode(Guid id, string businessType, string code, string title, string storageFileTitle, string category)
        {
            Id = id;
            BusinessType = businessType;
            Code = code;
            Title = title;
            StorageFileTitle = storageFileTitle;
            Category = category;
        }
    }
}
