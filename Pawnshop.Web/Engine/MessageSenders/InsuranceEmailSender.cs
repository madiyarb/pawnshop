using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.MessageSenders
{
    public class InsuranceEmailSender
    {
        private readonly BranchContext _branchContext;
        private readonly EmailSender _emailSender;
        private readonly EnviromentAccessOptions _options;

        public InsuranceEmailSender(BranchContext branchContext, EmailSender emailSender, IOptions<EnviromentAccessOptions> options)
        {
            _branchContext = branchContext;
            _emailSender = emailSender;
            _options = options.Value;
        }

        public void InsuranceCloseSend(Insurance insurance, Contract contract)
        {
            var message = $@"
<p>
    <strong>
        АО «Нефтяная страховая компания»<br />
        Председателю Правления<br />
        г-ну Альжанову Ж.К.<br />
        от ТОО «{_branchContext.Configuration.LegalSettings.LegalName}»
    </strong>
</p>
<p style=""text-align: center;""><strong>Заявление</strong></p>
<p style=""text-align: justify;"">
    Настоящим письмом ТОО «{_branchContext.Configuration.LegalSettings.LegalName}» просит внести изменения в Договора добровольного 
    страхования автомобильного транспорта №{insurance.InsuranceNumber} от {insurance.BeginDate.ToString("dd.MM.yyyy")} г., а именно:
</p>
<p>пункт 7.4 Договора изложить в следующей редакции:</p>
<p style=""text-align: justify;"">
    При досрочном прекращении договора страхования по требованию Страхователя или Страховщика по условиям, предусмотренным пп.7.1.5 Договора или п.1 ст.841 Гражданского кодекса РК, Страховщик возвращает Страхователю часть страховой премии, рассчитанной по следующей формуле: СП/N*n, где:
</p>
<p>СП – оплаченная  страховая премия по Договору страхования</p>
<p>N – срок действия Договора страхования (в днях)</p>
<p>n – количество дней, оставшихся до окончания срока действия Договора страхования.</p>
<p style=""text-align: justify;"">
    Также настоящим письмом ТОО «{_branchContext.Configuration.LegalSettings.LegalName}» просит расторгнуть Договора добровольного страхования 
    автомобильного транспорта №{insurance.InsuranceNumber} от {insurance.BeginDate.ToString("dd.MM.yyyy")} г., 
    в связи с тем, что перестал существовать объект страхования. Часть страховой премии, подлежащей возврату, 
    просим перечислить на следующие реквизиты: {_branchContext.Configuration.BankSettings.BankName}, БИК {_branchContext.Configuration.BankSettings.BankBik},
    счет: {_branchContext.Configuration.BankSettings.BankAccount}, Кбе {_branchContext.Configuration.BankSettings.BankKbe}
</p>";

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.NskEmailAddress,
                ReceiverName = _options.NskEmailName,
                CopyAddresses = new List<MessageReceiver>
                {
                    new MessageReceiver
                    {
                        ReceiverAddress = _options.NskEmailCopyAddress,
                        ReceiverName = _options.NskEmailCopyName
                    },
                    new MessageReceiver()
                    {
                        ReceiverAddress = _options.InsuranseManagerAddress,
                        ReceiverName = _options.InsuranseManagerName
                    }
                }
            };

            _emailSender.SendEmail("Заявление", message, messageReceiver);


            var messageAboutContract = $@"
<p>
<p>Уведомление о досрочном прекращении страхового договора №{insurance.InsuranceNumber} от {insurance.BeginDate.ToString("dd.MM.yyyy")} г.</p>
<p>Договор №{contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} г. </p>
<p>Клиент {contract.ContractData.Client.FullName}, ИИН {contract.ContractData.Client.IdentityNumber}</p>
<p>Действие было совершено в филиале {_branchContext.Branch.DisplayName}.</p>
</p>";

            var messageAboutContractReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.InsuranseManagerAddress,
                ReceiverName = _options.InsuranseManagerName,
                CopyAddresses = new List<MessageReceiver>
                {
                    new MessageReceiver
                    {
                        ReceiverAddress = _options.NskEmailCopyAddress,
                        ReceiverName = _options.NskEmailCopyName
                    }
                }
            };
            _emailSender.SendEmail("Дополнение к заявлению", messageAboutContract, messageAboutContractReceiver);
        }

        public void ContractNumberChange(Insurance insurance, Contract contract, Contract clone)
        {
            var message = $@"
<p>
    <strong>
        АО «Нефтяная страховая компания»<br />
        Председателю Правления<br />
        г-ну Альжанову Ж.К.<br />
        от ТОО «{_branchContext.Configuration.LegalSettings.LegalName}»
    </strong>
</p>            
<p style=""text-align: center;""><strong>Заявление</strong></p>
<p style=""text-align: justify;"">
    Настоящим письмом ТОО «{_branchContext.Configuration.LegalSettings.LegalName}» уведомляет об изменении пункта 
    1.3 Договора страхования №{insurance.InsuranceNumber} от {insurance.BeginDate.ToString("dd.MM.yyyy")} 
    с договора залога №{contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} 
    на договор залога №{clone.ContractNumber} от {clone.ContractDate.ToString("dd.MM.yyyy")} г.
</p>";

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.NskEmailAddress,
                ReceiverName = _options.NskEmailName,
                CopyAddresses = new List<MessageReceiver>
                {
                    new MessageReceiver
                    {
                        ReceiverAddress = _options.NskEmailCopyAddress,
                        ReceiverName = _options.NskEmailCopyName
                    }
                }
            };

            _emailSender.SendEmail("Заявление", message, messageReceiver);
        }
    }
}
