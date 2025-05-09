using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class CBSubjectRepository : RepositoryBase, IRepository<ICBSubject>
    {
        public CBSubjectRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ICBSubject entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBSubjects (CBContractId, RoleId)
VALUES(@CBContractId, @RoleId) SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                long addressesCollectionId = UnitOfWork.Session.ExecuteScalar<long>(@"
DECLARE @AddressNextTemp BIGINT
EXEC dbo.sp_GetNextSequenceId NULL, @AddressNextTemp OUTPUT;
SELECT @AddressNextTemp", UnitOfWork.Transaction);

                long identificationsCollectionId = UnitOfWork.Session.ExecuteScalar<long>(@"
DECLARE @AddressNextTemp BIGINT
EXEC dbo.sp_GetNextSequenceId NULL, @AddressNextTemp OUTPUT;
SELECT @AddressNextTemp", UnitOfWork.Transaction);

                entity.AddressesCollectionId = addressesCollectionId;
                entity.IdentificationsCollectionId = identificationsCollectionId;

                if (entity.IsIndividual)
                {
                    CBSubjectIndividual person = (CBSubjectIndividual)entity;

                    UnitOfWork.Session.Execute(@"
INSERT INTO CBSubjectIndividuals
(Id, FirstName, Surname, FathersName, Gender, Classification, Residency, DateOfBirth, Citizenship, AddressesCollectionId, IdentificationsCollectionId)
VALUES(@Id, @FirstName, @Surname, @FathersName, @Gender, @Classification, @Residency, @DateOfBirth, 
@Citizenship, @AddressesCollectionId, @IdentificationsCollectionId)", person, UnitOfWork.Transaction);
                }
                else
                {
                    CBSubjectCompany person = (CBSubjectCompany)entity;

                    person.CEOIdentificationsCollectionId = UnitOfWork.Session.ExecuteScalar<long>(@"
DECLARE @AddressNextTemp BIGINT
EXEC dbo.sp_GetNextSequenceId NULL, @AddressNextTemp OUTPUT;
SELECT @AddressNextTemp", UnitOfWork.Transaction);

                    UnitOfWork.Session.Execute(@"
INSERT INTO CBSubjectCompanies
(Id, Name, Status, TradeName, Abbreviation, LegalForm, Nationality, RegistrationDate, AddressesCollectionId, IdentificationsCollectionId, 
CEOFirstName, CEOSurname, CEOFathersName, CEODateOfBirth, CEOIdentificationsCollectionId)
VALUES(@Id, @Name, @Status, @TradeName, @Abbreviation, @LegalForm, @Nationality, @RegistrationDate, @AddressesCollectionId, @IdentificationsCollectionId, 
@CEOFirstName, @CEOSurname, @CEOFathersName, @CEODateOfBirth, @CEOIdentificationsCollectionId)", person, UnitOfWork.Transaction);

                    foreach (var identification in person.CEOIdentifications)
                    {
                        identification.CollectionId = person.CEOIdentificationsCollectionId;
                        InsertIdentification(identification);
                    }
                }

                foreach (var address in entity.Addresses)
                {
                    address.CollectionId = entity.AddressesCollectionId;
                    InsertAddress(address);
                }

                foreach (var identification in entity.Identifications)
                {
                    identification.CollectionId = entity.IdentificationsCollectionId;
                    InsertIdentification(identification);
                }

                transaction.Commit();
            }
        }

        private void InsertAddress(CBAddress entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBAddresses
(CollectionId, TypeId, LocationId, KATOID, StreetName)
VALUES(@CollectionId, @TypeId, @LocationId, @KATOID, @StreetName)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        private void InsertIdentification(CBIdentification entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBIdentifications
(CollectionId, TypeId, Rank, Number, IssueDate, ExpirationDate, DocumentTypeText, RegistrationDate)
VALUES(@CollectionId, @TypeId, @Rank, @Number, @IssueDate, @ExpirationDate, @DocumentTypeText, @RegistrationDate)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ICBSubject entity)
        {

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBSubjects SET CBContractId = @CBContractId, RoleId = @RoleId
WHERE Id=@id", (CBSubjectIndividual)entity, UnitOfWork.Transaction);

                if (entity.IsIndividual)
                {
                    UnitOfWork.Session.Execute(@"
UPDATE CBSubjectIndividuals SET FirstName = @FirstName, Surname = @Surname, FathersName = @FathersName, Gender = @Gender, 
Classification = @Classification, Residency = @Residency, DateOfBirth = @DateOfBirth, Citizenship = @Citizenship
WHERE Id=@id", entity, UnitOfWork.Transaction);
                }
                else
                {
                    CBSubjectCompany person = (CBSubjectCompany)entity;

                    UnitOfWork.Session.Execute(@"
UPDATE CBSubjectCompanies SET Name = @Name, Status = @Status, TradeName = @TradeName, Abbreviation = @Abbreviation, LegalForm = @LegalForm, 
Nationality = @Nationality, RegistrationDate = @RegistrationDate, CEOFirstName = @CEOFirstName, CEOSurname = @CEOSurname, CEOFathersName = @CEOFathersName, 
CEODateOfBirth = @CEODateOfBirth
WHERE Id=@id", person, UnitOfWork.Transaction);

                    foreach (var identification in person.CEOIdentifications)
                    {
                        if (!(identification.Id > 0))
                        {
                            identification.CollectionId = person.CEOIdentificationsCollectionId;
                            InsertIdentification(identification);
                        }
                        else
                        {
                            UpdateIdentification(identification);
                        }
                    }
                }

                foreach (var address in entity.Addresses)
                {
                    if (!(address.Id > 0))
                    {
                        address.CollectionId = entity.AddressesCollectionId;
                        InsertAddress(address);
                    }
                    else
                    {

                        UpdateAddress(address);
                    }
                }

                foreach (var identification in entity.Identifications)
                {
                    if (!(identification.Id > 0))
                    {
                        identification.CollectionId = entity.IdentificationsCollectionId;
                        InsertIdentification(identification);
                    }
                    else
                    {
                        UpdateIdentification(identification);
                    }
                }

                transaction.Commit();
            }
        }

        private void UpdateAddress(CBAddress entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBAddresses SET TypeId = @TypeId, LocationId = @LocationId, KATOID = @KATOID, StreetName = @StreetName
                  WHERE Id=@id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        private void UpdateIdentification(CBIdentification entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBIdentifications SET TypeId = @TypeId, Rank = @Rank, Number = @Number, IssueDate = @IssueDate, ExpirationDate = @ExpirationDate, DocumentTypeText = @DocumentTypeText,
RegistrationDate = @RegistrationDate
WHERE Id=@id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ICBSubject Get(int id)
        {
            throw new NotImplementedException();
        }

        public ICBSubject Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ICBSubject> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}

