using System;
using Pawnshop.Data.Access;
using Pawnshop.Core.Queries;
using Pawnshop.Core.Exceptions;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using System.IO;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Net.Http;
using System.Xml.Serialization;
using Hangfire;
using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.CreditBureau.UploadResultFCB;
using Pawnshop.Web.Models.CreditBureau.UploadResultSCB;
using System.Text.RegularExpressions;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Services.CBBatches;
using Pawnshop.Services.Exceptions;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBBatchUploadJob
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly EventLog _eventLog;
        private readonly EnviromentAccessOptions _options;
        private readonly IStorage _storage;
        private readonly JobLog _jobLog;
        private readonly EmailSender _emailSender;
        private readonly CBBatchesService _cbBatchesService;

        public CBBatchUploadJob(
            IOptions<EnviromentAccessOptions> options,
            EventLog eventLog,
            OuterServiceSettingRepository outerServiceSettingRepository,
            CBBatchRepository cbBatchRepository,
            IStorage storage,
            JobLog jobLog,
            EmailSender emailSender,
            CBBatchesService cbBatchesService
        )
        {
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _eventLog = eventLog;
            _options = options.Value;
            _cbBatchRepository = cbBatchRepository;
            _storage = storage;
            _jobLog = jobLog;
            _emailSender = emailSender;
            _cbBatchesService = cbBatchesService;
        }

        [Queue("cb")]
        public void Execute()
        {
            if (!_options.CBUpload) return;

            try
            {
                _jobLog.Log("CBBatchUploadJob", JobCode.Begin, JobStatus.Success);

                var batches =
                    _cbBatchRepository.List(new ListQuery() { Page = null }, new { Status = CBBatchStatus.XMLCreated });

                foreach (var batch in batches)
                {
                    if (batch.CBId == CBType.FCB)
                        UploadBatchFCB(batch);
                    else if (batch.CBId == CBType.SCB)
                        UploadBatchSCB(batch);
                }

                _jobLog.Log("CBBatchUploadJob", JobCode.End, JobStatus.Success);
            }
            catch (Exception e)
            {
                _jobLog.Log("CBBatchUploadJob", JobCode.Error, JobStatus.Failed, responseData: e.Message);
            }
        }

        private void UploadBatchFCB(CBBatch batch)
        {
            try
            {
                var config = _outerServiceSettingRepository.Find(new { Code = Constants.FIRST_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE });

                if (config != null)
                    _cbBatchesService.CheckConnectionToFCB(config);
                else
                    throw new CBBatchesException("Не найдены настройки подключения к удаленному серверу");

                var fileStream = _storage.Load(batch.FileName, ContainerName.CBBatches).Result;
                byte[] bytes = ReadAsBytes(fileStream);
                string file = Convert.ToBase64String(bytes);

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (var httpClient = new HttpClient(clientHandler))
                {
                    var response = httpClient.PostAsync(config.URL, new StringContent(UploadBatchToFCB(config, batch.SchemaId, file), Encoding.UTF8, "text/xml")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;

                        _eventLog.Log(EventCode.CBBatchUpload, EventStatus.Success, EntityType.CBBatch, batch.Id, responseData: result);

                        UploadResultFCB resultModel = new UploadResultFCB();
                        XmlSerializer serializer = new XmlSerializer(typeof(UploadResultFCB));
                        using (TextReader reader = new StringReader(result))
                        {
                            resultModel = (UploadResultFCB)serializer.Deserialize(reader);
                        }

                        batch.BatchId = resultModel.Body.UploadZippedData2Response.UploadZippedData2Result.CigResult
                            .Result.Batch.Id;
                        batch.BatchStatusId = (CBBatchStatus)resultModel.Body.UploadZippedData2Response
                            .UploadZippedData2Result.CigResult
                            .Result.Batch.StatusId;
                        batch.BatchStatusInfo = resultModel.Body.UploadZippedData2Response
                            .UploadZippedData2Result.CigResult
                            .Result.Batch.StatusName;
                    }
                    else
                    {
                        throw new CBBatchesException($"\nКБ: ПКБ\nId: {batch.Id}\nFileName: {batch.FileName}\nВремя: {DateTime.Now}\nКод ошибки: {response.StatusCode}, {response.ReasonPhrase}\n");
                    }
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBBatchUpload, EventStatus.Failed, EntityType.CBBatch, batch.Id, responseData: e.Message);
                ErrorNotification(e.Message);
            }
            finally
            {
                _cbBatchRepository.Update(batch);
            }
        }

        private void UploadBatchSCB(CBBatch batch)
        {
            try
            {
                var config = _outerServiceSettingRepository.Find(new { Code = Constants.STATE_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE });

                if (config != null)
                    _cbBatchesService.CheckConnectionToSCB(config);
                else
                    throw new CBBatchesException("Не найдены настройки подключения к удаленному серверу");

                var fileStream = _storage.Load(batch.FileName, ContainerName.CBBatches).Result;
                byte[] bytes = ReadAsBytes(fileStream);
                string file = Convert.ToBase64String(bytes);

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (var httpClient = new HttpClient(clientHandler))
                {
                    // в столбце Login хранится ClientId и Password в формате "ClientId:Password"
                    // в столбце Password хранится UserId
                    byte[] TextBytes = Encoding.UTF8.GetBytes(config.Login);
                    string auth = Convert.ToBase64String(TextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
                    var response = httpClient.PostAsync(
                            config.URL,
                            new StringContent(UploadBatchToSCB(config, batch.FileName, file, batch.SchemaId), Encoding.UTF8, "text/xml")
                        ).Result;


                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        {
                            Match match = Regex.Match(result, @"<batchPackageId>(.*?)<\/batchPackageId>", RegexOptions.Singleline);

                            response = null;
                            result = null;

                            var request = new StringContent(_cbBatchesService.GetBatchStatusRequestSCB(config.Password, Convert.ToInt32(match.Groups[1].Value)), Encoding.UTF8, "text/xml");

                            response = httpClient.PostAsync(config.URL, request).Result;
                            result = response.Content.ReadAsStringAsync().Result;

                            result = Regex.Match(result, @"<filesImportInfo>(.*?)<\/filesImportInfo>", RegexOptions.Singleline).ToString();
                        }

                        {
                            UploadResultSCB resultModel = new UploadResultSCB();
                            XmlSerializer serializer = new XmlSerializer(typeof(UploadResultSCB));
                            using (TextReader reader = new StringReader(result))
                            {
                                resultModel = (UploadResultSCB)serializer.Deserialize(reader);
                            }

                            batch.BatchId = resultModel.BatchFileDtoList.BatchPackage.PackageId;
                            batch.BatchStatusId = _cbBatchesService.SetBatchStatusSCB(resultModel.BatchFileDtoList.BatchUploadStatus);

                            batch.BatchStatusInfo = batch.BatchStatusId == CBBatchStatus.Processed || batch.BatchStatusId == CBBatchStatus.ErrorSCB ?
                                resultModel.BatchFileDtoList.BatchUploadStatus + " Количество ошибок: " + resultModel.BatchFileDtoList.NumberOfErrors :
                                resultModel.BatchFileDtoList.BatchUploadStatus;

                            _eventLog.Log(EventCode.CBBatchUpload, EventStatus.Success, EntityType.CBBatch, batch.Id, responseData: result);
                        }
                    }
                    else
                    {
                        throw new CBBatchesException($"\nКБ: ГКБ\nId: {batch.Id}\nFileName: {batch.FileName}\nВремя: {DateTime.Now}\nКод ошибки: {response.StatusCode}, {response.ReasonPhrase}\n");
                    }
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBBatchUpload, EventStatus.Failed, EntityType.CBBatch, batch.Id, responseData: e.Message);
                ErrorNotification(e.Message);
            }
            finally
            {
                _cbBatchRepository.Update(batch);
            }
        }


        private void ErrorNotification(string details)
        {
            string messageTitle = "Ошибка при отправке батча";
            var message = $@"
            <p style=""text-align: center;"">
            <strong>
                {messageTitle}
            </strong></p>
            <p>
                {details}
            <p>";

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail(messageTitle, message, messageReceiver);

        }

        private string UploadBatchToFCB(OuterServiceSetting config, int batchSchemaId, string file)
        {
            StringBuilder xml = new StringBuilder();

            xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"https://ws.creditinfo.com\">");
            xml.Append("<soapenv:Header>");
            xml.Append("<ws:CigWsHeader>");
            xml.Append($"<ws:UserName>{config.Login}</ws:UserName>");
            xml.Append($"<ws:Password>{config.Password}</ws:Password>");
            xml.Append("</ws:CigWsHeader>");
            xml.Append("</soapenv:Header>");
            xml.Append("<soapenv:Body>");
            xml.Append("<ws:UploadZippedData2>");
            xml.Append($"<ws:zippedXML>{file}</ws:zippedXML>");
            xml.Append($"<ws:schemaId>{batchSchemaId}</ws:schemaId>");
            xml.Append("</ws:UploadZippedData2>");
            xml.Append("</soapenv:Body>");
            xml.Append("</soapenv:Envelope>");

            return xml.ToString();
        }

        private string UploadBatchToSCB(OuterServiceSetting config, string batchFileName, string file, int schemaId)
        {
            StringBuilder xml = new StringBuilder();

            xml.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:data=\"http://data.chdb.scb.kz\">");
            xml.Append("<soapenv:Header>");
            // userId хранится в Password
            xml.Append($"<userId>{config.Password}</userId>");
            xml.Append("</soapenv:Header>");
            xml.Append("<soapenv:Body>");
            xml.Append("<data:uploadFile>");
            xml.Append($"<file>{file}</file>");
            xml.Append($"<fileName>{batchFileName}</fileName>");

            // 1 - еженедельный батч
            // 2 - если пакет, содержит банковскую гарантию и поручительство
            // 3 - оперативная загрузка / ежедневный батч
            // это и есть схема
            xml.Append($"<fileType>{schemaId}</fileType>");
            xml.Append("</data:uploadFile>");
            xml.Append("</soapenv:Body>");
            xml.Append("</soapenv:Envelope>");

            return xml.ToString();
        }


        private static byte[] ReadAsBytes(Stream input)
        {
            var buffer = new byte[16 * 1024];

            using var ms = new MemoryStream();

            int read;

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer, 0, read);

            return ms.ToArray();
        }
    }
}
