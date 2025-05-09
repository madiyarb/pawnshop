using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Online1C;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.Online1C
{
    public interface IOnline1CService
    {
        /// <summary>Ручная отправка отчета в 1C</summary>
        public Task<(bool, string)> SendReportManual(Online1CReportData data);

        /// <summary>Отправка отчета в 1C</summary>
        /// <param name="jsonTasks">jsonTasks принимает массив типа отчета и json</param>
        /// <returns>Возвращает успешен ли запрос или нет и ответ сервера 1С</returns>
        public Task<(bool, string)> SendReport(Online1CReportData data, params Task<(Online1CReportType, string)>[] jsonTasks);

        /// <summary>Получить json начисления</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetAccrualsJson(Online1CReportData data);

        /// <summary>Получить json погашения</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetPaymentJson(Online1CReportData data);

        /// <summary>Получить json выдач</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetIssuesJson(Online1CReportData data);

        /// <summary>Получить json поступлении денег</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetPrepaymentJson(Online1CReportData data);

        /// <summary>Получить json освоение аванса</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetDebitDeposJson(Online1CReportData data);

        /// <summary>Получить json кассовые операции, не относящиеся к кредитам</summary>
        /// <returns>Возвращает тип отчета и json</returns>
        Task<(Online1CReportType, string)> GetCashFlowsJson(Online1CReportData data);
    }
}
