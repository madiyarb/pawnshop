using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.Storage;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.Fcb
{
    public class Fcb4Kdn : IFcb4Kdn
    {
        private readonly HttpClient _http;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        private readonly FcbReportRepository _fcbReportRepository;
        private readonly ISessionContext _sessionContext;
        private readonly IStorage _storage;
        private readonly IClientService _clientService;
        private readonly DomainValueRepository _domainValueRepository;

        public Fcb4Kdn(HttpClient http, IRepository<OuterServiceSetting> outerServiceSettingRepository,
                       FcbReportRepository fcbReportRepository, ISessionContext sessionContext, IStorage storage,
                       IClientService clientService, DomainValueRepository domainValueRepository)
        {
            _http = http;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _fcbReportRepository = fcbReportRepository;
            _sessionContext = sessionContext;
            _storage = storage;
            _clientService = clientService;
            _domainValueRepository = domainValueRepository;
        }

        private decimal GetCheckedIncome(decimal income)
        {
            if (income == 0) return Constants.IncomeDefaultValue;
            return income;
        }

        public async Task<FcbKdnResponse> StorekdnReqWithIncome(FcbKdnRequest fcbKdnRequest)
        {
            var result = new FcbKdnResponse();
            var settings = _outerServiceSettingRepository.Find(new { Code = "FCBSERVICE" });
            if (settings == null)
            {
                result.ErrorCode = -1;
                result.ErrorMessage = "Не найдены настройки подключения к сервису FCBService";
                return result;
            }
            try
            {
                fcbKdnRequest.Income = GetCheckedIncome(fcbKdnRequest.Income);
                var data = new StringContent(JsonConvert.SerializeObject(fcbKdnRequest), Encoding.UTF8, "application/json");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    AuthenticationSchemes.Basic.ToString(),
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Login}:{settings.Password}"))
                    );
                var response = await _http.PostAsync($"{settings.URL}api/v1/kdn", data);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<FcbKdnResponse>(responseContent);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = -1;
                result.ErrorMessage = ex.Message;
            }
            result.ErrorCode = -1;
            result.ErrorMessage = "Ошибка при подключении к сервису FCBService";
            return result;
        }

        public async Task<FcbReportResponse> GetReport(FcbReportRequest fcbReportRequest)
        {
            var result = new FcbReportResponse();
            var avErr = new CigResultError();
            var errMess = new CigResultErrorErrmessage();
            avErr.Errmessage = errMess;
            result.AvailableReportsErrorResponse = avErr;

            var settings = _outerServiceSettingRepository.Find(new { Code = "FCBSERVICE" });
            if (settings == null)
            {
                result.AvailableReportsErrorResponse.Errmessage.code = -1;
                result.AvailableReportsErrorResponse.Errmessage.Value = "Не найдены настройки подключения к сервису FCBService";
                return result;
            }
            try
            {
                fcbReportRequest.ReportType = fcbReportRequest.ReportType.HasValue ? fcbReportRequest.ReportType.Value : FCBReportTypeCode.IndividualStandard;
                var data = new StringContent(JsonConvert.SerializeObject(fcbReportRequest), Encoding.UTF8, "application/json");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    AuthenticationSchemes.Basic.ToString(),
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Login}:{settings.Password}"))
                    );
                var response = await _http.PostAsync($"{settings.URL}api/v1/Reports", data);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<FcbReportResponse>(responseContent);
                    if (result.AvailableReportsErrorResponse == null)
                    {
                        var fcbReport = new FCBReport()
                        {
                            AuthorId = fcbReportRequest.AuthorId,
                            ClientId = fcbReportRequest.ClientId,
                            FolderName = result.FolderName,
                            XmlFileLink = result.XmlLink,
                            PdfFileLink = result.PdfLink,
                            CreateDate = DateTime.Now,
                            ReportType = (FCBReportTypeCode)(fcbReportRequest.ReportType != null ? fcbReportRequest.ReportType : FCBReportTypeCode.IndividualStandard)
                        };
                        _fcbReportRepository.Insert(fcbReport);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.AvailableReportsErrorResponse.Errmessage.code = -1;
                result.AvailableReportsErrorResponse.Errmessage.Value = ex.Message;
                return result;
            }
            result.AvailableReportsErrorResponse.Errmessage.code = -1;
            result.AvailableReportsErrorResponse.Errmessage.Value = "Ошибка при подключении к сервису FCBService";
            return result;
        }

        public async Task<FcbOurReportResponse> GetOurReport(FcbOurReportsRequest request)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "FCBSERVICE" });
            if (settings == null)
            {
                throw new PawnshopApplicationException("Не найдены настройки подключения к сервису FCBService");
            }
            var data = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                AuthenticationSchemes.Basic.ToString(),
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Login}:{settings.Password}"))
                );
            var response = await _http.PostAsync($"{settings.URL}api/OurReports", data);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FcbOurReportResponse>(responseContent);
            }
            return null;
        }

        public async Task<byte[]> GetOurReportExcel(FcbOurReportsRequest request)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "FCBSERVICE" });
            if (settings == null)
            {
                throw new PawnshopApplicationException("Не найдены настройки подключения к сервису FCBService");
            }
            var data = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                AuthenticationSchemes.Basic.ToString(),
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Login}:{settings.Password}"))
                );
            var response = await _http.PostAsync($"{settings.URL}api/OurReports/Excel", data);
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            if (response.IsSuccessStatusCode)
            {
                return responseContent;
            }
            return null;
        }

        public async Task<bool> CheckOverdueClient(Stream xmlStream)
        {
            using (StreamReader reader = new StreamReader(xmlStream))
            {
                string xmlData = await reader.ReadToEndAsync();

                string pattern = @"<OverdueContracts\b[^>]*>.*?</OverdueContracts>";
                Regex regex = new Regex(pattern, RegexOptions.Singleline);

                return regex.IsMatch(xmlData);
            }
        }

        public async Task<FCBChecksResult> FCBChecks(Stream xmlStream)
        {
            return new FCBChecksResult()
            {
                IsGumbler = await CheckGamblerFeature(xmlStream),
                IsStopCredit = await CheckStopCreditFromReport(xmlStream)
            };
        }

        public async Task<bool> CheckGamblerFeature(Stream xmlStream)
        {
            return await CheckReportFields(xmlStream, @"<gamblerfeature>(true|false)</gamblerfeature>");
        }

        public async Task<bool> CheckStopCreditFromReport(Stream xmlStream)
        {
            return await CheckReportFields(xmlStream, @"<stopcredit>(true|false)</stopcredit>");
        }

        private async Task<bool> CheckReportFields(Stream xmlStream, string pattern)
        {
            using (StreamReader reader = new StreamReader(xmlStream))
            {
                string xmlData = await reader.ReadToEndAsync();
                xmlData = xmlData.ToLower();

                Match match = Regex.Match(xmlData, pattern);
                if (match.Success)
                {
                    string value = match.Groups[1].Value;
                    return value switch
                    {
                        "true" => true,
                        _ => false,
                    };
                }
            }

            return false;
        }

        public async Task<bool> ValidateIsOverdueClient(int clientId)
        {
            if (_domainValueRepository.GetByCodeAndDomainCode(Constants.FCB_NEGATIVE_REPORT_CHECK_DOMAIN_VALUE, Constants.SETTINGS_DOMAIN).IsActive)
            {
                var client =  _clientService.GetOnlyClient(clientId);
                var request = new FcbReportRequest()
                {
                    Author = _sessionContext.UserName,
                    AuthorId = _sessionContext.UserId,
                    ClientId = client.Id,
                    Creditinfoid = 0,
                    DocumentType = 14,
                    IIN = client.IdentityNumber,
                    OrganizationId = 1,
                    ReportType = FCBReportTypeCode.IndividualNegative
                };

                var report = await GetReport(request);

                if (ValidateReportResponse(report))
                    throw new PawnshopApplicationException(report?.AvailableReportsErrorResponse?.Errmessage?.Value);

                if (report.XmlLink != null)
                    return await CheckOverdueClient(await _storage.Load(report.XmlLink, (ContainerName)Enum.Parse(typeof(ContainerName), report.FolderName)));
            }
            return false;
        }

        public bool ValidateReportResponse(FcbReportResponse fcbReportResponse)
        {
            if (fcbReportResponse != null &&
                fcbReportResponse.AvailableReportsErrorResponse != null && fcbReportResponse.AvailableReportsErrorResponse.Errmessage != null &&
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.Value != null &&
                (fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code == Constants.FCB_SERVICE_REPORT_NOT_FOUND ||
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code == Constants.FCB_SERVICE_SUBJECT_NOT_FOUND ||
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code == Constants.FCB_SERVICE_SUBJECT_NOT_FOUND_2))
                return false;

            if (fcbReportResponse == null || (
                fcbReportResponse != null && fcbReportResponse.AvailableReportsErrorResponse != null && fcbReportResponse.AvailableReportsErrorResponse.Errmessage != null &&
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.Value != null &&
                (fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code != Constants.FCB_SERVICE_REPORT_NOT_FOUND ||
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code != Constants.FCB_SERVICE_SUBJECT_NOT_FOUND ||
                fcbReportResponse.AvailableReportsErrorResponse.Errmessage.code != Constants.FCB_SERVICE_SUBJECT_NOT_FOUND_2)))
                return true;

            return false;
        }
    }
}
