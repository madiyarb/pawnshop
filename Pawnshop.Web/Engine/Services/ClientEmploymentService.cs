using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Utilities;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientEmploymentService : IClientEmploymentService
    {
        private readonly IDomainService _domainService;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;
        private readonly ClientEmploymentRepository _clientEmploymentRepository;
        private readonly DomainValueRepository _domainValueRepository;
        public ClientEmploymentService(IDomainService domainService, IClientService clientService, ClientEmploymentRepository clientEmploymentRepository, ISessionContext sessionContext, DomainValueRepository domainValueRepository)
        {
            _domainService = domainService;
            _clientEmploymentRepository = clientEmploymentRepository;
            _clientService = clientService;
            _sessionContext = sessionContext;
            _domainValueRepository = domainValueRepository;
        }

        public List<ClientEmployment> Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            List<ClientEmployment> employments = _clientEmploymentRepository.GetListByClientId(clientId);
            return employments;
        }

        public List<ClientEmployment> Save(int clientId, List<ClientEmploymentDto> employments)
        {
            if (employments == null)
                throw new ArgumentNullException(nameof(employments));

            Validate(employments);
            List<ClientEmployment> employmentsFromDB = Get(clientId);
            HashSet<int> employmentUniqueIds = employments.Where(e => e.Id != default).Select(e => e.Id).ToHashSet();
            Dictionary<int, ClientEmployment> employmentsFromDBDict = employmentsFromDB.ToDictionary(e => e.Id, e => e);
            if (!employmentUniqueIds.IsSubsetOf(employmentsFromDBDict.Keys))
                throw new PawnshopApplicationException($"В аргументе {nameof(employments)} присутствуют несуществующие или не принадлежащие Id мест работ данного клиента");

            var syncList = new List<ClientEmployment>();
            foreach (ClientEmploymentDto employment in employments)
            {
                ClientEmployment employmentFromDB = null;
                if (!employmentsFromDBDict.TryGetValue(employment.Id, out employmentFromDB))
                {
                    employmentFromDB = new ClientEmployment
                    {
                        ClientId = clientId,
                        IsDefault = employment.IsDefault,
                        Name = employment.Name,
                        EmployeeCountId = employment.EmployeeCountId.Value,
                        PhoneNumber = employment.PhoneNumber,
                        Address = employment.Address,
                        BusinessScopeId = employment.BusinessScopeId.Value,
                        WorkExperienceId = employment.WorkExperienceId.Value,
                        PositionName = employment.PositionName,
                        PositionTypeId = employment.PositionTypeId.Value,
                        Income = 0,
                        AuthorId = _sessionContext.UserId
                    };
                }
                else
                {
                    employmentFromDB.IsDefault = employment.IsDefault;
                    employmentFromDB.Name = employment.Name;
                    employmentFromDB.EmployeeCountId = employment.EmployeeCountId.Value;
                    employmentFromDB.PhoneNumber = employment.PhoneNumber;
                    employmentFromDB.Address = employment.Address;
                    employmentFromDB.BusinessScopeId = employment.BusinessScopeId.Value;
                    employmentFromDB.WorkExperienceId = employment.WorkExperienceId.Value;
                    employmentFromDB.PositionName = employment.PositionName;
                    employmentFromDB.PositionTypeId = employment.PositionTypeId.Value;
                    employmentFromDB.Income = 0;
                    employmentsFromDBDict.Remove(employmentFromDB.Id);
                }
                // Устанавливаем null для самозанятых
                var isSelfEmployer = _domainValueRepository.GetByCodeAndDomainCode("FREELANCER", Constants.POSITION_TYPE_DOMAIN);
                if (employmentFromDB.PositionTypeId == isSelfEmployer.Id)
                {
                    employmentFromDB.Address = null;
                    employmentFromDB.PhoneNumber = null;
                    employmentFromDB.PositionName = null;
                    employmentFromDB.Name = null;
                }

                syncList.Add(employmentFromDB);
            }

            // если есть что менять, то вызываем транзакцию
            if (syncList.Count > 0 || employmentsFromDBDict.Count > 0)
                using (var transaction = _clientEmploymentRepository.BeginTransaction())
                {
                    foreach (ClientEmployment employment in syncList)
                    {
                        if (employment.Id == default)
                        {
                            _clientEmploymentRepository.Insert(employment);
                            _clientEmploymentRepository.LogChanges(employment, _sessionContext.UserId, true);
                        }
                        else
                        {
                            _clientEmploymentRepository.Update(employment);
                            _clientEmploymentRepository.LogChanges(employment, _sessionContext.UserId);
                        }
                    }
                    // удаляем ненужные места работ
                    foreach ((int id, ClientEmployment _) in employmentsFromDBDict)
                    {
                        _clientEmploymentRepository.Delete(id);
                    }

                    transaction.Commit();
                }

            return syncList;
        }

        private void Validate(List<ClientEmploymentDto> employments)
        {
            if (employments == null)
                throw new ArgumentNullException(nameof(employments));

            var errors = new HashSet<string>();
            HashSet<int> workExperienceDomainValuesIds = _domainService.GetDomainValues(Constants.WORK_EXPERIENCE_DOMAIN).Select(v => v.Id).ToHashSet();
            HashSet<int> businessScopeDomainValuesIds = _domainService.GetDomainValues(Constants.BUSINESS_SCOPE_DOMAIN).Select(v => v.Id).ToHashSet();
            HashSet<int> employeeCountDomainValuesIds = _domainService.GetDomainValues(Constants.EMPLOYEE_COUNT_DOMAIN).Select(v => v.Id).ToHashSet();
            HashSet<int> positionTypeDomainValuesIds = _domainService.GetDomainValues(Constants.POSITION_TYPE_DOMAIN).Select(v => v.Id).ToHashSet();
            int defaultEmploymentsCount = 0;
            foreach (ClientEmploymentDto employment in employments)
            {
                if (employment == null)
                    errors.Add($"Обнаружен пустой объект места работы");

                if (!employment.WorkExperienceId.HasValue)
                    errors.Add($"Поле {nameof(employment.WorkExperienceId)} не должно быть пустым");
                else if (!workExperienceDomainValuesIds.Contains(employment.WorkExperienceId.Value))
                    errors.Add($"Поле {nameof(employment.WorkExperienceId)} имеет неправильное значение");

                if (!employment.BusinessScopeId.HasValue)
                    errors.Add($"Поле {nameof(employment.BusinessScopeId)} не должно быть пустым");
                else if (!businessScopeDomainValuesIds.Contains(employment.BusinessScopeId.Value))
                    errors.Add($"Поле {nameof(employment.BusinessScopeId)} имеет неправильное значение");

                if (!employment.EmployeeCountId.HasValue)
                    errors.Add($"Поле {nameof(employment.EmployeeCountId)} не должно быть пустым");
                else if (!employeeCountDomainValuesIds.Contains(employment.EmployeeCountId.Value))
                    errors.Add($"Поле {nameof(employment.EmployeeCountId)} имеет неправильное значение");

                if (!employment.PositionTypeId.HasValue)
                    errors.Add($"Поле {nameof(employment.PositionTypeId)} не должно быть пустым");
                else if (!positionTypeDomainValuesIds.Contains(employment.PositionTypeId.Value))
                    errors.Add($"Поле {nameof(employment.PositionTypeId)} имеет неправильное значение");

                var isSelfEmployer = _domainValueRepository.GetByCodeAndDomainCode("FREELANCER", Constants.POSITION_TYPE_DOMAIN);
                if (employment.PositionTypeId != isSelfEmployer.Id)
                {
                    if (string.IsNullOrWhiteSpace(employment.Address))
                        errors.Add($"Поле {nameof(employment.Address)} не должно быть пустым");

                    if (string.IsNullOrWhiteSpace(employment.Name))
                        errors.Add($"Поле {nameof(employment.Name)} не должно быть пустым");

                    if (string.IsNullOrWhiteSpace(employment.PhoneNumber))
                        errors.Add($"Поле {nameof(employment.PhoneNumber)} не должно быть пустым");
                    else if (!RegexUtilities.IsValidKazakhstanPhone(employment.PhoneNumber))
                        errors.Add($"Поле {nameof(employment.PhoneNumber)} не является телефонным номером");

                    if (string.IsNullOrWhiteSpace(employment.PositionName))
                        errors.Add($"Поле {nameof(employment.PositionName)} не должно быть пустым");
                }
                /*if (!employment.Income.HasValue)
                    errors.Add($"Поле {nameof(employment.Income)} не должно быть null");
                else if (employment.Income.Value <= 0)
                    errors.Add($"Поле {nameof(employment.Income)} должно быть положительным числом");*/

                if (employment.IsDefault == true)
                    defaultEmploymentsCount++;
            }

            if (employments.Count > 0)
            {
                if (defaultEmploymentsCount == 0)
                    errors.Add($"Ни одно из мест работ не отмечено как основное, наличие основного места работы обязательно");
                else if (defaultEmploymentsCount > 1)
                    errors.Add($"{defaultEmploymentsCount} мест работ были выбраны как основное, выбери одну основную работу");
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }
    }
}
