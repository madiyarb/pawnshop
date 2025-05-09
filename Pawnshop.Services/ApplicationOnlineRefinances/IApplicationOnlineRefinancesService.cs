using Pawnshop.Data.Models.ApplicationsOnline;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.ApplicationOnlineRefinances
{
    public interface IApplicationOnlineRefinancesService
    {
        /// <summary>
        /// Создает записи о необходимости рефинансирования в случае если авто уже есть в базе с действующими договорами
        /// </summary>
        /// <param name="vinCode">Вин код авто</param>
        /// <param name="applicationOnlineId">Идентификатор заявки</param>
        /// <returns></returns>
        public Task<bool> CreateRequiredRefinances(string vinCode, Guid applicationOnlineId);

        /// <summary>
        /// Получение информации о договорах которые участвуют в рефинансировании 
        /// </summary>
        /// <param name="applicationOnlineId"></param>
        /// <returns></returns>
        public Task<List<ApplicationOnlineRefinancedContractInfo>> GetRefinancedContractInfo(Guid applicationOnlineId);

        /// <summary>
        /// Метод изменения договоров выбранных для рефинансирования
        /// </summary>
        /// <param name="applicationId">Идентификатор заявки</param>
        /// <param name="refinancedContractsIds">Идентификаторы договоров для рефинансирования</param>
        /// <returns></returns>
        public Task UpdateApplicationOnlineRefinancesList(Guid applicationId, List<int> refinancedContractsIds);

        /// <summary>
        /// Установка идентификатора договора за счет которого будут рефинансироваться договора(когда они создаются в /approve)
        /// </summary>
        /// <param name="application">Идентификатор заявки</param>
        /// <returns></returns>
        public Task SetContractIdForRefinancedItems(ApplicationOnline application);

        /// <summary>
        /// Вычислисление суммы необходимой для рефинансирования договоров
        /// </summary>
        /// <param name="contractId">Идентификатор договора за счет которого происходит рефинансировани</param>
        /// <returns></returns>
        public Task<decimal> CalculateRefinanceAmountForContract(int contractId);

        /// <summary>
        /// Метод перемещения денег счетов КЛ на счета Траншей 
        /// </summary>
        /// <param name="contractId">Идентификатор договора</param>
        /// <returns></returns>
        public Task<bool> MovePrepaymentForRefinance(int contractId, int branchId);

        /// <summary>
        /// Определение является ли договор участвующим в рефинансировании
        /// </summary>
        /// <param name="contractId">Идентификатор договора</param>
        /// <returns></returns>
        public Task<bool> IsRefinance(int contractId);

        /// <summary>
        /// Рефинансировании всех связаных с договором, траншей
        /// </summary>
        /// <param name="contractId">Идентификатор договора за счет которого осуществляется реф</param>
        /// <returns></returns>
        public Task<bool> RefinanceAllAssociatedContracts(int contractId);

        /// <summary>
        /// Изменение суммы заявки для того чтобы рефинанс произошел без добора(вывода денег на карту или счет)
        /// </summary>
        /// <param name="applicationId">Идентификатор заявки</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public Task CorrectApplicationAmountSumForRefinancedContracts(Guid applicationId, int userId);

        /// <summary>
        /// Метод внутреннего рефинансирования без вывода денег 
        /// </summary>
        /// <param name="contractId">Идентификатор договра</param>
        /// <param name="insurance">Есть ли страховка на договоре</param>
        /// <returns></returns>
        public Task InternalRefinance(int contractId);

        /// <summary>
        /// Метод проверяет есть ли необходимая сумма для рефинансирования на балансах кредитной линии 
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public Task<string> EnoughMoneyForRefinancing(int contractId);


        public Task<bool> IsApplicationAmountMoreThenRefinancedSum(ApplicationOnline application);
    }
}
