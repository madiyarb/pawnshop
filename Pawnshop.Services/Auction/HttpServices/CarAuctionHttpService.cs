using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.Auction.HttpResponseModel;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Serilog;

namespace Pawnshop.Services.Auction.HttpServices
{
    public class CarAuctionHttpService : ICarAuctionHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public CarAuctionHttpService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CreatedAuctionDto> CreateCarAuction(CreateCarAuctionRequest request)
        {
            var url = $"{_httpClient.BaseAddress}/api/auction/create";

            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var createdAuctionDto = JsonConvert.DeserializeObject<CreatedAuctionDto>(jsonString);

                return createdAuctionDto;
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "Ошибка при отправке HTTP запроса на создание аукциона");
                throw new PawnshopApplicationException("Ошибка при отправке запроса на создание аукциона", e);
            }
            catch (TaskCanceledException)
            {
                throw new PawnshopApplicationException("Превышено ожидание ответа");
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Не удалось обработать HTTP ответ сервера (ошибка десериализации)");
                throw new PawnshopApplicationException(
                    "Не удалось обработать HTTP ответ сервера (ошибка десериализации)", e);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при обработке HTTP запроса на создание аукциона");
                throw new PawnshopApplicationException("Неизвестная ошибка при создании аукциона", e);
            }
        }

        public async Task RollbackCreated(int auctionId)
        {
            var url = $"{_httpClient.BaseAddress}/api/rollback/{auctionId}";

            try
            {
                var response = await _httpClient.PutAsync(url, null);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                _logger.Error(e, "Ошибка при отправке HTTP запроса на откат аукциона");
                throw new PawnshopApplicationException("Ошибка при отправке запроса на откат аукциона", e);
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Не удалось обработать HTTP ответ сервера (ошибка десериализации)");
                throw new PawnshopApplicationException(
                    "Не удалось обработать HTTP ответ сервера (ошибка десериализации)", e);
            }
            catch (TaskCanceledException)
            {
                throw new PawnshopApplicationException("Превышено ожидание ответа");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при обработке HTTP запроса на откат аукциона");
                throw new PawnshopApplicationException("Неизвестная ошибка при откате аукциона", e);
            }
        }
    }
}