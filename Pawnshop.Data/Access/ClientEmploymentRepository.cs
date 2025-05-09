using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ClientEmploymentRepository : RepositoryBase, IRepository<ClientEmployment>
    {
        private readonly ISessionContext _sessionContext;
        public ClientEmploymentRepository(IUnitOfWork unitOfWork, ISessionContext sessionContext) : base(unitOfWork)
        {
            _sessionContext = sessionContext;
        }

        public void Insert(ClientEmployment entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO ClientEmployments
                        (ClientId, IsDefault, Name,
                         EmployeeCountId, PhoneNumber, Address, BusinessScopeId, WorkExperienceId, 
                         PositionName, PositionTypeId, AuthorId, CreateDate, DeleteDate, Income)
                    VALUES
                        (@ClientId, @IsDefault, @Name,
                         @EmployeeCountId, @PhoneNumber, @Address, @BusinessScopeId, @WorkExperienceId, 
                         @PositionName, @PositionTypeId, @AuthorId, @CreateDate, @DeleteDate, @Income)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ClientEmployment entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                    UPDATE ClientEmployments SET 
                        ClientId = @ClientId, 
                        IsDefault = @IsDefault, 
                        Name = @Name,
                        EmployeeCountId = @EmployeeCountId, 
                        PhoneNumber = @PhoneNumber,
                        Address = @Address, 
                        BusinessScopeId = @BusinessScopeId, 
                        WorkExperienceId = @WorkExperienceId, 
                        PositionName = @PositionName,
                        PositionTypeId = @PositionTypeId, 
                        AuthorId = @AuthorId, 
                        CreateDate = @CreateDate, 
                        DeleteDate = @DeleteDate,
                        Income = @Income
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            var entity = Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Запись ClientEmployments с Id {id}  не найдена");

            entity.DeleteDate = DateTime.Now;
            Update(entity);
        }

        public ClientEmployment Get(int id)
        {
            return UnitOfWork.Session.Query<ClientEmployment>(@"
                SELECT ce.* FROM ClientEmployments ce
                    WHERE ce.Id = @Id AND ce.DeleteDate IS NULL",
                    new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientEmployment> GetListByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientEmployment>(@"
                SELECT ce.* FROM ClientEmployments ce
                WHERE ce.ClientId = @clientId AND ce.DeleteDate IS NULL",
                new { clientId }, UnitOfWork.Transaction).ToList();
        }

        public ClientEmployment GetDefaultEmploymentByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientEmployment>(@"
                SELECT ce.* FROM ClientEmployments ce
                WHERE ce.ClientId = @clientId AND ce.IsDefault = 1 AND ce.DeleteDate IS NULL",
                new { clientId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientEmployment> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public ClientEmployment Find(object query)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(ClientEmployment entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientEmploymentLog
            {
                ClientEmploymentId = entity.Id,
                ClientId = entity.ClientId,
                IsDefault = entity.IsDefault,
                Name = entity.Name,
                EmployeeCountId = entity.EmployeeCountId,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                BusinessScopeId = entity.BusinessScopeId,
                WorkExperienceId = entity.WorkExperienceId,
                PositionName = entity.PositionName,
                PositionTypeId = entity.PositionTypeId,
                //Income = entity.Income,
                Income = 0,
                AuthorId = entity.AuthorId,
                CreateDate = entity.CreateDate,
                DeleteDate = entity.DeleteDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };

            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientEmploymentLogs 
                    (ClientEmploymentId, ClientId, IsDefault, Name,
                     EmployeeCountId, PhoneNumber, Address, BusinessScopeId, WorkExperienceId, 
                     PositionName, PositionTypeId, AuthorId, CreateDate, DeleteDate, UpdatedByAuthorId, UpdateDate, Income)
                VALUES 
                    (@ClientEmploymentId, @ClientId, @IsDefault, @Name,
                     @EmployeeCountId, @PhoneNumber, @Address, @BusinessScopeId, @WorkExperienceId, 
                     @PositionName, @PositionTypeId, @AuthorId, @CreateDate, @DeleteDate, @UpdatedByAuthorId, @UpdateDate, @Income)
                SELECT SCOPE_IDENTITY()", log, UnitOfWork.Transaction);
        }
    }
}
