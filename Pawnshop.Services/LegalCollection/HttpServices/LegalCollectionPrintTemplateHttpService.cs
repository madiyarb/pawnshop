using Newtonsoft.Json;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionPrintTemplateHttpService : ILegalCollectionPrintTemplateHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionPrintTemplateHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<List<LegalCasePrintTemplate>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcaseprintform/getlegalcaseprintformlist";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<LegalCasePrintTemplate>>(responseBody);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        
        private string GetUrl(string login)
        {
            var settings = _settingsRepository.Find(new { Code = login });
            if (settings is null)
            {
                throw new PawnshopApplicationException("Не удалось получить настройки Legal-collection");
            }
            
            var url = settings.URL;
            if (!url.HasValue())
            {
                throw new PawnshopApplicationException("Не удалось получить Url микросервиса Legal-collection");
            }
            
            return url;

        }
    }
}
