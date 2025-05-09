using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Helpers;
using Pawnshop.Data.Models.ClientAddressLogItems;
using Pawnshop.Data.Models.ClientDocumentLogItems;
using Pawnshop.Data.Models.ClientLogItems;
using Pawnshop.Data.Models.ClientRequisiteLogItems;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.Views;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.PrintFormInfo;

namespace Pawnshop.Data.Access
{
    public class ClientRepository : RepositoryBase, IRepository<Client>
    {
        private readonly PensionAgesRepository _pensionAgesRepository;
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClientRepository(IUnitOfWork unitOfWork,
            PensionAgesRepository pensionAgesRepository,
            IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _service = service;
            _pensionAgesRepository = pensionAgesRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Insert(Client entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                var query = @"
INSERT INTO Clients 
( CardType, CardNumber, IdentityNumber, FullName, MobilePhone, StaticPhone, Email, Note, UserId, Surname, Name, Patronymic, MaidenName, IsMale, BirthDay, IsResident, IsPolitician,
IdentityNumberIsValid, BankIdentifierCode, BeneficiaryCode, BankCode, TradeName, Abbreviation,
LegalFormId, ChiefId, CitizenshipId, CodeWord, AuthorId, CreateDate, ReceivesASP, SubTypeId, DocumentNumber, DocumentDate, ChiefName, IsSeller, PartnerCode)
VALUES ( @CardType, @CardNumber, @IdentityNumber, @FullName, @MobilePhone, @StaticPhone, @Email, @Note,
@UserId, @Surname, @Name, @Patronymic, @MaidenName, @IsMale, @BirthDay, @IsResident, @IsPolitician,
@IdentityNumberIsValid, @BankIdentifierCode, @BeneficiaryCode, @BankCode, @TradeName, @Abbreviation,
@LegalFormId, @ChiefId, @CitizenshipId, @CodeWord, @AuthorId, @CreateDate, @ReceivesASP, @SubTypeId, @DocumentNumber, @DocumentDate, @ChiefName, @IsSeller, @PartnerCode)
SELECT SCOPE_IDENTITY()";

                try
                {
                    entity.Id = UnitOfWork.Session.ExecuteScalar<int>(query, entity, UnitOfWork.Transaction);

                }
                catch (SqlException e)
                {
                    if (e.Number == 2627)
                    {
                        throw new PawnshopApplicationException($"Поле ИИН/БИН должно быть уникальным ({entity.IdentityNumber})");
                    }
                    throw new PawnshopApplicationException(e.Message);
                }

                _service.LogClientData(new ClientLogData(entity),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
                foreach (var requisite in entity.Requisites)
                {
                    requisite.ClientId = entity.Id;
                    InsertRequisite(requisite);
                }

                foreach (var document in entity.Documents)
                {
                    document.ClientId = entity.Id;
                    document.Number = document.DocumentType.Code == Constants.CHARTER ? entity.Id.ToString() : document.Number;
                    InsertDocument(document);
                }

                foreach (var address in entity.Addresses)
                {
                    address.ClientId = entity.Id;
                    InsertAddress(address);
                }

                transaction.Commit();
            }

        }

        private void InsertRequisite(ClientRequisite requisite)
        {
            try
            {
                requisite.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
INSERT INTO ClientRequisites(ClientId, RequisiteTypeId, BankId, Value, Note, AuthorId, CreateDate, IsDefault, CardExpiryDate, CardHolderName)
VALUES
(@ClientId, @RequisiteTypeId, @BankId, @Value, @Note, @AuthorId, @CreateDate, @IsDefault, @CardExpiryDate, @CardHolderName)
SELECT SCOPE_IDENTITY()
", requisite, UnitOfWork.Transaction);

                _service.LogClientRequisiteData(new ClientRequisiteLogData(requisite),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException($"Реквизит должен быть уникальным ({requisite.Number})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        private void UpdateRequisite(ClientRequisite requisite)
        {
            try
            {
                UnitOfWork.Session.Execute($@"
UPDATE ClientRequisites
SET RequisiteTypeId = @RequisiteTypeId, BankId = @BankId, Value = @Value, Note = @Note, IsDefault = @IsDefault,
CardExpiryDate = @CardExpiryDate, CardHolderName = @CardHolderName
WHERE Id=@Id
", requisite, UnitOfWork.Transaction);

                _service.LogClientRequisiteData(new ClientRequisiteLogData(requisite),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException($"Реквизит должен быть уникальным ({requisite.Number})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        private void InsertDocument(ClientDocument document)
        {
            try
            {
                document.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
INSERT INTO ClientDocuments(ClientId, TypeId, Number, Series, ProviderId, ProviderName, Date, DateExpire, AuthorId, CreateDate, BirthPlace)
VALUES
(@ClientId, @TypeId, @Number, @Series, @ProviderId, @ProviderName, @Date, @DateExpire, @AuthorId, @CreateDate, @BirthPlace)
SELECT SCOPE_IDENTITY()
", document, UnitOfWork.Transaction);

                _service.LogClientDocumentData(new ClientDocumentLogData(document),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    if (document.DocumentType.Code == Constants.CHARTER)
                        throw new PawnshopApplicationException($"Клиент не может иметь больше одного устава");

                    throw new PawnshopApplicationException($"Документ должен быть уникальным ({document.Number})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        public void InsertAddress(ClientAddress address)
        {
            try
            {
                if (address.ATEId.HasValue && address.GeonimId.HasValue)
                {
                    address.FullPathRus = UnitOfWork.Session.QuerySingleOrDefault<string>(
                        $@"SELECT dbo.GetShortNameOfAddressFullPath(@ATEId, @GeonimId, @BuildingNumber, @RoomNumber)", address,
                        UnitOfWork.Transaction);

                    address.FullPathKaz = UnitOfWork.Session.QuerySingleOrDefault<string>(
                        $@"SELECT dbo.GetShortNameOfAddressFullPathAlt(@ATEId, @GeonimId, @BuildingNumber, @RoomNumber)", address,
                        UnitOfWork.Transaction);
                }

                address.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
INSERT INTO ClientAddresses
(ClientId, AddressTypeId, CountryId, ATEId, GeonimId, BuildingNumber, RoomNumber,
FullPathRus, FullPathKaz, CreateDate, AuthorId, IsActual, Note)
VALUES
(@ClientId, @AddressTypeId, @CountryId, @ATEId, @GeonimId, @BuildingNumber, @RoomNumber,
@FullPathRus, @FullPathKaz, @CreateDate, @AuthorId, @IsActual, @Note)
SELECT SCOPE_IDENTITY()
", address, UnitOfWork.Transaction);

                _service.LogClientAddressData(new ClientAddressLogData(address),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException($"Адресс должен быть уникальным ({address.FullPathRus})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        private void InsertFile(FileRow file, Client client, ClientDocument document)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            UnitOfWork.Session.Execute($@"
IF((SELECT COUNT(*) FROM ClientFileRows WHERE ClientId = @clientId AND FileRowId = fileRowId AND (DocumentId IS NULL OR documentId = DocumentId)) = 0)
BEGIN
    INSERT INTO ClientFileRows (ClientId, FileRowId, DocumentId)
    SELECT @clientId, @fileRowId, @documentId
END", new { fileRowId = file.Id, clientId = client.Id, documentId = document?.Id }, UnitOfWork.Transaction);
        }

        private void UpdateDocument(ClientDocument document)
        {
            try
            {
                UnitOfWork.Session.Execute($@"
UPDATE ClientDocuments
SET TypeId = @TypeId, Number = @Number, Series = @Series, ProviderId = @ProviderId, ProviderName = @ProviderName, Date = @Date, DateExpire = @DateExpire, BirthPlace = @BirthPlace
WHERE Id=@Id
", document, UnitOfWork.Transaction);

                _service.LogClientDocumentData(new ClientDocumentLogData(document),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    if (document.DocumentType.Code == Constants.CHARTER)
                        throw new PawnshopApplicationException($"Клиент не может иметь больше одного устава");

                    throw new PawnshopApplicationException($"Документ должен быть уникальным ({document.Number})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        public void UpdateAddress(ClientAddress address)
        {
            try
            {
                if (address.ATEId.HasValue && address.GeonimId.HasValue)
                {
                    address.FullPathRus = UnitOfWork.Session.QuerySingleOrDefault<string>(
                        $@"SELECT dbo.GetShortNameOfAddressFullPath(@ATEId, @GeonimId, @BuildingNumber, @RoomNumber)", address,
                        UnitOfWork.Transaction);

                    address.FullPathKaz = UnitOfWork.Session.QuerySingleOrDefault<string>(
                        $@"SELECT dbo.GetShortNameOfAddressFullPathAlt(@ATEId, @GeonimId, @BuildingNumber, @RoomNumber)", address,
                        UnitOfWork.Transaction);
                }

                UnitOfWork.Session.Execute($@"
UPDATE ClientAddresses
SET AddressTypeId = @AddressTypeId, CountryId = @CountryId, ATEId = @ATEId, GeonimId = @GeonimId, BuildingNumber = @BuildingNumber, RoomNumber = @RoomNumber, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, IsActual = @IsActual, Note = @Note
WHERE Id=@Id
", address, UnitOfWork.Transaction);

                _service.LogClientAddressData(new ClientAddressLogData(address),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException($"Адресс должен быть уникальным ({address.FullPathRus})");
                }
                throw new PawnshopApplicationException(e.Message);
            }
        }

        public void Update(Client entity)
        {
            using (var transaction = BeginTransaction())
            {
                var query = @"
UPDATE Clients
SET CardType = @CardType, CardNumber = @CardNumber, IdentityNumber = @IdentityNumber, FullName = @FullName,
MobilePhone = @MobilePhone, StaticPhone = @StaticPhone, Email = @Email, Note = @Note, UserId = @UserId, Surname = @Surname, Name = @Name, Patronymic = @Patronymic, MaidenName = @MaidenName,
IsMale = @IsMale, BirthDay = @BirthDay, IsResident = @IsResident, IsPolitician = @IsPolitician,
IdentityNumberIsValid = @IdentityNumberIsValid, BankIdentifierCode = @BankIdentifierCode,
BeneficiaryCode = @BeneficiaryCode, BankCode = @BankCode, TradeName = @TradeName, Abbreviation = @Abbreviation,
LegalFormId = @LegalFormId, ChiefId = @ChiefId, CitizenshipId = @CitizenshipId, CodeWord = @CodeWord, ReceivesASP = @ReceivesASP, SubTypeId = @SubTypeId,
DocumentNumber = @DocumentNumber, DocumentDate = @DocumentDate, ChiefName = @ChiefName, IsSeller = @IsSeller, PartnerCode = @PartnerCode
WHERE Id = @Id";

                UnitOfWork.Session.Execute(query, entity, UnitOfWork.Transaction);

                var currentClient = Get(entity.Id);

                _service.LogClientData(new ClientLogData(entity),
                    _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);

                var currentRequisites = currentClient.Requisites;

                var toDeleteRequisites = currentRequisites
                    .Where(w => !entity.Requisites.Any(x => w.Id == x.Id));

                foreach (var requisite in toDeleteRequisites)
                {
                    requisite.DeleteDate = DateTime.Now;
                    UnitOfWork.Session.Execute(@"
UPDATE ClientRequisites
SET DeleteDate = @DeleteDate
WHERE Id = @id", requisite, UnitOfWork.Transaction);
                }

                foreach (var requisite in entity.Requisites)
                {
                    if (requisite.Id > 0)
                    {
                        if (!requisite.Equals(currentRequisites.FirstOrDefault(req => req.Id == requisite.Id)))
                        {
                            UpdateRequisite(requisite);
                        }
                    }
                    else
                    {
                        requisite.ClientId = entity.Id;
                        InsertRequisite(requisite);
                    }
                }

                var currentAddresses = currentClient.Addresses;

                var toDeleteAddresses = currentAddresses
                    .Where(w => !entity.Addresses.Any(x => w.Id == x.Id));

                foreach (var address in toDeleteAddresses)
                {
                    address.DeleteDate = DateTime.Now;
                    address.IsActual = false;
                    UnitOfWork.Session.Execute(@"
                        UPDATE ClientAddresses
                        SET DeleteDate = @DeleteDate, IsActual = @IsActual
                        WHERE Id = @id", address, UnitOfWork.Transaction);
                }

                var currentDocuments = currentClient.Documents;

                foreach (var document in entity.Documents)
                {
                    if (document.Id > 0)
                    {
                        if (!document.Equals(currentDocuments.FirstOrDefault(doc => doc.Id == document.Id)))
                        {
                            UpdateDocument(document);
                        }
                    }
                    else
                    {
                        document.ClientId = entity.Id;
                        document.Number = document.DocumentType.Code == Constants.CHARTER ? entity.Id.ToString() : document.Number;
                        InsertDocument(document);
                    }
                    foreach (var file in document.Files)
                    {
                        InsertFile(file, entity, document);
                    }
                }

                var toDeleteDocuments = currentDocuments.Where(document => !entity.Documents.Any(newDocument => document.Id == newDocument.Id));

                foreach (var document in toDeleteDocuments)
                {
                    DeleteClientDocument(document.Id);
                }

                foreach (var address in entity.Addresses)
                {
                    if (address.Id > 0)
                    {
                        if (!address.Equals(currentAddresses
                                .FirstOrDefault(addr => addr.Id == address.Id)))
                        {
                            UpdateAddress(address);
                        }
                    }
                    else
                    {
                        address.ClientId = entity.Id;
                        InsertAddress(address);
                    }
                }

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Clients SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Client GetOnlyClient(int id)
        {
            return UnitOfWork.Session.Query<Client>(@"SELECT c.* from Clients c WHERE c.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<Client> GetOnlyClientAsync(int id)
        {
            var client = await UnitOfWork.Session.QueryFirstOrDefaultAsync<Client>(@"SELECT c.* from Clients c WHERE c.Id = @id", new { id }, UnitOfWork.Transaction);

            if (client == null)
                throw new PawnshopApplicationException("Клиент не найден");

            return client;
        }

        public Client GetClientWithLegalForm(int id)
        {
            return UnitOfWork.Session.Query<Client, ClientLegalForm, Client>(@"
                SELECT c.*, lf.*
                  FROM Clients c
                    JOIN ClientLegalForms lf ON lf.Id = c.LegalFormId
                    WHERE c.Id = @id",
                (c, lf) =>
                {
                    c.LegalForm = lf;
                    return c;
                }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Client Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Client, ClientLegalForm, Country, ClientContact, Client>(@"
SELECT c.*, lf.*, co.*, cc.*
  FROM Clients c
LEFT JOIN ClientLegalForms lf ON lf.Id = c.LegalFormId
LEFT JOIN Countries co ON co.id = c.CitizenshipId
LEFT JOIN ClientContacts cc ON cc.clientId = c.id AND cc.DeleteDate is null AND cc.isDefault = 1
WHERE c.Id = @id", (c, lf, co, cc) =>
            {
                c.LegalForm = lf;
                c.Citizenship = co;
                if (cc != null)
                    c.MobilePhone = cc.Address;
                return c;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                throw new PawnshopApplicationException("Клиент не найден");

            entity.Files = UnitOfWork.Session.Query<FileRow>(@"
SELECT FileRows.*
  FROM ClientFileRows
  JOIN FileRows ON ClientFileRows.FileRowId = FileRows.Id
 WHERE ClientFileRows.ClientId = @id", new { id }, UnitOfWork.Transaction).ToList();

            entity.Requisites = UnitOfWork.Session.Query<ClientRequisite>(@"
SELECT * FROM ClientRequisites WHERE DeleteDate IS NULL AND ClientId = @id", new { id }, UnitOfWork.Transaction).ToList();

            entity.ClientSVG = UnitOfWork.Session.Query<ClientSociallyVulnerableGroup>(@"
SELECT *
  FROM ClientSociallyVulnerableGroups
 WHERE ClientId = @id", new { id }, UnitOfWork.Transaction).ToList();

            entity.Documents = UnitOfWork.Session.Query<ClientDocument, ClientDocumentType, ClientDocumentProvider, User, ClientDocument>(@"
SELECT d.*, t.*, p.*, u.* FROM ClientDocuments d
LEFT JOIN ClientDocumentTypes t ON t.Id=d.TypeId
LEFT JOIN ClientDocumentProviders p ON p.Id=d.ProviderId
LEFT JOIN Users u ON d.AuthorId=u.Id
WHERE d.ClientId=@id AND d.DeleteDate IS NULL", (d, t, p, u) =>
            {
                d.DocumentType = t;
                d.Provider = p;
                d.Author = u;
                return d;
            }, new { id }, UnitOfWork.Transaction).ToList();

            entity.Addresses = UnitOfWork.Session.Query<ClientAddress, AddressType, Country, AddressATE, AddressGeonim, ClientAddress>(@"
SELECT ca.*, t.*, c.*, ate.*, geo.*
FROM ClientAddresses ca
JOIN AddressTypes t ON ca.AddressTypeId = t.Id
LEFT JOIN Countries c ON ca.CountryId = c.Id
LEFT JOIN AddressATEs ate ON ate.Id = ca.ATEId
LEFT JOIN AddressGeonims geo ON geo.Id = ca.GeonimId
WHERE ca.ClientId=@id", (ca, t, c, ate, geo) =>
            {
                ca.AddressType = t;
                ca.Country = c;
                ca.ATE = ate;
                ca.Geonim = geo;
                return ca;
            }, new { id }, UnitOfWork.Transaction).ToList();

            entity.Addresses.ForEach(x => x.Country = UnitOfWork.Session.QueryFirstOrDefault<Country>(@"
SELECT *
FROM Countries
WHERE Id = @CountryId", new { x.CountryId }, UnitOfWork.Transaction));

            if (entity.ChiefId.HasValue)
            {
                entity.Chief = Get(entity.ChiefId.Value);
            }

            if (!entity.BirthDay.HasValue || !entity.IsMale.HasValue)
                entity.IsPensioner = false;
            else
            {
                if (entity.IsMale.HasValue && entity.IsMale.Value)
                {
                    double malePensionAge = _pensionAgesRepository.GetMaleAge();
                    if (malePensionAge != 0 &&
                        entity.BirthDay.Value.AddYears((int)malePensionAge).AddMonths((int)(Constants.MONTHS_IN_YEAR * (malePensionAge - (int)malePensionAge))) <= DateTime.Today)
                    {
                        entity.IsPensioner = true;
                    }
                }
                else
                {
                    double femalePensionAge = _pensionAgesRepository.GetFemaleAge();
                    if (femalePensionAge != 0 &&
                        entity.BirthDay.Value.AddYears((int)femalePensionAge).AddMonths((int)(Constants.MONTHS_IN_YEAR * (femalePensionAge - (int)femalePensionAge))) <= DateTime.Today)
                    {
                        entity.IsPensioner = true;
                    }
                }
            }
            return entity;
        }

        public async Task<Client> GetByIdentityNumberAsync(string iin)
        {
            var parameters = new { IdentityNumber = iin };
            var sqlQuery = @"
                SELECT * FROM Clients
                WHERE IdentityNumber = @IdentityNumber";

            var result =
                await UnitOfWork.Session
                    .QueryFirstOrDefaultAsync<Client>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public Client Find(object query)
        {
            throw new NotImplementedException();
        }

        public Client FindByIdentityNumber(string identityNumber)
        {
            var clientId = UnitOfWork.Session.ExecuteScalar<int?>($@"
SELECT TOP 1 id FROM Clients WHERE DeleteDate IS NULL AND IdentityNumberBin LIKE N'%{identityNumber}%'", UnitOfWork.Transaction);

            if (!clientId.HasValue) return null;

            return Get(clientId.Value);
        }

        public List<Client> FindAllByIdentityNumberForProcessing(string identityNumber)
        {
            var clientIds = UnitOfWork.Session.Query<int>($@"
SELECT Id FROM Clients WHERE DeleteDate IS NULL AND IdentityNumberBin LIKE N'%{identityNumber}%'", UnitOfWork.Transaction).ToList();
            if (!clientIds.Any()) return null;
            var clients = new List<Client>();
            foreach (var clientId in clientIds)
            {
                clients.Add(Get(clientId));
            }
            return clients;
        }

        public Client FindByIdentityNumberAndLegalFormCode(string identityNumber, string legalFormCode)
        {
            var clientId = UnitOfWork.Session.ExecuteScalar<int?>($@"
                SELECT cl.id 
                    FROM Clients cl 
                    JOIN ClientLegalForms clf ON clf.Id = cl.LegalFormId
                        WHERE cl.DeleteDate IS NULL 
                        AND clf.DeleteDate IS NULL
                        AND cl.IdentityNumberBin LIKE N'%{identityNumber}%'
                        AND clf.Code = '{legalFormCode}'
                    ", UnitOfWork.Transaction);

            if (!clientId.HasValue) return null;

            return Get(clientId.Value);
        }

        public List<Client> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isBank = query?.Val<bool?>("IsBank");
            var isIndividual = query?.Val<bool?>("IsIndividual");

            var pre = "c.DeleteDate IS NULL AND cc.DeleteDate IS NULL";
            pre += isBank.HasValue ? " AND c.BankIdentifierCode IS NOT NULL AND c.BankCode IS NOT NULL " : string.Empty;
            pre += isIndividual.HasValue ? " AND ClientLegalForms.IsIndividual = @isIndividual " : string.Empty;

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "FullName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            string conditionText = "";
            string likeText = "";
            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                conditionText = @"declare @filter2 NVARCHAR(4000) = upper(@filter);
                                declare @FilteredClients as Table(ClientId int not null);
                                insert into @FilteredClients(ClientId)
                                SELECT c.Id
                                FROM Clients c
                                WHERE c.FullNameBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
                                OR c.IdentityNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                OR c.MobilePhoneBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                union all
                                select cd.ClientId 
                                from ClientDocuments cd 
                                where cd.NumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                union all
                                select con.ClientId 
                                from Contracts con 
                                where con.ContractNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%';";
                likeText = @"AND exists(select* from @FilteredClients fc where fc.ClientId = c.Id)";
            }

            var list = UnitOfWork.Session.Query<Client, ClientLegalForm, Country, ClientContact, Client>($@"
{conditionText}
SELECT c.*, ClientLegalForms.*, Countries.*, cc.*
FROM Clients c
LEFT JOIN ClientLegalForms ON c.LegalFormId = ClientLegalForms.Id
LEFT JOIN Countries ON c.CitizenshipId = Countries.Id
LEFT JOIN ClientContacts cc ON cc.ClientId = c.Id AND cc.IsDefault = 1
WHERE {pre} 
{likeText}
{order} {page}", (c, lf, co, cc) =>
        {
            c.LegalForm = lf;
            c.Citizenship = co;
            if (string.IsNullOrWhiteSpace(c.MobilePhone) && cc != null)
                c.MobilePhone = cc.Address;
            return c;
        }, new
        {
            listQuery.Page?.Offset,
            listQuery.Page?.Limit,
            listQuery.Filter,
            isIndividual
        }, UnitOfWork.Transaction, commandTimeout: 90).ToList();

            return list;
        }

        public List<Client> ListEstimationCompanies(ListQuery listQuery, int subTypeId)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "c.DeleteDate IS NULL AND c.SubTypeId = @subTypeId";
            var condition = listQuery.Like(pre, "c.FullName", "c.IdentityNumber", "clf.name", "c.name", "c.DocumentNumber", "c.DocumentDate", "u.FullName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "c.FullName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            var list = UnitOfWork.Session.Query<Client, ClientDocument, User, ClientLegalForm, Client>($@"
                SELECT c.*, cd.*, u.*, clf.*
                   FROM Clients c
                  LEFT JOIN ClientDocuments cd on cd.ClientId = c.Id
                  LEFT JOIN Users u on u.Id = c.authorId
                  LEFT JOIN ClientLegalForms clf on clf.Id = c.LegalFormId
                {condition} {order} {page}", (c, cd, u, clf) =>
            {
                c.Documents.Add(cd);
                c.Author = u;
                c.LegalForm = clf;
                return c;
            },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    subTypeId
                }, UnitOfWork.Transaction, commandTimeout: 90).ToList();

            return list;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var isBank = query?.Val<bool?>("IsBank");
            var isIndividual = query?.Val<bool?>("IsIndividual");

            var pre = "c.DeleteDate IS NULL";
            pre += isBank.HasValue ? " AND c.BankIdentifierCode IS NOT NULL AND c.BankCode IS NOT NULL " : string.Empty;
            pre += isIndividual.HasValue ? " AND ClientLegalForms.IsIndividual = @isIndividual " : string.Empty;

            string conditionText = "";
            string likeText = "";
            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                conditionText = @"declare @filter2 NVARCHAR(4000) = upper(@filter);
                                declare @FilteredClients as Table(ClientId int not null);
                                insert into @FilteredClients(ClientId)
                                SELECT c.Id
                                FROM Clients c
                                WHERE c.FullNameBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%' 
                                OR c.IdentityNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                OR c.MobilePhoneBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                union all
                                select cd.ClientId 
                                from ClientDocuments cd 
                                where cd.NumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%'
                                union all
                                select con.ClientId 
                                from Contracts con 
                                where con.ContractNumberBin LIKE N'%' + @filter2 COLLATE Latin1_General_100_BIN2 + N'%';";
                likeText = @"AND exists(select* from @FilteredClients fc where fc.ClientId = c.Id)";
            }

            return UnitOfWork.Session.ExecuteScalar<int>($@"
{conditionText}
SELECT COUNT(c.Id)
FROM Clients c
LEFT JOIN ClientLegalForms ON c.LegalFormId = ClientLegalForms.Id
WHERE {pre}
{likeText}
", new
            {
                listQuery.Filter,
                isIndividual
            });
        }

        public int CountEstimationCompanies(ListQuery listQuery, int subTypeId)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "c.DeleteDate IS NULL AND c.SubTypeId = @subTypeId";
            var condition = listQuery.Like(pre, "c.FullName", "c.IdentityNumber", "clf.name", "c.name", "c.DocumentNumber", "c.DocumentDate", "u.FullName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(DISTINCT c.Id)
  FROM Clients c
LEFT JOIN ClientDocuments cd on cd.ClientId = c.Id
LEFT JOIN Users u on u.Id = c.authorId
LEFT JOIN ClientLegalForms clf on clf.Id = c.LegalFormId
{condition}",
          new
          {
              listQuery.Page?.Offset,
              listQuery.Page?.Limit,
              listQuery.Filter,
              subTypeId
          });
        }

        public int RelationCount(int clientId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(q)
FROM (
SELECT COUNT(*) as q
FROM Contracts
WHERE ClientId = @clientId
UNION
SELECT COUNT(*) as q
FROM CashOrders
WHERE ClientId = @clientId) as t", new { clientId });
        }

        public int RelationCountEstCompany(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(cl.Id) FROM Contracts c
					INNER JOIN contractPositions cp ON cp.contractId = c.id
					INNER JOIN PositionEstimates pe ON pe.PositionId = cp.PositionId
					INNER JOIN Clients cl ON cl.Id = pe.CompanyId
					WHERE c.Status IN (0, 30, 40, 24)
						AND pe.DeleteDate is null
						AND cl.DeleteDate is null
						AND cp.DeleteDate is null
						AND c.DeleteDate is null
						AND cl.Id = {id}", UnitOfWork.Transaction);
        }

        public void UpdateCrmInfo(int id, int crmId)
        {
            using (var transaction = BeginTransaction())
            {
                var query = @"
UPDATE Clients
SET CrmId=@CrmId
WHERE Id = @Id";

                UnitOfWork.Session.Execute(query, new { id, crmId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        /// <summary>
        /// Получить список клиентов у кого день рождения на дату
        /// В список не попадают если у клиента есть договор со статусом
        /// На "Реализаций" или "Реализован"
        /// </summary>
        /// <param name="month">Месяц</param>
        /// <param name="paymentDays">День</param>
        /// <returns></returns>
        public IEnumerable<BaseNotificationModel> GetByBirthday(int month, int day)
        {
            return UnitOfWork.Session.Query<BaseNotificationModel>($@"SELECT 
                    cl.Id ClientId, 
                    ct.Id ContractId, 
                    ct.branchId 
	            FROM Clients as cl
	            INNER JOIN (SELECT 
					            ct.ClientId, 
					            MAX(ct.Id) as LastId 
				            FROM Contracts ct WHERE 
				            ct.DeleteDate IS NOT NULL 
				            GROUP BY ct.ClientId ) lc ON cl.Id = lc.ClientId
	            INNER JOIN Contracts ct ON ct.Id = lc.LastId
	            LEFT JOIN Contracts c ON c.ClientId = cl.Id And c.Status IN (50, 60)
	            INNER JOIN ClientLegalForms clf ON clf.Id = cl.LegalFormId
	            WHERE cl.DeleteDate IS NULL 
		            AND MONTH(cl.BirthDay) = @month 
		            AND DAY(cl.BirthDay) = @day 
		            AND cl.BlackListReasonId IS NULL 
		            AND clf.IsIndividual = 1
		            AND (c.Id IS NULL OR c.DeleteDate IS NOT NULL)",
                            new { month, day }, UnitOfWork.Transaction);
        }

        public ClientDocument GetClientDocumentByType(int clientId, string documentTypeCode)
        {
            return UnitOfWork.Session.Query<ClientDocument, ClientDocumentType, ClientDocument>(@"
                SELECT d.*, t.* FROM ClientDocuments d
                LEFT JOIN ClientDocumentTypes t ON t.Id=d.TypeId
                WHERE d.ClientId=@clientId AND d.DeleteDate IS NULL AND t.Code = @documentTypeCode", (d, t) =>
            {
                d.DocumentType = t;
                return d;
            }, new { clientId, documentTypeCode }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientDocument GetClientDocumentByNumber(string documentNumber)
        {
            return UnitOfWork.Session.Query<ClientDocument, ClientDocumentType, ClientDocument>(@"
                SELECT d.*, t.* FROM ClientDocuments d
                LEFT JOIN ClientDocumentTypes t ON t.Id=d.TypeId
                WHERE d.Number=@documentNumber AND d.DeleteDate IS NULL", (d, t) =>
            {
                d.DocumentType = t;
                return d;
            }, new { documentNumber }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientAddress> GetClientAddresses(int clientId)
        {
            return UnitOfWork.Session.Query<ClientAddress, AddressType, Country, AddressATE, AddressGeonim, ClientAddress>(@"
SELECT ca.*, t.*, c.*, ate.*, geo.*
FROM ClientAddresses ca
JOIN AddressTypes t ON ca.AddressTypeId = t.Id
LEFT JOIN Countries c ON ca.CountryId = c.Id
LEFT JOIN AddressATEs ate ON ate.Id = ca.ATEId
LEFT JOIN AddressGeonims geo ON geo.Id = ca.GeonimId
WHERE ca.ClientId=@clientId", (ca, t, c, ate, geo) =>
            {
                ca.AddressType = t;
                ca.Country = c;
                ca.ATE = ate;
                ca.Geonim = geo;
                return ca;
            }, new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<ClientAddress>> GetClientAddressesAsync(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<ClientAddress, AddressType, Country, AddressATE, AddressGeonim, ClientAddress>(@"
SELECT ca.*, t.*, c.*, ate.*, geo.*
FROM ClientAddresses ca
JOIN AddressTypes t ON ca.AddressTypeId = t.Id
LEFT JOIN Countries c ON ca.CountryId = c.Id
LEFT JOIN AddressATEs ate ON ate.Id = ca.ATEId
LEFT JOIN AddressGeonims geo ON geo.Id = ca.GeonimId
WHERE ca.ClientId=@clientId", (ca, t, c, ate, geo) =>
            {
                ca.AddressType = t;
                ca.Country = c;
                ca.ATE = ate;
                ca.Geonim = geo;
                return ca;
            }, new { clientId }, UnitOfWork.Transaction);
        }

        public List<ClientDocument> GetClientDocumentsByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientDocument, ClientDocumentType, ClientDocumentProvider, User, ClientDocument>(@"
SELECT d.*, t.*, p.*, u.* FROM ClientDocuments d
LEFT JOIN ClientDocumentTypes t ON t.Id=d.TypeId
LEFT JOIN ClientDocumentProviders p ON p.Id=d.ProviderId
LEFT JOIN Users u ON d.AuthorId=u.Id
WHERE d.ClientId=@clientId AND d.DeleteDate IS NULL", (d, t, p, u) =>
            {
                d.DocumentType = t;
                d.Provider = p;
                d.Author = u;
                return d;
            }, new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public void DeleteClientDocument(int documentId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE ClientDocuments SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @documentId", new { documentId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineClientView GetClientForApplicationOnlineView(int clientId)
        {
            var view = UnitOfWork.Session.Query<ApplicationOnlineClientView>(@"SELECT Name,
       Surname,
       Patronymic,
       ReceivesASP,
       BirthDay,
       PartnerCode,
       IdentityNumber,
       IsMale
  FROM Clients
 WHERE Id = @clientId",
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();

            #region fill email & phone & additional phone
            view.EMail = UnitOfWork.Session.QueryFirstOrDefault<string>(@"SELECT TOP 1 Address
  FROM ClientContacts cc
  JOIN DomainValues dv ON dv.Id = cc.ContactTypeId
 WHERE cc.DeleteDate IS NULL
   AND cc.ClientId = @clientId
   AND dv.Code = @domainCode
 ORDER BY cc.CreateDate DESC",
                new { clientId, domainCode = Constants.DOMAIN_VALUE_EMAIL_CODE }, UnitOfWork.Transaction);

            view.MobilePhone = UnitOfWork.Session.QueryFirstOrDefault<string>(@"SELECT TOP 1 Address
  FROM ClientContacts cc
  JOIN DomainValues dv ON dv.Id = cc.ContactTypeId
 WHERE cc.DeleteDate IS NULL
   AND cc.IsDefault = 1
   AND cc.ClientId = @clientId
   AND dv.Code = @domainCode
 ORDER BY cc.CreateDate DESC",
                new { clientId, domainCode = Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE }, UnitOfWork.Transaction);

            view.AdditionalPhone = UnitOfWork.Session.QueryFirstOrDefault<string>(@"SELECT TOP 1 Address
  FROM ClientContacts cc
  JOIN DomainValues dv ON dv.Id = cc.ContactTypeId
 WHERE cc.DeleteDate IS NULL
   AND cc.IsDefault = 0
   AND cc.ClientId = @clientId
   AND dv.Code = @domainCode
 ORDER BY cc.CreateDate DESC",
                new { clientId, domainCode = Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE }, UnitOfWork.Transaction);
            #endregion

            return view;
        }

        public ApplicationOnlineClient GetClientForApplicationOnline(int clientId)
        {
            var view = UnitOfWork.Session.Query<ApplicationOnlineClient>(@"SELECT Id,
       Name,
       Surname,
       Patronymic,
       ReceivesASP,
       BirthDay,
       PartnerCode,
       IdentityNumber,
       IsMale,
       FullName
  FROM Clients
 WHERE Id = @clientId",
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();

            return view;
        }

        public void Update(ApplicationOnlineClient entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"UPDATE Clients
   SET IdentityNumber = @IdentityNumber,
       FullName = @FullName,
       Surname = @Surname,
       Name = @Name,
       Patronymic = @Patronymic,
       IsMale = @IsMale,
       BirthDay = @BirthDay,
       ReceivesASP = @ReceivesASP,
       PartnerCode = @PartnerCode
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Client GetClientByBankCode(string code)
        {
            return UnitOfWork.Session
                .Query<Client>(@"SELECT * FROM Clients WHERE Clients.BankIdentifierCode = @code and Clients.BankCode IS NOT NULL",
                    new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void GetHardCollectionClientData(int clientId)
        {
            var clientAddressList = GetClientAddresses(clientId);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireClientInfo> GetClientInfoForPrintForm(int clientId)
        {

            var builder = new SqlBuilder();
            builder.Select("Clients.FullName");
            builder.Select("Clients.IdentityNumber");
            builder.Select("Clients.BirthDay");
            builder.Select("Clients.IsPolitician");
            builder.Where("Clients.id = @clientId",
                new { clientId });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from Clients /**where**/ ");
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireClientInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);

        }

        public async Task<PrintFormOpenCreditLineQuestionnaireClientDocumentInfo> GetClientDocumentInfoForPrintForm(int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientDocuments.Date AS DocumentDate");
            builder.Select("ClientDocuments.Number AS DocumentNumber");
            builder.Select("ClientDocumentProviders.Name AS DocumentProviderRu");
            builder.Select("ClientDocumentProviders.NameKaz AS DocumentProviderKz");
            builder.Select("ClientDocuments.DateExpire AS DateExpire");

            builder.Where("ClientDocuments.ClientId = @clientId",
                new { clientId });

            builder.LeftJoin("ClientDocumentProviders ON ClientDocumentProviders.Id = ClientDocuments.ProviderId");

            builder.OrderBy("ClientDocuments.Id Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientDocuments /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireClientDocumentInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireClientContactInfo> GetClientContractInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientContacts.Address AS MobilePhone");

            builder.Where("ClientContacts.ClientId = @clientId",
                new { clientId });
            builder.Where("ClientContacts.IsDefault = 1");

            builder.OrderBy("ClientContacts.Id Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientContacts /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireClientContactInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<PrintFormOpenCreditLineQuestionnaireAddressInfo>> GetAddressInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientAddresses.FullPathRus AS AddressNameRus");
            builder.Select("ClientAddresses.FullPathKaz AS AddressNameKaz");
            builder.Select("AddressTypes.Code AS Code ");

            builder.Where("ClientAddresses.ClientId = @clientId",
                new { clientId });
            builder.Where("ClientAddresses.IsActual = 1");


            builder.LeftJoin("AddressTypes ON AddressTypes.Id = ClientAddresses.AddressTypeId");

            builder.OrderBy("ClientAddresses.Id Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientAddresses /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryAsync<PrintFormOpenCreditLineQuestionnaireAddressInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireFamilyInfo> GetClientFamilyInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientProfiles.SpouseFullname");
            builder.Select("DomainValues.Name AS MaritalStatusRus");
            builder.Select("DomainValues.Name AS MartialStatusKaz");
            builder.Select("ClientProfiles.ChildrenCount");
            builder.Select("ClientProfiles.AdultDependentsCount");
            builder.Select("ClientProfiles.UnderageDependentsCount");

            builder.Where("ClientProfiles.ClientId = @clientId",
                new { clientId });


            builder.LeftJoin("DomainValues ON ClientProfiles.MaritalStatusId = DomainValues.Id");

            builder.OrderBy("ClientProfiles.Clientid Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientProfiles /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireFamilyInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireEmploymentInfo> GetClientEmploymentInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientEmployments.Name");
            builder.Select("ClientEmployments.PositionName");
            builder.Select("ClientEmployments.PhoneNumber");
            builder.Select("ClientEmployments.Address");

            builder.Where("ClientEmployments.ClientId = @clientId",
                new { clientId });
            builder.Where("ClientEmployments.DeleteDate IS NULL");

            builder.OrderBy("ClientEmployments.Id Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientEmployments /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireEmploymentInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireIncomeInfo> GetClientIncomeInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("COALESCE(SUM(ClientIncomes.IncomeAmount),0) AS Amount");

            builder.Where("ClientIncomes.ClientId = @clientId", new { clientId });
            builder.Where("ClientIncomes.IncomeType IN (10,20)");
            builder.Where("ClientIncomes.DeleteDate IS NULL");

            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientIncomes /**where**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireIncomeInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<PrintFormOpenCreditLineQuestionnaireAdditionalContacts>> GetClientAdditionalContactsForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientAdditionalContacts.ContactOwnerFullname AS Name");
            builder.Select("ClientAdditionalContacts.PhoneNumber AS MobilePhone");
            builder.Select("DomainValues.Name AS RelationshipRus");
            builder.Select("DomainValues.NameAlt AS RelationshipKaz");

            builder.Where("ClientAdditionalContacts.ClientId = @clientId",
                new { clientId });
            builder.Where("ClientAdditionalContacts.DeleteDate IS NULL");


            builder.LeftJoin("DomainValues ON DomainValues.Id = ClientAdditionalContacts.ContactOwnerTypeId");

            builder.OrderBy("ClientAdditionalContacts.Id Desc");
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientAdditionalContacts /**leftjoin**/ /**where**/ /**orderby**/ ");
            return await UnitOfWork.Session.QueryAsync<PrintFormOpenCreditLineQuestionnaireAdditionalContacts>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireExpenseInfo> GetClientExpenseInfoForPrintForm(
            int clientId)
        {
            var builder = new SqlBuilder();
            builder.Select("ClientExpenses.Loan");
            builder.Select("ClientExpenses.Other");
            builder.Select("ClientExpenses.Housing");
            builder.Select("ClientExpenses.Family");
            builder.Select("ClientExpenses.Vehicle");

            builder.Where("ClientExpenses.ClientId = @clientId",
                new { clientId });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ClientExpenses /**where**/ ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireExpenseInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }

        public async Task<Client> GetByApplicationOnlineId(Guid applicationId)
        {
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<Client>(@"SELECT c.*
  FROM Clients c
  JOIN ApplicationsOnline ao ON ao.ClientId = c.Id
 WHERE ao.Id = @applicationId",
                new { applicationId }, UnitOfWork.Transaction);
        }
    }
}