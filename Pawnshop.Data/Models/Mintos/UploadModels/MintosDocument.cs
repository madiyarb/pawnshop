using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// Документы
    /// </summary>
    public class MintosDocument
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Файл в base64
        /// Разрешенные форматы:  png, gif, jpeg, bmp
        /// </summary>
        public string Content { get; set; }
    }
}
