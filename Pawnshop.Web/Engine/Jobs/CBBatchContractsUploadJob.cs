using Pawnshop.Data.Access;
using System.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Services;
using Pawnshop.Data.Models.Audit;
using Newtonsoft.Json;
using Pawnshop.Web.Engine.Audit;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CBBatchContractsUploadJob
    {
        private readonly HttpClient _http;
        private readonly CBBatchContractsUploadRepository _cbBatchContractsUploadRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly JobLog _jobLog;
        public CBBatchContractsUploadJob(
            HttpClient http,
            CBBatchContractsUploadRepository cbBatchContractsUploadRepository,
            OuterServiceSettingRepository outerServiceSettingRepository,
            JobLog jobLog
            )
        {
            _http = http;
            _cbBatchContractsUploadRepository = cbBatchContractsUploadRepository;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _jobLog = jobLog;
        }

        public void Execute()
        {
            try
            {
                var batchSize = 5;
                var totalList = _cbBatchContractsUploadRepository.List();
                var totalCount = totalList.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / batchSize);
                string title = "Ошибки при отправке батчей в ПКБ/ГКБ\n\n";
                var serviceSetting = _outerServiceSettingRepository.Find(new { Code = Constants.TELEGRAM_BATCH_LOGS_SETTINGS_CODE });

                _jobLog.Log("CBBatchContractsUploadJob", JobCode.Start, JobStatus.Success, EntityType.CBBatchContract);
                for (int page = 0; page < totalPages; page++)
                {
                    var list = totalList
                        .Skip(page * batchSize)
                        .Take(batchSize)
                        .Select(contract => contract)
                        .ToList();
                    string message = "";
                    foreach (var item in list)
                    {
                        message += @$"
*№{list.IndexOf(item) + 1}*📃
*ContractId - {item.ContractId}*
ContractClass - {item.ContractClass}
*BranchName - {item.BranchName}*
ClientId - {item.ClientId}
*IIN - {item.IIN}*
CreateDate - {item.CreateDate}
*BatchId - {item.BatchId}*
Причина ошибки: ```sql {item.ResponseData}```

";
                    }

                    var parameters = new
                    {
                        chat_id = serviceSetting.Login,
                        text = title + message,
                        parse_mode = "Markdown"
                    };

                    string json = JsonConvert.SerializeObject(parameters);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = _http.PostAsync(serviceSetting.URL, content).Result;
                    response.EnsureSuccessStatusCode();

                    _cbBatchContractsUploadRepository.SetUploadedStatus(list.Select(item => item.EventLogId).ToList());
                }

                _jobLog.Log("CBBatchContractsUploadJob", JobCode.End, JobStatus.Success, EntityType.CBBatchContract);
            }
            catch (Exception ex)
            {
                _jobLog.Log("CBBatchContractsUploadJob", JobCode.Error, JobStatus.Failed, EntityType.CBBatchContract, null, JsonConvert.SerializeObject(ex));
            }
        }
    }
}
