using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Models.ClientAdditionalContactLogItems;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Helpers;

namespace Pawnshop.Data.Access
{
    public class ClientAdditionalContactRepository : RepositoryBase, IRepository<ClientAdditionalContact>
    {
        private readonly ClientsMobilePhoneContactsRepository _clientsMobilePhoneContactsRepository;
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClientAdditionalContactRepository(IUnitOfWork unitOfWork, 
            ClientsMobilePhoneContactsRepository clientsMobilePhoneContactsRepository,
            IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _clientsMobilePhoneContactsRepository = clientsMobilePhoneContactsRepository;
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Insert(ClientAdditionalContact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO ClientAdditionalContacts
                    (ClientId, PhoneNumber, ContactOwnerTypeId, ContactOwnerFullname, AuthorId, CreateDate, DeleteDate, IsMainPayer, FromContactList)
                VALUES 
                    (@ClientId, @PhoneNumber, @ContactOwnerTypeId, @ContactOwnerFullname, @AuthorId, @CreateDate, @DeleteDate, @IsMainPayer, @FromContactList)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            _service.LogClientAdditionalContactData(new ClientAdditionalContactLogData(entity),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public void Update(ClientAdditionalContact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ClientAdditionalContacts
                SET 
                    ClientId = @ClientId, 
                    PhoneNumber = @PhoneNumber,
                    ContactOwnerTypeId = @ContactOwnerTypeId, 
                    ContactOwnerFullname = @ContactOwnerFullname, 
                    AuthorId = @AuthorId, 
                    CreateDate = @CreateDate, 
                    DeleteDate = @DeleteDate,   
                    IsMainPayer = @IsMainPayer,
                    FromContactList = @FromContactList
                WHERE Id = @Id", entity, UnitOfWork.Transaction);

            _service.LogClientAdditionalContactData(new ClientAdditionalContactLogData(entity),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public void Delete(int id)
        {
            ClientAdditionalContact entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Запись ClientAdditionalContacts с Id {id} не найдена");

            entity.DeleteDate = DateTime.Now;
            Update(entity);
        }

        public ClientAdditionalContact Get(int id)
        {
            return UnitOfWork.Session.Query<ClientAdditionalContact>(@"
                SELECT cac.*
                FROM ClientAdditionalContacts cac
                WHERE cac.Id = @id AND cac.DeleteDate IS NULL",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ClientAdditionalContact> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ClientAdditionalContact>(@"
                SELECT cac.*
                FROM ClientAdditionalContacts cac
                WHERE cac.Id = @id AND cac.DeleteDate IS NULL",
                new { id }, UnitOfWork.Transaction);
        }

        public List<ClientAdditionalContact> GetListByClientId(int clientId)
        {
            var result = UnitOfWork.Session.Query<ClientAdditionalContact>(@"
                SELECT cac.*
                FROM ClientAdditionalContacts cac
                WHERE cac.ClientId = @clientId AND cac.DeleteDate IS NULL",
            new { clientId }, UnitOfWork.Transaction).ToList();

            result.ForEach(x =>
            {
                var findResult = _clientsMobilePhoneContactsRepository.Find(clientId, new { x.PhoneNumber });

                if (findResult != null)
                {
                    x.FromContactList = true;
                    x.ContactListName = findResult.Name;
                }
            });

            return result;
        }

        public async Task<IEnumerable<ClientAdditionalContact>> GetListByClientIdAsync(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<ClientAdditionalContact>(@"
                SELECT cac.*
                FROM ClientAdditionalContacts cac
                WHERE cac.ClientId = @clientId AND cac.DeleteDate IS NULL",
            new { clientId }, UnitOfWork.Transaction);
        }

        public ClientAdditionalContact Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ClientAdditionalContact> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(ClientAdditionalContact entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientAdditionalContactLog
            {
                ClientAdditionalContactId = entity.Id,
                ClientId = entity.ClientId,
                PhoneNumber = entity.PhoneNumber,
                ContactOwnerTypeId = entity.ContactOwnerTypeId,
                ContactOwnerFullname = entity.ContactOwnerFullname,
                AuthorId = entity.AuthorId,
                CreateDate = entity.CreateDate,
                DeleteDate = entity.DeleteDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientAdditionalContactLogs 
                    (ClientAdditionalContactId, ClientId, PhoneNumber, ContactOwnerTypeId, 
                     ContactOwnerFullname, AuthorId, CreateDate, DeleteDate,
                     UpdatedByAuthorId, UpdateDate, IsMainPayer)
                VALUES 
                    (@ClientAdditionalContactId, @ClientId, @PhoneNumber, @ContactOwnerTypeId, 
                     @ContactOwnerFullname, @AuthorId, @CreateDate, @DeleteDate,
                     @UpdatedByAuthorId, @UpdateDate, @IsMainPayer)", log, UnitOfWork.Transaction);
        }
    }
}
