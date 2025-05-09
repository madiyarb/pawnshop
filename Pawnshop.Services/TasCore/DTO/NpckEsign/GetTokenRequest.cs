namespace Pawnshop.Services.TasCore.DTO.NpckEsign
{
    public class GetTokenRequest
    {
        /// <summary>
        /// Код для подписания
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Адрес перенаправления после успешного подписания
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
