using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Operations;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class UserRepository : RepositoryBase, IRepository<User>
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(User entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO Members (OrganizationId, CreateDate, Locked)
VALUES (@organizationId, @createDate, @locked);

DECLARE @memberId INT;
    SET @memberId = SCOPE_IDENTITY();

INSERT INTO Users (Id, Login, IdentityNumber, Fullname, Email, Password, Salt, ExpireDate, AttorneyTextRus, AttorneyTextKaz, InvalidAttempts, PhoneNumber)
     VALUES (@memberId, @login, @identitynumber, @fullname, @email, '', '', @expireDate, @attorneyTextRus, @attorneyTextKaz, @InvalidAttempts, @PhoneNumber);

INSERT INTO MemberRelations (LeftMemberId, RightMemberId, RelationType)
VALUES (@memberId, @memberId, 0);

SELECT @memberId", entity, UnitOfWork.Transaction);

                if (!string.IsNullOrEmpty(entity.InternalPhoneNumber))
                    UnitOfWork.Session.Execute(@"INSERT INTO UsersInternalPhone ( CreateDate, UserId, InternalPhoneNumber )
VALUES ( @date, @userId, @phone )",
                        new { date = DateTime.Now, userId = entity.Id, phone = entity.InternalPhoneNumber }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(User entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Members
   SET Locked = @locked,
       OrganizationId = @organizationid
       WHERE Id = @id;


UPDATE Users
   SET Login = @login,
       IdentityNumber = @identitynumber,
       Fullname = @fullname,
       Email = @email,
       ExpireDate = @expireDate,
       AttorneyTextRus = @attorneyTextRus,
       AttorneyTextKaz = @attorneyTextKaz,
       InvalidAttempts = @InvalidAttempts,
       PhoneNumber = @PhoneNumber
 WHERE Id = @id;",
                    entity, UnitOfWork.Transaction);

                if (!string.IsNullOrEmpty(entity.InternalPhoneNumber))
                    UnitOfWork.Session.Execute(@"IF EXISTS (SELECT * FROM UsersInternalPhone WHERE UserId = @userId)
  BEGIN
    UPDATE UsersInternalPhone
       SET InternalPhoneNumber = @phoneNumber
     WHERE UserId = @userId
  END
ELSE 
  BEGIN
    INSERT INTO UsersInternalPhone ( CreateDate, UserId, InternalPhoneNumber )
    VALUES ( @date, @userId, @phoneNumber)
  END",
                        new { date = DateTime.Now, userId = entity.Id, phoneNumber = entity.InternalPhoneNumber }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public User Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<User>(@"SELECT u.*, m.*, uip.InternalPhoneNumber
  FROM Users u
  JOIN Members m ON m.Id = u.Id
  LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
 WHERE u.Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public async Task<User> GetAsync(int id)
        {
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<User>(@"SELECT u.*, m.*, uip.InternalPhoneNumber
              FROM Users u
              JOIN Members m ON m.Id = u.Id
              LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
             WHERE u.Id = @id", 
                new { id }, UnitOfWork.Transaction);
        }

        public User Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var login = query.Val<string>("login");

            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException();

            return UnitOfWork.Session.QueryFirstOrDefault<User>(@"SELECT u.*, m.*, uip.InternalPhoneNumber
  FROM Users u
  JOIN Members m ON m.Id = u.Id
  LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
 WHERE u.Login LIKE @login
    OR u.IdentityNumber LIKE @login",
                new { login });
        }

        public async Task<User> FindAsync(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var login = query.Val<string>("login");
            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException();

            return await UnitOfWork.Session.QueryFirstAsync<User>(@"SELECT u.*, m.*, uip.InternalPhoneNumber
  FROM Users u
  JOIN Members m ON m.Id = u.Id
  LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
 WHERE u.Login LIKE @login
    OR u.IdentityNumber LIKE @login", new { login });
        }

        public List<User> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query.Val<int?>("OrganizationId");
            var locked = query.Val<bool?>("Locked");
            var branchId = query.Val<int?>("BranchId");
            var roleId = query.Val<int?>("RoleId");

            var condition = listQuery.Like(
                BuildCondition(organizationId, locked, branchId, roleId),
                "Fullname", "Login", "IdentityNumber");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Fullname",
                Direction = SortDirection.Asc
            });

            var page = listQuery.Page();

            string branchQuery = "";
            string roleQuery = "";

            if (branchId.HasValue)
            {
                branchQuery = " INNER JOIN MemberRelations mre ON mre.LeftMemberId = u.Id ";
            }

            if (roleId.HasValue)
            {
                roleQuery = " INNER JOIN MemberRoles mro ON mro.MemberId = u.Id ";
            }

            return UnitOfWork.Session.Query<User>($@"SELECT DISTINCT u.*, uip.InternalPhoneNumber
  FROM Users u
  JOIN Members m ON m.Id = u.Id {branchQuery} {roleQuery}
  LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
{condition} {order} {page}", new
            {
                organizationId,
                locked,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                branchId,
                roleId
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query.Val<int?>("OrganizationId");
            var locked = query.Val<bool?>("Locked");
            var branchId = query.Val<int?>("BranchId");
            var roleId = query.Val<int?>("RoleId");

            var condition = listQuery.Like(
                BuildCondition(organizationId, locked, branchId, roleId),
                "Fullname", "Login", "IdentityNumber");

            string branchQuery = "";
            string roleQuery = "";

            if (branchId.HasValue)
            {
                branchQuery = " INNER JOIN MemberRelations mre ON mre.LeftMemberId = Users.Id ";
            }

            if (roleId.HasValue)
            {
                roleQuery = " INNER JOIN MemberRoles mro ON mro.MemberId = Users.Id ";
            }

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT Count( Distinct Users.Id)
  FROM Users
  JOIN Members ON Members.Id = Users.Id {branchQuery} {roleQuery}
{condition}", new
            {
                organizationId,
                locked,
                listQuery.Filter,
                branchId,
                roleId
            });
        }

        public void GetPasswordAndSalt(int id, out string password, out string salt)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = UnitOfWork.Session.QuerySingle(@"SELECT Password, Salt FROM Users where Id = @id", new { id });

            password = result.Password;
            salt = result.Salt;
        }

        public void SetPasswordAndSalt(int id, string password, string salt, int expireDay)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Value cannot be null or empty.", nameof(password));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Value cannot be null or empty.", nameof(salt));

            var expireDate = DateTime.Now.Date.AddDays(expireDay);

            UnitOfWork.Session.Execute(@"
UPDATE Users SET Password = @password, Salt = @salt, ExpireDate = @expireDate
WHERE Id = @id", new { id, password, salt, expireDate }, UnitOfWork.Transaction);
        }

        public User SearchUserByIdentityNumber(string identityNumber)
        {
            var result = UnitOfWork.Session.ExecuteScalar<int>(@"SELECT TOP 1 Id
  FROM Users
 WHERE IdentityNumber LIKE '%' + @identityNumber + '%'",
                new { identityNumber }, UnitOfWork.Transaction);

            if (result > 0)
                return Get(result);
            else
                return null;
        }

        public List<User> GetAllTasOnlineManagers()
        {
            return UnitOfWork.Session.Query<User>($@"SELECT DISTINCT u.*
  FROM Users u
  JOIN MemberRoles mr ON mr.MemberId = u.Id
  JOIN Roles r ON r.Id = mr.RoleId
  JOIN RolesAdditionalParameters rap ON rap.RoleId = r.Id AND rap.DeleteDate IS NULL
  LEFT JOIN UsersInternalPhone uip ON uip.UserId = u.Id
 WHERE rap.IsManager = 1;", new
            {
            }).ToList();
        }

        private string BuildCondition(int? organizationId, bool? locked, int? branchId, int? roleId)
        {
            var result = new StringBuilder();
            var wasClause = false;

            if (organizationId.HasValue && organizationId > 0)
            {
                result.Append("OrganizationId = @organizationId");
                wasClause = true;
            }
            if (locked.HasValue)
            {
                if (wasClause) result.Append(" AND ");
                result.Append("Locked = @locked");
                wasClause = true;
            }
            if (branchId.HasValue)
            {
                if (wasClause) result.Append(" AND ");
                result.Append(" mre.RightMemberId = @branchId ");
                wasClause = true;
            }
            if (roleId.HasValue)
            {
                if (wasClause) result.Append(" AND ");
                result.Append(" mro.RoleId = @roleId ");
            }

            return result.ToString();
        }

        public int? GetByInternalPhone(string phone)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<int?>(@"SELECT UserId FROM UsersInternalPhone WHERE InternalPhoneNumber = @phone",
                new { phone }, UnitOfWork.Transaction);
        }
    }
}