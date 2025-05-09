
namespace Pawnshop.Services.OTP
{
    public interface IOTPCodeGeneratorService
    {
        /// <summary>
        /// Создает случайную строку
        /// </summary>
        /// <param name="iOTPLength">Длина строки</param>
        /// <returns></returns>
        public string GenerateRandomOTP(int iOTPLength);
    }
}
