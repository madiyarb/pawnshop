using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using System.Linq;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Crm;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Web.Models.Membership;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CrmUploadContactJob
    {
        private readonly ClientRepository _clientRepository;
        private readonly CrmSyncContactRepository _crmSyncContactRepository;
        private readonly CrmSyncContactQueueRepository _crmSyncContactQueueRepository;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;
        private readonly ICrmUploadService _crmUploadService;

        public CrmUploadContactJob(
            ClientRepository clientRepository, 
            CrmSyncContactRepository crmSyncContactRepository, 
            CrmSyncContactQueueRepository crmSyncContactQueueRepository,
            JobLog jobLog, 
            IOptions<EnviromentAccessOptions> options, 
            ICrmUploadService crmUploadService)
        {
            _clientRepository = clientRepository;
            _crmSyncContactRepository = crmSyncContactRepository;
            _crmSyncContactQueueRepository = crmSyncContactQueueRepository;
            _jobLog = jobLog;
            _options = options.Value;
            _crmUploadService = crmUploadService;
        }

        public async Task Execute()
        {
            if (!_options.CrmUpload) return;
            try
            {
                _jobLog.Log("CrmUploadContactJob", JobCode.Start, JobStatus.Success, EntityType.BitrixUploadContact);

                List<CrmSyncContact> crmContactsToUpload = _crmSyncContactQueueRepository.GenerateQueue();

                foreach (var crmContact in crmContactsToUpload)
                {
                    var client = _clientRepository.Get(crmContact.ClientId);
                    await CrmUpload(client, crmContact);
                }

                _jobLog.Log("CrmUploadContactJob", JobCode.End, JobStatus.Success, EntityType.BitrixUploadContact);
            }
            catch (Exception e)
            {
                _jobLog.Log("CrmUploadContactJob", JobCode.Error, JobStatus.Failed, EntityType.BitrixUploadContact, requestData: e.Message);
            }
        }

        private async Task CrmUpload(Client client, CrmSyncContact crmSyncContact)
        {            
            var contactId = await _crmUploadService.CreateOrUpdateContactInCrm(client);
            client.CrmId = contactId;
            _clientRepository.Update(client);
               
            crmSyncContact.UploadDate = DateTime.Now;
            _crmSyncContactQueueRepository.Update(crmSyncContact);
        }
    }
}
