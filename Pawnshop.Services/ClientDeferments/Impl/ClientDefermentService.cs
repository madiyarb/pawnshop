using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.TasLabRecruit;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Services.ClientDeferments.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Audit;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Services.ClientDeferments.Impl
{
    public class ClientDefermentService : IClientDefermentService
    {
        private readonly ClientDefermentRepository _clientDefermentsRepository;
        private readonly ContractRepository _contractRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly IClientDefermentsTelegramService _clientDefermentsTelegramService;
        private readonly ClientRepository _clientRepository;
        private readonly IEventLog _eventLog;

        public ClientDefermentService(
            ClientDefermentRepository clientDefermentsRepository,
            ContractRepository contractRepository,
            DomainValueRepository domainValueRepository,
            IClientDefermentsTelegramService clientDefermentsTelegramService,
            ClientRepository clientRepository,
            IEventLog eventLog)
        {
            _clientDefermentsRepository = clientDefermentsRepository;
            _contractRepository = contractRepository;
            _domainValueRepository = domainValueRepository;
            _clientDefermentsTelegramService = clientDefermentsTelegramService;
            _clientRepository = clientRepository;
            _eventLog = eventLog;
        }

        /// <summary>
        /// регистрация призывника на отсрочку
        /// </summary>
        /// <param name="recruit"></param>
        public async Task RegisterRecruitDeferment(Recruit recruit)
        {
            try
            {
                var client = _clientRepository.FindByIdentityNumber(recruit.IIN);
                if (client == null)
                    return;

                var contracts = _contractRepository.GetContractsByClientId(client.Id, new List<ContractStatus>() { ContractStatus.Signed });

                if (contracts.Any())
                {
                    contracts = contracts.Where(contract => contract.ContractClass == ContractClass.Credit || contract.ContractClass == ContractClass.Tranche).ToList();
                    foreach (var contract in contracts)
                    {
                        // получаем последнюю отсрочку
                        var oldDeferment = _clientDefermentsRepository.GetContractDeferment(contract.Id);
                        if (oldDeferment != null)
                        {
                            // если статус призывника изменился
                            if (recruit.Status != oldDeferment.RecruitStatus)
                            {
                                oldDeferment.RecruitStatus = recruit.Status;
                                if (recruit.Status)
                                    oldDeferment.StartDate = recruit.Date;
                                else
                                    oldDeferment.EndDate = recruit.Date;

                                oldDeferment.UpdateDate = DateTime.Now;
                                _clientDefermentsRepository.Update(oldDeferment);

                                await _clientDefermentsTelegramService.SendMessageToTelegramUpdateRecruit(oldDeferment, client.IdentityNumber, client.FullName, contract.ContractNumber);
                            }
                        }
                        else
                        {
                            // создаем новую отсрочку
                            var deferment = CreateNewDeferment(contract, recruit);
                            if (!deferment.StartDate.HasValue && deferment.EndDate.HasValue)
                            {
                                deferment.Status = RestructuringStatusEnum.New;
                            }
                            _clientDefermentsRepository.Insert(deferment);

                            await _clientDefermentsTelegramService.SendMessageToTelegramNewRecruit(deferment, client.IdentityNumber, client.FullName, contract.ContractNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.RegistrRecruit, EventStatus.Failed, EntityType.ClientDefermentsForRecruit, responseData: JsonConvert.SerializeObject(ex), requestData: JsonConvert.SerializeObject(recruit));
            }
        }

        public async Task CreateDefermentForMilitary(Contract contract, Recruit recruit)
        {
            var client = await _clientRepository.GetOnlyClientAsync(contract.ClientId);
            var oldDeferment = _clientDefermentsRepository.GetContractDeferment(contract.Id);
            var defermentType = _domainValueRepository.GetByCodeAndDomainCode(Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL, Constants.DEFERMENT_TYPES_TYPE_DOMAIN);

            if (oldDeferment != null)
            {
                // если статус призывника изменился
                if (recruit.Status != oldDeferment.RecruitStatus)
                {
                    oldDeferment.RecruitStatus = recruit.Status;
                    oldDeferment.StartDate = recruit.Status ? recruit.Date : oldDeferment.StartDate;
                    oldDeferment.UpdateDate = DateTime.Now;
                    oldDeferment.Status = RestructuringStatusEnum.Restructured;
                    _clientDefermentsRepository.Update(oldDeferment);

                    await _clientDefermentsTelegramService.SendMessageToTelegramUpdateContract(
                        oldDeferment, client.IdentityNumber,
                        client.FullName, contract.ContractNumber,
                        isMilitary: true,
                        defermentType.Name);
                }
            }
            else
            {
                var deferment = CreateNewDeferment(contract, recruit);
                _clientDefermentsRepository.Insert(deferment);
            }
        }

        public ClientDefermentModel MapDefermentToModel(ClientDeferment deferment, Client client = null)
        {
            client ??= _clientRepository.GetOnlyClient(deferment.ClientId);
            var defermentType = _domainValueRepository.Get(deferment.DefermentTypeId);
            var contract = _contractRepository.GetOnlyContract(deferment.ContractId);

            return new ClientDefermentModel
            {
                FullName = client.FullName,
                ClientId = client.Id,
                IIN = client.IdentityNumber,
                ClientStatus = defermentType.Code == Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL
                               ? (deferment.RecruitStatus ? "На службе" : "Не на службе")
                               : "",
                ContractId = deferment.ContractId,
                ContractNumber = contract.ContractNumber,
                IsContractRestructured = deferment.Status == RestructuringStatusEnum.Restructured,
                DefermentTypeName = defermentType.Name,
                StartDate = deferment.StartDate,
                EndDate = deferment.EndDate,
                CreateDate = deferment.CreateDate,
                UpdateDate = deferment.UpdateDate,
            };
        }

        public ContractDefermentInformation GetDefermentInformation(int contractId, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }

                var contractDeferment = _clientDefermentsRepository.GetContractDeferment(contractId);
                if (contractDeferment == null)
                {
                    return null;
                }

                var defermentInformation = new ContractDefermentInformation()
                {
                    Status = contractDeferment.Status,
                    IsInDefermentPeriod = false
                };

                if (contractDeferment.Status == RestructuringStatusEnum.Restructured &&
                    contractDeferment.StartDate.HasValue &&
                    contractDeferment.EndDate.HasValue &&
                    contractDeferment.StartDate < date &&
                    contractDeferment.EndDate >= date)
                {
                    defermentInformation.IsInDefermentPeriod = true;
                }

                return defermentInformation;
            }
            catch
            {
                return null;
            }
        }

        public List<ContractDefermentInformation> GetCreditLineDefermentInformation(int creditLineId, DateTime? date = null)
        {
            try
            {
                if (!date.HasValue)
                {
                    date = DateTime.Now;
                }

                var contractDeferment = _clientDefermentsRepository.GetCreditLineDeferments(creditLineId);
                if (!contractDeferment.Any())
                {
                    return null;
                }

                var informationList = new List<ContractDefermentInformation>();
                foreach(var deferment in contractDeferment)
                {
                    var defermentInformation = new ContractDefermentInformation()
                    {
                        Status = deferment.Status,
                        IsInDefermentPeriod = false
                    };

                    if (deferment.Status == RestructuringStatusEnum.Restructured &&
                        deferment.StartDate.HasValue &&
                        deferment.EndDate.HasValue &&
                        deferment.StartDate < date &&
                        deferment.EndDate >= date)
                    {
                        defermentInformation.IsInDefermentPeriod = true;
                    }

                    informationList.Add(defermentInformation);
                }
                

                return informationList;
            }
            catch
            {
                return null;
            }
        }

        public async Task CreateDeferment(Contract contract, DateTime startDefermentDate, DateTime endDefermentDate, string type)
        {
            var client = await _clientRepository.GetOnlyClientAsync(contract.ClientId);
            var oldDeferment = _clientDefermentsRepository.GetContractDeferment(contract.Id);
            var defermentType = _domainValueRepository.GetByCodeAndDomainCode(type, Constants.DEFERMENT_TYPES_TYPE_DOMAIN);

            if (oldDeferment != null)
            {
                oldDeferment.Status = RestructuringStatusEnum.Restructured;
                oldDeferment.StartDate = startDefermentDate;
                oldDeferment.EndDate = endDefermentDate;
                oldDeferment.UpdateDate = DateTime.Now;
                oldDeferment.IsRestructured = true;
                _clientDefermentsRepository.Update(oldDeferment);

                await _clientDefermentsTelegramService.SendMessageToTelegramUpdateContract(
                    oldDeferment,
                    client.IdentityNumber, client.FullName,
                    contract.ContractNumber,
                    isMilitary: false,
                    defermentType.Name);

            }
            else
            {
                var deferment = new ClientDeferment()
                {
                    ClientId = contract.ClientId,
                    ContractId = contract.Id,
                    IsRestructured = true,
                    Status = RestructuringStatusEnum.Restructured,
                    DefermentTypeId = defermentType.Id,
                    CreateDate = DateTime.Now,
                    StartDate = startDefermentDate,
                    EndDate = endDefermentDate
                };
                _clientDefermentsRepository.Insert(deferment);

                await _clientDefermentsTelegramService.SendMessageToTelegramUpdateContract(
                    deferment,
                    client.IdentityNumber, client.FullName,
                    contract.ContractNumber,
                    isMilitary: false,
                    defermentType.Name);
            }
        }

        public IEnumerable<ClientDeferment> GetActiveDeferments(int clientId)
        {
            return _clientDefermentsRepository.GetClientDeferment(clientId, true);
        }

        public ClientDeferment GetActiveDeferment(int contractId)
        {
            return _clientDefermentsRepository.GetContractDeferment(contractId);
        }

        public void CancelClientDeferment(ClientDeferment clientDeferment)
        {
            clientDeferment.DeleteDate = DateTime.Now;
            clientDeferment.UpdateDate = DateTime.Now;
            clientDeferment.Status = RestructuringStatusEnum.Cancelled;
            _clientDefermentsRepository.Update(clientDeferment);
        }

        public int GetRestructuredMonthCount(int contractId, List<ContractPaymentSchedule> contractPaymentSchedule)
        {
            var clientDeferment = GetActiveDeferment(contractId);

            var futurePaymentScheduleItem = contractPaymentSchedule.OrderBy(x => x.Date).FirstOrDefault(x => x.Date.Date >= clientDeferment.StartDate.Value.Date);

            bool isOnControlDate = clientDeferment.StartDate.Value.Day == futurePaymentScheduleItem.Date.Day;

            var restructuredSchedule = contractPaymentSchedule.Where(x => x.Date.Date >= clientDeferment.StartDate.Value.Date && clientDeferment.EndDate.Value.Date <= x.Date.Date).OrderBy(x => x.Date);

            if (restructuredSchedule != null && restructuredSchedule.Count() > 0)
                return isOnControlDate ? restructuredSchedule.Count() : restructuredSchedule.Count() - 2;

            return 0;
        }

        private ClientDeferment CreateNewDeferment(Contract contract, Recruit recruit)
        {
            var typeId = _domainValueRepository.GetByCodeAndDomainCode(Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL, Constants.DEFERMENT_TYPES_TYPE_DOMAIN).Id;
            var clientDeferment = new ClientDeferment()
            {
                ClientId = contract.ClientId,
                ContractId = contract.Id,
                Status = RestructuringStatusEnum.Frozen,
                RecruitStatus = recruit.Status,
                DefermentTypeId = typeId,
                CreateDate = DateTime.Now
            };

            if (recruit.Status)
                clientDeferment.StartDate = recruit.Date;
            else
                clientDeferment.EndDate = recruit.Date;

            return clientDeferment;
        }
    }
}
