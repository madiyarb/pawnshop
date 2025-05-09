namespace Pawnshop.Services.MaximumLoanTermDetermination
{
    public sealed class MaximumLoanTermCarDeterminationModel
    {
        /// <summary>
        /// Год выпуска
        /// </summary>
        public int ReleaseYear { get; set; }

        /// <summary>
        /// Идентификатор марки машины
        /// </summary>
        public int CarMarkId { get; set; }

        /// <summary>
        /// Идентификатор модели машины
        /// </summary>
        public int CarModelId { get; set; }
    }
}
