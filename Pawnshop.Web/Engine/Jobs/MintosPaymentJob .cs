using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MintosApi;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Data.Models.Mintos.UploadModels;
using System.Dynamic;
using Pawnshop.Data.Models.Dictionaries;
using System.Globalization;
using Hangfire;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Mintos.AnswerModels;

namespace Pawnshop.Web.Engine.Jobs
{
    public class MintosPaymentJob
    {
        private readonly EnviromentAccessOptions _options;
        private readonly EventLog _eventLog;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly MintosConfigRepository _mintosConfigRepository;
        private readonly CurrencyRepository _currencyRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly JobLog _jobLog;
        private readonly MintosBlackListRepository _mintosBlackListRepository;
        private MintosApi.MintosApi _api;

        public MintosPaymentJob(EventLog eventLog, ContractRepository contractRepository,
            GroupRepository groupRepository, OrganizationRepository organizationRepository, MintosConfigRepository mintosConfigRepository, CurrencyRepository currencyRepository,
            MintosContractRepository mintosContractRepository, MintosContractActionRepository mintosContractActionRepository, IOptions<EnviromentAccessOptions> options, JobLog jobLog,
            MintosBlackListRepository mintosBlackListRepository)
        {
            _eventLog = eventLog;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _mintosConfigRepository = mintosConfigRepository;
            _currencyRepository = currencyRepository;
            _mintosContractRepository = mintosContractRepository;
            _mintosContractActionRepository = mintosContractActionRepository;
            _options = options.Value;
            _jobLog = jobLog;
            _mintosBlackListRepository = mintosBlackListRepository;
            _api = new MintosApi.MintosApi(_eventLog, options);
        }

        [Queue("mintos")]
        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            if (!_options.MintosUpload) return;

            try
            {
                var paymentsToUpload = _mintosContractActionRepository.List(new ListQuery(){Page = null}, new { Status = MintosUploadStatus.Await });

                if (paymentsToUpload.Count == 0) return;

                IList<MintosContractAction> payments = paymentsToUpload;
                foreach (var payment in payments)
                {
                    BackgroundJob.Enqueue<MintosPaymentJob>(x => x.RegisterPayment(payment.Id));
                }
            }
            catch (Exception e)
            {
                _jobLog.Log(
                    "MintosPaymentJob",
                    JobCode.Error,
                    JobStatus.Failed,
                    responseData: e.Message);
            }
        }

