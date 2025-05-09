using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineClientDocumentView
    {
        /// <summary>
        /// Документ
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Вид документа
        /// </summary>
        public string DocumentTypeDescribed { get; set; }

        /// <summary>
        /// Номер документа
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Дата документа
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Организация выдавшая документ
        /// </summary>
        public string Organization { get; set; }


    }
}
