using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Utilities;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientProfileService : IClientProfileService
    {
        private readonly ClientProfileRepository _clientProfileRepository;
        private readonly IDomainService _domainService;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;
        public ClientProfileService(ClientProfileRepository clientProfileRepository,
            IClientService clientService, IDomainService domainService, ISessionContext sessionContext)
        {
            _clientProfileRepository = clientProfileRepository;
            _clientService = clientService;
            _domainService = domainService;
            _sessionContext = sessionContext;
        }

        public ClientProfile Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            return _clientProfileRepository.Get(clientId);
        }

        public ClientProfile Save(int clientId, ClientProfileDto profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            Validate(profile);
            ClientProfile profileFromDB = Get(clientId);
            using (var transaction = _clientProfileRepository.BeginTransaction()) 
            {
                if (profileFromDB == null)
                {
                    profileFromDB = new ClientProfile
                    {
                        ClientId = clientId,
                        EducationTypeId = profile.EducationTypeId,
                        TotalWorkExperienceId = profile.TotalWorkExperienceId,
                        MaritalStatusId = profile.MaritalStatusId,
                        SpouseFullname = profile.SpouseFullname,
                        SpouseIncome = profile.SpouseIncome,
                        ChildrenCount = profile.ChildrenCount,
                        AdultDependentsCount = profile.AdultDependentsCount,
                        UnderageDependentsCount = profile.UnderageDependentsCount,
                        ResidenceAddressTypeId = profile.ResidenceAddressTypeId,
                        IsWorkingNow = profile.IsWorkingNow,
                        HasAssets = profile.HasAssets,
                        AuthorId = _sessionContext.UserId
                    };

                    _clientProfileRepository.Insert(profileFromDB);
                    _clientProfileRepository.LogChanges(profileFromDB, _sessionContext.UserId, true);
                }
                else
                {
                    profileFromDB.EducationTypeId = profile.EducationTypeId;
                    profileFromDB.TotalWorkExperienceId = profile.TotalWorkExperienceId;
                    profileFromDB.MaritalStatusId = profile.MaritalStatusId;
                    profileFromDB.SpouseFullname = profile.SpouseFullname;
                    profileFromDB.SpouseIncome = profile.SpouseIncome;
                    profileFromDB.ChildrenCount = profile.ChildrenCount;
                    profileFromDB.AdultDependentsCount = profile.AdultDependentsCount;
                    profileFromDB.UnderageDependentsCount = profile.UnderageDependentsCount;
                    profileFromDB.ResidenceAddressTypeId = profile.ResidenceAddressTypeId;
                    profileFromDB.IsWorkingNow = profile.IsWorkingNow;
                    profileFromDB.HasAssets = profile.HasAssets;
                    _clientProfileRepository.Update(profileFromDB);
                    _clientProfileRepository.LogChanges(profileFromDB, _sessionContext.UserId);
                }

                transaction.Commit();
                return profileFromDB;
            }
        }

        public bool IsClientProfileFilled(int clientId)
        {
            ClientProfile profile = Get(clientId);
            if (profile == null)
                return false;

            DomainValue marriedMaritalStatusDomainValue = _domainService.GetDomainValue(Constants.MARITAL_STATUS_DOMAIN, Constants.MARRIED_MARITAL_STATUS_DOMAIN_VALUE);
            if (profile.MaritalStatusId == marriedMaritalStatusDomainValue.Id && (string.IsNullOrWhiteSpace(profile.SpouseFullname) || !profile.SpouseIncome.HasValue))
                return false;

            return profile.AdultDependentsCount.HasValue && profile.ChildrenCount.HasValue
                && profile.EducationTypeId.HasValue
                && profile.IsWorkingNow.HasValue && profile.MaritalStatusId.HasValue
                && profile.ResidenceAddressTypeId.HasValue && profile.TotalWorkExperienceId.HasValue
                && profile.UnderageDependentsCount.HasValue;
        }

        private void Validate(ClientProfileDto clientProfile)
        {
            if (clientProfile == null)
                throw new ArgumentNullException(nameof(clientProfile));

            List<string> errors = new List<string>();
            Dictionary<int, string> educationTypeDomainValueDict = _domainService.GetDomainValues(Constants.EDUCATION_TYPE_DOMAIN).ToDictionary(v => v.Id, v => v.Code);
            Dictionary<int, string> totalWorkExperienceDomainValueDict = _domainService.GetDomainValues(Constants.TOTAL_WORK_EXPERIENCE_DOMAIN).ToDictionary(v => v.Id, v => v.Code);
            Dictionary<int, string> maritalStatusDomainValueDict = _domainService.GetDomainValues(Constants.MARITAL_STATUS_DOMAIN).ToDictionary(v => v.Id, v => v.Code);
            Dictionary<int, string> residenceAddressTypeDomainValueDict = _domainService.GetDomainValues(Constants.RESIDENCE_ADDRESS_TYPE_DOMAIN).ToDictionary(v => v.Id, v => v.Code);

            if (clientProfile.EducationTypeId.HasValue
                && !educationTypeDomainValueDict.ContainsKey(clientProfile.EducationTypeId.Value))
                errors.Add($"Получено неправильное значение поля 'Тип образования заемщика'");

            if (clientProfile.TotalWorkExperienceId.HasValue
                && !totalWorkExperienceDomainValueDict.ContainsKey(clientProfile.TotalWorkExperienceId.Value))
                errors.Add($"Получено неправильное значение поля 'Общий стаж работы заемщика'");

            if (clientProfile.MaritalStatusId.HasValue)
            {
                if (!maritalStatusDomainValueDict.ContainsKey(clientProfile.MaritalStatusId.Value))
                    errors.Add($"Получено неправильное значение поля 'Семейный статус заемщика'");
                else if (maritalStatusDomainValueDict[clientProfile.MaritalStatusId.Value] == Constants.MARRIED_MARITAL_STATUS_DOMAIN_VALUE)
                {
                    if (string.IsNullOrWhiteSpace(clientProfile.SpouseFullname))
                        errors.Add($"Поле 'ФИО супруга/супруги заемщика' обязательно для заполнения");

                    if (!clientProfile.SpouseIncome.HasValue)
                        errors.Add($"Поле 'Доход супруга/супруги заемщика' обязательно для заполнения");
                    else if (clientProfile.SpouseIncome.Value < 0)
                        errors.Add($"Поле 'Доход супруга/супруги заемщика' не должно быть отрицательным числом");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(clientProfile.SpouseFullname))
                        errors.Add($"Поле 'ФИО супруга/супруги заемщика' не должно быть заполнено, так как клиент не женат/замужем");

                    if (clientProfile.SpouseIncome.HasValue)
                        errors.Add($"Поле 'Доход супруга/супруги заемщика' не должно быть заполнено, так как клиент не женат/замужем");
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(clientProfile.SpouseFullname))
                    errors.Add($"Поле 'ФИО супруга/супруги заемщика' не должно быть заполнено, так как не выбран статус семейного положения");

                if (clientProfile.SpouseIncome.HasValue)
                    errors.Add($"Поле 'Доход супруга/супруги заемщика' не должно быть заполнено, так как не выбран статус семейного положения");
            }

            if (clientProfile.ResidenceAddressTypeId.HasValue
                && !residenceAddressTypeDomainValueDict.ContainsKey(clientProfile.ResidenceAddressTypeId.Value))
                errors.Add($"Получено неправильное значение поля 'Где проживает заемщик'");

            if (clientProfile.ChildrenCount < 0)
                errors.Add($"Поле 'Количество детей заемщика' должно быть положительным числом(больше или равно 0)");

            if (clientProfile.AdultDependentsCount < 0)
                errors.Add($"Поле 'Количество взрослых иждивенцев заемщика' должно быть положительным числом(больше или равно 0)");

            if (clientProfile.UnderageDependentsCount < 0)
                errors.Add($"Поле 'Количество несовершеннолетних иждивенцев заемщика' должно быть положительным числом(больше или равно 0)");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }
    }
}
