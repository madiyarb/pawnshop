using System.Collections.Generic;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class UnsetFcbContractsResponse
    {
        /// <summary>
        /// Параметр шины <b><u>iin</u></b> (ИИН)
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>error</u></b> (код ошибки: 0 - ок, 1 - ошибка)
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Параметр шины <b><u>message</u></b> (текст сообщения)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contracts</u></b> (Список займов не отправленных в ПКБ)
        /// </summary>
        public List<UnsetFcbContractViewModel> Contracts { get; set; } = new List<UnsetFcbContractViewModel>();
    }
}
