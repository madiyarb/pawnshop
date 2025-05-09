using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.TasLabRecruit
{
    public class RecruitListResponse
    {
        /// <summary>
        /// Индекс данных. Необходимо указывать данное значение в методе delta для дальнейшего получения данных, которые изменились/появились  между датами запроса метода list и delta.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// данные о призывниках
        /// </summary>
        public List<Recruit> Data { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string Message { get; set; }
    }
}
