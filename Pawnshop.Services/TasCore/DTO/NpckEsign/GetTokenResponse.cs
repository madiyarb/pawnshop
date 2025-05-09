namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class GetTokenResponse : TasCoreBaseResponse
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Время истечения токена в секундах
        /// </summary>
        public int? ExpiresIn { get; set; }
    }
}
