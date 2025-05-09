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
    public class ClientProfileRepository : RepositoryBase
    {
        public ClientProfileRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientProfile entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientProfiles 
                    (ClientId, EducationTypeId, TotalWorkExperienceId, MaritalStatusId, SpouseFullname, SpouseIncome,
                        ChildrenCount, AdultDependentsCount, UnderageDependentsCount, 
                        ResidenceAddressTypeId, IsWorkingNow, HasAssets, AuthorId, CreateDate)
                VALUES 
                    (@ClientId, @EducationTypeId, @TotalWorkExperienceId, @MaritalStatusId, @SpouseFullname, @SpouseIncome,
                        @ChildrenCount, @AdultDependentsCount, @UnderageDependentsCount, 
                        @ResidenceAddressTypeId, @IsWorkingNow, @HasAssets, @AuthorId, @CreateDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ClientProfile entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ClientProfiles
                SET 
                    EducationTypeId = @EducationTypeId,
                    TotalWorkExperienceId = @TotalWorkExperienceId,
                    MaritalStatusId = @MaritalStatusId,
                    SpouseFullname = @SpouseFullname,
                    SpouseIncome = @SpouseIncome,
                    ChildrenCount = @ChildrenCount,
                    AdultDependentsCount = @AdultDependentsCount,
                    UnderageDependentsCount = @UnderageDependentsCount,
                    ResidenceAddressTypeId = @ResidenceAddressTypeId,
                    IsWorkingNow = @IsWorkingNow,
                    HasAssets = @HasAssets,
                    AuthorId = @AuthorId,
                    CreateDate = @CreateDate
                WHERE ClientId = @ClientId", entity, UnitOfWork.Transaction);
        }

        public ClientProfile Get(int clientId)
        {
            return UnitOfWork.Session.Query<ClientProfile>(@"
                SELECT cp.*
                FROM ClientProfiles cp
                WHERE cp.ClientId = @clientId",
            new { clientId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void LogChanges(ClientProfile entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientProfileLog
            {
                ClientId = entity.ClientId,
                EducationTypeId = entity.EducationTypeId,
                TotalWorkExperienceId = entity.TotalWorkExperienceId,
                MaritalStatusId = entity.MaritalStatusId,
                SpouseFullname = entity.SpouseFullname,
                SpouseIncome = entity.SpouseIncome,
                ChildrenCount = entity.ChildrenCount,
                AdultDependentsCount = entity.AdultDependentsCount,
                UnderageDependentsCount = entity.UnderageDependentsCount,
                ResidenceAddressTypeId = entity.ResidenceAddressTypeId,
                IsWorkingNow = entity.IsWorkingNow,
                HasAssets = entity.HasAssets,
                AuthorId = entity.AuthorId,
                CreateDate = entity.CreateDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };

            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientProfileLogs 
                    (ClientId, EducationTypeId, TotalWorkExperienceId, MaritalStatusId, 
                     SpouseFullname, SpouseIncome, ChildrenCount, AdultDependentsCount, UnderageDependentsCount, 
                     ResidenceAddressTypeId, IsWorkingNow, HasAssets,
                     AuthorId, CreateDate, UpdatedByAuthorId, UpdateDate)
                VALUES 
                    (@ClientId, @EducationTypeId, @TotalWorkExperienceId, @MaritalStatusId, 
                     @SpouseFullname, @SpouseIncome, @ChildrenCount, @AdultDependentsCount, @UnderageDependentsCount, 
                     @ResidenceAddressTypeId, @IsWorkingNow, @HasAssets,
                     @AuthorId, @CreateDate, @UpdatedByAuthorId, @UpdateDate)", log, UnitOfWork.Transaction);
        }
    }
}
