using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.Calculation;
using System;
using System.Text.RegularExpressions;
using Pawnshop.Data.Models.Contracts;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Notifications
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ContractRepository _contractRepository;
        private readonly NotificationTemplateRepository _notificationTemplateRepository;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IMemoryCache _memoryCache;
        private readonly IContractService _contractService;

        public NotificationTemplateService(
            ContractRepository contractRepository, 
            NotificationTemplateRepository notificationTemplateRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            IMemoryCache memoryCache,
            IContractService contractService)
        {
            _notificationTemplateRepository = notificationTemplateRepository;
            _contractRepository = contractRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _memoryCache = memoryCache;
            _contractService = contractService;
        }

        public string GetNotificationTextByFilters(int contractId, MessageType messageType, NotificationPaymentType notificationPaymentType, decimal successPaymentCost = -1, decimal failPaymentCost = -1, decimal displayAmount = 0)
        {
            Contract contract = _contractService.GetOnlyContract(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            NotificationTemplate notificationTemplate = _notificationTemplateRepository.Select(messageType, notificationPaymentType);
            if (notificationTemplate == null)
                throw new PawnshopApplicationException($"Шаблон уведомления по фильтрам {nameof(messageType)} = {messageType}, {nameof(notificationPaymentType)} = {notificationPaymentType} не найдена");

            string text = notificationTemplate.Message;
            if (string.IsNullOrWhiteSpace(text))
                throw new PawnshopApplicationException($"Ожидалось что {nameof(notificationTemplate)}.{nameof(notificationTemplate.Message)} не будет пустый");

            decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id);
            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
                contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
                contract.PercentPaymentType == PercentPaymentType.Product)
            //Аннуитетный договор
            {
                if (contract.NextPaymentDate.HasValue)
                {
                    text = ReplacePart(text, "###MonthlyPaymentDate&", contract.NextPaymentDate.Value.ToString("dd.MM.yyyy"));
                    text = ReplacePart(text, "###ProlongDate&", contract.NextPaymentDate.Value.ToString("dd.MM.yyyy"));
                }
                else
                {
                    if (contract.ContractClass == ContractClass.CreditLine)
                    {
                        var trancheNextPaymentDate =
                            _contractService.GetNearestTranchePaymentDateOfCreditLine(contract.Id).Result;
                        if (trancheNextPaymentDate != null)
                        {
                            text = ReplacePart(text, "###MonthlyPaymentDate&", trancheNextPaymentDate.Value.ToString("dd.MM.yyyy"));
                            text = ReplacePart(text, "###ProlongDate&", trancheNextPaymentDate.Value.ToString("dd.MM.yyyy"));
                        }
                    }
                    else
                    {
                        text = ReplacePart(text, "###NextMonthlyPaymentDate&", "ДД.ММ.ГГГГ");
                        text = ReplacePart(text, "###MonthlyPaymentDate&", "ДД.ММ.ГГГГ");
                    }
                }

                text = ReplacePart(text, "###DisplayAmount&", Math.Round(successPaymentCost, 2).ToString());
                text = ReplacePart(text, "###PrepaymentBalance&", Math.Round(failPaymentCost, 2).ToString());
                text = ReplacePart(text, "###PrepaymentCost&", Math.Round(depoBalance, 2).ToString());
            }
            else
            //Дискретный договор
            {
                if (contract.NextPaymentDate.HasValue)
                    text = ReplacePart(text, "###ProlongDate&", contract.NextPaymentDate.Value.ToString("dd.MM.yyyy"));

                text = ReplacePart(text, "###PrepaymentCost&", Math.Round(depoBalance, 2).ToString());
                text = ReplacePart(text, "###DisplayAmount&", Math.Round(successPaymentCost, 2).ToString());
                text = ReplacePart(text, "###PrepaymentBalance&", Math.Ceiling(displayAmount).ToString());
                text = ReplacePart(text, "###NextMonthlyPaymentDate&", "ДД.ММ.ГГГГ");
                text = ReplacePart(text, "###MonthlyPaymentDate&", "ДД.ММ.ГГГГ");
            }

            return text;
        }

        private string ReplacePart(string text, string part, string replace)
        {
            var regex = new Regex(part);
            return regex.Replace(text, replace ?? string.Empty);
        }

        public NotificationTemplate GetTemplate(string code)
        {
            if (!_memoryCache.TryGetValue(code, out NotificationTemplate template))
            {
                template = _notificationTemplateRepository.GetByCode(code);
                _memoryCache.Set(code, template,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });
            }

            return template;
        }

        public async Task<NotificationTemplate> GetTemplateAsync(string code)
        {
            if (!_memoryCache.TryGetValue(code, out NotificationTemplate template))
            {
                template = await _notificationTemplateRepository.GetByCodeAsync(code);
                _memoryCache.Set(code, template,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });
            }

            return template;
        }

        public bool IsValidTemplate(NotificationTemplate template)
        {
            if(template != null 
                && template.Subject != null 
                && template.Message != null
                && template.IsActive)
            {
                return true;
            }

            return false;
        }
    }
}
