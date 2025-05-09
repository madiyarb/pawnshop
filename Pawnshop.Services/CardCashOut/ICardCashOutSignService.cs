using System.Threading.Tasks;

namespace Pawnshop.Services.CardCashOut
{
    public interface ICardCashOutSignService
    {
        /// <summary>
        /// Сервис подтверждает ранее созданые действия для подписания договора и создает проводки, осуществляет рефинансирование если заказано
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="oldRefinance"></param>
        /// <returns></returns>
        public Task Sign(int contractId, bool oldRefinance = false);
    }
}
