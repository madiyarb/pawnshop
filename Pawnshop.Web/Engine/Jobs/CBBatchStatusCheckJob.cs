using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using Pawnshop.Core.Queries;
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
using Pawnshop.Web.Models.CreditBureau.StatusCheckResult;
using System.Text.RegularExpressions;
using Pawnshop.Web.Models.CreditBureau.UploadResultSCB;
using Pawnshop.Services.CBBatches;
using Pawnshop.Services.Exceptions;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBBatchStatusCheckJob
    {
        private readonly CBBatchRepository _cbBatchRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly EventLog _eventLog;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;
        private readonly CBBatchesService _cbBatchesService;

        public CBBatchStatusCheckJob(
            IOptions<EnviromentAccessOptions> options,
            EventLog eventLog,
            OuterServiceSettingRepository outerServiceSettingRepository,
            CBBatchRepository cbBatchRepository,
            JobLog jobLog,
            CBBatchesService cbBatchesService
        )
        {
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _eventLog = eventLog;
            _options = options.Value;
            _cbBatchRepository = cbBatchRepository;
            _jobLog = jobLog;
            _cbBatchesService = cbBatchesService;
        }

        [Queue("cb")]
        public void Execute()
        {
            if (!_options.CBUpload) return;

            try
            {
                _jobLog.Log("CBBatchStatusCheckJob", JobCode.Begin, JobStatus.Success);

                var batches = new List<CBBatch>();
                var statusesToCheck = new List<CBBatchStatus>() {
                    CBBatchStatus.WaitingForImport,
                    CBBatchStatus.ImportMirror,
                    CBBatchStatus.WaitingForCheck,
                    CBBatchStatus.ImportLive,

                    CBBatchStatus.SentSCB,
                    CBBatchStatus.PreparedForProcessing,
                    CBBatchStatus.StartProcessing,
                    CBBatchStatus.Processing
                };

                statusesToCheck.ForEach(status =>
                {
                    batches.AddRange(_cbBatchRepository.List(new ListQuery() { Page = null }, new { Status = status }));
                });

                foreach (var batch in batches)
                {
                    if (batch.CBId == CBType.FCB)
                        CheckStatusFCB(batch);
                    else if (batch.CBId == CBType.SCB)
                        CheckStatusSCB(batch);
                }

                _jobLog.Log("CBBatchStatusCheckJob", JobCode.End, JobStatus.Success);
            }
            catch (Exception e)
            {
                _jobLog.Log("CBBatchStatusCheckJob", JobCode.Error, JobStatus.Failed, responseData: e.Message);
            }
        }

        private void CheckStatusFCB(CBBatch batch)
        {
            try
            {
                var config = _outerServiceSettingRepository.Find(new { Code = Constants.FIRST_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE });

                if (config == null || string.IsNullOrEmpty(config.Login) || string.IsNullOrEmpty(config.Password) || string.IsNullOrEmpty(config.URL)) throw new CBBatchesException($"Ошибка настроек интеграции");

                _cbBatchesService.CheckConnectionToFCB(config);


                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                };

                using (var httpClient = new HttpClient(clientHandler))
                {
                    var response = httpClient.PostAsync(
                            config.URL,
                            new StringContent(_cbBatchesService.GetBatchStatusRequestFCB(config, batch.BatchId.Value), Encoding.UTF8, "text/xml")
                        ).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;

                        _eventLog.Log(EventCode.CBStatusCheck, EventStatus.Success, EntityType.CBBatch, batch.Id, responseData: result);

                        StatusCheckResult resultModel = new StatusCheckResult();
                        XmlSerializer serializer = new XmlSerializer(typeof(StatusCheckResult));
                        using (TextReader reader = new StringReader(result))
                        {
                            resultModel = (StatusCheckResult)serializer.Deserialize(reader);
                        }

                        batch.BatchStatusId = (CBBatchStatus)resultModel.Body.GetBatchStatus3Response.GetBatchStatus3Result.CigResult.Result
                            .Batch.StatusId;
                        batch.BatchStatusInfo = resultModel.Body.GetBatchStatus3Response.GetBatchStatus3Result.CigResult.Result
                            .Batch.StatusName;
                    }
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBStatusCheck, EventStatus.Failed, EntityType.CBBatch, batch.Id, responseData: e.Message);
            }
            finally
            {
                _cbBatchRepository.Update(batch);
            }
        }
        private void CheckStatusSCB(CBBatch batch)
        {
            try
            {
                var config = _outerServiceSettingRepository.Find(new { Code = Constants.STATE_CREDIT_BUREAU_INTEGRATION_SETTINGS_CODE });

                if (config == null || string.IsNullOrEmpty(config.Login) || string.IsNullOrEmpty(config.Password) || string.IsNullOrEmpty(config.URL)) throw new CBBatchesException($"Ошибка настроек интеграции");

                _cbBatchesService.CheckConnectionToSCB(config);


                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                };

                using (var httpClient = new HttpClient(clientHandler))
                {
                    byte[] TextBytes = Encoding.UTF8.GetBytes(config.Login);
                    string auth = Convert.ToBase64String(TextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
                    var response = httpClient.PostAsync(config.URL, new StringContent(_cbBatchesService.GetBatchStatusRequestSCB(config.Password, batch.BatchId.Value), Encoding.UTF8, "text/xml")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        result = Regex.Match(result, @"<filesImportInfo>(.*?)<\/filesImportInfo>", RegexOptions.Singleline).ToString();



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
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.CBStatusCheck, EventStatus.Failed, EntityType.CBBatch, batch.Id, responseData: e.Message);
            }
            finally
            {
                _cbBatchRepository.Update(batch);
            }
        }
    }
}