        [Queue("mintos")]
        public void RegisterPayment(int id)
        {
            if (!_options.MintosUpload) return;

            var payment = _mintosContractActionRepository.Get(id);

            if(payment.Status != MintosUploadStatus.Await || payment.UploadDate.HasValue) return;

            var contract = _contractRepository.Get(payment.ContractId);
            var mintosContract = _mintosContractRepository.Get(payment.MintosContractId);

            var uploadConfig = _mintosConfigRepository.Find(new
            {
                mintosContract.OrganizationId,
                CurrencyId = mintosContract.MintosCurrencyId
            });

            if (uploadConfig == null)
            {
                _eventLog.Log(
                    EventCode.MintosContractPayment,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(payment),
                    "Настройки для выгрузки не найдены"
                );
                payment.Status = MintosUploadStatus.Error;
                _mintosContractActionRepository.Update(payment);
                return;
            }

            if (mintosContract.MintosStatus != "active")
            {
                _eventLog.Log(
                    EventCode.MintosContractPayment,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(payment),
                    "Договор не активирован/проверен в Mintos"
                );
                payment.Status = MintosUploadStatus.Declined;
                _mintosContractActionRepository.Update(payment);
                return;
            }
            try
            {
                switch (payment.ContractAction.ActionType)
                {
                    /*  Статусы для rebuy:
                     *  agreement_termination - расторжение договора
                     *  agreement_amendment - поправка к соглашению
                     *  early_repayment - досрочное погашение
                     *  agreement_prolongation - продление договора
                     *  other - другое
                     */
                    case ContractActionType.MonthlyPayment://Ежемесячное погашение по аннуитету
                                                           //регистрируем оплату по договору
                        RegisterPayment(uploadConfig, payment, mintosContract);
                        break;
                    case ContractActionType.Prolong://Продление по дискрету
                        DateTime? nextPaymentDate = mintosContract.PaymentSchedule
                            .Where(x => x.Status == MintosInvestorPaymentStatus.Await).OrderBy(z => z.Number).FirstOrDefault()?
                            .MintosDate;
                        if (!nextPaymentDate.HasValue || payment.ContractAction.Date.Date > nextPaymentDate.Value.Date)
                        {
                            MakeRebuy(uploadConfig, "agreement_prolongation", mintosContract, payment);
                            //добавляем договор в ч/с по выгрузке в mintos
                            MintosBlackList mintosBlackList = new MintosBlackList()
                            {
                                ContractId = payment.ContractId,
                                LockUntilDate = DateTime.Now.AddDays(1)
                            };
                            _mintosBlackListRepository.Insert(mintosBlackList);
                        }
                        else
                        {
                            //расширяем расписание оплат, затем регистрируем оплату по договору
                            if (mintosContract.PaymentSchedule.Count <= 5)
                            {
                                //расширяем расписание платежей
                                ExtendSchedule(uploadConfig, payment, mintosContract, contract);

                                mintosContract = _mintosContractRepository.Get(mintosContract.Id);
                                //регистрируем оплату
                                RegisterPayment(uploadConfig, payment, mintosContract);
                            }
                            else
                            {
                                //регистрируем оплату, договор при этой оплате автоматически выкупится
                                RegisterPayment(uploadConfig, payment, mintosContract);
                                //добавляем договор в ч/ с по выгрузке в mintos
                                MintosBlackList mintosBlackList = new MintosBlackList()
                                {
                                    ContractId = payment.ContractId,
                                    LockUntilDate = DateTime.Now.AddDays(1)
                                };
                                _mintosBlackListRepository.Insert(mintosBlackList);
                            }
                        }
                        break;
                    case ContractActionType.Addition://Добор
                    case ContractActionType.PartialPayment://Частичное гашение
                    case ContractActionType.Refinance://Рефинанс
                    case ContractActionType.PartialBuyout://Частичный выкуп

                        MakeRebuy(uploadConfig, "agreement_amendment", mintosContract, payment);
                        if (payment.ContractAction.FollowedId > 0)
                        {
                            //добавляем порожденный договор в черный список по загрузке договоров в минтос
                            MintosBlackList mintosBlackList = new MintosBlackList()
                            { ContractId = payment.ContractAction.FollowedId.Value };
                            _mintosBlackListRepository.Insert(mintosBlackList);
                        }

                        foreach (var schedule in mintosContract.PaymentSchedule.Where(x => x.Status == MintosInvestorPaymentStatus.Await))
                        {
                            schedule.Status = MintosInvestorPaymentStatus.Canceled;
                            schedule.CancelDate = DateTime.Now;
                        }
                        _mintosContractRepository.Update(mintosContract);
                        break;
                    case ContractActionType.Buyout://Выкуп
                                                   //делаем rebuy с причиной early_repayment()
                        if (payment.ContractAction.Date.Date >= mintosContract.FinalPaymentDate.Date)
                        {
                            RegisterPayment(uploadConfig, payment, mintosContract);
                        }
                        else
                        {
                            MakeRebuy(uploadConfig, "early_repayment", mintosContract, payment);
                        }

                        foreach (var schedule in mintosContract.PaymentSchedule.Where(x => x.Status == MintosInvestorPaymentStatus.Await))
                        {
                            schedule.Status = MintosInvestorPaymentStatus.Canceled;
                            schedule.CancelDate = DateTime.Now;
                        }
                        _mintosContractRepository.Update(mintosContract);
                        break;
                    case ContractActionType.Selling://Реализация
                                                    //делаем rebuy с причиной other()
                        MakeRebuy(uploadConfig, "agreement_termination", mintosContract, payment);
                        foreach (var schedule in mintosContract.PaymentSchedule.Where(x => x.Status == MintosInvestorPaymentStatus.Await)
                        )
                        {
                            schedule.Status = MintosInvestorPaymentStatus.Canceled;
                            schedule.CancelDate = DateTime.Now;
                        }
                        _mintosContractRepository.Update(mintosContract);
                        break;
                    default:
                        _eventLog.Log(
                            EventCode.MintosContractPayment,
                            EventStatus.Failed,
                            EntityType.MintosContract,
                            payment.MintosContractId,
                            JsonConvert.SerializeObject(payment),
                            "Данное действие никак не учитывается при выгрузке"
                            );
                        payment.Status = MintosUploadStatus.Error;
                        _mintosContractActionRepository.Update(payment);
                        break;
                }
            }
            catch (Exception e)
            {
                payment.Status = MintosUploadStatus.Error;
                _mintosContractActionRepository.Update(payment);
                _eventLog.Log(
                    EventCode.MintosContractPayment,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(payment),
                    e.Message
                );
            }
        }

