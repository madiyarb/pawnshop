using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.AbsOnline
{
    /// <summary>
    /// Интерфейс для запросов AbsOnline
    /// </summary>
    public interface IAbsOnlineService
    {
        /// <summary>
        /// Метод создает в БД болванки страхования
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <param name="contract">Контракт</param>
        void CreateInsurancePolicy(int contractId, Contract contract = null);

        /// <summary>
        /// Метод удаляет не завершенные записи страхования в БД
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        void DeleteInsuranceRecords(int contractId);

        /// <summary>
        /// Метод возвращет СМС код
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <returns>СМС код</returns>
        string GetSmsCode(int contractId);

        /// <summary>
        /// Метод возвращает URL API формирования PDF
        /// </summary>
        /// <returns>URL API формирования PDF</returns>
        string GetUrlPdfApi();

        /// <summary>
        /// Метод регистрирует страховой запрос в СК
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <param name="contract">Контракт</param>
        /// <returns>Результат обработки</returns>
        string RegisterPolicy(int contractId, Contract contract = null);

        /// <summary>
        /// Метод сохраняет дополнительную информацию по контракту
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <param name="smsCode">СМС код</param>
        /// <param name="branchId">Идентификатор филиала</param>
        /// <param name="partnerCode">Код партнера</param>
        void SaveAdditionalInfo(int contractId, string smsCode = null, int? branchId = null, string partnerCode = null);

        /// <summary>
        /// Метод сохраняет запись запроса заявки на страхование для повторной отправки
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        void SaveRetrySendInsurance(int contractId);

        /// <summary>
        /// Метод отправляет уведомление о закрытии контракта
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <param name="contract">Контракт</param>
        /// <returns>Текст ошибки</returns>
        Task<string> SendNotificationCloseContractAsync(int contractId, Contract contract = null);

        /// <summary>
        /// Метод отправляет уведомление об изменениях в КЛ
        /// </summary>
        /// <param name="contractId">Идентификатор контракта</param>
        /// <param name="contract">Контракт</param>
        Task SendNotificationCreditLineChangedAsync(int contractId, Contract contract = null);

        /// <summary>
        /// Метод отправляет уведомление о выходе на просрочку
        /// </summary>
        /// <param name="overdueContracts">Список контрактов с просрочкой</param>
        Task SendNotificationOverdueContractListAsync(List<OverdueForCrm> overdueContracts);
    }
}
