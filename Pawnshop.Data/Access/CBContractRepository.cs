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
    public class CBContractRepository : RepositoryBase, IRepository<CBContract>
    {
        public CBContractRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(CBContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBContracts
(CBBatchId, ContractId, ContractCode, AgreementNumber, FundingType, FundingSource, CreditPurpose, CreditObject, ContractPhase,
ContractStatus, ThirdPartyHolder, StartDate, EndDate, ActualDate, AvailableDate, RealPaymentDate, SpecialRelationship,
Classification, ParentContractCode, ParentProvider, ParentContractStatus, ParentOperationDate, ProlongationCount, OperationId, AnnualEffectiveRate, NominalRate)
VALUES
(@CBBatchId, @ContractId, @ContractCode, @AgreementNumber, @FundingType, @FundingSource, @CreditPurpose, @CreditObject, @ContractPhase,
@ContractStatus, @ThirdPartyHolder, @StartDate, @EndDate, @ActualDate, @AvailableDate, @RealPaymentDate, @SpecialRelationship,
@Classification, @ParentContractCode, @ParentProvider, @ParentContractStatus, @ParentOperationDate, @ProlongationCount, @OperationId, @AnnualEffectiveRate, @NominalRate)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Insert(List<CBContract> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
INSERT INTO CBContracts
(CBBatchId, ContractId, ContractCode, AgreementNumber, FundingType, FundingSource, CreditPurpose, CreditObject, ContractPhase,
ContractStatus, ThirdPartyHolder, StartDate, EndDate, ActualDate, AvailableDate, RealPaymentDate, SpecialRelationship,
Classification, ParentContractCode, ParentProvider, ParentContractStatus, ParentOperationDate, ProlongationCount, OperationId, AnnualEffectiveRate, NominalRate)
VALUES
(@CBBatchId, @ContractId, @ContractCode, @AgreementNumber, @FundingType, @FundingSource, @CreditPurpose, @CreditObject, @ContractPhase,
@ContractStatus, @ThirdPartyHolder, @StartDate, @EndDate, @ActualDate, @AvailableDate, @RealPaymentDate, @SpecialRelationship,
@Classification, @ParentContractCode, @ParentProvider, @ParentContractStatus, @ParentOperationDate, @ProlongationCount, @OperationId, @AnnualEffectiveRate, @NominalRate)
", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(CBContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBContracts SET CBBatchId=@CBBatchId, ContractId = @ContractId, ContractCode = @ContractCode, AgreementNumber = @AgreementNumber,
FundingType = @FundingType, FundingSource = @FundingSource, CreditPurpose = @CreditPurpose, CreditObject = @CreditObject, 
ContractPhase = @ContractPhase, ContractStatus = @ContractStatus, ThirdPartyHolder = @ThirdPartyHolder, StartDate = @StartDate, 
EndDate = @EndDate, ActualDate = @ActualDate, AvailableDate = @AvailableDate, RealPaymentDate = @RealPaymentDate, 
SpecialRelationship = @SpecialRelationship, Classification = @Classification, ParentContractCode = @ParentContractCode, 
ParentProvider = @ParentProvider, ParentContractStatus = @ParentContractStatus, ParentOperationDate = @ParentOperationDate, 
ProlongationCount = @ProlongationCount, OperationId = @OperationId, AnnualEffectiveRate = @AnnualEffectiveRate, NominalRate = @NominalRate
                  WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE CBContracts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public CBContract Get(int id)
        {
            var contract = UnitOfWork.Session.QuerySingleOrDefault<CBContract>(@"
                    SELECT * 
                    FROM CBContracts
                    WHERE Id=@id", new { id }, UnitOfWork.Transaction);

            contract.Subjects = GetSubjects(contract.Id);

            contract.Collaterals = UnitOfWork.Session.Query<CBCollateral>(@"
                    SELECT * FROM CBCollaterals WHERE CBContractId=@id", new { id }, UnitOfWork.Transaction).ToList();

            //TODO: переделать Installment при реализации кредитных линий
            contract.Installment = UnitOfWork.Session.Query<CBInstallment>(@"
                    SELECT * 
                    FROM CBInstallments
                    WHERE CBContractId=@id", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            contract.Installment.Records = UnitOfWork.Session.Query<CBInstallmentRecord>(@"SELECT * FROM CBInstallmentRecords WHERE CBInstallmentId = @id", new { id = contract.Installment.Id }, UnitOfWork.Transaction).ToList();

            return contract;
        }

        private List<ICBSubject> GetSubjects(int contractId)
        {
            var subjects = UnitOfWork.Session.Query<CBSubjectIndividual>(@"
SELECT * 
FROM CBSubjects cs 
WHERE cs.CBContractId=@id", new { id = contractId }, UnitOfWork.Transaction);

            var result = new List<ICBSubject>();

            foreach (var s in subjects)
            {
                if (s.IsIndividual)
                {
                    var persons = UnitOfWork.Session.Query<CBSubjectIndividual>(@"
SELECT * 
FROM CBSubjects cs JOIN CBSubjectIndividuals csi ON cs.Id = csi.Id
WHERE cs.Id=@id", new
                    {
                        id = s.Id
                    }, UnitOfWork.Transaction);

                    foreach (var person in persons)
                    {
                        person.Addresses = GetAddresses(person.AddressesCollectionId);
                        person.Identifications = GetIdentifications(person.IdentificationsCollectionId);
                    }

                    result.AddRange(persons);
                }
                else
                {
                    var persons = UnitOfWork.Session.Query<CBSubjectCompany>(@"
SELECT * 
FROM CBSubjects cs JOIN CBSubjectCompanies csi ON cs.Id = csi.Id
WHERE cs.Id=@id", new { id = s.Id }, UnitOfWork.Transaction);

                    foreach (var person in persons)
                    {
                        person.Addresses = GetAddresses(person.AddressesCollectionId);
                        person.Identifications = GetIdentifications(person.IdentificationsCollectionId);
                        person.CEOIdentifications = GetIdentifications(person.CEOIdentificationsCollectionId);
                    }

                    result.AddRange(persons);
                }
            }

            return result.ToList();
        }

        private List<CBAddress> GetAddresses(long id)
        {
            return UnitOfWork.Session.Query<CBAddress>(@"
                    SELECT * FROM CBAddresses WHERE CollectionId=@id", new { id }, UnitOfWork.Transaction).ToList();
        }
        private List<CBIdentification> GetIdentifications(long id)
        {
            return UnitOfWork.Session.Query<CBIdentification>(@"SELECT * FROM CBIdentifications  WHERE CollectionId=@id", new { id }, UnitOfWork.Transaction).ToList();
        }

        public CBContract Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CBContract> List(ListQuery listQuery, object query = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var batchId = query?.Val<int?>("BatchId");

            var pre = "DeleteDate IS NULL";

            pre += batchId.HasValue ? " AND CBBatchId = @batchId" : string.Empty;

            var condition = new ListQuery().Like(pre);

            return UnitOfWork.Session.Query<CBContract>($@"SELECT * FROM CBContracts {condition}", new { batchId }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM CBContracts", UnitOfWork.Transaction);
        }
    }
}

