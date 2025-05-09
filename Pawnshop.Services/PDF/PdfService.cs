using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Serilog;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.PDF
{
    public class PdfService : IPdfService
    {
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly ILogger _logger;
        private readonly EnviromentAccessOptions _options;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;

        public PdfService(
            ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            ILogger logger,
            IOptions<EnviromentAccessOptions> options,
            OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _logger = logger;
            _options = options.Value;
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }


        public async Task<byte[]> GetFile(int contractId, int? creditLineId, ApplicationOnlineSignType signType, CancellationToken cancellationToken)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    var pdfApiUrl = _outerServiceSettingRepository.Find(new { Code = Constants.ABS_ONLINE_PDF_INTEGRATION_SETTINGS_CODE }).URL;

                    var query = HttpUtility.ParseQueryString(string.Empty);
                    if (creditLineId.HasValue)
                        query.Add("contractId", contractId.ToString());

                    if (creditLineId.HasValue)
                        query.Add("creditLineId", creditLineId.ToString());//JS регистрозависимый не меняйте на CreditLineId 

                    var additionalInfo = _contractAdditionalInfoRepository.Get(contractId);

                    if (signType == ApplicationOnlineSignType.SMS && additionalInfo != null && !string.IsNullOrEmpty(additionalInfo.SmsCode))
                        query.Add("code", additionalInfo.SmsCode);
                    else if (signType == ApplicationOnlineSignType.NPCK && additionalInfo != null && additionalInfo.LoanStorageFileId.HasValue)
                        query.Add("fileGuid", additionalInfo.LoanStorageFileId.ToString());

                    var request = await http.GetAsync($"{pdfApiUrl}/sendPdf?{query}", cancellationToken);

                    var content = await request.Content.ReadAsStringAsync();

                    var response = JsonConvert.DeserializeObject<PdfResponse>(content);

                    if (response == null || string.IsNullOrEmpty(response.binary_data))
                        return null;

                    return Convert.FromBase64String(response.binary_data);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }
    }

    internal class PdfResponse
    {
        public string binary_data { get; set; }
        public string contract_id { get; set; }
    }
}
