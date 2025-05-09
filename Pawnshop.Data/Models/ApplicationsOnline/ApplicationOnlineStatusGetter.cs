using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public static class ApplicationOnlineStatusGetter
    {

        public static Dictionary<int, string> GetOnlineEstimationStatuses()
        {
            Dictionary<int, string> estimationStatuses = new Dictionary<int, string>();
            estimationStatuses.Add(0, "Новая");
            estimationStatuses.Add(1, "На Рассмотрении До");
            estimationStatuses.Add(2, "Одобрен");
            estimationStatuses.Add(3, "Договор Заключен");
            estimationStatuses.Add(4, "Отказ");
            estimationStatuses.Add(5, "Требует Корректирования");
            estimationStatuses.Add(6, "Аннулирован");
            estimationStatuses.Add(7, "На Оценке");
            estimationStatuses.Add(8, "Договор Просрочен");
            estimationStatuses.Add(9, "Договор Закрыт");
            estimationStatuses.Add(10, "Расчет_Кдн");
            estimationStatuses.Add(11, "Кредитный_Комитет");
            return estimationStatuses;
        }
    }
}
