using System;

namespace Pawnshop.Services.OTP
{
    public sealed class OTPCodeGeneratorService : IOTPCodeGeneratorService
    {
        private readonly string[] _otpCharacters;
        public OTPCodeGeneratorService()
        {
            _otpCharacters = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        }
        public string GenerateRandomOTP(int iOTPLength)
        {
            string sOTP = string.Empty;
            string sTempChars = string.Empty;
            Random rand = new Random();
            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, _otpCharacters.Length); 
                sTempChars = _otpCharacters[rand.Next(0, _otpCharacters.Length)];
                sOTP += sTempChars;
            }

            return sOTP;
        }
    }
}