        private void MakeRebuy(MintosConfig config, string purpose, MintosContract mintosContract, MintosContractAction action)
        {
            try
            {
                _api.TryPost($"rebuy/{mintosContract.MintosId}", config.ApiKey, new MintosRebuyUpload(purpose), EventCode.MintosContractRebuy, EntityType.MintosContract, mintosContract.Id);

                action.Status = MintosUploadStatus.Success;
                action.UploadDate = DateTime.Now;
                _mintosContractActionRepository.Update(action);
            }
            catch (Exception e)
            {
                action.Status = MintosUploadStatus.Error;
                _mintosContractActionRepository.Update(action);
                _eventLog.Log(
                    EventCode.MintosContractPayment,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(action),
                    e.Message);
            }
        }

        private void RegisterPayment(MintosConfig config, MintosContractAction action, MintosContract mintosContract)
        {
            try
            {
                var curentContract = _api.TryGetOneContract("loans", config.ApiKey, mintosContract.MintosId, mintosContract.Id);

                var nextSavedPayment = mintosContract.PaymentSchedule.Where(x =>
                    x.IsRepaid == false && x.Status == MintosInvestorPaymentStatus.Await).OrderBy(x => x.Number).FirstOrDefault();
                var nextPaymentCalculatedOnMintos = curentContract.data.payment_summary;

                MintosPaymentUploadModel payment = new MintosPaymentUploadModel(
                    nextSavedPayment.Number,
                    action.ContractAction.Date,
                    nextPaymentCalculatedOnMintos.next_payment_principal,
                    (Parse(nextPaymentCalculatedOnMintos.next_payment_interest) + Parse(nextPaymentCalculatedOnMintos.next_payment_delayed_interest)).ToString(),
                    nextPaymentCalculatedOnMintos.next_payment_late_payment_fee,
                    nextPaymentCalculatedOnMintos.outstanding_principal
                    );

                _api.TryPost($"loans/{mintosContract.MintosId}/payments", config.ApiKey, payment, EventCode.MintosContractPayment, EntityType.MintosContract, mintosContract.Id);

                mintosContract.PaymentSchedule.Remove(mintosContract.PaymentSchedule.Where(x => x.Id == nextSavedPayment.Id).FirstOrDefault());

                nextSavedPayment.Status = MintosInvestorPaymentStatus.Paid;
                nextSavedPayment.TotalSumPaid = Parse(nextPaymentCalculatedOnMintos.next_payment_total);
                nextSavedPayment.PrincipalAmountPaid = Parse(nextPaymentCalculatedOnMintos.next_payment_principal);
                nextSavedPayment.InterestAmountPaid = Parse(nextPaymentCalculatedOnMintos.next_payment_interest);
                nextSavedPayment.DelayedAmountPaid = Parse(nextPaymentCalculatedOnMintos.next_payment_late_payment_fee);

                mintosContract.PaymentSchedule.Add(nextSavedPayment);
                _mintosContractRepository.Update(mintosContract);

                action.Status = MintosUploadStatus.Success;
                action.UploadDate = DateTime.Now;
                action.InvestorScheduleId = nextSavedPayment.Id;
                _mintosContractActionRepository.Update(action);
            }
            catch (Exception e)
            {
                action.Status = MintosUploadStatus.Error;
                _mintosContractActionRepository.Update(action);
                _eventLog.Log(
                    EventCode.MintosContractPayment,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(action),
                    e.Message);
            }
        }

