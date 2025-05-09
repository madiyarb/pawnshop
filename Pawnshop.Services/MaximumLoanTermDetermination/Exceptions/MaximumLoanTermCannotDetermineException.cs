using System;

namespace Pawnshop.Services.MaximumLoanTermDetermination.Exceptions
{
    /// <summary>
    /// Exception для информации о невозможности определения максимального срока не найден продукт или другие ошибки настройки продукта 
    /// </summary>
    public sealed class MaximumLoanTermCannotDetermineException : Exception
    {
        public MaximumLoanTermCannotDetermineException(string message) : base(message)
        {
        }
    }
}
