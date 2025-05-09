using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ClientContactRepository : RepositoryBase, IRepository<ClientContact>
    {
        public ClientContactRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientContact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO ClientContacts ( Address, ContactTypeId, IsDefault, AuthorId, ClientId, CreateDate, SendUkassaCheck, ContactCategoryId, IsActual, SourceId, Note)
                VALUES ( @Address, @ContactTypeId, @IsDefault, @AuthorId, @ClientId, @CreateDate, @SendUkassaCheck, @ContactCategoryId, @IsActual, @SourceId, @Note)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public async Task<int> InsertFullContact(ClientContact contact)
        {
            using (var transaction = BeginTransaction())
            {
                var id = await UnitOfWork.Session.InsertAsync(contact, UnitOfWork.Transaction);
                transaction.Commit();
                return id;
            }
        }

        public void Update(ClientContact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ClientContacts
                SET 
                    Address = @Address, 
                    ContactTypeId = @ContactTypeId, 
                    IsDefault = @IsDefault,
                    ClientId = @ClientId, 
                    VerificationExpireDate = @VerificationExpireDate,
                    AuthorId = @AuthorId, 
                    DeleteDate = @DeleteDate,
                    CreateDate = @CreateDate,
                    SendUkassaCheck = @SendUkassaCheck,
                    ContactCategoryId = @ContactCategoryId,
                    IsActual = @IsActual,
                    SourceId = @SourceId,
                    Note = @Note
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            var contact = Get(id);
            if (contact == null)
                throw new PawnshopApplicationException("Запись ClientContact не найдена");

            contact.DeleteDate = DateTime.Now;
            Update(contact);
        }

        public ClientContact GetActualDefaultContact(int clientId)
        {
            return UnitOfWork.Session.Query<ClientContact>(@$"
                SELECT cv.*
                FROM ClientContacts cv
                WHERE cv.IsDefault = @isDefault AND cv.ClientId = @clientId
                AND cv.VerificationExpireDate > dbo.GETASTANADATE() AND cv.DeleteDate IS NULL", new { isDefault = true, clientId}, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientContact Find(object query)
        {
            List<string> conditions = new List<string> { "cc.DeleteDate IS NULL" };
            int? clientId = query?.Val<int>("ClientId");
            bool? isDefault = query?.Val<bool>("IsDefault");
            string address = query?.Val<string>("Address");
            bool? sendCheck = query?.Val<bool?>("SendCheck");
            int? contactType = query?.Val<int?>("ContactType");
            if (clientId.HasValue && clientId.Value != default)
                conditions.Add("cc.ClientId = @clientId");

            if (isDefault.HasValue)
                conditions.Add("cc.IsDefault = @isDefault");

            if(!string.IsNullOrWhiteSpace(address))
                conditions.Add("cc.Address = @address");

            if (sendCheck.HasValue)
                conditions.Add("cc.SendUkassaCheck = @sendCheck");
            if (contactType.HasValue && contactType.Value != default)
                conditions.Add("cc.ContactTypeId = @contactType");

            string condition = "WHERE " + string.Join(" AND ", conditions);
            return UnitOfWork.Session.Query<ClientContact>(@$"
                SELECT cc.*
                FROM ClientContacts cc
                {condition}", new { clientId, isDefault, address, contactType, sendCheck }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientContact Get(int id)
        {
            return UnitOfWork.Session.Query<ClientContact>(@"
                SELECT cc.*
                FROM ClientContacts cc
                WHERE cc.Id=@id AND cc.DeleteDate IS NULL",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ClientContact> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ClientContact>(@"
                SELECT cc.*
                FROM ClientContacts cc
                WHERE cc.Id=@id AND cc.DeleteDate IS NULL",
            new { id }, UnitOfWork.Transaction);
        }

        public List<ClientContact> List(ListQuery listQuery, object query = null)
        {
            string pre = "cc.DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "cc.Address");
            string order = listQuery.Order(string.Empty, new Sort
            {
                Name = "cc.IsDefault, cc.CreateDate",
                Direction = SortDirection.Desc
            });

            int? clientId = query?.Val<int>("ClientId");
            if (clientId.HasValue)
                condition += " AND cc.ClientId = @clientId";

            int? contactTypeId = query?.Val<int?>("ContactTypeId");
            if (contactTypeId.HasValue)
                condition += " AND cc.ContactTypeId = @contactTypeId";

            int? contactCategoryId = query?.Val<int?>("ContactCategoryId");
            if (contactCategoryId.HasValue)
                condition += " AND cc.ContactCategoryId = @contactCategoryId";

            return UnitOfWork.Session.Query<ClientContact, DomainValue, ClientContact>(@$"
                SELECT cc.*, dv.*
                FROM ClientContacts cc
                LEFT JOIN DomainValues dv ON cc.ContactCategoryId = dv.id
                {condition} {order}", (cc, dv) =>
            {
                cc.ContactCategoryCode = dv == null ? "" : dv.Code;
                return cc;
            },
                new
                {
                    listQuery.Filter,
                    clientId,
                    contactTypeId,
                    contactCategoryId
                }, UnitOfWork.Transaction).ToList(); 
        }

        public async Task<IEnumerable<ClientContact>> ListAsync(ListQuery listQuery, object query = null)
        {
            string pre = "cc.DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "cc.Address");
            string order = listQuery.Order(string.Empty, new Sort
            {
                Name = "cc.IsDefault, cc.CreateDate",
                Direction = SortDirection.Desc
            });

            int? clientId = query?.Val<int>("ClientId");
            if (clientId.HasValue)
                condition += " AND cc.ClientId = @clientId";

            int? contactTypeId = query?.Val<int?>("ContactTypeId");
            if (contactTypeId.HasValue)
                condition += " AND cc.ContactTypeId = @contactTypeId";

            int? contactCategoryId = query?.Val<int?>("ContactCategoryId");
            if (contactCategoryId.HasValue)
                condition += " AND cc.ContactCategoryId = @contactCategoryId";

            return await UnitOfWork.Session.QueryAsync<ClientContact, DomainValue, ClientContact>(@$"
                SELECT cc.*, dv.*
                FROM ClientContacts cc
                LEFT JOIN DomainValues dv ON cc.ContactCategoryId = dv.id
                {condition} {order}", (cc, dv) =>
            {
                cc.ContactCategoryCode = dv == null ? "" : dv.Code;
                return cc;
            },
                new
                {
                    listQuery.Filter,
                    clientId,
                    contactTypeId,
                    contactCategoryId
                }, UnitOfWork.Transaction);
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(ClientContact clientContact, int userId, bool isNew = false)
        {
            var log = new ClientContactLog
            {
                Address = clientContact.Address,
                AuthorId = clientContact.AuthorId,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                ClientContactId = clientContact.Id,
                ClientId = clientContact.ClientId,
                ContactTypeId = clientContact.ContactTypeId,
                CreateDate = clientContact.CreateDate,
                DeleteDate = clientContact.DeleteDate,
                IsDefault = clientContact.IsDefault,
                UpdateDate = DateTime.Now,
                VerificationExpireDate = clientContact.VerificationExpireDate,
                ContactCategoryId = clientContact.ContactCategoryId,
                IsActual = clientContact.IsActual,
                SourceId = clientContact.SourceId,
                Note = clientContact.Note
            };

            UnitOfWork.Session.Query(@"
                    INSERT INTO ClientContactLogs (ClientContactId, Address, ContactTypeId, IsDefault, AuthorId, ClientId, CreateDate, DeleteDate, UpdateDate, UpdatedByAuthorId, VerificationExpireDate, SendUkassaCheck, ContactCategoryId, IsActual, SourceId, Note)
                    VALUES ( @ClientContactId, @Address, @ContactTypeId, @IsDefault, @AuthorId, @ClientId, @CreateDate, @DeleteDate, @UpdateDate, @UpdatedByAuthorId, @VerificationExpireDate, @SendUkassaCheck, @ContactCategoryId, @IsActual, @SourceId, @Note)
                    SELECT SCOPE_IDENTITY()", log, UnitOfWork.Transaction);
        }

        public void UpdateUkassaCheckReceive(int contactId, bool receive)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE ClientContacts
                SET 
                    SendUkassaCheck = @receive
                WHERE Id = @contactId", new { contactId, receive }, UnitOfWork.Transaction);
        }

        public int? GetClientIdByDefaultPhone(string phoneNumber)
        {
            return UnitOfWork.Session.ExecuteScalar<int?>(@"SELECT ClientId
  FROM ClientContacts
 WHERE DeleteDate IS NULL
   AND IsDefault = 1
   AND ContactTypeId = 1
   AND Address = @phoneNumber",
                new { phoneNumber }, UnitOfWork.Transaction);
        }
    }
}
