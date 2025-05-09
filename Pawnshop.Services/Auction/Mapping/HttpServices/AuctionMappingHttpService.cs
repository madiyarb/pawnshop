using System.Net.Http;
using System.Threading.Tasks;
using Pawnshop.Services.Auction.Mapping.HttpServices.Interfaces;
using Pawnshop.Data.Models.Auction.Dtos.Mapping;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using Pawnshop.Services.Auction.Mapping.Dtos;

// todo удалить после успешного маппинга
namespace Pawnshop.Services.Auction.Mapping.HttpServices
{
    public class AuctionMappingHttpService : IAuctionMappingHttpService
    {
        private readonly ILogger<AuctionMappingHttpService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        

        public AuctionMappingHttpService(
            ILogger<AuctionMappingHttpService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;

            var auctionBaseUrl = _configuration.GetSection("AuctionSettings").GetSection("BaseUrl").Value;
            _httpClient.BaseAddress = new Uri(auctionBaseUrl);
        }

        public async Task<CreateMappingCarAuctionDto> SendCarDataAsync(AuctionMappingCarApiDto request)
        {
            CreateMappingCarAuctionDto response = await SendAuctionCarDataAsync(request);
            return response;
        }

        public async Task<bool> SendExpenseDataAsync(AuctionMappingExpenseApiDto request)
        {
            var response = await SendAuctionExpenseDataAsync(request);
            return response;
        }

        private async Task<CreateMappingCarAuctionDto> SendAuctionCarDataAsync(AuctionMappingCarApiDto request)
        {
            if (request is null)
            {
                _logger.LogWarning("Запрос на создание авто маппинга не может быть null.");
                return null;
            }

            try
            {
                var url = $"{_httpClient.BaseAddress}/api/mapping/car-map";
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CreateMappingCarAuctionDto>(responseContent);
                    _logger.LogInformation("Авто успешно создано в процессе маппинга: {Response}", responseContent);
                    return result;
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка при создании авто маппинга. Статус: {Status}, Ответ: {Error}",
                    response.StatusCode, errorMessage);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Неожиданная ошибка при создании авто маппинга: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<bool> SendAuctionExpenseDataAsync(AuctionMappingExpenseApiDto request)
        {
            try
            {
                var url = $"{_httpClient.BaseAddress}/api/mapping/expense-map";
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("При попытке создания расхода в процессе маппинга произошла ошибка: {Error}", errorMessage);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("При попытке создания расхода в процессе маппинга произошла ошибка: {Error}", ex.Message);
                throw;
            }
        }
    }
}
