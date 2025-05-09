using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using System;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Services.ClientGeoPositions;
using Serilog;

namespace Pawnshop.Services.ApplicationsOnline
{
    public sealed class ApplicationOnlineCheckCreationService : IApplicationOnlineCheckCreationService
    {
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ApplicationOnlineChecksRepository _applicationOnlineChecksRepository;
        private readonly ApplicationOnlineTemplateChecksRepository _applicationOnlineTemplateChecksRepository;
        private readonly ClientGeoPositionsService _clientGeoPositionsService;
        private readonly CarRepository _carRepository;
        private readonly ClientExternalValidationDataRepository _clientExternalValidationDataRepository;
        private readonly ILogger _logger;
        public ApplicationOnlineCheckCreationService(
            ApplicationOnlineRepository applicationOnlineRepository,
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ApplicationOnlineChecksRepository applicationOnlineChecksRepository,
            ApplicationOnlineTemplateChecksRepository applicationOnlineTemplateChecksRepository,
            ClientGeoPositionsService clientGeoPositionsService,
            CarRepository carRepository,
            ClientExternalValidationDataRepository clientExternalValidationDataRepository,
            ILogger logger)
        {
            _applicationOnlineRepository = applicationOnlineRepository;
            _carRepository = carRepository;
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _applicationOnlineChecksRepository = applicationOnlineChecksRepository;
            _applicationOnlineTemplateChecksRepository = applicationOnlineTemplateChecksRepository;
            _clientGeoPositionsService = clientGeoPositionsService;
            _clientExternalValidationDataRepository = clientExternalValidationDataRepository;
            _logger = logger;
        }

        public void CreateChecksForManager(Guid applicationOnlineId)
        {
            try
            {
                var applicationOnline = _applicationOnlineRepository.Get(applicationOnlineId);

                var templateChecksQuery = new
                {
                    IsActual = true,
                    ToManager = true,
                    Stage = applicationOnline.Stage,
                    IsManual = true
                };

                var templateChecks = _applicationOnlineTemplateChecksRepository.List(null, templateChecksQuery);
                var checks = _applicationOnlineChecksRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });

                var createChecks = templateChecks
                    .Where(x => checks.All(c => c.TemplateId != x.Id)).ToList();

