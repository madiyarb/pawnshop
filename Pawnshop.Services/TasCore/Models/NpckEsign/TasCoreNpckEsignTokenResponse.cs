namespace Pawnshop.Services.TasCore.Models.NpckEsign
{
    public class TasCoreNpckEsignTokenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Время истечения токена в секундах
        /// </summary>
        public int? ExpiresIn { get; set; }


        public TasCoreNpckEsignTokenResponse() { }

        public TasCoreNpckEsignTokenResponse(bool success, string message, string token = null, int? expiresIn = null)
        {
            Success = success;
            Message = message;
            Token = token;
            ExpiresIn = expiresIn;
        }
    }
}
