using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Serilog;

namespace Pawnshop.Services.Auction.HttpServices
{
    public class AuctionOperationHttpService : IAuctionOperationHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public AuctionOperationHttpService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> Approve(ApproveAuctionCommand command)
        {
            var url = $"{_httpClient.BaseAddress}/api/operation/approve";

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(responseBody);
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "Ошибка при отправке HTTP запроса на подтверждение аукциона");
                throw new PawnshopApplicationException("Ошибка при отправке запроса на подтверждение аукциона", e);
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Ошибка при десериализации HTTP ответа при подтверждении аукциона");
                throw new PawnshopApplicationException(
                    "Ошибка при десериализации HTTP ответа при подтверждении аукциона", e);
            }
            catch (TaskCanceledException)
            {
                throw new PawnshopApplicationException("Превышено ожидание ответа");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Неизвестная ошибка при обработке HTTP запроса на подтверждение аукциона");
                throw new PawnshopApplicationException(
                    "Неизвестная ошибка при обработке запроса на подтверждение аукциона", e);
            }
        }

        public async Task<bool> Approve(ApproveAuctionCommand command, CancellationToken cancellationToken)
        {
            var url = $"{_httpClient.BaseAddress}/api/operation/approve";

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8,
                    "application/json");
                var response = await _httpClient.PutAsync(url, jsonContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(responseBody);
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "Ошибка при отправке HTTP запроса на подтверждение аукциона с токеном отмены");
                throw new PawnshopApplicationException(
                    "Ошибка при отправке запроса на подтверждение аукциона с токеном отмены", e);
            }
            catch (JsonException e)
            {
                _logger.Error(e,
                    "Ошибка при десериализации HTTP ответа при подтверждении аукциона с токеном отмены");
                throw new PawnshopApplicationException(
                    "Ошибка при десериализации HTTP ответа при подтверждении аукциона с токеном отмены", e);
            }
            catch (TaskCanceledException)
            {
                throw new PawnshopApplicationException("Превышено ожидание ответа");
            }
            catch (Exception e)
            {
                _logger.Error(e,
                    "Неизвестная ошибка при обработке HTTP запроса на подтверждение аукциона с токеном отмены");
                throw new PawnshopApplicationException(
                    "Неизвестная ошибка при обработке запроса на подтверждение аукциона с токеном отмены", e);
            }
        }

        public async Task<bool> Reject(Guid requestId)
        {
            var url = $"{_httpClient.BaseAddress}/api/operation/reject?requestId={requestId}";

            try
            {
                var response = await _httpClient.PutAsync(url, null);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(responseBody);
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "Ошибка при отправке HTTP запроса на отклонение аукциона");
                throw new PawnshopApplicationException("Ошибка при отправке запроса на отклонение аукциона", e);
            }
            catch (TaskCanceledException)
            {
                throw new PawnshopApplicationException("Превышено ожидание ответа");
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Ошибка при десериализации HTTP ответа при отклонении аукциона");
                throw new PawnshopApplicationException("Ошибка при десериализации HTTP ответа при отклонении аукциона", e);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Неизвестная ошибка при обработке HTTP запроса на отклонение аукциона");
                throw new PawnshopApplicationException(
                    "Неизвестная ошибка при обработке запроса на отклонение аукциона", e);
            }
        }
    }
}