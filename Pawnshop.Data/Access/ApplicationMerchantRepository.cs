using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.MobileApp;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Queries;
using Dapper;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ApplicationMerchantRepository : RepositoryBase, IRepository<ApplicationMerchant>
    {
        public ApplicationMerchantRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public void Insert(ApplicationMerchant entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ApplicationMerchants (Name, Surname, MiddleName, BirthDay, DocumentTypeCode, BirthOfPlace, IdentityNumber, LicenseNumber, LicenseDateOfIssue, LicenseDateOfEnd, LicenseIssuer, DefinitionLegalPerson, IsAutocredit)
                VALUES (@Name, @Surname, @MiddleName, @BirthDay, @DocumentTypeCode, @BirthOfPlace, @IdentityNumber, @LicenseNumber, @LicenseDateOfIssue, @LicenseDateOfEnd, @LicenseIssuer, @DefinitionLegalPerson, @IsAutocredit)
                SELECT SCOPE_IDENTITY()", 
                entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }

        public void Update(ApplicationMerchant entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
        UPDATE ApplicationMerchants 
        SET Name = @Name, 
            Surname = @Surname, 
            MiddleName = @MiddleName, 
            BirthDay = @BirthDay, 
            DocumentTypeCode = @DocumentTypeCode, 
            BirthOfPlace = @BirthOfPlace, 
            LicenseNumber = @LicenseNumber, 
            LicenseDateOfIssue = @LicenseDateOfIssue, 
            LicenseDateOfEnd = @LicenseDateOfEnd, 
            LicenseIssuer = @LicenseIssuer, 
            DefinitionLegalPerson = @DefinitionLegalPerson, 
            IsAutocredit = @IsAutocredit
        WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }

        }

        public ApplicationMerchant Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationMerchant FindByIdentityNumber(string IdentityNumber)
        {
            try
            {
                return UnitOfWork.Session.Query<ApplicationMerchant>(@"
                    SELECT * FROM ApplicationMerchants 
                    WHERE IdentityNumber = @IdentityNumber
                    ORDER BY Id DESC"
                , new { IdentityNumber }, UnitOfWork.Transaction).FirstOrDefault();
            } 
            catch
            {
                return null;
            }
        }

        public ApplicationMerchant Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationMerchant>(@"
                SELECT *
                FROM ApplicationMerchants
                WHERE Id = @id
                ORDER BY Id DESC"
            , new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }
        
        public List<ApplicationMerchant> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
