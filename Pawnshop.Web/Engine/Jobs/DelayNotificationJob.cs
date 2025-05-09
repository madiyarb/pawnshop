using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Notifications.Interfaces;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Notifications;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Data.Models.Sms;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Engine.Jobs
{
    public class DelayNotificationJob
    {
        private readonly EnviromentAccessOptions _options;
        private readonly NotificationRepository _notificationRepository;
        private readonly JobLog _jobLog;
        private readonly ContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly BlackListReasonRepository _blackListReasonRepository;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly ISmsNotificationService _notificationService;
        public DelayNotificationJob(
            IOptions<EnviromentAccessOptions> options, 
            NotificationRepository notificationRepository, 
            JobLog jobLog,
            ContractRepository contractRepository, 
            IContractAmount contractAmount,
            IContractService contractService, 
            IClientBlackListService clientBlackListService, 
            BlackListReasonRepository blackListReasonRepository,
            INotificationTemplateService notificationTemplateService,
            ISmsNotificationService notificationService)
        {
            _options = options.Value;
            _notificationRepository = notificationRepository;
            _jobLog = jobLog;
            _contractRepository = contractRepository;
            _contractService = contractService;
            _clientBlackListService = clientBlackListService;
            _blackListReasonRepository = blackListReasonRepository;
            _notificationTemplateService = notificationTemplateService;
            _notificationService = notificationService;
        }

        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            try
            {
                _jobLog.Log("DelayNotificationJob", JobCode.Begin, JobStatus.Success, EntityType.Notification);
                if (!_options.DelayNotification)
                {
                    _jobLog.Log("DelayNotificationJob", JobCode.End, JobStatus.Success);
                    return;
                }
                
                SendEarlyDelayNotifications();
                SendCarLateDelayNotifications();
                SendRealtyLateDelayNotifications();

                _jobLog.Log("DelayNotificationJob", JobCode.End, JobStatus.Success, EntityType.None);
            }
            catch (Exception ex)
            {
                _jobLog.Log("DelayNotificationJob", JobCode.Error, JobStatus.Failed, EntityType.None, responseData: JsonConvert.SerializeObject(ex).ToString());
            }
        }

        private void SendEarlyDelayNotifications()
        {
            string responseData = $"Создано {0} количество смс уведомлений методом SendEarlyDelayNotifications";
            var delayContracts = _contractRepository.ListForDelayNotification(DateTime.Now.Date, Constants.DAYS_FOR_DELAY_NOTIFICATION);
            var templateKaz = _notificationTemplateService.GetTemplate(Constants.DELAY_EARLY);
            var templateRus = _notificationTemplateService.GetTemplate(Constants.DELAY_EARLY_RUS);
            int smsCounter = 0;
            if(!_notificationTemplateService.IsValidTemplate(templateKaz) || !_notificationTemplateService.IsValidTemplate(templateRus))
            {
                return;
            }

            foreach (var item in delayContracts)
            {
                var contractBalance = _contractRepository.GetBalances(new List<int>() { item.ContractId.GetValueOrDefault() }).FirstOrDefault();
                var totalAmount = contractBalance.OverdueAccountAmount + contractBalance.OverdueProfitAmount + contractBalance.PenyAmount;

                var messageKaz = string.Format(templateKaz.Message, DateTime.Now.Date.ToString("dd.MM.yyyy"), totalAmount, contractBalance.OverdueAccountAmount, contractBalance.OverdueProfitAmount, contractBalance.PenyAmount);
                var newSmsKaz = new SmsCreateNotificationModel(item.ClientId, item.BranchId, templateKaz.Subject, messageKaz, item.ContractId);
                _notificationService.CreateSmsNotification(newSmsKaz);
                
                var messageRus = string.Format(templateRus.Message, DateTime.Now.Date.ToString("dd.MM.yyyy"), totalAmount, contractBalance.OverdueAccountAmount, contractBalance.OverdueProfitAmount, contractBalance.PenyAmount);
                var newSmsRus = new SmsCreateNotificationModel(item.ClientId, item.BranchId, templateRus.Subject, messageRus, item.ContractId);
                _notificationService.CreateSmsNotification(newSmsRus);
                smsCounter++;
            }
            _jobLog.Log("DelayNotificationJob", JobCode.Processing, JobStatus.Success, EntityType.None, responseData: string.Format(responseData, smsCounter));
            
            return;
        }

        private void SendCarLateDelayNotifications()
        {
            var responseData = $"Создано {0} количество смс уведомлений методом SendCarLateDelayNotifications";
            var delayCarContracts = _contractRepository.ListForDelayNotificationPosition(DateTime.Now.Date, Constants.DAYS_FOR_SECOND_DELAY_NOTIFICATION_CAR, CollateralType.Car);
            var template = _notificationTemplateService.GetTemplate(Constants.DELAY_LATE);
            int smsCounter = 0;
            if (!_notificationTemplateService.IsValidTemplate(template))
            {
                return;
            }

            foreach (var item in delayCarContracts)
            {
                var newSms = new SmsCreateNotificationModel(item.ClientId, item.BranchId, template.Subject, template.Message);
                _notificationService.CreateSmsNotification(newSms);
                smsCounter++;
            }
            _jobLog.Log("DelayNotificationJob", JobCode.Processing, JobStatus.Success, EntityType.None, responseData: string.Format(responseData, smsCounter));

            return;
        }

        private void SendRealtyLateDelayNotifications()
        {
            var responseData = $"Создано {0} количество смс уведомлений методом SendRealtyLateDelayNotifications";
            var delayRealtyContracts = _contractRepository.ListForDelayNotificationPosition(DateTime.Now.Date, Constants.DAYS_FOR_SECOND_DELAY_NOTIFICATION_REALTY, CollateralType.Realty);
            var template = _notificationTemplateService.GetTemplate(Constants.DELAY_LATE);
            int smsCounter = 0;
            if (!_notificationTemplateService.IsValidTemplate(template))
            {
                return;
            }

            foreach (var item in delayRealtyContracts)
            {
                var newSms = new SmsCreateNotificationModel(item.ClientId, item.BranchId, template.Subject, template.Message, item.ContractId);
                _notificationService.CreateSmsNotification(newSms);
                smsCounter++;
            }
            _jobLog.Log("SendRealtyLateDelayNotifications", JobCode.Processing, JobStatus.Success, EntityType.None, responseData: string.Format(responseData, smsCounter));

            return;
        }
    }
}
