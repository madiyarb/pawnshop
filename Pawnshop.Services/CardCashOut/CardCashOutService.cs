using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OuterServiceSettings;

namespace Pawnshop.Services.CardCashOut
{
    public sealed class CardCashOutService : ICardCashOutService
    {
        private HttpClient _httpClient;
        private readonly CardMerchantOptions _merchantOptions;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        private readonly ILogger<CardCashOutService> _logger;
        private readonly IMemoryCache _memoryCache;

        public CardCashOutService(IOptions<CardMerchantOptions> options, OuterServiceSettingRepository outerServiceSettingRepository,
            ILogger<CardCashOutService> logger, IMemoryCache memoryCache)
        {
            _merchantOptions = options.Value;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(handler);
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<StartCashOutTransaction.Envelope> StartCashOutTransaction(
            string cardNumber, string trancheAmount, string customerReference, int contractId, string baseReturnUrl, CancellationToken cancellationToken)
        {
            string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
            string returnUrlBase =
                _outerServiceSettingRepository.Find(new { Code = Constants.FINCORE_URL }).URL;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:startCashOutTransaction");
            
            var url = returnUrlBase.Trim('/') + "/" + $"contracts/{contractId}?referenceNr={customerReference}";
            string requestBody = CreateStartCashOutTransactionXml(userId: _merchantOptions.UserId,
                cardId: _merchantOptions.CardId,
                userLogin: _merchantOptions.UserLogin,
                merchantId: _merchantOptions.MerchantId, cardNumber: cardNumber,
                trancheAmount: trancheAmount, merchantKeyword: _merchantOptions.MerchantKeyword,
                returnUrl: url, customerReference: customerReference);
            var content = new StringContent(requestBody, Encoding.UTF8,
                "text/xml");
            try
            {
                var response = await _httpClient.PostAsync(endpointUrl,
                    content, cancellationToken: cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("xsi:type=\"ax23:StartTransactionResult\"", "");


                _logger.LogInformation($"Request to processing {requestBody} response {responseBody}");
                XmlSerializer serializer = new XmlSerializer(typeof(StartCashOutTransaction.Envelope));
                try 
                { 
                    using (StringReader reader = new StringReader(responseBody))
                    {
                        return (StartCashOutTransaction.Envelope)serializer.Deserialize(reader);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    throw new UnexpectedResponseException(new Response((int)response.StatusCode, responseBody), exceptionMessage: exception.Message, requestBody);
                }
                catch (NullReferenceException exception)
                {
                    throw new UnexpectedResponseException(new Response((int)response.StatusCode, responseBody), exceptionMessage: exception.Message, requestBody);
                }
            }
            catch (HttpRequestException exception)
            {
                throw new ProcessingServiceUnavailableException(endpointUrl, exception.Message);
            }
        }
        public async Task<bool> EnterCardNumber(string cardNumber, string tranGuid, string cardHolderName, CancellationToken cancellationToken)
        {
            string enterCardUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_CARD }).URL;
            _httpClient.DefaultRequestHeaders.Clear();
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pan", cardNumber),
                new KeyValuePair<string, string>("checkData", ""),
                new KeyValuePair<string, string>("cardHolder", cardHolderName),
                new KeyValuePair<string, string>("tranGUID", tranGuid),
                new KeyValuePair<string, string>("javascriptSupport", "true"),
                new KeyValuePair<string, string>("firstRequest", "false"),
                new KeyValuePair<string, string>("flagP2P", "1"),
                new KeyValuePair<string, string>("customMerchant", "SOL"),
                new KeyValuePair<string, string>("browserJavaEnabled", "false"),
                new KeyValuePair<string, string>("threeDsCompInd", "U"),
                new KeyValuePair<string, string>("threeDsVersion", "2.2.0"),
                new KeyValuePair<string, string>("threeDsServerTransID", "null")
            };
            FormUrlEncodedContent urlEncodedContent = new FormUrlEncodedContent(pairs);
            var response =
                await _httpClient.PostAsync(enterCardUrl, urlEncodedContent);
            return true;
        }