                if (createChecks.Any())
                    createChecks.ForEach(x => _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                    {
                        ApplicationOnlineId = applicationOnlineId,
                        TemplateId = x.Id,
                        CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                    }));
                CreateAutoChecksForManager(applicationOnline);

            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw new Exception($"Ошибка создания списка проверок заявки: {exception.Message}");
            }
        }


        public void CreateChecksForVerificator(Guid applicationOnlineId)
        {
            try
            {
                var applicationOnline = _applicationOnlineRepository.Get(applicationOnlineId);

                var templateChecksQuery = new
                {
                    IsActual = true,
                    ToVerificator = true,
                    Stage = applicationOnline.Stage,
                    IsManual = true
                };

                var templateChecks = _applicationOnlineTemplateChecksRepository.List(null, templateChecksQuery);
                var checks = _applicationOnlineChecksRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });

                var createChecks = templateChecks.Where(x => !checks.Any(c => c.TemplateId == x.Id)).ToList();

                if (createChecks.Any())
                    createChecks.ForEach(x => _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                    {
                        ApplicationOnlineId = applicationOnlineId,
                        TemplateId = x.Id,
                        CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                    }));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw new Exception($"Ошибка создания списка проверок заявки: {exception.Message}");
            }
        }


        private void CreateAutoChecksForManager (ApplicationOnline applicationOnline)
        {
            if (_applicationOnlineChecksRepository.GetCheckByCode(applicationOnline.Id, "geolocationcheck") == null)
            {
                var geoPositionTemplate = _applicationOnlineTemplateChecksRepository.GetByCode("geolocationcheck");
                if (_clientGeoPositionsService.HasActualGeoPosition(applicationOnline.ClientId))
                {
                    _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                    {
                        ApplicationOnlineId = applicationOnline.Id,
                        TemplateId = geoPositionTemplate.Id,
                        CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Checked = true
                    });
                }
                else
                {
                    _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                    {
                        ApplicationOnlineId = applicationOnline.Id,
                        TemplateId = geoPositionTemplate.Id,
                        CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Checked = true
                    });
                }
            }


            if (_applicationOnlineChecksRepository.GetCheckByCode(applicationOnline.Id, "CarAlreadyExistInDatabase") == null)
            {
                var car = _applicationOnlineCarRepository.Get(applicationOnline.ApplicationOnlinePositionId);
                var carAlreadyExistInDatabaseTemplate = _applicationOnlineTemplateChecksRepository.GetByCode("CarAllreadyExistInDatabase");

                if (car.CarId != null)
                {
                    var contractsAndClients = _carRepository.GetShortContractByCarId(car.CarId.Value).Result;
                    string note = "Автомобиль указан как залог по следующим договорам: \r\n";
                    foreach (var contractAndClient in contractsAndClients)
                    {
                        note +=
                            $"Номер договора: {contractAndClient.ContractNumber} клиента {contractAndClient.ClientId}. Дата подписания {contractAndClient.SignDate?.ToString("dd/MM/yyyy")}. Статус {contractAndClient.Status.GetDisplayName()} \r\n";
                    }

                    _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                    {
                        ApplicationOnlineId = applicationOnline.Id,
                        TemplateId = carAlreadyExistInDatabaseTemplate.Id,
                        CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Checked = false,
                        Note = note
                    });
                }
            }


            if (_applicationOnlineChecksRepository.GetCheckByCode(applicationOnline.Id, "blacklistcheck") == null)
            {
                var clientInBlackListCheck = _applicationOnlineTemplateChecksRepository.GetByCode("blacklistcheck");
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnline.Id,
                    TemplateId = clientInBlackListCheck.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = true,
                    Note = "ИИН не найден в черном списке"
                });
            }

            if (_applicationOnlineChecksRepository.GetCheckByCode(applicationOnline.Id, "FMCcheck") == null)
            {
                var FMCcheckTemplate = _applicationOnlineTemplateChecksRepository.GetByCode("FMCcheck");

                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnline.Id,
                    TemplateId = FMCcheckTemplate.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = true,
                    Note = "ИИН не найден в списке КФМ"
                });
            }

            var clientExternalData = _clientExternalValidationDataRepository.GetByClientId(applicationOnline.ClientId).Result;
            if (clientExternalData != null)
            {
                if (clientExternalData.SUSNValidation == false)
                {
                    var clientExternalDataCheckTemplate =
                        _applicationOnlineTemplateChecksRepository.GetByCode("CheckSUSN");

                    if (clientExternalDataCheckTemplate != null)
                    {
                        _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                        {
                            ApplicationOnlineId = applicationOnline.Id,
                            TemplateId = clientExternalDataCheckTemplate.Id,
                            CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now,
                            Checked = false
                        });
                    }
                }

                if (clientExternalData.BankruptValidation == false)
                {
                    var clientExternalDataCheckTemplate =
                        _applicationOnlineTemplateChecksRepository.GetByCode("CheckBankrupt");

                    if (clientExternalDataCheckTemplate != null)
                    {
                        _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                        {
                            ApplicationOnlineId = applicationOnline.Id,
                            TemplateId = clientExternalDataCheckTemplate.Id,
                            CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now,
                            Checked = false
                        });
                    }
                }

                if (clientExternalData.DebtorRegistryValidation == false)
                {
                    var clientExternalDataCheckTemplate =
                        _applicationOnlineTemplateChecksRepository.GetByCode("CheckDebtorRegistry");

                    if (clientExternalDataCheckTemplate != null)
                    {
                        _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                        {
                            ApplicationOnlineId = applicationOnline.Id,
                            TemplateId = clientExternalDataCheckTemplate.Id,
                            CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now,
                            Checked = false
                        });
                    }
                }
            }
        }

        public void CreateAutoChecksForBiometric(Guid applicationOnlineId)
        {
            var applicationOnline = _applicationOnlineRepository.Get(applicationOnlineId);
            var f2FLivenessCheckTemplate =
                _applicationOnlineTemplateChecksRepository.GetByCode("F2F_LIVENESS");
            if (f2FLivenessCheckTemplate != null)
            {
                bool isChecked = applicationOnline.Status.Equals(ApplicationOnlineStatus.RequisiteCheck.ToString());
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnline.Id,
                    TemplateId = f2FLivenessCheckTemplate.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = isChecked
                });

            }

            var f2FCheck = _applicationOnlineTemplateChecksRepository.GetByCode("F2F");
            if (f2FCheck != null)
            {
                bool isChecked = applicationOnline.Status.Equals(ApplicationOnlineStatus.RequisiteCheck.ToString());
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnline.Id,
                    TemplateId = f2FCheck.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = isChecked
                });
            }
        }

        public void CreateAutoChecksForRequisiteValidation(Guid applicationOnlineId)
        {
            var applicationOnline = _applicationOnlineRepository.Get(applicationOnlineId);
            var checkClientRequisitesTemplate =
                _applicationOnlineTemplateChecksRepository.GetByCode("CheckClientRequisites");
            if (checkClientRequisitesTemplate != null)
            {
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnline.Id,
                    TemplateId = checkClientRequisitesTemplate.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = false
                });
            }
        }

        public void CreateCheckForIncorrectAmountRequested(Guid applicationOnlineId,
            decimal requestedAmount,
            decimal totalDebtAmount,
            decimal creditLineLimit,
            decimal carCreditLineLimit)
        {
            var incorrectAmountCheck =
                _applicationOnlineTemplateChecksRepository.GetByCode("CreditlineLimitCheck");
            if (incorrectAmountCheck != null)
            {
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnlineId,
                    Note = $"Клиент запрашивает {requestedAmount}. " +
                           $"Cумма текущего основного долга по траншу {totalDebtAmount}." +
                           $"В сумме {requestedAmount + totalDebtAmount}" +
                           $"Это превышает сумму текущего лимита КЛ {creditLineLimit}" +
                           $"Либо лимит кредитной линии оценки {carCreditLineLimit}",
                    TemplateId = incorrectAmountCheck.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = false
                });
            }
        }

        public void CreateCheckAttentionApplicationAmountChanged(Guid applicationOnlineId, decimal? oldAmount, decimal? newAmount)
        {
            string extensionMessage = string.Empty;

            if (oldAmount.HasValue && newAmount.HasValue)
            {
                extensionMessage = $"Старое значение {oldAmount} Новое значение {newAmount}";
            }
            var template =
                _applicationOnlineTemplateChecksRepository.GetByCode("AmountChanged");
            if (template != null)
            {
                _applicationOnlineChecksRepository.Insert(new ApplicationOnlineCheck
                {
                    ApplicationOnlineId = applicationOnlineId,
                    Note = "Суммая заявки изменилась. "+extensionMessage,
                    TemplateId = template.Id,
                    CreateBy = Constants.ADMINISTRATOR_IDENTITY,
                    CreateDate = DateTime.Now,
                    Checked = false
                });
            }
        }
    }
}
