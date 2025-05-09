using System;
using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.CardCashOut
{
    public interface ICardCashOutService
    {
        /// <summary>
        /// Метод который создает транзакцию вывода на карту сервисе Processing KZ 
        /// </summary>
        /// <param name="cardNumber">Номер карты на которую должен происходить вывод</param>
        /// <param name="trancheAmount">Количество денег в тиынах</param>
        /// <param name="customerReference">Уникальный идентификатор транзакции 12 знаков</param>
        /// <param name="contractId">Используется для формирования redirectedUrl на которую пользователь перейдет после ввода данных карты</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedResponseException">Сервис обработал запрос но ответ не получилось распарсить</exception>
        /// <exception cref="ProcessingServiceUnavailableException">Processing.kz не ответил на запрос</exception>
        public Task<StartCashOutTransaction.Envelope> StartCashOutTransaction(string cardNumber, string trancheAmount, string customerReference,
            int contractId, string baseReturnUrl, CancellationToken cancellationToken);

        public Task<bool> EnterCardNumber(string cardNumber, string tranGuid, string cardHolderName, CancellationToken cancellationToken);

        /// <summary>
        /// Подтверждение транзакции в сервисе Processing.kz 
        /// </summary>
        /// <param name="referenceNr">Уникальный идентификатор транзакции 12 символов</param>
        /// <param name="amount">Сумма в тыинах на которую происходит перевод</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedResponseException">Processing.KZ обработал запрос но мы не смогли его десериализовать</exception>
        /// <exception cref="ProcessingServiceUnavailableException">Сервис Processing.kz не обработал запрос</exception>
        public Task<CompleteCashOutTransaction.Envelope> CompleteCashOutTransaction(string referenceNr, string amount,
            CancellationToken cancellationToken);

        /// <summary>
        /// Получение информации о транзакции
        /// </summary>
        /// <param name="referenceNr">Уникальный идентификатор транзакции</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="UnexpectedResponseException"></exception>
        /// <exception cref="ProcessingServiceUnavailableException"></exception>
        public Task<GetCashOutTransactionStatus.Envelope> GetCashOutTransactionStatus(string referenceNr,
            CancellationToken cancellationToken);
    }
}