        public async Task<CompleteCashOutTransaction.Envelope> CompleteCashOutTransaction(string referenceNr, string amount, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start SendRequest. idempKey: {referenceNr}");
            //проверяю по кэшу отправляли ли мы такой запрос
            var alreadySent = _memoryCache.Get(referenceNr);
            if (alreadySent != null)
            {
                _logger.LogInformation( $"SendRequest double idempKey: {referenceNr}. Return");
                return null;
            }

            //добавляю в кэш ключ чтоб не отправлять такой же запрос еще раз
            _memoryCache.Set(referenceNr, 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

            string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:completeCashOutTransaction");

            var body = CreateCompleteCashOutTransactionXML(merchantId: _merchantOptions.MerchantId,
                referenceNr: referenceNr,
                merchantKeyword: _merchantOptions.MerchantKeyword, amount);

            var content = new StringContent(body,
                Encoding.UTF8,
                "text/plain");

            try 
            {
                var response = await _httpClient.PostAsync(
                    endpointUrl,
                    content, cancellationToken: cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                responseBody = responseBody.Replace("xsi:type=\"ax21:StoredTransferTransactionStatus\"", "");
                XmlSerializer serializer = new XmlSerializer(typeof(CompleteCashOutTransaction.Envelope));
                try 
                {
                    using (StringReader reader = new StringReader(responseBody))
                    {
                        return (CompleteCashOutTransaction.Envelope)serializer.Deserialize(reader);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    throw new UnexpectedResponseException(new Response((int)response.StatusCode, responseBody), exceptionMessage: exception.Message);
                }
            }
            catch (HttpRequestException exception)
            {
                throw new ProcessingServiceUnavailableException(endpointUrl, exception.Message);
            }
        }

        public async Task<GetCashOutTransactionStatus.Envelope> GetCashOutTransactionStatus(string referenceNr, CancellationToken cancellationToken)
        {
            string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.PROCESSING_KZ_ENDPOINT }).URL;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("SOAPAction", "urn:getCashOutTransactionStatus");

            try
            {
                var content = new StringContent(
                    CreateGetCashOutTransactionStatusXMl(merchantId: _merchantOptions.MerchantId,
                        referenceNr: referenceNr,
                        merchantKeyword: _merchantOptions.MerchantKeyword),
                    Encoding.UTF8,
                    "text/plain");

                var response = await _httpClient.PostAsync(
                    endpointUrl,
                    content, cancellationToken: CancellationToken.None);
                var responseBody = (await response.Content.ReadAsStringAsync())
                    .Replace("xsi:type=\"ax21:StoredTransferTransactionStatus\"", "");
                XmlSerializer serializer = new XmlSerializer(typeof(GetCashOutTransactionStatus.Envelope));
                try 
                {
                    using (StringReader reader = new StringReader(responseBody))
                    {
                        return (GetCashOutTransactionStatus.Envelope)serializer.Deserialize(reader);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    throw new UnexpectedResponseException(new Response((int)response.StatusCode, responseBody), exceptionMessage: exception.Message);
                }
            }
            catch (HttpRequestException exception)
            {
                throw new ProcessingServiceUnavailableException(endpointUrl, exception.Message);
            }
        }
        private string CreateStartCashOutTransactionXml(string userId, string cardId, string userLogin, string merchantId, 
            string cardNumber, string trancheAmount, string merchantKeyword, string returnUrl, string customerReference,
            string senderName = "FinCore")
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://kz.processing.cnp.merchant_ws/xsd"" xmlns:xsd1=""http://beans.common.cnp.processing.kz/xsd"">
           <soapenv:Header/>
           <soapenv:Body>
              <xsd:startCashOutTransaction>
                 <xsd:transaction>

                    <xsd1:additionalInformationList>
                       <xsd1:key>USER_ID</xsd1:key>
                       <xsd1:value>{userId}</xsd1:value>
                    </xsd1:additionalInformationList>
                    
                        <xsd1:additionalInformationList>
                       <xsd1:key>CARD_ID</xsd1:key>
                       <xsd1:value>{cardId}</xsd1:value>
                    </xsd1:additionalInformationList>

                      <xsd1:additionalInformationList>
                       <xsd1:key>USER_LOGIN</xsd1:key>
                       <xsd1:value>{userLogin}</xsd1:value>
                    </xsd1:additionalInformationList>

                    <xsd1:currencyCode>398</xsd1:currencyCode>
                    <xsd1:customerReference>{customerReference}</xsd1:customerReference>
                    <xsd1:description></xsd1:description>
                    <xsd1:languageCode>ru</xsd1:languageCode>
                    <xsd1:merchantId>{merchantId}</xsd1:merchantId>
                    <xsd1:merchantKeyword>{merchantKeyword}</xsd1:merchantKeyword>
                    <xsd1:merchantLocalDateTime>{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}</xsd1:merchantLocalDateTime>
                    <xsd1:receiverAccount>{cardNumber}</xsd1:receiverAccount>
                    <xsd1:returnURL>{returnUrl}</xsd1:returnURL>
                    <xsd1:senderName>{senderName}</xsd1:senderName>
                    <xsd1:terminalId>{_merchantOptions.TerminalId}</xsd1:terminalId>
                    <xsd1:tranAmount>{trancheAmount}</xsd1:tranAmount>
                    
                 </xsd:transaction>
              </xsd:startCashOutTransaction>
           </soapenv:Body>
        </soapenv:Envelope>";


        }
        private string CreateGetCashOutTransactionStatusXMl(string merchantId, string referenceNr, string merchantKeyword)
        {
            return @$"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                      <soap12:Body>
                        <getCashOutTransactionStatus xmlns=""http://kz.processing.cnp.merchant_ws/xsd"">
                          <merchantId>{merchantId}</merchantId>
                          <referenceNr>{referenceNr}</referenceNr>
                          <merchantKeyword>{merchantKeyword}</merchantKeyword>
                        </getCashOutTransactionStatus>
                      </soap12:Body>
                    </soap12:Envelope>";
        }
        private string CreateCompleteCashOutTransactionXML(string merchantId, string referenceNr, string merchantKeyword, string amount,
            bool transactionSuccess = true)
        {
            return @$"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
                      <soap12:Body>
                        <completeCashOutTransaction xmlns=""http://kz.processing.cnp.merchant_ws/xsd"">
                          <merchantId>{merchantId}</merchantId>
                          <referenceNr>{referenceNr}</referenceNr>
                          <transactionSuccess>{transactionSuccess}</transactionSuccess>
                          <merchantKeyword>{merchantKeyword}</merchantKeyword>
                          <overrideAmount></overrideAmount>
                        </completeCashOutTransaction>
                      </soap12:Body>
                    </soap12:Envelope>";
        }
    }

}
