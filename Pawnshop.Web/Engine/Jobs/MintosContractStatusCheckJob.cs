using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pawnshop.Web.Engine.Audit;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Mintos;
using System.Globalization;
using System.Text;
using Hangfire;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Mintos.UploadModels;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Web.Engine.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs
{
    public class MintosContractStatusCheckJob
    {
        private readonly EnviromentAccessOptions _options;
        private readonly EventLog _eventLog;
        private readonly ContractRepository _contractRepository;
        private readonly ClientRepository _clientRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly MintosConfigRepository _mintosConfigRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly JobLog _jobLog;
        private readonly EmailSender _emailSender;
        private readonly MintosApi.MintosApi _mintosApi;
        private readonly MintosUploadQueueRepository _mintosUploadQueueRepository;
        private readonly CurrencyRepository _currencyRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;

        public MintosContractStatusCheckJob(EventLog eventLog, ContractRepository contractRepository, ClientRepository clientRepository,
            OrganizationRepository organizationRepository, MintosConfigRepository mintosConfigRepository,
            MintosContractRepository mintosContractRepository, IOptions<EnviromentAccessOptions> options, JobLog jobLog, EmailSender emailSender,
            MintosApi.MintosApi mintosApi, MintosUploadQueueRepository mintosUploadQueueRepository, CurrencyRepository currencyRepository,
            MintosContractActionRepository mintosContractActionRepository)
        {
            _eventLog = eventLog;
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
            _organizationRepository = organizationRepository;
            _mintosConfigRepository = mintosConfigRepository;
            _mintosContractRepository = mintosContractRepository;
            _options = options.Value;
            _jobLog = jobLog;
            _emailSender = emailSender;
            _mintosApi = mintosApi;
            _mintosUploadQueueRepository = mintosUploadQueueRepository;
            _currencyRepository = currencyRepository;
            _mintosContractActionRepository = mintosContractActionRepository;
        }

        /// <summary>
        /// Ставит все договора Mintos в очередь для проверки графиков и статуса оплаты
        /// </summary>
        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            if (!_options.MintosUpload) return;

            try
            {
                //список договоров для проверки и актуализации статуса на Mintos
                var contractsToCheck = new List<MintosContract>();

                //договора, которые проверяются роботом(недавно выгруженные)
                var contractsOnDecision = _mintosContractRepository.List(new ListQuery() { Page = null }, new { Status = "decision" });
                if (contractsOnDecision.Count > 0)
                {
                    contractsToCheck.AddRange(contractsOnDecision);
                }
                //договора, которые проверяются вручную(возможно выгружены с ошибкой)
                var contractsOnPayout = _mintosContractRepository.List(new ListQuery() { Page = null }, new { Status = "payout" });
                if (contractsOnPayout.Count > 0)
                {
                    contractsToCheck.AddRange(contractsOnPayout);
                }
                //активные договора(размещенные на Mintos)
                var contractsActive = _mintosContractRepository.List(new ListQuery() { Page = null }, new { Status = "active" });
                if (contractsActive.Count > 0)
                {
                    contractsToCheck.AddRange(contractsActive);
                }

                if (contractsToCheck.Count==0) return;
                IList<MintosContract> contracts = contractsToCheck;
                foreach (var mintosContract in contracts)
                {
                    try
                    {
                        BackgroundJob.Enqueue<MintosContractStatusCheckJob>(x=>x.CheckStatus(mintosContract.Id));
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpdate,
                            EventStatus.Failed,
                            EntityType.MintosContract,
                            mintosContract.Id,
                            responseData: e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                _jobLog.Log(
                    "MintosContractStatusCheckJob",
                    JobCode.Error,
                    JobStatus.Failed,
                    responseData: e.Message);
            }
        }
        
        /// <summary>
        /// Акт сверки с Mintos
        /// </summary>
        [Queue("mintos")]
        public void CheckStatuses()
        {
            if (!_options.MintosUpload) return;

            var message = new StringBuilder();
            int errors = 0, fixes = 0, failed = 0, counter = 0;

            try
            {
                var configs = _mintosConfigRepository.List(new ListQuery() {Page = null});

                foreach (var config in configs)
                {
                    var all = _mintosApi.TryGetAll("loans", config.ApiKey);
                    var damagedContracts = _mintosContractRepository.ValidateContractsWithMintos(all.All.Select(x=> new MintosValidationModel
                    {
                        Id = x.Key,
                        CreatedAt = x.Value.CreatedAt.Date,
                        Number = x.Value.LenderId,
                        Status = x.Value.Status
                    }).ToList());

                    errors += damagedContracts.Count;

                    foreach (var damaged in damagedContracts.OrderBy(x=>x.ErrorCode).Where(x=>x.Status=="active"))
                    {
                        message.AppendLine(counter == 0 ? string.Empty : "<hr>");
                        counter++;

                        if (damaged.ErrorCode == ValidationErrorCode.NotSaved)
                        {
                            message.AppendLine(
                                $"{counter}. Договор {damaged.ContractNumber}({damaged.Number}, {damaged.Id}) - не найден в нашей системе;<br />");
                            try
                            {
                                var search = _contractRepository.List(new ListQuery()
                                    {Filter = damaged.ContractNumber});
                                if (search.Count == 1)
                                {
                                    var contract = _contractRepository.Get(search.FirstOrDefault().Id);
                                    message.AppendLine($@"	- Договор {contract.ContractNumber} найден в системе, с идентификатором {contract.Id}<br />");
                                    var contractInfo = _mintosApi.TryGetOneContract("loans", config.ApiKey, damaged.Id, damaged.MintosContractId);
                                    var mintosUploadQueue = _mintosUploadQueueRepository.Find(new { ContractId = contract.Id, UploadDate = DateTime.Parse(contractInfo.data.loan.mintos_issue_date.date) });

                                    if (mintosUploadQueue == null)
                                    {
                                        message.AppendLine($@"	- Договор {contract.ContractNumber}({contract.Id}) не найден в очереди на выгрузку, дальнейшие действия невозможны <br />");
                                        failed++;
                                        continue;
                                    }

                                    var contractCurrency = _currencyRepository.Find(new { IsDefault = true });
                                    mintosUploadQueue.Currency = _currencyRepository.GetFromHistory(mintosUploadQueue.CurrencyId, mintosUploadQueue.CreateDate);

                                    var mintosContract = contractInfo.ConvertToMintosContract(contractCurrency,
                                        mintosUploadQueue, config);

                                    
                                    var actionsToEnqueue = mintosContract.FindNotUploadedActions(contract.Actions, mintosUploadQueue.CreateDate);

                                    foreach (var action in actionsToEnqueue)
                                    {
                                        var contractAction =
                                            contract.Actions.FirstOrDefault(x => x.Id == action.ContractActionId);
                                        message.AppendLine($@"	- Действиe {contractAction.ActionType.GetDisplayName()} по договору от {contractAction.Date:dd.MM.yyyy} будет поставлено в очередь на выгрузку<br />");
                                    }

                                    using (var transaction = _mintosContractRepository.BeginTransaction())
                                    {
                                        _mintosContractRepository.Insert(mintosContract);

                                        mintosUploadQueue.UploadDate = DateTime.Now;
                                        mintosUploadQueue.Status = MintosUploadStatus.Success;
                                        mintosUploadQueue.MintosContractId = mintosContract.Id;

                                        _mintosUploadQueueRepository.Update(mintosUploadQueue);

                                        foreach (var action in actionsToEnqueue)
                                        {
                                            action.MintosContractId = mintosContract.Id;

                                            _mintosContractActionRepository.Insert(action);
                                        }

                                        transaction.Commit();
                                    }
                                    message.AppendLine($@"	✓ Договор успешно сохранен в системе с идентификатором {mintosContract.Id} <br />");
                                    fixes++;
                                }
                                else
                                {
                                    message.AppendLine($@"	× Не удалось найти в системе, так как по номеру договора доступно {search.Count} договоров<br />");
                                    failed++;
                                }
                            }
                            catch(Exception e)
                            {
                                message.AppendLine($@"	× Не удалось восстановить договор, так как возникла ошибка: '{e.Message}' <br />");
                                failed++;
                            }
                        }
                        else if (damaged.ErrorCode == ValidationErrorCode.WrongStatus)
                        {
                            message.AppendLine($@"{counter}. Договор {damaged.ContractNumber}({damaged.Number}, {damaged.Id}) - разные статусы в Mintos и в нашей системе;<br />");
                            if (damaged.MintosContractId.HasValue)
                            {
                                try
                                {
                                    var mintosContract = _mintosContractRepository.Get(damaged.MintosContractId.Value);
                                    mintosContract.MintosStatus = damaged.Status;
                                    using (var transaction = _mintosContractRepository.BeginTransaction())
                                    {
                                        _mintosContractRepository.Update(mintosContract);
                                        transaction.Commit();
                                    }

                                    message.AppendLine($@"	✓ Статус успешно изменен с {damaged.MintosStatus} на {damaged.Status}<br />");
                                    fixes++;
                                }
                                catch (Exception e)
                                {
                                    message.AppendLine($@"	× Ошибка изменения статуса:{e.Message}<br />");
                                    failed++;
                                }
                            }
                            else
                            {
                                message.AppendLine($@"	× Не удалось найти договор {damaged.ContractNumber}({damaged.Number}, {damaged.Id}) в нашей системе<br />");
                                failed++;
                            }
                        }
                        else if (damaged.ErrorCode == ValidationErrorCode.BadDate)
                        {
                            message.AppendLine($"{counter}. Договор {damaged.ContractNumber}({damaged.Number}, {damaged.Id}) - не сходится дата выгрузки;<br />");
                            try
                            {
                                var mintosContract = _mintosContractRepository.Get(damaged.MintosContractId.Value);
                                message.AppendLine($@"	- Дата выгрузки будет изменена с {mintosContract.UploadDate:dd.MM.yyyy} на {damaged.CreatedAt.Date:dd.MM.yyyy}<br />");
                                mintosContract.UploadDate = damaged.CreatedAt.Date;

                                using (var transaction = _mintosContractRepository.BeginTransaction())
                                {
                                    _mintosContractRepository.Update(mintosContract);
                                    transaction.Commit();
                                }
                                message.AppendLine($@"	✓ Дата выгрузки успешно изменена");

                                fixes++;
                            }
                            catch (Exception e)
                            {
                                message.AppendLine($@"	× Ошибка изменения даты выгрузки: {e.Message}");
                                failed++;
                            }
                        }
                        else if (damaged.ErrorCode == ValidationErrorCode.NotLoadedPayment)
                        {
                            message.AppendLine($"{counter}. По договору {damaged.ContractNumber}({damaged.Number}, {damaged.Id}) выгруженным {damaged.UploadDate:dd.MM.yyyy}, есть не выгруженная оплата;<br />");
                            try
                            {
                                var contract = _contractRepository.Get(damaged.ContractId.Value);
                                var action = contract.Actions.FirstOrDefault(x => x.Id == damaged.ContractActionId);

                                message.AppendLine($@"	- Действие {action.ActionType.GetDisplayName()} от {action.Date:dd.MM.yyyy} не выгружено");

                                var actionToUpload = new MintosContractAction(action);

                                var mintosContract = _mintosContractRepository.Get(damaged.MintosContractId.Value);

                                using (var transaction = _mintosContractActionRepository.BeginTransaction())
                                {
                                    actionToUpload.MintosContractId = mintosContract.Id;
                                    _mintosContractActionRepository.Insert(actionToUpload);

                                    transaction.Commit();
                                }

                                message.AppendLine($@"	✓ Оплата поставлена в очередь на выгрузку");
                                fixes++;
                            }
                            catch (Exception e)
                            {
                                message.AppendLine($@"	× Ничего не было сделано, возникла ошибка {e.Message}");
                                failed++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _jobLog.Log(
                    "MintosContractStatusCheckJob",
                    JobCode.Error,
                    JobStatus.Failed,
                    responseData: e.Message);
            }
            finally
            {
                if (message.Length > 0)
                {
                    SendWarningMessage($"Ошибок: {errors} (исправлено {fixes}, ошибка при исправлении у {failed})",
                        message.ToString());
                }
                else
                {
                    _eventLog.Log(EventCode.MintosValidation, EventStatus.Success, EntityType.None);
                }
            }
        }

        [Queue("mintos")]
        public void CheckStatus(int id)
        {
            if (!_options.MintosUpload) return;

            var mintosContract = _mintosContractRepository.Get(id);
            var contract = _contractRepository.Get(mintosContract.ContractId);
            var parentContractId = contract.ParentId; //_contractRepository.GetParentContract(contract.Id);
            Contract parentContract = null;
            if (parentContractId.HasValue)
            {
                parentContract = _contractRepository.Get((int)parentContractId);
                parentContract.Client = _clientRepository.Get(parentContract.ClientId);
            }

            var organization = _organizationRepository.Get(mintosContract.OrganizationId);
            var uploadConfig = _mintosConfigRepository.Find(new
            {
                OrganizationId = organization.Id,
                CurrencyId = mintosContract.MintosCurrencyId
            });

            if (uploadConfig == null)
            {
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: "Настройки для выгрузки не найдены"
                );
                return;
            }

            var response = _mintosApi.TryGetOneContract("loans", uploadConfig.ApiKey, mintosContract.MintosId, mintosContract.Id);
            var scheduleUpdated = false;

            //Если количество оплат Mintos не совпадает с сохраненным - не делаем ничего, пишем в лог
            if (response.data.payment_schedule.Length != mintosContract.PaymentSchedule.Count)
            {
                var error = string.Empty;
                if (response.data.payment_schedule.Length >
                    mintosContract.PaymentSchedule.Count)
                {
                    if (mintosContract.PaymentSchedule.Count > 0)
                    {
                        var toSave = response.data.payment_schedule.Where(x => mintosContract.PaymentSchedule.All(p => p.MintosDate.Date != DateTime.Parse(x.date).Date));
                        foreach (var item in toSave)
                        {
                            _mintosContractRepository.InsertExtendSchedule(item.ConvertToSaved(mintosContract.ContractId, mintosContract.Id));
                        }
                    }
                    else
                    {
                        foreach (var item in response.data.payment_schedule)
                        {
                            var itemToSave = item.ConvertToSaved(mintosContract.ContractId, mintosContract.Id);
                            _mintosContractRepository.InsertExtendSchedule(itemToSave);
                            mintosContract.PaymentSchedule.Add(itemToSave);
                        }
                    }
                    error = $"В нашей базе платежей МЕНЬШЕ - ({mintosContract.PaymentSchedule.Count}), чем в Mintos - ({response.data.payment_schedule.Length}), ошибка исправлена автоматически";

                    //SendWarningMessage($"[ИСПРАВЛЕНО]Критическая ошибка актуализации договора {response.data.loan.lender_id}({mintosContract.Id})", error);
                }
                else
                {
                    try
                    {
                        using (var transaction = _mintosContractRepository.BeginTransaction())
                        {
                            _mintosContractRepository.DeleteSchedule(mintosContract.Id);

                            foreach (var item in response.data.payment_schedule)
                            {
                                var itemToSave = item.ConvertToSaved(mintosContract.ContractId, mintosContract.Id);
                                _mintosContractRepository.InsertExtendSchedule(itemToSave);
                                mintosContract.PaymentSchedule.Add(itemToSave);
                            }

                            transaction.Commit();
                        }

                        error = $"В нашей базе платежей БОЛЬШЕ - ({mintosContract.PaymentSchedule.Count}), чем в Mintos - ({response.data.payment_schedule.Length}), ошибка исправлена автоматически";
                        //SendWarningMessage($"[ИСПРАВЛЕНО]Критическая ошибка актуализации договора {response.data.loan.lender_id}({mintosContract.Id})", error);
                    }
                    catch (Exception e)
                    {
                        error = $"В нашей базе платежей БОЛЬШЕ - ({mintosContract.PaymentSchedule.Count}), чем в Mintos - ({response.data.payment_schedule.Length}), ошибка не исправлена:\n{e.StackTrace}";
                        SendWarningMessage($"[ОШИБКА]Критическая ошибка актуализации договора {response.data.loan.lender_id}({mintosContract.Id})", error);
                    }

                }

                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: $"{error}. Необходимо вручную актуализировать данные."
                );
                return;
            }
            else
            {
                //Проверяем платежи на признак оплаты. Если оплачен - сохраняем в базу признак оплаты и сумму, которую выплатили
                foreach (var downloadedSchedule in response.data.payment_schedule)
                {
                    var savedSchedules = mintosContract.PaymentSchedule.Where(x => x.Number == downloadedSchedule.number || x.MintosDate.Date == DateTime.Parse(downloadedSchedule.date).Date);
                    if (savedSchedules.Count() == 1)
                    {
                        var savedSchedule = savedSchedules.FirstOrDefault();

                        if (savedSchedule.IsRepaid != downloadedSchedule.is_repaid
                                 || savedSchedule.TotalSumPaid != Parse(downloadedSchedule.received_amount))
                        {
                            savedSchedule.IsRepaid = downloadedSchedule.is_repaid;
                            savedSchedule.TotalSumPaid = Parse(downloadedSchedule.received_amount);
                            scheduleUpdated = true;
                        }
                    }
                    else
                    {
                        var error = $"Ошибка в платеже от {downloadedSchedule.date} - в нашей системе он зарегестрирован {savedSchedules.Count()} раз:{JsonConvert.SerializeObject(savedSchedules)}";
                        SendWarningMessage($"[ОШИБКА]Критическая ошибка актуализации договора {response.data.loan.lender_id}({mintosContract.Id})", error);
                    }
                }
            }

            var contractUpdated = false;
            if (response.data.loan.status != mintosContract.MintosStatus
                || mintosContract.FinalPaymentDate != DateTime.Parse(response.data.loan.final_payment_date.date))
            {
                mintosContract.FinalPaymentDate = DateTime.Parse(response.data.loan.final_payment_date.date);
                mintosContract.MintosStatus = response.data.loan.status;
                contractUpdated = true;
            }

            if (scheduleUpdated || contractUpdated)
            {
                using (var transaction = _mintosContractRepository.BeginTransaction())
                {
                    _mintosContractRepository.Update(mintosContract);
                    transaction.Commit();
                }
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Success,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    JsonConvert.SerializeObject(mintosContract),
                    JsonConvert.SerializeObject(response)
                );
            }
            else
            {
                return;
            }
        }

        private decimal Parse(string s)
        {
            s = s.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            return decimal.Parse(s, NumberStyles.Any,
                CultureInfo.InvariantCulture);
        }

        private void SendWarningMessage(string info, string message)
        {
            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail($"Сверка договоров с Mintos. {info}", message, messageReceiver);
            
            _eventLog.Log(EventCode.MintosValidation, EventStatus.Failed, EntityType.None, responseData: info);
        }
    }
}
