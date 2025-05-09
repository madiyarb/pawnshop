using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Services;
using Pawnshop.Services.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Tests.Web.ClientValidateServiceTest
{
    [TestClass()]
    public class ClientModelValidateServiceTest
    {
        /*
        private IClientModelValidateService _clientModelValidateService;
        private ContractRepository _contractRepository;
        private AddressTypeRepository _addressTypeRepository;

        public ClientModelValidateServiceTest()
        {
            string connectionString = "Data Source=37.18.91.106,1433;User ID=dev;Password=xFEyjcD5dkMXYic07M1W;Encrypt=False;Initial Catalog=test;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True;Connect Timeout=240;";

            IUnitOfWork unitOfWork = new UnitOfWork(connectionString);
            ClientLegalFormValidationFieldRepository _clientLegalFormValidationFieldRepository = new ClientLegalFormValidationFieldRepository(unitOfWork);
            ClientLegalFormRequiredDocumentRepository _clientLegalFormRequiredDocumentRepository = new ClientLegalFormRequiredDocumentRepository(unitOfWork);
            _contractRepository = new ContractRepository(unitOfWork);
            _addressTypeRepository = new AddressTypeRepository(unitOfWork);
            _clientModelValidateService = new ClientModelValidateService(_clientLegalFormValidationFieldRepository,
                                                                         _clientLegalFormRequiredDocumentRepository,
                                                                         _addressTypeRepository);
        }

        [TestMethod()]
        public void ValidateClientModel()
        {
            int errorCount = 1;
            try
            {
                var mockClient = GenerateClientMock();

                //Без внешней транзакции
                _clientModelValidateService.ValidateClientModel(mockClient.Object);
                
                //Внутри внешней транзакции
                using (var transaction = _contractRepository.BeginTransaction())
                {
                    _clientModelValidateService.ValidateClientModel(mockClient.Object);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                errorCount += 1;
            }

            Assert.AreEqual(1, errorCount);
        }

        private Mock<Client> GenerateClientMock()
        {
            var mock = new Mock<Client>();
            mock.Object.BeneficiaryCode = 19;
            mock.Object.CardType = CardType.Standard;            
            mock.Object.CodeWord = "KUKU";
            mock.Object.BirthDay = new DateTime(1976, 12, 26);
            mock.Object.FullName = "Бердаулет Рустем Муратович";
            mock.Object.Name = "Рустем";
            mock.Object.Surname = "Бердаулет";
            mock.Object.Patronymic = "Муратович";
            mock.Object.IdentityNumber = "761226300148";
            mock.Object.IsResident = true;
            mock.Object.LegalFormId = 16;
            mock.Object.LegalForm = new ClientLegalForm() { Code = "INDIVIDUAL", Id = 16 , HasBirthDayValidation = true, HasIINValidation  = true, IsIndividual = true};
            mock.Object.Id = 0; //новый клиент
            mock.Object.Citizenship = new Country() { CBId = 110, Code = "KAZ", Id = 118 };
            mock.Object.CitizenshipId = 118;            
            mock.Object.Documents = new List<ClientDocument>() {
                                                         new ClientDocument() {
                                                            BirthPlace = "Г.АЛМАТЫ",
                                                            Date = new DateTime(2021,07,12),
                                                            DateExpire = new DateTime(2022,07,12),
                                                            Number = "N1254578",
                                                            DocumentType = new ClientDocumentType()
                                                            {
                                                                BirthPlacePlaceholder="Место рождения",
                                                                CBId=6,
                                                                Code="PASSPORTKZ",
                                                                DateExpirePlaceholder = "Действителен до",
                                                                DatePlaceholder = "Когда выдан",
                                                                Disabled = false,
                                                                HasSeries = true,
                                                                Id = 2,
                                                                IsIndividual = true,
                                                                Name = "Паспорт РК",
                                                                NameKaz = "ҚР төлқұжаты",
                                                                NumberMask = "^N\\\\d{7,8}$",
                                                                NumberMaskError = "Поле номер документа должно содержать N и 7-8 цифр",
                                                                NumberPlaceholder = "Номер документа",
                                                                ProviderPlaceholder = "Орган выдачи",
                                                                SeriesPlaceholder = "Серия паспорта"
                                                            },
                                                            Provider = new ClientDocumentProvider()
                                                            {
                                                                Abbreviature = "МВД РК",
                                                                AbbreviatureKaz = "ҚРІІМ",
                                                                Code = "MIA_RK",
                                                                DeleteDate = null,
                                                                Name = "МВД РК",
                                                                Id = 2
                                                            },
                                                            ProviderId = 2,
                                                            Series = "N1234567",
                                                            TypeId = 2
                                                         }
            };
            mock.Object.Addresses = new List<ClientAddress>()
            {
                new ClientAddress()
                {
                    AddressType = new AddressType()
                    {
                        CBId = 3,
                        Code = "WORKINGPLACE",
                        DeleteDate = null,
                        Id = 2,
                        IsIndividual = true,                        
                        Name = "Место работы"
                    },
                    AddressTypeId = 2,
                    ATEId = 138439,
                    BuildingNumber = "25",
                    Country = new Country()
                    {
                        CBId = 110,
                        Code = "KAZ",
                        DeleteDate = null,
                        Id = 118
                    },
                    CountryId = 118,
                    GeonimId = 288928,
                    IsActual = true,
                    RoomNumber = ""
                },
                new ClientAddress()
                {
                    AddressType = new AddressType()
                    {
                        CBId = 3,
                        Code = "RESIDENCE",
                        DeleteDate = null,
                        Id = 5,
                        IsIndividual = true,
                        Name = "Постоянное место жительства"
                    },
                    AddressTypeId = 5,
                    ATEId = 138439,
                    BuildingNumber = "25",
                    Country = new Country()
                    {
                        CBId = 110,
                        Code = "KAZ",
                        DeleteDate = null,
                        Id = 118
                    },
                    CountryId = 118,
                    GeonimId = 288928,
                    IsActual = true,
                    RoomNumber = ""
                }
            };
            mock.Object.IsMale = true;
            mock.Object.IsPolitician = false;
            mock.Object.IsResident = true;
            return mock;
        }
    */
    }
}