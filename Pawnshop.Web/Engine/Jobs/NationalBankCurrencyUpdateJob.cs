using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Models.NationalBankExchangeRates;
using System.Net.Http;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Core.Queries;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs
{
    public class NationalBankCurrencyUpdateJob
    {
        private static readonly string Url = "https://nationalbank.kz/rss/get_rates.cfm";
        private RatesXmlParser parser = new RatesXmlParser();
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly CurrencyRepository _currencyRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly EmailSender _emailSender;

        public NationalBankCurrencyUpdateJob(EventLog eventLog, JobLog jobLog, CurrencyRepository currencyRepository, IOptions<EnviromentAccessOptions> options, EmailSender emailSender)
        {
            _eventLog = eventLog;
            _jobLog = jobLog;
            _currencyRepository = currencyRepository;
            _options = options.Value;
            _emailSender = emailSender;
        }

        public void Execute()
        {
            try
            {
                var rate = TryGetRates(DateTime.Today);

                IList<Currency> currencies = _currencyRepository.List(new ListQuery() {Page = null});

                foreach (var currency in currencies.Where(x=>!x.IsDefault))
                {
                    var currentRate = rate.Currencies.Where(x => x.Title.Contains(currency.Code)).FirstOrDefault();
                    if (currentRate == null)
                    {
                        EmailErrorNotification(
                            $"Курс валюты не обновлен, т.к. код валюты не найден в списке nationalbank.kz ({(currency.Code)})");
                        continue;
                    }
                    currency.CurrentNationalBankExchangeRate = currentRate.ExchangeRate;
                    currency.CurrentNationalBankExchangeQuantity = currentRate.Quantity;

                    _currencyRepository.Update(currency);
                    _eventLog.Log(
                        EventCode.NationalBankRatesUpdate,
                        EventStatus.Success,
                        EntityType.Currency,
                        currency.Id,
                        responseData: JsonConvert.SerializeObject(currency),
                        uri: GetUrl(DateTime.Today),
                        userId: Constants.ADMINISTRATOR_IDENTITY
                        );
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.NationalBankRatesUpdate,
                    EventStatus.Failed,
                    EntityType.None,
                    responseData: JsonConvert.SerializeObject(e),
                    uri: GetUrl(DateTime.Today),
                    userId: Constants.ADMINISTRATOR_IDENTITY
                    );
                _jobLog.Log("NationalBankCurrencyUpdateJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(e));
                EmailErrorNotification(JsonConvert.SerializeObject(e));
            }

        }

        private Rate TryGetRates(DateTime date)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(GetUrl(DateTime.Today)).Result;
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;

                return parser.Parse(content);
            }
        }

        private string GetUrl(DateTime date)
        {
            return $"{Url}?fdate={date.ToString("dd'.'MM'.'yyyy")}";
        }

        private void EmailErrorNotification(string message)
        {
            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail("[ОШИБКА] При загрузке курса валют от nationalbank.kz", message, messageReceiver);
        }
    }
}
