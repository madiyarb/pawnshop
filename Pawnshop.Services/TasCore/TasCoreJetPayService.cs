using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.CardCashOutTransaction;
using Pawnshop.Services.TasCore.DTO.JetPay;
using Serilog;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pawnshop.Services.TasCore.Models.JetPay;
using Pawnshop.Services.TasCore.Options;

namespace Pawnshop.Services.TasCore
{
    public class TasCoreJetPayService : ITasCoreJetPayService
    {
        private HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger _logger;

        public TasCoreJetPayService(
            HttpClient httpClient,
            IOptions<TasCoreOptions> options,
            ILogger logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.TimeoutSeconds);
            _logger = logger;
        }

        public async Task<TasCoreJetPayCardCashoutResponse> CreateCardCashoutAsync(TasCoreJetPayCardCashoutRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var requestStr = JsonSerializer.Serialize(request, _serializerOptions);
                var content = new StringContent(requestStr, Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"/api/jetpay/cardcashout",
                    content, cancellationToken: cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    var responseStr = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<TasCoreCardCashoutCreateResponse>(responseStr);

                    return new TasCoreJetPayCardCashoutResponse { Success = false, Message = response.Message };
                }

                return new TasCoreJetPayCardCashoutResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка выдачи средств по договору {request.ContractId}: {ex.Message}");
                return new TasCoreJetPayCardCashoutResponse { Success = false, Message = "Ошибка вызова сервиса TasCoreJetPayService." };
            }
        }

        public async Task<CardCashOutTransactionStatus> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _httpClient.GetAsync($"/api/jetpay/cardcashout/{paymentId}/status", cancellationToken);
                var responseStr = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<TasCoreCardCashoutStatusResponse>(responseStr);

                if (result.IsSuccessStatusCode)
                    return (CardCashOutTransactionStatus)response.Status;

                throw new PawnshopApplicationException(response.Message);
            }
            catch (Exception ex)
            {
                var errMsg = $"Ошибка получения статуса выдачи {paymentId}: {ex.Message}";
                _logger.Error(errMsg);
                throw new PawnshopApplicationException(errMsg);
            }
        }
    }
}
