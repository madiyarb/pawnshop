namespace Pawnshop.Services.MaximumLoanTermDetermination
{
    public interface IMaximumLoanTermDeterminationService
    {
        /// <summary>
        /// Сервис определения максимального срока займа исходя из продукта и авто.
        /// </summary>
        /// <param name="productId"> Идентификатор продукта LoanPercentSettingsId</param>
        /// <param name="car"> Сущность автомобиля идентификаторы mark, model. И дата выпуска автомобиля</param>
        /// <returns> Максимальный срок займа</returns>
        /// <exception cref="MaximumLoanTermCannotDetermineException">В случае если какие либо параметры не найдены в базе</exception>
        public int Determinate(int productId, MaximumLoanTermCarDeterminationModel? car = null);
    }
}
