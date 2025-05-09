using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Files
{
    /// <summary>
    /// Информация о файле
    /// </summary>
    public class FileRow : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Тип содержимого
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Удален
        /// </summary>
        public bool IsDelete { get; set; }
    }
}