using Dapper;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientsMobilePhoneContacts;
using Pawnshop.Data.Models.ClientsMobilePhoneContacts.Views;

namespace Pawnshop.Data.Access
{
    public sealed class ClientsMobilePhoneContactsRepository : RepositoryBase
    {
        public ClientsMobilePhoneContactsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientsMobilePhoneContact clientGeoPosition)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                INSERT INTO ClientsMobilePhoneContacts (Id, PhoneNumber, Name, CreateDate, ClientId) 
                VALUES (@Id, @PhoneNumber, @Name, @CreateDate, @ClientId)
                SELECT SCOPE_IDENTITY()", clientGeoPosition, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ClientsMobilePhoneContactListView GetClientsMobilePhoneContacts(int clientId, int offset, int limit)
        {
            string query =
                @"Select * from ClientsMobilePhoneContacts where clientId = @clientId";
            string tail = @$" Order by CreateDate desc OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY";

            string result = query + tail;

            string count = @"SELECT COUNT (*) FROM ClientsMobilePhoneContacts WHERE clientid = @clientId";

            ClientsMobilePhoneContactListView contactList = new ClientsMobilePhoneContactListView();

            contactList.Count = UnitOfWork.Session.Query<int>(count,
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();

            contactList.ClientsMobilePhoneContacts = UnitOfWork.Session.Query<ClientsMobilePhoneContact>(result,
                new { clientId }, UnitOfWork.Transaction).ToList();
            return contactList;
        }

        public List<ClientsMobilePhoneContact> GetClientsMobilePhoneContacts(int clientId)
        {
            string query =
                @"Select * from ClientsMobilePhoneContacts where clientId = @clientId";

            return UnitOfWork.Session.Query<ClientsMobilePhoneContact>(query,
                new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public ClientsMobilePhoneContact GetClientContact(int clientId, string phoneNumber, string name)
        {
            string query =
                @"Select * from ClientsMobilePhoneContacts where clientId = @clientId and PhoneNumber = @phoneNumber and Name = @name";

            return UnitOfWork.Session.Query<ClientsMobilePhoneContact>(query,
                new { clientId, phoneNumber, name }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public bool HasContactList(int clientId, string phoneNumber)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<bool>(@"SELECT 1
  FROM ClientsMobilePhoneContacts
 WHERE ClientId = @clientId
   AND PhoneNumber = @phoneNumber",
                new { clientId, phoneNumber }, UnitOfWork.Transaction);
        }

        public ClientsMobilePhoneContact Find(int clientId, object query)
        {
            if (query == null)
                return null;

            var phoneNumber = query.Val<string>("PhoneNumber");
            var name = query.Val<string>("Name");

            var predicateList = new List<string> { "WHERE ClientId = @clientId" };

            if (phoneNumber != null)
                predicateList.Add("PhoneNumber = @phoneNumber");

            if (name != null)
                predicateList.Add("Name = @name");

            if (predicateList.Count <= 1)
                return null;

            var predicate = string.Join(" AND ", predicateList.ToArray());

            return UnitOfWork.Session.QueryFirstOrDefault<ClientsMobilePhoneContact>($@"SELECT *
  FROM ClientsMobilePhoneContacts
 {predicate}",
                new { phoneNumber, name, clientId }, UnitOfWork.Transaction);
        }
    }
}
