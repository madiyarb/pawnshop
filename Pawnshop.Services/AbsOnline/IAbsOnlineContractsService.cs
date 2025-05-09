using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.AbsOnline
{
    /// <summary>
    /// Интерфейс обработки запросов по контрактам для AbsOnline
    /// </summary>
    public interface IAbsOnlineContractsService
    {
        /// <summary>
        /// Метод возвращает список контрактов по ИИН
        /// </summary>
        /// <param name="iin">ИИН</param>
        /// <returns>Список контрактов</returns>
        Task<IEnumerable<Contract>> GetContractsByIdentityNumberAsync(string iin);

        /// <summary>
        /// Метод возвращает список контрактов для МП
        /// </summary>
        /// <param name="contracts">Список контрактов</param>
        /// <returns>Список контрактов</returns>
        List<AbsOnlineContractView> GetContractViewListAsync(IEnumerable<Contract> contracts);

        /// <summary>
        /// Метод возвращает статус парковки и ЧСИ КЛ
        /// </summary>
        /// <param name="creditLineNumber">Номер кредитной линии</param>
        /// <returns>Статус парковки и ЧСИ КЛ</returns>
        Task<CreditLineParkingInscriptionStatusView> GetCreditLineParkingInscriptionStatusAsync(string creditLineNumber);

        /// <summary>
        /// Метод возвращает модель списка кредитных линий
        /// </summary>
        /// <param name="contracts">Список контрактов</param>
        /// <returns>Модель списка кредитных линий</returns>
        List<AbsOnlineCreditLineView> GetCreditLineViewListAsync(IEnumerable<Contract> contracts);

        /// <summary>
        /// Метод возвращает список кредитных линий и список контрактов для МП
        /// </summary>
        /// <param name="contracts">Список контрактов</param>
        /// <returns>Список кредитных линий и список контрактов</returns>
        AbsOnlineContractList GetViewListAsync(IEnumerable<Contract> contracts);

        /// <summary>
        /// Метод возвращает договора клиента для mobile_main_screen
        /// </summary>
        /// <param name="iin">Иин клиента</param>
        /// <returns></returns>
        Task<MobileMainScreenView> GetMobileMainScreenView(string iin);

        /// <summary>
        /// Метод возвращает информация о контракте
        /// </summary>
        /// <param name="contractNumber">Номер контракта</param>
        /// <returns>Информация о контракте</returns>
        Task<AbsOnlineContractMobileView> GetMobileContractData(string contractNumber);
    }
}