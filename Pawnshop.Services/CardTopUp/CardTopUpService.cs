using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OuterServiceSettings;

namespace Pawnshop.Services.CardTopUp
{
    public sealed class CardTopUpService : ICardTopUpService
    {
        private HttpClient _httpClient;
        CardTopUpOptions _merchantOptions;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        public CardTopUpService(IOptions<CardTopUpOptions> options, IRepository<OuterServiceSetting> outerServiceSettingRepository)
        {
            _merchantOptions = options.Value;
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(handler);

            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        public async Task<StartTransaction.Envelope> StartTransaction(string referenceNr, string amount, int orderId,  CancellationToken cancellationToken)
        {
            try
            {
                string EndpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:startTransaction");
                var content = new StringContent(
                    CreateStartTransactionXMl(merchantId: _merchantOptions.MerchantId, referenceNr: referenceNr, amount, orderId: orderId.ToString()),
                    Encoding.UTF8,
                    "text/plain");
                var response = await _httpClient.PostAsync(
                    EndpointUrl,
                    content, cancellationToken: cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("xsi:type=\"ax23:StartTransactionResult\"", "");
                XmlSerializer serializer = new XmlSerializer(typeof(StartTransaction.Envelope));
                using (StringReader reader = new StringReader(responseBody))
                {
                    return (StartTransaction.Envelope)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<GetTransactionStatusCode.Envelope?> GetTransactionStatusCode(string referenceNr, CancellationToken cancellationToken)
        {
            try
            {
                string EndpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:getTransactionStatusCode");
                var content = new StringContent(
                    CreateGetTransactionStatusCodeXML(merchantId: _merchantOptions.MerchantId, referenceNr: referenceNr),
                    Encoding.UTF8,
                    "text/plain");
                var response = await _httpClient.PostAsync(
                    EndpointUrl,
                    content, cancellationToken: cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("xsi:type=\"ax21:StoredTransactionStatusCode\"", "");
                XmlSerializer serializer = new XmlSerializer(typeof(GetTransactionStatusCode.Envelope));
                using (StringReader reader = new StringReader(responseBody))
                {
                    return (GetTransactionStatusCode.Envelope)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }

        public async Task<CompleteTransaction.Envelope> CompleteTransaction(string referenceNr,
            CancellationToken cancellationToken)
        {
            try
            {
                string EndpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:completeTransaction");
                var content = new StringContent(
                    CreateCompleteTransactionXML(merchantId: _merchantOptions.MerchantId, referenceNr: referenceNr),
                    Encoding.UTF8,
                    "text/plain");
                var response = await _httpClient.PostAsync(
                    EndpointUrl,
                    content, cancellationToken: cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("xmlns=\"http://kz.processing.cnp.merchant_ws/xsd\"", "");
                XmlSerializer serializer = new XmlSerializer(typeof(CompleteTransaction.Envelope));
                using (StringReader reader = new StringReader(responseBody))
                {
                    return (CompleteTransaction.Envelope)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string CreateStartTransactionXMl(string merchantId, string referenceNr, string amount,
        string nameOfGoods = "Оплата по кредиту", string paymentType = "0", string orderId = "15")
        {
            string XML = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                            <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                              <soap12:Body>
                                <startTransaction xmlns=""http://kz.processing.cnp.merchant_ws/xsd"">
                                  <transaction>
                                    <currencyCode>398</currencyCode>
                                    <customerReference>{referenceNr}</customerReference>
                                    <goodsList>
                                      <amount>{amount}</amount>
                                      <currencyCode>398</currencyCode>
                                      <nameOfGoods>{nameOfGoods}</nameOfGoods>
                                      <expirationDate>{DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddZ")}</expirationDate>
                                    </goodsList>
                                    <languageCode>ru</languageCode>
                                    <merchantId>{merchantId}</merchantId>
                                    <merchantLocalDateTime>{DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss")}</merchantLocalDateTime>
                                    <orderId></orderId>
                                    <paymentType>{paymentType}</paymentType>
                                    <returnURL>www.google.com</returnURL>
                                    <terminalID>{_merchantOptions.TerminalId}</terminalID>
                                    <totalAmount>{amount}</totalAmount>
                                  </transaction>
                                </startTransaction>
                              </soap12:Body>
                            </soap12:Envelope>
                             ";// todo check this :  Payment type?, order id name of goods, merchantId?
            return XML;
        }

        private string CreateGetTransactionStatusCodeXML(string merchantId, string referenceNr)
        {
            string XML = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                          <soap12:Body>
                            <getTransactionStatusCode xmlns=""http://kz.processing.cnp.merchant_ws/xsd"">
                              <merchantId>{merchantId}</merchantId>
                              <referenceNr>{referenceNr}</referenceNr>
                            </getTransactionStatusCode>
                          </soap12:Body>
                        </soap12:Envelope>";

            return XML;
        }

        private string CreateCompleteTransactionXML(string merchantId, string referenceNr)
        {
            string XMl = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                  <soap12:Body>
                    <completeTransaction xmlns=""http://kz.processing.cnp.merchant_ws/xsd"">
                      <merchantId>{merchantId}</merchantId>
                      <referenceNr>{referenceNr}</referenceNr>
                      <transactionSuccess>true</transactionSuccess>
                    </completeTransaction>
                  </soap12:Body>
                </soap12:Envelope>";
            return XMl;
        }
    }
}