        private void ExtendSchedule(MintosConfig config, MintosContractAction action, MintosContract mintosContract, Contract contract)
        {
            try
            {
                var nextPaymentDate = mintosContract.PaymentSchedule.OrderBy(z => z.Number)
                    .FirstOrDefault(x => x.Status == MintosInvestorPaymentStatus.Await).MintosDate;
                if (action.ContractAction.Date > nextPaymentDate)
                {
                    _eventLog.Log(
                        EventCode.MintosContractExtendSchedule,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(action),
                        "Платеж просрочен, невозможно продлить расписание");
                    return;
                }

                MintosExtendScheduleModel scheduleToUpload = new MintosExtendScheduleModel();
                ExtendPaymentSchedule scheduleItemToUpload = new ExtendPaymentSchedule()
                {
                    number = mintosContract.PaymentSchedule.Max(x => x.Number) + 1,
                    date = contract.MaturityDate.ToString("yyyy-MM-dd")
                };
                
                scheduleToUpload.payment_schedule.Add(scheduleItemToUpload);

                var response = _api.TryPost($"loans/{mintosContract.MintosId}/extend-schedule",
                    config.ApiKey,
                    scheduleToUpload,
                    EventCode.MintosContractExtendSchedule,
                    EntityType.MintosContract,
                    mintosContract.Id);

                var schedule = JsonConvert.DeserializeObject<AnswerExtendScheduleModel>(response);

                var lastItem = schedule.data.OrderBy(x => x.number).LastOrDefault();

                MintosInvestorPaymentScheduleItem scheduleItemToSave = new MintosInvestorPaymentScheduleItem();
                scheduleItemToSave.Number = scheduleItemToUpload.number;
                scheduleItemToSave.ContractId = contract.Id;
                scheduleItemToSave.MintosContractId = mintosContract.Id;
                scheduleItemToSave.Number = mintosContract.PaymentSchedule.Max(x => x.Number) + 1;
                scheduleItemToSave.SendedDate = scheduleItemToSave.MintosDate = DateTime.Parse(lastItem.date);
                scheduleItemToSave.SendedPrincipalAmount = scheduleItemToSave.MintosPrincipalAmount = Parse(lastItem.principal_amount);
                scheduleItemToSave.SendedInterestAmount = scheduleItemToSave.MintosInterestAmount = Parse(lastItem.interest_amount);
                scheduleItemToSave.SendedDelayedAmount = scheduleItemToSave.MintosDelayedAmount = Parse(lastItem.delayed_amount);
                scheduleItemToSave.SendedTotalSum = scheduleItemToSave.MintosTotalSum = Parse(lastItem.sum);
                scheduleItemToSave.SendedTotalRemainingPrincipal = scheduleItemToSave.MintosTotalRemainingPrincipal = Parse(lastItem.total_remaining_principal);
                scheduleItemToSave.Status = MintosInvestorPaymentStatus.Await;

                _mintosContractRepository.InsertExtendSchedule(scheduleItemToSave);
            }
            catch (Exception e)
            {
                action.Status = MintosUploadStatus.Error;
                _mintosContractActionRepository.Update(action);
                _eventLog.Log(
                    EventCode.MintosContractExtendSchedule,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(action),
                    e.Message);
            }
        }

        private decimal Parse(string s)
        {
            s = s.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            return decimal.Parse(s, NumberStyles.Any,
                CultureInfo.InvariantCulture);
        }
    }
}
