using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.OnlineApplications;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public class OnlineApplicationRepository : RepositoryBase, IRepository<OnlineApplication>
    {
        private readonly OnlineApplicationCarRepository _onlineApplicationCarRepository;
        private readonly OnlineApplicationPositionRepository _onlineApplicationPositionRepository;

        public OnlineApplicationRepository(
            IUnitOfWork unitOfWork,
            OnlineApplicationCarRepository onlineApplicationCarRepository,
            OnlineApplicationPositionRepository onlineApplicationPositionRepository
            ) : base(unitOfWork)
        {
            _onlineApplicationCarRepository = onlineApplicationCarRepository;
            _onlineApplicationPositionRepository = onlineApplicationPositionRepository;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public OnlineApplication Find(object query)
        {
            throw new NotImplementedException();
        }

        public OnlineApplication Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(OnlineApplication entity)
        {
            using (var transaction = BeginTransaction())
            {
                if (entity.Position != null)
                    InsertPosition(entity);

                InternalInsert(entity);
                transaction.Commit();
            }
        }

        public void InternalInsert(OnlineApplication entity)
        {
            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO OnlineApplications ( ContractNumber, Status, CreateDate, MaturityDate, BranchId, Period, LoanCost, LTV, WithInsurance, ClientId, ClientDocumentId,
       OnlinePositionId, SettingId, CreditLineSettingId, PartnerCode, CreditLineId, IsOpeningCreditLine, Source, UtmTags )
VALUES ( @ContractNumber, @Status, @CreateDate, @MaturityDate, @BranchId, @Period, @LoanCost, @LTV, @WithInsurance, @ClientId, @ClientDocumentId,
       @OnlinePositionId, @SettingId, @CreditLineSettingId, @PartnerCode, @CreditLineId, @IsOpeningCreditLine, @Source, @UtmTags )

SELECT SCOPE_IDENTITY()",
            entity, UnitOfWork.Transaction);
        }

        public List<OnlineApplication> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(OnlineApplication entity)
        {
            using (var transaction = BeginTransaction())
            {
                if (entity.Position != null)
                {
                    if (entity.Position.Id > 0)
                        UpdatePosition(entity.Position);
                    else
                        InsertPosition(entity);
                }

                if (entity.OnlineApplicationRefinances.Count != 0)
                {
                    foreach (var refinance in entity.OnlineApplicationRefinances)
                    {
                        if (refinance.Id == 0)
                            InsertRefinance(refinance);
                        else
                            UpdateRefinance(refinance);
                    }
                }

                InternalUpdate(entity);
                transaction.Commit();
            }
        }

        public void InternalUpdate(OnlineApplication entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE OnlineApplications
   SET Status = @Status, MaturityDate = @MaturityDate, BranchId = @BranchId, Period = @Period, LoanCost = @LoanCost, LTV = @LTV,
       WithInsurance = @WithInsurance, ClientId = @ClientId, ClientDocumentId = @ClientDocumentId, OnlinePositionId = @OnlinePositionId,
       SettingId = @SettingId, CreditLineSettingId = @CreditLineSettingId, PartnerCode = @PartnerCode, FirstPaymentDate = @FirstPaymentDate,
       PayDay = @PayDay, ContractId = @ContractId, CreditLineId = @CreditLineId, IsOpeningCreditLine = @IsOpeningCreditLine 
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }

        public async Task<OnlineApplication> FindByContractIdAsync(object query)
        {
            if (query == null)
                throw new ArgumentException(nameof(query));

            var predicate = string.Empty;
            var contractId = query?.Val<string>("ContractId");

            if (!string.IsNullOrEmpty(contractId))
                predicate = " WHERE ContractId = @ContractId";
            else
                return null;

            var application = (await UnitOfWork.Session
                .QueryAsync<OnlineApplication, OnlineApplicationPosition, OnlineApplicationCar, OnlineApplication>(
$@"SELECT TOP 1 oa.*, oap.*, oac.*
  FROM OnlineApplications oa
  LEFT JOIN OnlineApplicationPositions oap ON oap.Id = oa.OnlinePositionId
  LEFT JOIN OnlineApplicationCars oac ON oac.Id = oa.OnlinePositionId
 {predicate}",
                (oa, oap, oac) =>
                {
                    if (oap != null)
                    {
                        oa.Position = oap;

                        if (oac != null)
                        {
                            oa.Position.Car = oac;
                        }
                    }
                    return oa;
                },
                new { contractId }, UnitOfWork.Transaction)).FirstOrDefault();
            if (application != null)
            {
                application.OnlineApplicationRefinances = UnitOfWork.Session.Query<OnlineApplicationRefinance>(@"
SELECT * FROM OnlineApplicationRefinances WHERE contractId = @contractId", new { application.ContractId },
                    UnitOfWork.Transaction).Where(refinance => refinance.DeleteDate is null).ToList();
            }
            return application;
        }

        public async Task<OnlineApplication> FindAsync(object query)
        {
            if (query == null)
                throw new ArgumentException(nameof(query));

            var predicate = string.Empty;
            var contractNumber = query?.Val<string>("ContractNumber");

            if (!string.IsNullOrEmpty(contractNumber))
                predicate = " WHERE ContractNumber = @contractNumber";
            else
                return null;

            var application = (await UnitOfWork.Session
                .QueryAsync<OnlineApplication, OnlineApplicationPosition, OnlineApplicationCar, OnlineApplication>(
                    $@"SELECT TOP 1 oa.*, oap.*, oac.*
  FROM OnlineApplications oa
  LEFT JOIN OnlineApplicationPositions oap ON oap.Id = oa.OnlinePositionId
  LEFT JOIN OnlineApplicationCars oac ON oac.Id = oa.OnlinePositionId
 {predicate}",
                    (oa, oap, oac) =>
                    {
                        if (oap != null)
                        {
                            oa.Position = oap;

                            if (oac != null)
                            {
                                oa.Position.Car = oac;
                            }
                        }
                        return oa;
                    },
                    new { contractNumber }, UnitOfWork.Transaction)).FirstOrDefault();
            if (application != null)
            {
                application.OnlineApplicationRefinances = UnitOfWork.Session.Query<OnlineApplicationRefinance>(@"
SELECT * FROM OnlineApplicationRefinances WHERE ContractNumber = @ContractNumber", new { application.ContractNumber },
                    UnitOfWork.Transaction).Where(refinance => refinance.DeleteDate is null).ToList();
            }
            return application;
        }

        private void InsertPosition(OnlineApplication entity)
        {
            if (entity.Position == null)
                return;

            _onlineApplicationPositionRepository.InternalInsert(entity.Position);

            entity.OnlinePositionId = entity.Position.Id;

            if (entity.Position.CollateralType == CollateralType.Car && entity.Position.Car != null)
            {
                entity.Position.Car.Id = entity.Position.Id;
                _onlineApplicationCarRepository.InternalInsert(entity.Position.Car);
            }
        }

        private void UpdatePosition(OnlineApplicationPosition position)
        {
            if (position == null)
                return;

            _onlineApplicationPositionRepository.InternalUpdate(position);

            if (position.CollateralType == CollateralType.Car && position.Car != null)
            {
                _onlineApplicationCarRepository.InternalUpdate(position.Car);
            }
        }

        private void InsertRefinance(OnlineApplicationRefinance entity)
        {
            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            INSERT INTO OnlineApplicationRefinances ( ContractNumber, RefinancedContractNumber, ContractId, RefinancedContractId, CreateDate, DeleteDate )
            VALUES ( @ContractNumber, @RefinancedContractNumber, @ContractId, @RefinancedContractId, @CreateDate, @DeleteDate  )

            SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);
        }

        private void UpdateRefinance(OnlineApplicationRefinance entity)
        {
            UnitOfWork.Session.Execute(@"
            UPDATE OnlineApplicationRefinances
               SET ContractNumber = @ContractNumber, RefinancedContractNumber = @RefinancedContractNumber, ContractId = @ContractId, 
                RefinancedContractId = @RefinancedContractId, DeleteDate = @DeleteDate
             WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }
    }
}
