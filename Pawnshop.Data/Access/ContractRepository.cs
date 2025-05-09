using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Data.Models.Contracts.Query;
using Pawnshop.Data.Models.Contracts.Views;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.Postponements;
using Pawnshop.Data.Models.Transfers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models;
using Account = Pawnshop.Data.Models.AccountingCore.Account;
using SRegex = System.Text.RegularExpressions;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts.Query;
using Pawnshop.Data.Models.Contracts.Views;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Data.Models.ClientDeferments;
using Pawnshop.Data.Models.Notifications;
using System.Collections;


namespace Pawnshop.Data.Access
{
    public class ContractRepository : RepositoryBase, IRepository<Contract>
    {
        private readonly ClientRepository _clientRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly LoanProductTypeRepository _loanProductTypeRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly GroupRepository _groupRepository;
        private readonly ContractRateRepository _contractRateRepository;
        private readonly PositionEstimatesRepository _positionEstimatesRepository;
        private readonly RealtyAddressRepository _realtyAddressRepository;
        private readonly RealtyDocumentsRepository _realtyDocumentsRepository;
        private readonly PositionEstimateHistoryRepository _positionEstimateHistoryRepository;
        private readonly CreditLineRepository _creditLineRepository;
        private readonly ClientDefermentRepository _clientDefermentRepository;

        public ContractRepository(
            IUnitOfWork unitOfWork,
            LoanPercentRepository loanPercentRepository,
            ClientRepository clientRepository,
            LoanSubjectRepository loanSubjectRepository,
            LoanProductTypeRepository loanProductTypeRepository,
            GroupRepository groupRepository,
            ContractRateRepository contractRateRepository,
            PositionEstimatesRepository positionEstimatesRepository,
            RealtyAddressRepository realtyAddressRepository,
            RealtyDocumentsRepository realtyDocumentsRepository,
            PositionEstimateHistoryRepository positionEstimateHistoryRepository,
            CreditLineRepository creditLineRepository,
            ClientDefermentRepository clientDefermentRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
            _loanPercentRepository = loanPercentRepository;
            _groupRepository = groupRepository;
            _contractRateRepository = contractRateRepository;
            _positionEstimatesRepository = positionEstimatesRepository;
            _realtyAddressRepository = realtyAddressRepository;
            _realtyDocumentsRepository = realtyDocumentsRepository;
            _positionEstimateHistoryRepository = positionEstimateHistoryRepository;
            _creditLineRepository = creditLineRepository;
            _clientDefermentRepository = clientDefermentRepository;
        }

        public void Insert(Contract entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Contracts
( ClientId, ContractNumber, ContractDate, CollateralType, PercentPaymentType, MaturityDate, OriginalMaturityDate, EstimatedCost,
LoanCost, LoanPeriod, LoanPercent, LoanPercentCost, Note, ContractData, ContractSpecific, OwnerId, Status,
ProlongDate, BranchId, AuthorId, Locked, PartialPaymentParentId, FirstPaymentDate, AttractionChannelId, AnnuityType, SignDate, ParentId, APR, ClosedParentId, RequiredInitialFee, PayedInitialFee, ProductTypeId, SettingId, MinimalInitialFee, LoanPurposeId, OtherLoanPurpose, ContractTypeId, PeriodTypeId, NextPaymentDate, IsOffBalance, PaymentOrderSchema, SignerId, LCDate, LCDecisionNumber, UsePenaltyLimit, ContractClass, BusinessLoanPurposeId)
VALUES ( @ClientId, @ContractNumber, @ContractDate, @CollateralType, @PercentPaymentType, @MaturityDate, @OriginalMaturityDate, @EstimatedCost,
@LoanCost, @LoanPeriod, @LoanPercent, @LoanPercentCost, @Note, @ContractData, @ContractSpecific, @OwnerId, @Status,
@ProlongDate, @BranchId, @AuthorId, @Locked, @PartialPaymentParentId, @FirstPaymentDate, @AttractionChannelId, @AnnuityType, @SignDate, @ParentId, @APR, @ClosedParentId, @RequiredInitialFee, @PayedInitialFee, @ProductTypeId, @SettingId, @MinimalInitialFee, @LoanPurposeId, @OtherLoanPurpose, @ContractTypeId, @PeriodTypeId, @NextPaymentDate, @IsOffBalance, @PaymentOrderSchema, @SignerId, @LCDate, @LCDecisionNumber, @UsePenaltyLimit, @ContractClass, @BusinessLoanPurposeId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                foreach (var position in entity.Positions)
                {
                    if (position.PositionEstimate != null && position.Position.CollateralType == CollateralType.Realty)
                    {
                        FillPositionEstimate(entity, position);
                        _positionEstimatesRepository.Insert(position.PositionEstimate);
                        position.EstimationId = position.PositionEstimate.Id;
                    }
                    position.ContractId = entity.Id;
                    UnitOfWork.Session.Execute(@"
INSERT INTO ContractPositions
( ContractId, PositionId, PositionCount, LoanCost, CategoryId, Note, PositionSpecific, EstimatedCost, RequiredInitialFee, MinimalInitialFee, EstimationId, CollateralCost )
VALUES ( @ContractId, @PositionId, @PositionCount, @LoanCost, @CategoryId, @Note, @PositionSpecific, @EstimatedCost, @RequiredInitialFee, @MinimalInitialFee, @EstimationId, @CollateralCost )",
                        position, UnitOfWork.Transaction);
                }

                if (entity.Checks != null && entity.Checks.Count > 0)
                {
                    if (entity.Checks != null && entity.Checks.Count > 0)
                    {
                        entity.Checks.ForEach(check =>
                        {
                            check.ContractId = entity.Id;
                            check.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                                INSERT INTO ContractCheckValues
                                       ( ContractId, CheckId, Value, BeginDate, EndDate, AuthorId, CreateDate)
                                VALUES ( @ContractId, @CheckId, @Value, @BeginDate, @EndDate, @AuthorId, @CreateDate )
                                SELECT SCOPE_IDENTITY()",
                                check, UnitOfWork.Transaction);
                        });
                    }
                }

                if (entity.Subjects != null && entity.Subjects.Count > 0)
                {

                    foreach (ContractLoanSubject subject in entity.Subjects)
                    {
                        subject.ContractId = entity.Id;
                        subject.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                            INSERT INTO ContractLoanSubjects
                                ( SubjectId, ContractId, ClientId, AuthorId, CreateDate )
                            VALUES ( @SubjectId, @ContractId, @ClientId, @AuthorId, @CreateDate )
                            SELECT SCOPE_IDENTITY()",
                            subject, UnitOfWork.Transaction);
                    }
                }

                if (entity.ContractRates != null && entity.ContractRates.Any())
                {
                    entity.ContractRates.ForEach(rate => rate.ContractId = entity.Id);

                    _contractRateRepository.DeleteAndInsert(entity.ContractRates, entity.Setting?.IsFloatingDiscrete);
                    entity.ContractRates = _contractRateRepository.List(new ListQuery(), new { ContractId = entity.Id });
                }

                if (entity.ContractClass == ContractClass.Credit)
                {
                    UnitOfWork.Session.Query("Insert into dogs.Loans values (@ContractId)", new { ContractId = entity.Id }, UnitOfWork.Transaction);
                }
                else if (entity.ContractClass == ContractClass.CreditLine)
                {
                    var creditLineSettings = _loanPercentRepository.Get(entity.SettingId.Value);
                    entity.CreditLineId = UnitOfWork.Session.ExecuteScalar<int>(@"
                        Insert into dogs.CreditLines values (@ContractId, 1, 30, @IsLiquidityOn, @IsInsuranceAdditionalLimitOn) 
                        SELECT SCOPE_IDENTITY()",
                    new
                    {
                        ContractId = entity.Id,
                        IsLiquidityOn = creditLineSettings.IsLiquidityOn,
                        IsInsuranceAdditionalLimitOn = creditLineSettings.IsInsuranceAdditionalLimitOn
                    }, UnitOfWork.Transaction);
                }
                else if (entity.ContractClass == ContractClass.Tranche)
                {
                    var trancheSettings = _loanPercentRepository.Get(entity.SettingId.Value);
                    UnitOfWork.Session.Query(@"
                        Insert into dogs.Tranches (Id, CreditLineId, MaxCreditLineCost, IsLiquidityOn, IsInsuranceAdditionalLimitOn) 
                        values (@ContractId, @CreditLineId, @MaxCreditLineCost, @IsLiquidityOn, @IsInsuranceAdditionalLimitOn)",
                        new
                        {
                            ContractId = entity.Id,
                            CreditLineId = entity.CreditLineId,
                            MaxCreditLineCost = entity.MaxCreditLineCost,
                            IsLiquidityOn = trancheSettings.IsLiquidityOn,
                            IsInsuranceAdditionalLimitOn = trancheSettings.IsInsuranceAdditionalLimitOn
                        }, UnitOfWork.Transaction);
                }

                if (entity.DebtGracePeriod.HasValue && entity.DebtGracePeriod.Value != 0)
                    SetDebtGracePeriodForContract(entity.Id, entity.DebtGracePeriod.Value);

                transaction.Commit();
            }
        }

        private void FillPositionEstimate(Contract entity, ContractPosition position)
        {
            var estimate = position.PositionEstimate;
            estimate.AuthorId = entity.AuthorId;
            estimate.CreateDate = DateTime.Now;
            estimate.PositionId = position.PositionId;
        }

        public void ContractStatusUpdate(int id, ContractStatus status)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                UPDATE Contracts
                SET Status = @status
                WHERE Id = @Id", new { id, status }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Contract entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Contracts
SET ClientId = @ClientId, ContractDate = @ContractDate, CollateralType = @CollateralType,
PercentPaymentType = @PercentPaymentType, MaturityDate = @MaturityDate, OriginalMaturityDate = @OriginalMaturityDate, EstimatedCost = @EstimatedCost,
LoanCost = @LoanCost, LoanPeriod = @LoanPeriod, LoanPercent = @LoanPercent, LoanPercentCost = @LoanPercentCost,
Note = @Note, ContractData = @ContractData, ContractSpecific = @ContractSpecific, OwnerId = @OwnerId,
Status = @Status, ProlongDate = @ProlongDate, BranchId = @BranchId, AuthorId = @AuthorId, Locked = @Locked,
PartialPaymentParentId = @PartialPaymentParentId, PoolNumber = @PoolNumber, BuyoutDate = @BuyoutDate, FirstPaymentDate = @FirstPaymentDate, AttractionChannelId = @AttractionChannelId,
AnnuityType = @AnnuityType, SignDate = @SignDate, ParentId = @ParentId, APR = @APR, ClosedParentId = @ClosedParentId, RequiredInitialFee = @RequiredInitialFee, PayedInitialFee = @PayedInitialFee,
ProductTypeId = @ProductTypeId, SettingId = @SettingId, MinimalInitialFee = @MinimalInitialFee, LoanPurposeId = @LoanPurposeId, OtherLoanPurpose = @OtherLoanPurpose,
ContractTypeId = @ContractTypeId, PeriodTypeId = @PeriodTypeId, NextPaymentDate = @NextPaymentDate, IsOffBalance = @IsOffBalance, PaymentOrderSchema = @PaymentOrderSchema, SignerId = @SignerId,
LCDate = @LCDate, LCDecisionNumber = @LCDecisionNumber, UsePenaltyLimit = @UsePenaltyLimit, BuyoutReasonId = @BuyoutReasonId, BusinessLoanPurposeId = @BusinessLoanPurposeId, CreatedInOnline = @CreatedInOnline, OkedForIndividualsPurposeId = @OkedForIndividualsPurposeId, TargetPurposeId = @TargetPurposeId,
InscriptionId = @InscriptionId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                UpdatePositions
                    (entity.Id, entity.Positions.ToArray());

                if (entity.Checks != null && entity.Checks.Count > 0)
                {
                    entity.Checks.ForEach(check =>
                    {
                        if (check.Id > 0)
                        {
                            UnitOfWork.Session.Execute(@"
UPDATE ContractCheckValues SET Value = @Value WHERE Id = @Id",
                                check, UnitOfWork.Transaction);
                        }
                        else
                        {
                            check.ContractId = entity.Id;
                            check.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ContractCheckValues
       ( ContractId, CheckId, Value, BeginDate, EndDate, AuthorId, CreateDate)
VALUES ( @ContractId, @CheckId, @Value, @BeginDate, @EndDate, @AuthorId, @CreateDate ) SELECT SCOPE_IDENTITY()", check, UnitOfWork.Transaction);
                        }
                    });
                }

                if (entity.Subjects != null && entity.Subjects.Count > 0)
                {
                    entity.Subjects.ForEach(subject =>
                    {
                        if (subject.Id > 0)
                        {
                            UnitOfWork.Session.Execute(@"UPDATE ContractLoanSubjects SET SubjectId = @SubjectId, ContractId = @ContractId,
                                ClientId = @ClientId, DeleteDate = @DeleteDate  WHERE Id = @Id", subject, UnitOfWork.Transaction);
                        }
                        else
                        {
                            subject.ContractId = entity.Id;

                            subject.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                                INSERT INTO ContractLoanSubjects
                                       ( SubjectId, ContractId, ClientId, AuthorId, CreateDate )
                                VALUES ( @SubjectId, @ContractId, @ClientId, @AuthorId, @CreateDate )
                                SELECT SCOPE_IDENTITY()", subject, UnitOfWork.Transaction);
                        }
                    });
                }

                if (entity.ContractRates != null && entity.ContractRates.Any())
                {
                    entity.ContractRates.ForEach(rate => rate.ContractId = entity.Id);

                    _contractRateRepository.DeleteAndInsert(entity.ContractRates, entity.Setting?.IsFloatingDiscrete);
                    entity.ContractRates = _contractRateRepository.List(new ListQuery(), new { ContractId = entity.Id });
                }
                if (entity.ContractClass == ContractClass.Tranche)
                {
                    UnitOfWork.Session.Query(@"
                    UPDATE dogs.Tranches
                    SET
                        MaxCreditLineCost = @MaxCreditLineCost
                    WHERE
                        Id = @ContractId",
                    new
                    {
                        ContractId = entity.Id,
                        CreditLineId = entity.CreditLineId,
                        MaxCreditLineCost = entity.MaxCreditLineCost
                    }, UnitOfWork.Transaction);
                }

                if (entity.DebtGracePeriod.HasValue && entity.DebtGracePeriod.Value != 0)
                    SetDebtGracePeriodForContract(entity.Id, entity.DebtGracePeriod.Value);

                transaction.Commit();
            }
        }

        public void UpdatePositions(int contractId, ContractPosition[] positions)
        {
            var positionIds = positions.Select(p => p.Id).ToList();
            positionIds.Add(0);
            UnitOfWork.Session.Execute(@"
DELETE FROM ContractPositions WHERE ContractId = @contractId AND Id NOT IN @positionIds",
                new { contractId, positionIds = positionIds.ToArray() }, UnitOfWork.Transaction);

            foreach (var position in positions)
            {
                if (position.PositionEstimate != null)
                {
                    UpdatePositionEstimates(contractId, position);
                }
                position.ContractId = contractId;
                position.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
IF NOT EXISTS (SELECT Id FROM ContractPositions WHERE Id = @Id)
BEGIN
    INSERT INTO ContractPositions
    ( ContractId, PositionId, PositionCount, LoanCost, CategoryId, Note, PositionSpecific, EstimatedCost, RequiredInitialFee, MinimalInitialFee, EstimationId, CollateralCost )
    VALUES ( @ContractId, @PositionId, @PositionCount, @LoanCost, @CategoryId, @Note, @PositionSpecific, @EstimatedCost, @RequiredInitialFee, @MinimalInitialFee, @EstimationId, @CollateralCost )
    SELECT SCOPE_IDENTITY()
END
ELSE
BEGIN
    UPDATE ContractPositions
    SET ContractId = @ContractId, PositionId = @PositionId, PositionCount = @PositionCount,
    LoanCost = @LoanCost, CategoryId = @CategoryId, Note = @Note, PositionSpecific = @PositionSpecific, EstimatedCost = @EstimatedCost,
    RequiredInitialFee = @RequiredInitialFee, MinimalInitialFee = @MinimalInitialFee, EstimationId = @EstimationId, CollateralCost = @CollateralCost
    WHERE Id = @Id
    SELECT @Id
END", position, UnitOfWork.Transaction);
            }
        }

        private void UpdatePositionEstimates(int contractId, ContractPosition position)
        {
            if (position.PositionEstimate.Id > 0)
                _positionEstimatesRepository.Update(position.PositionEstimate);
            else
            {
                FillPositionEstimate(GetOnlyContract(contractId), position);
                _positionEstimatesRepository.Insert(position.PositionEstimate);
                position.EstimationId = position.PositionEstimate.Id;
            }
        }

        public List<Contract> GetContractsByPaymentScheduleFilter(DateTime? fromDate, DateTime? endDate, IEnumerable<ContractStatus> contractStatuses, IEnumerable<PercentPaymentType> neededPercentPaymentTypes = null)
        {
            if (!fromDate.HasValue && !endDate.HasValue)
                throw new ArgumentException($"Один из аргументов {nameof(fromDate)}, {nameof(endDate)} должен быть заполнен");

            var innerConditions = new List<string>();
            if (fromDate.HasValue)
                innerConditions.Add("cps.Date >= @fromDate");

            if (endDate.HasValue)
                innerConditions.Add("cps.Date <= @endDate");

            var outerConditions = new List<string>();
            if (contractStatuses != null && contractStatuses.Any())
                outerConditions.Add("c.Status IN @contractStatuses");

            if (neededPercentPaymentTypes != null)
                outerConditions.Add("c.PercentPaymentType IN @neededPercentPaymentTypes");

            string innerCondition = string.Join(" AND ", innerConditions);
            string outerCondition = outerConditions.Any() ? "AND " + string.Join(" AND ", outerConditions) : string.Empty;

            return UnitOfWork.Session.Query<Contract>($@"
                SELECT c.*, tr.CreditLineId, tr.MaxCreditLineCost FROM Contracts c
                left join dogs.Tranches tr on c.Id = tr.Id
                WHERE c.Id IN (
                    SELECT cps.ContractId FROM ContractPaymentSchedule cps
                    WHERE cps.DeleteDate IS NULL AND cps.Canceled IS NULL 
                    AND cps.ActualDate IS NULL
                    AND cps.ActionId IS NULL AND {innerCondition} GROUP BY cps.ContractId
                ) AND c.DeleteDate IS NULL {outerCondition}",
                new { fromDate, endDate, contractStatuses, neededPercentPaymentTypes }, UnitOfWork.Transaction).ToList();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Contracts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id",
                    new { id }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
UPDATE ContractPositions SET DeleteDate = dbo.GETASTANADATE() WHERE ContractId = @id",
                    new { id }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
UPDATE ContractPaymentSchedule SET DeleteDate = dbo.GETASTANADATE() WHERE ContractId = @id",
                    new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UndoDelete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Contracts SET DeleteDate = NULL WHERE Id = @id",
                    new { id }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
UPDATE ContractPositions SET DeleteDate = NULL WHERE ContractId = @id",
                    new { id }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
UPDATE ContractPaymentSchedule SET DeleteDate = NULL WHERE ContractId = @id",
                    new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Contract GetOnlyContract(int id)
        {
            var entity = UnitOfWork.Session.Query<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.DeleteDate IS NULL
   AND c.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            entity.ClientDeferment = _clientDefermentRepository.GetContractDeferment(id);
            if (entity.ClientDeferment != null)
            {
                entity.IsContractRestructured = entity.ClientDeferment.Status == RestructuringStatusEnum.Restructured;
            }
            return entity;
        }

        public async Task<Contract> GetOnlyContractAsync(int id)
        {
            var entity = await UnitOfWork.Session.QueryFirstOrDefaultAsync<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.DeleteDate IS NULL
   AND c.Id = @id", new { id }, UnitOfWork.Transaction);

            entity.ClientDeferment = _clientDefermentRepository.GetContractDeferment(id);
            if (entity.ClientDeferment != null)
            {
                entity.IsContractRestructured = entity.ClientDeferment.Status == RestructuringStatusEnum.Restructured;
            }
            return entity;
        }

        public Contract GetContractWithSubject(int id)
        {
            var entity = UnitOfWork.Session.Query<Contract, Client, Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod,
       cl.*
  FROM Contracts c
  JOIN Clients cl ON c.ClientId = cl.Id
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.Id = @id",
            (c, cl) =>
            {
                c.Client = cl;
                return c;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity != null)
            {
                entity.Subjects = UnitOfWork.Session.Query<ContractLoanSubject, Client, ClientLegalForm, LoanSubject, ContractLoanSubject>(
                    @"SELECT sub.*, cl.*, lg.*, ls.*
                    FROM ContractLoanSubjects sub WITH(NOLOCK)
                    JOIN LoanSubjects ls ON ls.Id = sub.SubjectId
                    JOIN Clients cl ON sub.ClientId = cl.Id
                    JOIN ClientLegalForms lg ON lg.Id = cl.LegalFormId
                    WHERE sub.ContractId = @id
                    AND sub.DeleteDate IS NULL",
                    (sub, cl, lg, ls) =>
                    {
                        sub.Client = cl;
                        sub.Client.LegalForm = lg;
                        sub.Subject = ls;
                        return sub;
                    },
                    new { id }, UnitOfWork.Transaction).ToList();
            }
            return entity;
        }

        public List<ContractPaymentSchedule> GetOnlyPaymentSchedule(int contractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                SELECT * 
                  FROM ContractPaymentSchedule cps
                WHERE cps.ContractId = @contractId
                  AND cps.DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public Contract Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Contract, User, CollectionContractStatus, Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod,
       u.*,
       ccs.*
  FROM Contracts c WITH(NOLOCK)
  JOIN Users u ON c.AuthorId = u.Id
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
  LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id AND ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
 WHERE c.Id = @id",
                (c, u, ccs) =>
                {
                    c.Author = u;
                    if (c.ProductTypeId.HasValue)
                        c.ProductType = _loanProductTypeRepository.Get(c.ProductTypeId.Value);
                    if (c.SettingId.HasValue)
                        c.Setting = _loanPercentRepository.Get(c.SettingId.Value);
                    if (ccs != null)
                    {
                        c.CollectionStatusCode = ccs.CollectionStatusCode;
                        c.DelayDays = DateTime.Now.Subtract(ccs.StartDelayDate.Date).Days > 0 ? DateTime.Now.Subtract(ccs.StartDelayDate.Date).Days : 0;
                    }
                    return c;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                throw new PawnshopApplicationException($"Договор {id} не найден");

            entity.Client = _clientRepository.Get(entity.ClientId);

            entity.Branch = _groupRepository.Get(entity.BranchId);

            var positions = new List<ContractPosition>();


            if (entity.ContractClass == ContractClass.CreditLine && entity.Branch.Name != "TSO")
            {
                entity.LeftLoanCost = GetAllSignedTranches(id).Result.Sum(x => x.LoanCost);
            }
            else if (entity.ContractClass == ContractClass.Tranche && entity.Branch.Name != "TSO")
            {
                entity.LeftLoanCost = GetAllSignedTranches(entity.CreditLineId.Value).Result.Sum(x => x.LoanCost);
            }

            var contractPositions = UnitOfWork.Session.Query<ContractPosition, Position, ContractPosition>(@"
SELECT cp.*, p.* FROM ContractPositions cp
JOIN Positions p ON p.Id = cp.PositionId
WHERE cp.DeleteDate IS NULL
AND cp.ContractId = @id",
(cp, p) =>
{
    cp.Position = p;
    return cp;
},
new { id }, UnitOfWork.Transaction).ToList();

            foreach (var contractPosition in contractPositions)
            {
                if (contractPosition.Position.CollateralType == CollateralType.Car)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Car, VehicleMark, VehicleModel, Category, Application, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Cars.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Cars.Mark,
                        Cars.Model,
                        Cars.ReleaseYear,
                        Cars.TransportNumber,
                        Cars.MotorNumber,
                        Cars.BodyNumber,
                        Cars.TechPassportNumber,
                        Cars.TechPassportDate,
                        Cars.Color,
                        Cars.ParkingStatusId,
                        Cars.VehicleMarkId,
	                    Cars.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        Categories.*,
                        Applications.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Cars ON Cars.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Cars.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Cars.VehicleModelId
                    LEFT JOIN Applications ON Applications.AppId in (SELECT TOP 1 AppId from Applications WHERE PositionId = ContractPositions.PositionId ORDER BY AppId DESC)
                    WHERE ContractPositions.Id = @contractPositionId",
                    (cp, p, m, mod, c, a) =>
                    {
                        cp.Position = p;
                        p.VehicleMark = m;
                        p.VehicleModel = mod;
                        cp.Category = c;
                        if (cp.Position.ClientId.HasValue)
                            cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                        if (a != null)
                        {
                            cp.TurboCost = a.TurboCost;
                            cp.MotorCost = a.MotorCost;
                        }
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else if (contractPosition.Position.CollateralType == CollateralType.Realty)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Realty, Category, PositionEstimate, DomainValue, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Realties.*,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Categories.*,
                        pe.*,
                        dv.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Realties ON Realties.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    LEFT JOIN PositionEstimates pe ON ContractPositions.EstimationId = pe.Id
                    LEFT JOIN DomainValues dv ON dv.Id = Realties.RealtyTypeId
                    WHERE ContractPositions.Id = @contractPositionId",
                    (cp, r, c, pe, dv) =>
                    {
                        r.Address = _realtyAddressRepository.Get(r.Id);
                        r.RealtyDocuments = _realtyDocumentsRepository.GetDocumentsForRealty(r.Id);
                        cp.Position = r;
                        r.RealtyType = dv;
                        cp.Category = c;
                        if (cp.EstimationId.HasValue)
                        {
                            cp.PositionEstimate = _positionEstimatesRepository.Get(cp.EstimationId.Value);
                        }
                        cp.PositionEstimateHistory = _positionEstimateHistoryRepository.ListEstimationHistoryForPosition(r.Id);
                        if (cp.Position.ClientId.HasValue)
                            cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else if (contractPosition.Position.CollateralType == CollateralType.Machinery)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Machinery, VehicleMark, VehicleModel, VehicleLiquidity, Category, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Machineries.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Machineries.Mark,
                        Machineries.Model,
                        Machineries.ReleaseYear,
                        Machineries.TransportNumber,
                        Machineries.MotorNumber,
                        Machineries.BodyNumber,
                        Machineries.TechPassportNumber,
                        Machineries.TechPassportDate,
                        Machineries.Color,
                        Machineries.ParkingStatusId,
                        Machineries.VehicleMarkId,
	                    Machineries.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        VehicleLiquid.*,
                        Categories.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Machineries ON Machineries.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Machineries.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Machineries.VehicleModelId
                    WHERE ContractPositions.Id = @contractPositionId",
                (cp, p, m, mod, vl, c) =>
                    {
                        cp.Position = p;
                        p.VehicleMark = m;
                        p.VehicleModel = mod;
                        cp.Category = c;
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Position, Category, ContractPosition>(@"
SELECT *
FROM ContractPositions WITH(NOLOCK)
JOIN Positions ON Positions.Id = ContractPositions.PositionId
JOIN Categories ON ContractPositions.CategoryId = Categories.Id
WHERE ContractPositions.Id = @contractPositionId", (cp, p, c) =>
                    {
                        cp.Position = p;
                        cp.Category = c;
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
            }
            entity.Positions = positions;

            entity.Files = UnitOfWork.Session.Query<FileRow>(@"
SELECT FileRows.*
  FROM ContractFileRows WITH(NOLOCK)
  JOIN FileRows ON ContractFileRows.FileRowId = FileRows.Id
 WHERE DeleteDate IS NULL AND ContractFileRows.ContractId = @id", new { id }, UnitOfWork.Transaction).ToList();

            entity.Actions = UnitOfWork.Session.Query<ContractAction>(@"
SELECT *
FROM ContractActions WITH(NOLOCK)
WHERE ContractId = @id AND DeleteDate IS NULL
ORDER BY Date, CreateDate", new { id }, UnitOfWork.Transaction).ToList();
            foreach (var action in entity.Actions)
            {
                if (action.Data != null && action.Data.CategoryChanged)
                    action.CategoryChanged = true;

                action.Rows = UnitOfWork.Session.Query<ContractActionRow, Account, Account, ContractActionRow>(@"
                SELECT car.*, da.*, ca.*
                FROM ContractActionRows car
                LEFT JOIN Accounts da ON car.DebitAccountId = da.Id
                LEFT JOIN Accounts ca ON car.CreditAccountId = ca.Id
                WHERE car.ActionId = @id", (row, da, ca) =>
                {
                    row.DebitAccount = da;
                    row.CreditAccount = ca;
                    return row;
                }, new { id = action.Id }, UnitOfWork.Transaction).ToArray();

                action.Files = UnitOfWork.Session.Query<FileRow>(@"
                SELECT fr.* FROM FileRows fr WITH(NOLOCK)
                JOIN ContractFileRows cfr ON cfr.FileRowId=fr.Id
                WHERE cfr.DeleteDate IS NULL AND cfr.ActionId= @id", new { id = action.Id }, UnitOfWork.Transaction).ToList();

                action.Checks = UnitOfWork.Session.Query<ContractActionCheckValue, ContractActionCheck, ContractActionCheckValue>(@"
                SELECT val.*, ch.* FROM ContractActionCheckValues val WITH(NOLOCK)
                LEFT JOIN ContractActionChecks ch ON ch.Id = val.CheckId
                WHERE val.ActionId = @id",
                (val, ch) =>
                {
                    val.Check = ch;
                    return val;
                },
                new { id = action.Id }, UnitOfWork.Transaction).ToList();

                if (action.ExpenseId.HasValue)
                {
                    action.Expense = UnitOfWork.Session.Query<ContractExpense>(@"
SELECT *
FROM ContractExpenses1 WITH(NOLOCK)
WHERE Id = @id", new { id = action.ExpenseId }, UnitOfWork.Transaction).FirstOrDefault();
                }

                action.Discount = new ContractDutyDiscount();
                action.Discount.Discounts = UnitOfWork.Session.Query<Discount, Blackout, ContractDiscount, PersonalDiscount, Category, Discount>($@"
                SELECT d.*, b.*, cd.*, pd.*, cat.*
                  FROM Discounts d WITH(NOLOCK)
                  LEFT JOIN Blackouts b ON b.Id = d.BlackoutId
                  LEFT JOIN ContractDiscounts cd ON cd.Id = d.ContractDiscountId
                  LEFT JOIN PersonalDiscounts pd ON pd.Id = cd.PersonalDiscountId
                  LEFT JOIN Categories cat ON cat.Id = pd.CategoryId
                WHERE d.ActionId = @id",//todo AND d.DeleteDate IS NULL
                    (d, b, cd, pd, cat) =>
                    {
                        d.Blackout = b;
                        d.ContractDiscount = cd;
                        if (d.ContractDiscount != null)
                        {
                            d.ContractDiscount.PersonalDiscount = pd;
                        }
                        return d;
                    }, new
                    {
                        id = action.Id
                    }, UnitOfWork.Transaction).ToList();
                action.Discount.Discounts.ForEach(discount =>
                {
                    discount.Rows = UnitOfWork.Session.Query<DiscountRow>($@"
                SELECT *
                  FROM DiscountRows WITH(NOLOCK)
                WHERE DiscountId = @id", new
                    {
                        id = discount.Id
                    }, UnitOfWork.Transaction).ToList();
                });
            }

            entity.PaymentSchedule = UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                SELECT *,
                CASE 
                    WHEN ActionId IS NOT NULL AND ((SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 200 
				    OR (SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 201) THEN 1
                    WHEN ActionId IS NULL AND EXISTS (SELECT 1 FROM ClientDeferments cd WHERE cd.ContractId = cps.ContractId AND cd.StartDate < cps.Date AND Status = 10) THEN 2
                    WHEN ActionId IS NOT NULL AND (Select ActionType From ContractActions Where Id = ActionId) = 40 THEN 15
                    WHEN ActionId IS NOT NULL THEN 10
                    WHEN Canceled IS NOT NULL THEN 30	
                    WHEN ISNULL(NextWorkingDate, Date) < CONVERT(DATE, dbo.GETASTANADATE()) THEN 20
                    ELSE 0
                END AS Status
                FROM ContractPaymentSchedule cps WITH(NOLOCK)
                WHERE ContractId = @id AND DeleteDate IS NULL
                ORDER BY Date, Id", new { id }, UnitOfWork.Transaction).ToList();

            entity.ClientDeferment = _clientDefermentRepository.GetContractDeferment(id);
            if (entity.ClientDeferment != null)
            {
                entity.IsContractRestructured = entity.ClientDeferment.Status == RestructuringStatusEnum.Restructured;
            }

            if (entity.IsContractRestructured)
            {
                entity.RestructedPaymentSchedule = UnitOfWork.Session.Query<RestructuredContractPaymentSchedule>(@"
                SELECT *,
                CASE 
                    WHEN ActionId IS NOT NULL AND ((SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 200 
                    OR (SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 201) THEN 1
	                WHEN ActionId IS NULL AND EXISTS (SELECT 1 FROM ClientDeferments cd WHERE cd.ContractId = cps.ContractId AND cd.StartDate < cps.Date AND Status = 10) THEN 2
                    WHEN ActionId IS NOT NULL AND (Select ActionType From ContractActions Where Id = ActionId) = 40 THEN 15
                    WHEN ActionId IS NOT NULL THEN 10
                    WHEN Canceled IS NOT NULL THEN 30	
                    WHEN ISNULL(NextWorkingDate, Date) < CONVERT(DATE, dbo.GETASTANADATE()) THEN 20
                    ELSE 0
                END AS Status
                FROM ContractPaymentSchedule cps WITH(NOLOCK)
                LEFT JOIN RestructuredContractPaymentSchedule rcps on rcps.Id = cps.Id
                WHERE ContractId = @id AND DeleteDate IS NULL
                ORDER BY Date, cps.Id", new { id }, UnitOfWork.Transaction).ToList();
            }

            entity.Expenses = UnitOfWork.Session.Query<ContractExpense>(@"
                SELECT ce.*
                FROM ContractExpenses1 ce WITH(NOLOCK)
                WHERE ce.ContractId = @id AND ce.DeleteDate IS NULL
                ORDER BY ce.Date", new { id }, UnitOfWork.Transaction).ToList();

            foreach (ContractExpense expense in entity.Expenses)
            {
                expense.ContractExpenseRows = UnitOfWork.Session.Query<ContractExpenseRow>(@"
                    SELECT cer.*
                    FROM ContractExpenseRows cer
                    WHERE cer.ContractExpenseId = @contractExpenseId AND cer.DeleteDate IS NULL",
                    new { contractExpenseId = expense.Id }, UnitOfWork.Transaction).ToList();

                foreach (ContractExpenseRow contractExpenseRow in expense.ContractExpenseRows)
                {
                    contractExpenseRow.ContractExpenseRowOrders = UnitOfWork.Session.Query<ContractExpenseRowOrder, CashOrder, Account, Account, ContractExpenseRowOrder>(@"
                    SELECT cero.*, co.*, da.*, ca.*
                    FROM ContractExpenseRowOrders cero
                    INNER JOIN CashOrders co ON co.Id = cero.OrderId
                    LEFT JOIN Accounts da on da.Id = co.DebitAccountId
                    LEFT JOIN Accounts ca on ca.Id = co.CreditAccountId
                    WHERE cero.ContractExpenseRowId = @contractExpenseRowId AND cero.DeleteDate IS NULL",
                    (cero, co, da, ca) =>
                    {
                        if (co != null)
                        {
                            co.DebitAccount = da;
                            co.CreditAccount = ca;
                        }

                        cero.Order = co;
                        return cero;
                    }, new { contractExpenseRowid = contractExpenseRow.Id }, UnitOfWork.Transaction).ToList();
                }
            }

            if (entity.InscriptionId > 0)
            {
                entity.Inscription = GetContractInscription((int)entity.InscriptionId, UnitOfWork);
            }

            entity.Postponements = UnitOfWork.Session.Query<ContractPostponement, Postponement, ContractPostponement>(@"
SELECT cp.*, p.*
FROM ContractPostponements cp WITH(NOLOCK)
LEFT JOIN Postponements p ON p.Id=cp.PostponementId
WHERE cp.ContractId = @id AND cp.DeleteDate IS NULL
ORDER BY cp.Date", (cp, p) =>
            {
                cp.Postponement = p;
                return cp;
            }, new { id }, UnitOfWork.Transaction).ToList();

            entity.Discounts = UnitOfWork.Session.Query<ContractDiscount, PersonalDiscount, ContractDiscount>(@"
SELECT 
CASE WHEN cd.DeleteDate IS NOT NULL THEN 20
WHEN cd.ContractActionId IS NOT NULL THEN 10
WHEN dbo.GETASTANADATE() NOT BETWEEN cd.BeginDate AND cd.EndDate THEN 30
ELSE 0 END AS Status,
cd.*, pd.*
FROM ContractDiscounts cd WITH(NOLOCK)
LEFT JOIN PersonalDiscounts pd ON cd.PersonalDiscountId=pd.Id
WHERE cd.ContractId = @id AND cd.DeleteDate IS NULL
ORDER BY cd.Id", (cd, pd) =>
            {
                if (cd != null)
                {
                    cd.PersonalDiscount = pd;
                }
                return cd;
            }, new { id }, UnitOfWork.Transaction).ToList();

            entity.Checks = UnitOfWork.Session.Query<ContractCheckValue, ContractCheck, User, ContractCheckValue>(@"
                SELECT val.*, ch.*, u.* FROM ContractCheckValues val WITH(NOLOCK)
                LEFT JOIN ContractChecks ch ON ch.Id = val.CheckId
				LEFT JOIN Users u ON val.AuthorId = u.Id
                WHERE val.ContractId = @id",
                (val, ch, u) =>
                {
                    val.Check = ch;
                    val.Author = u;
                    return val;
                },
                new { id = entity.Id }, UnitOfWork.Transaction).ToList();

            entity.Subjects = UnitOfWork.Session.Query<ContractLoanSubject, User, ContractLoanSubject>(@"SELECT sub.*, u.*
FROM ContractLoanSubjects sub WITH(NOLOCK)
LEFT JOIN Users u ON sub.AuthorId = u.Id
WHERE sub.ContractId = @id AND DeleteDate IS NULL", (sub, u) =>
            {
                sub.Author = u;
                sub.Client = _clientRepository.Get(sub.ClientId);
                sub.Subject = _loanSubjectRepository.Get(sub.SubjectId);
                return sub;
            }, new { id }, UnitOfWork.Transaction).ToList();

            entity.ContractTransfers = UnitOfWork.Session.Query<ContractTransfer>(@"
                SELECT *
                FROM ContractTransfers 
                WHERE ContractId = @id", new { id }, UnitOfWork.Transaction).ToList();

            entity.TransferDate = entity.ContractTransfers?.LastOrDefault(x => !x.BackTransferDate.HasValue)?.TransferDate;
            entity.PoolNumber = entity.ContractTransfers?.LastOrDefault(x => !x.BackTransferDate.HasValue)?.PoolNumber;

            if (entity.SignerId.HasValue)
            {
                entity.Signer = UnitOfWork.Session.QuerySingleOrDefault<ClientSigner>(@"
                    SELECT *
                      FROM ClientSigners
                     WHERE Id = @id AND DeleteDate IS NULL", new { id = entity.SignerId }, UnitOfWork.Transaction);
            }

            entity.ContractRates = _contractRateRepository.List(new ListQuery(), new { ContractId = entity.Id });

            return entity;
        }

        public Contract Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var positionId = query?.Val<int?>("PositionId");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var status = query?.Val<ContractStatus?>("Status");
            var contractNumber = query?.Val<string>("ContractNumber");
            var appId = query?.Val<int?>("AppId");

            var condition = @"WHERE cp.DeleteDate IS NULL";

            condition += positionId != null ? " AND cp.PositionId = @positionId" : "";
            condition += collateralType != null ? " AND c.CollateralType = @collateralType" : "";
            condition += status != null ? " AND c.Status = @status" : "";
            condition += !string.IsNullOrWhiteSpace(contractNumber) ? " AND c.ContractNumber = @contractNumber" : "";
            condition += appId != null ? " AND app.AppId = @appId" : string.Empty;

            return UnitOfWork.Session.Query<Contract, LoanPercentSetting, Contract>($@"SELECT TOP 1 c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod,
       lps.*
  FROM Contracts c WITH(NOLOCK)
  LEFT JOIN ContractPositions cp ON cp.ContractId = c.Id
  LEFT JOIN LoanPercentSettings lps ON lps.Id = c.SettingId
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
  {((appId != null) ? @"
  LEFT JOIN ApplicationDetails ad ON ad.ContractId = c.Id
  LEFT JOIN Applications app ON app.Id = ad.ApplicationId
  " : string.Empty)}
 {condition}",
                (c, lps) =>
                {
                    c.Setting = lps;
                    return c;
                },
                new
                {
                    positionId,
                    collateralType,
                    status,
                    contractNumber,
                    appId
                }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public ContractPosition FindContractPositionByBodyNumber(string bodyNumber, string techPassportNumber = "", bool isCreditLine = false)
        {
            var pre = "WHERE car.BodyNumber = @bodyNumber AND c.Status = 30 AND c.DeleteDate IS NULL";

            if (!string.IsNullOrEmpty(techPassportNumber))
                pre += " AND car.TechPassportNumber = @techPassportNumber";

            if (isCreditLine)
                pre += " AND c.ContractClass = 2";

            return UnitOfWork.Session.Query<ContractPosition, Contract, ContractPosition>($@"SELECT cp.*,
       c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Cars car
  JOIN ContractPositions cp ON cp.PositionId = car.Id
  JOIN Contracts c ON c.Id = cp.ContractId
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 {pre};",
                (cp, c) =>
                {
                    cp.Contract = c;

                    return cp;
                }, new { bodyNumber, techPassportNumber }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Contract> FindForProcessing(int clientId)
        {
            var statuses = new List<ContractStatus> { ContractStatus.Signed, ContractStatus.AwaitForInitialFee };

            return UnitOfWork.Session.Query<Contract, User, Contract>($@"
SELECT DISTINCT c.*,
    (CASE 
        WHEN c.DeleteDate IS NOT NULL THEN 60
        WHEN c.Status = 0 THEN 0
        WHEN c.Status = 20 THEN 5
        WHEN c.Status = 24 THEN 24
        WHEN c.Status = 30 AND InscriptionId IS NOT NULL AND i.Status != 20 THEN 25
        WHEN c.Status = 30 AND c.NextPaymentDate < CAST(dbo.GETASTANADATE() AS DATE) THEN 20
        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
        WHEN c.Status = 40 THEN 40
        WHEN c.Status = 50 THEN 50
        WHEN c.Status = 60 THEN 55
        ELSE 0
    END) AS DisplayStatus,
    tr.CreditLineId,
    tr.MaxCreditLineCost,
    cai.DebtGracePeriod,
    u.*
FROM Contracts c
JOIN Users u ON c.AuthorId = u.Id
LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
LEFT JOIN Groups g ON g.Id = c.BranchId
LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
WHERE c.DeleteDate IS NULL AND c.ClientId = @clientId AND c.Status in @statuses AND c.ContractClass != 2",
                (c, u) =>
                {
                    c.Branch = _groupRepository.Get(c.BranchId);
                    c.Author = u;
                    return c;
                },
                new
                {
                    clientId,
                    statuses
                }, UnitOfWork.Transaction).ToList();
        }

        public List<Contract> FindCreditLinesForProcessing(int clientId)
        {
            var statuses = new List<ContractStatus> { ContractStatus.Signed, ContractStatus.AwaitForInitialFee };

            return UnitOfWork.Session.Query<Contract, User, Contract>($@"
SELECT DISTINCT c.*,
    (CASE 
        WHEN c.DeleteDate IS NOT NULL THEN 60
        WHEN c.Status = 0 THEN 0
        WHEN c.Status = 20 THEN 5
        WHEN c.Status = 24 THEN 24
        WHEN c.Status = 30 AND InscriptionId IS NOT NULL AND i.Status != 20 THEN 25
        WHEN c.Status = 30 AND c.NextPaymentDate < CAST(dbo.GETASTANADATE() AS DATE) THEN 20
        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
        WHEN c.Status = 40 THEN 40
        WHEN c.Status = 50 THEN 50
        WHEN c.Status = 60 THEN 55
        ELSE 0
    END) AS DisplayStatus,
    u.*
FROM Contracts c
JOIN Users u ON c.AuthorId = u.Id
LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
WHERE c.DeleteDate IS NULL AND c.ClientId = @clientId AND c.Status in @statuses AND ContractClass = 2",
                (c, u) =>
                {
                    c.Branch = _groupRepository.Get(c.BranchId);
                    c.Author = u;
                    return c;
                },
                new
                {
                    clientId,
                    statuses
                }, UnitOfWork.Transaction).ToList();
        }

        [Obsolete]
        public List<Contract> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var clientId = query?.Val<int?>("ClientId");
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var isTransferred = query?.Val<bool?>("IsTransferred");
            var identityNumber = query?.Val<string>("IdentityNumber");
            var status = query?.Val<ContractStatus?>("Status");
            var organizationId = query?.Val<int?>("OrganizationId");
            var hasParent = query?.Val<bool?>("HasParent");
            var hasFcbChecked = query?.Val<bool?>("HasFcbChecked");
            var statuses = query?.Val<List<ContractStatus>>("Statuses");
            var nextPaymentDate = query?.Val<DateTime?>("NextPaymentDate");
            var nextPaymentEndDate = query?.Val<DateTime?>("NextPaymentEndDate");
            var paymentDate = query?.Val<DateTime?>("PaymentDate");
            var isNotInscription = query?.Val<bool?>("IsNotInscription");
            var allBranches = query?.Val<bool?>("AllBranches");
            var carNumber = query?.Val<string?>("CarNumber");
            var rca = query?.Val<string?>("Rca");
            var settingId = query?.Val<int?>("SettingId");
            var pre = displayStatus.HasValue && displayStatus.Value == ContractDisplayStatus.Deleted ? "c.DeleteDate IS NOT NULL" : "c.DeleteDate IS NULL";

            if (!String.IsNullOrEmpty(identityNumber) || !String.IsNullOrEmpty(carNumber) || !String.IsNullOrEmpty(rca))
            {
                if (!String.IsNullOrEmpty(identityNumber))
                    pre += " AND cl.IdentityNumber = @identityNumber";
                if (!String.IsNullOrEmpty(carNumber))
                    pre += " AND cars.TransportNumber = @carNumber";
                if (!String.IsNullOrEmpty(rca))
                    pre += " AND r.rca = @rca";
            }
            else
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }



            pre += collateralType.HasValue ? " AND c.CollateralType = @collateralType" : string.Empty;
            pre += clientId.HasValue ? " AND c.ClientId = @clientId" : string.Empty;
            pre += organizationId.HasValue ? " AND m.OrganizationId = @organizationId" : string.Empty;
            pre += status.HasValue ? " AND c.Status = @status" : string.Empty;
            pre += statuses != null && statuses.Count > 0 ? " AND c.Status IN @statuses" : string.Empty;
            pre += nextPaymentDate.HasValue ? " AND c.NextPaymentDate = @nextPaymentDate" : string.Empty;
            pre += nextPaymentEndDate.HasValue ? " AND c.NextPaymentDate < @nextPaymentEndDate" : string.Empty;
            pre += paymentDate.HasValue ? " AND c.NextPaymentDate != @paymentDate AND EXISTS (SELECT * FROM ContractPaymentSchedule WHERE ContractId = c.Id AND Date = @paymentDate AND Canceled IS NULL AND DeleteDate IS NULL AND ActionId IS NULL)" : string.Empty;
            pre += isNotInscription.HasValue && isNotInscription.Value ? " AND (c.InscriptionId IS NULL OR NOT EXISTS(SELECT 1 FROM Inscriptions i WHERE i.Id = c.InscriptionId AND i.Status != 20))" : null;
            pre += settingId.HasValue ? " AND c.SettingId = @settingId" : string.Empty;

            if (isTransferred.HasValue && isTransferred.Value) pre += " AND EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";
            else pre += " AND NOT EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";

            if (hasParent.HasValue && hasParent.Value) pre += " AND c.ParentId IS NOT NULL";
            if (hasParent.HasValue && !hasParent.Value) pre += " AND c.ParentId IS NULL";

            //TODO: Добавить проверку на дату окончания действия разрешения, чтобы исключать уже выгруженные закрытые договора
            if (hasFcbChecked.HasValue && hasFcbChecked.Value) pre += " AND EXISTS (SELECT * FROM ContractCheckValues ccv JOIN ContractChecks cc ON ccv.CheckId = cc.Id WHERE ccv.ContractId = c.Id AND ccv.[Value] = 1 AND cc.Code = N'FCB')";


            var from = "FROM Contracts c";
            if (displayStatus.HasValue)
            {
                var buildedFilter = BuildStatusQuery(displayStatus.Value, pre, from, beginDate, endDate);
                pre = buildedFilter.Item1;
                from = buildedFilter.Item2;
            }
            else
            {
                pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
            }

            var condition = listQuery.Like(pre, "c.ContractNumber", "cl.FullName", "cl.FullName", "cl.IdentityNumber", "cl.MobilePhone", "cars.TransportNumber");
            var order = listQuery.Order("c.ContractDate DESC", new Sort
            {
                Name = "c.ContractNumber",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();
            if (page == null) order = null;
            return UnitOfWork.Session.Query<Contract, Group, User, CollectionContractStatus, Contract>($@"

DECLARE @current_date DATE = CONVERT(DATE, dbo.GETASTANADATE());

WITH ContractPaged AS (
    SELECT DISTINCT c.Id,c.ContractNumber,c.ContractDate, i.Status, IIF(Encumbrance.Id > 0, 1, 0) AS HasEncumbrance, (SELECT SUM(acc.Balance)
                                                                    FROM Accounts acc, AccountSettings accSetting
                                                                    WHERE acc.ContractId=c.Id
                                                                    AND acc.AccountSettingId = accSetting.Id
                                                                    AND accSetting.Code in ('PENY_ACCOUNT', 'PENY_PROFIT', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE')
                                                                    GROUP BY acc.ContractId
                                                                    ) AS allPenBal
    {from}
    JOIN Clients cl ON cl.Id = c.ClientId
    JOIN Members m ON m.Id = c.BranchId
    LEFT JOIN Inscriptions i ON c.InscriptionId = i.Id
    LEFT JOIN ContractPositions cpp ON cpp.ContractId = c.Id
    LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
    LEFT JOIN Realties r ON r.Id = cpp.PositionId
    OUTER APPLY(
        SELECT ce.Id FROM ContractExpenses1 ce 
        LEFT JOIN Expenses e ON e.Id = ce.ExpenseId AND e.DeleteDate IS NULL
	    WHERE ce.ContractId = c.Id AND ce.DeleteDate IS NULL
	    AND e.ActionType = 50 AND e.Cost > 0) Encumbrance
    {condition} {order} {page}
)
SELECT DISTINCT c.*,
    (CASE 
        WHEN c.DeleteDate IS NOT NULL THEN 60
        WHEN c.Status = 0 THEN 0
        WHEN c.Status = 20 THEN 5
        WHEN c.Status = 24 THEN 24
        WHEN c.Status = 30 AND InscriptionId IS NOT NULL AND cp.Status!=20 THEN 25
        WHEN c.Status = 30 AND ISNULL(cps.NextPaymentDate, @current_date) < @current_date THEN 20
        WHEN c.Status = 30 AND cp.allPenBal < 0 THEN 20
        WHEN c.Status = 30 AND c.MaturityDate >= @current_date AND c.ProlongDate IS NOT NULL THEN 30
        WHEN c.Status = 30 AND c.MaturityDate >= @current_date AND c.ProlongDate IS NULL THEN 10        
        WHEN c.Status = 40 THEN 40
        WHEN c.Status = 50 THEN 50
        WHEN c.Status = 60 THEN 55
        ELSE 0
    END) AS DisplayStatus,
	(SELECT CONCAT(' ', IIF(cars.Id IS NOT NULL, CONCAT_WS(' ', cars.Mark, cars.Model, REPLACE(cars.TransportNumber, ' ', '')), CONCAT(dv.Name,' ',realty.RCA)),',',CHAR(10)) 
	 FROM ContractPositions cpp 
	 LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
	 LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
     LEFT JOIN DomainValues dv ON dv.Id = realty.RealtyTypeId
	 WHERE cpp.ContractId = c.Id AND cpp.DeleteDate IS NULL 
	 order by cpp.Id DESC
	 FOR XML PATH('')) AS grnzCollat,
	cp.HasEncumbrance,
    g.*,
    u.*,
    ccs.*
FROM ContractPaged cp
JOIN Contracts c ON cp.Id = c.Id
JOIN Groups g ON c.BranchId = g.Id
JOIN Users u ON c.AuthorId = u.Id
LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id
OUTER APPLY (SELECT TOP 1 ISNULL(cps.NextWorkingDate, cps.Date) as NextPaymentDate
               FROM ContractPaymentSchedule cps
              WHERE cps.ContractId = c.Id
                AND cps.DeleteDate IS NULL
                AND cps.ActualDate IS NULL
                AND cps.Canceled IS NULL
                AND cps.Date <= @current_date
              ORDER BY cps.Date) cps
{order}",
                (c, g, u, ccs) =>
                {
                    c.Client = _clientRepository.GetOnlyClient(c.ClientId);
                    c.Branch = g;
                    c.Author = u;
                    if (c.ProductTypeId.HasValue)
                    {
                        c.ProductType = _loanProductTypeRepository.Get(c.ProductTypeId.Value);
                    }
                    if (c.SettingId.HasValue)
                    {
                        c.Setting = _loanPercentRepository.Get(c.SettingId.Value);
                    }
                    c.CollectionStatusCode = ccs != null && ccs.CollectionStatusCode != null ? ccs.CollectionStatusCode : "";
                    c.DelayDays = DateTime.Now.Date.Subtract(c.NextPaymentDate.HasValue ? c.NextPaymentDate.Value : DateTime.Now.Date).Days;
                    return c;
                },
                new
                {
                    beginDate,
                    endDate,
                    collateralType,
                    clientId,
                    ownerIds,
                    identityNumber,
                    status,
                    organizationId,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    statuses,
                    nextPaymentDate,
                    paymentDate,
                    nextPaymentEndDate,
                    carNumber,
                    rca,
                    settingId
                }, UnitOfWork.Transaction, commandTimeout: 1800).ToList();
        }

        [Obsolete]
        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var clientId = query?.Val<int?>("ClientId");
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var isTransferred = query?.Val<bool?>("IsTransferred");
            var identityNumber = query?.Val<string>("IdentityNumber");
            var status = query?.Val<ContractStatus?>("Status");
            var organizationId = query?.Val<int?>("OrganizationId");
            var hasParent = query?.Val<bool?>("HasParent");
            var hasFcbChecked = query?.Val<bool?>("HasFcbChecked");
            var statuses = query?.Val<List<ContractStatus>>("Statuses");
            var nextPaymentDate = query?.Val<DateTime?>("NextPaymentDate");
            var paymentDate = query?.Val<DateTime?>("PaymentDate");
            var isNotInscription = query?.Val<bool?>("IsNotInscription");
            var allBranches = query?.Val<bool?>("AllBranches");
            var settingId = query?.Val<int?>("SettingId");

            var pre = displayStatus.HasValue && displayStatus.Value == ContractDisplayStatus.Deleted ? "c.DeleteDate IS NOT NULL" : "c.DeleteDate IS NULL";
            pre += collateralType.HasValue ? " AND c.CollateralType = @collateralType" : string.Empty;
            pre += clientId.HasValue ? " AND c.ClientId = @clientId" : string.Empty;
            pre += organizationId.HasValue ? " AND m.OrganizationId = @organizationId" : string.Empty;
            pre += status.HasValue ? " AND c.Status = @status" : string.Empty;
            pre += statuses != null && statuses.Count > 0 ? " AND c.Status IN @statuses" : string.Empty;
            pre += nextPaymentDate.HasValue ? " AND c.NextPaymentDate = @nextPaymentDate" : string.Empty;
            pre += paymentDate.HasValue ? " AND c.NextPaymentDate != @paymentDate AND EXISTS (SELECT * FROM ContractPaymentSchedule WHERE ContractId = c.Id AND Date = @paymentDate AND Canceled IS NULL AND DeleteDate IS NULL AND ActionId IS NULL)" : string.Empty;
            pre += isNotInscription.HasValue && isNotInscription.Value ? " AND (c.InscriptionId IS NULL OR NOT EXISTS(SELECT 1 FROM Inscriptions i WHERE i.Id = c.InscriptionId AND i.Status != 20))" : null;
            pre += settingId.HasValue ? " AND c.SettingId = @settingId" : string.Empty;

            if (!String.IsNullOrEmpty(identityNumber))
            {
                pre += " AND cl.IdentityNumber = @identityNumber";
            }
            else
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }

            if (isTransferred.HasValue && isTransferred.Value) pre += " AND EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";
            else pre += " AND NOT EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";

            if (hasParent.HasValue && hasParent.Value) pre += " AND c.ParentId IS NOT NULL";
            if (hasParent.HasValue && !hasParent.Value) pre += " AND c.ParentId IS NULL";

            //TODO: Добавить проверку на дату окончания действия разрешения, чтобы исключать уже выгруженные закрытые договора
            if (hasFcbChecked.HasValue && hasFcbChecked.Value) pre += " AND EXISTS (SELECT * FROM ContractCheckValues ccv JOIN ContractChecks cc ON ccv.CheckId = cc.Id WHERE ccv.ContractId = c.Id AND ccv.[Value] = 1 AND cc.Code = N'FCB')";

            var from = "FROM Contracts c";
            if (displayStatus.HasValue)
            {
                var buildedFilter = BuildStatusQuery(displayStatus.Value, pre, from, beginDate, endDate);
                pre = buildedFilter.Item1;
                from = buildedFilter.Item2;
            }
            else
            {
                pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
            }

            var condition = listQuery.Like(pre, "c.ContractNumber", "cl.FullName", "cl.FullName", "cl.IdentityNumber", "cl.MobilePhone");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(DISTINCT c.Id)
{from}
JOIN Clients cl ON cl.Id = c.ClientId
JOIN Groups g ON g.Id = c.BranchId
JOIN Members m ON m.Id = g.Id
LEFT JOIN Inscriptions i ON c.InscriptionId = i.Id
{condition}", new
            {
                beginDate,
                endDate,
                collateralType,
                clientId,
                ownerIds,
                identityNumber,
                status,
                organizationId,
                listQuery.Filter,
                statuses,
                nextPaymentDate,
                paymentDate,
                settingId
            }, UnitOfWork.Transaction);
        }

        [Obsolete]
        private (string, string) BuildStatusQuery(ContractDisplayStatus displayStatus, string pre, string from, DateTime? beginDate, DateTime? endDate)
        {
            switch (displayStatus)
            {
                case ContractDisplayStatus.New:
                    pre += " AND c.Status = 0";
                    pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
                    break;
                case ContractDisplayStatus.AwaitForMoneySend:
                    pre += " AND c.Status = 20 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL";
                    pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
                    break;
                case ContractDisplayStatus.Open:
                    pre += " AND c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL";
                    pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
                    break;
                case ContractDisplayStatus.Overdue:
                    pre += " AND c.Status = 30 AND (c.NextPaymentDate < CONVERT(DATE, dbo.GETASTANADATE()))";
                    pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
                    break;
                case ContractDisplayStatus.SoftCollection:
                    pre += $" AND ccs.FincoreStatusId = {ContractDisplayStatus.SoftCollection}";
                    break;
                case ContractDisplayStatus.HardCollection:
                    pre += $" AND ccs.FincoreStatusId = {ContractDisplayStatus.HardCollection}";
                    break;
                case ContractDisplayStatus.LegalCollection:
                    pre += $" AND ccs.FincoreStatusId = {ContractDisplayStatus.LegalCollection}";
                    break;
                case ContractDisplayStatus.LegalHardCollection:
                    pre += $" AND ccs.FincoreStatusId = {ContractDisplayStatus.LegalHardCollection}";
                    break;
                case ContractDisplayStatus.Prolong:
                    pre += " AND c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL";
                    pre += " AND ca.ActionType = 10";
                    pre += beginDate.HasValue ? " AND ca.[Date] >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND ca.[Date] <= @endDate" : string.Empty;
                    from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                    break;
                case ContractDisplayStatus.BoughtOut:
                    pre += " AND c.Status = 40";
                    pre += " AND ca.ActionType IN (20, 30, 40)";
                    pre += beginDate.HasValue ? " AND ca.[Date] >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND ca.[Date] <= @endDate" : string.Empty;
                    from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                    break;
                case ContractDisplayStatus.SoldOut:
                    pre += " AND c.Status = 50";
                    pre += " AND ca.ActionType = 60";
                    pre += beginDate.HasValue ? " AND ca.[Date] >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND ca.[Date] <= @endDate" : string.Empty;
                    from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                    break;
                case ContractDisplayStatus.Signed:
                    pre += " AND c.Status = 30";
                    pre += beginDate.HasValue ? " AND c.ContractDate >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND c.ContractDate <= @endDate" : string.Empty;
                    break;
                case ContractDisplayStatus.MonthlyPayment:
                    pre += " AND c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE())";
                    pre += " AND ca.ActionType = 80";
                    pre += beginDate.HasValue ? " AND ca.[Date] >= @beginDate" : string.Empty;
                    pre += endDate.HasValue ? " AND ca.[Date] <= @endDate" : string.Empty;
                    from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                    break;
                default:
                    break;
            }

            return (pre, from);
        }

        public List<ContractHistory> GetContractHistory(int id)
        {
            return UnitOfWork.Session.Query<ContractHistory>($@"
WITH CTE AS
(
    SELECT Date, ContractId,FollowedId,ActionType
    FROM ContractActions
    WHERE (FollowedId= @Id OR ContractId=@Id) AND DeleteDate IS NULL AND FollowedId IS NOT NULL
    UNION ALL
    SELECT g.Date, g.ContractId, g.FollowedId, g.ActionType
    FROM CTE c
    INNER JOIN ContractActions g ON g.FollowedId= c.ContractId  AND DeleteDate IS NULL AND g.FollowedId IS NOT NULL
)

SELECT DISTINCT 
	CTE.Date,
	CTE.ContractId as ParentContractId,
	CTE.FollowedId as ChildContractId,
	CTE.ActionType,
	_from.ContractNumber AS ParentContractNumber,
	_to.ContractNumber AS ChildContractNumber
FROM CTE
LEFT JOIN Contracts _from ON _from.Id=CTE.ContractId
LEFT JOIN Contracts _to ON _to.Id=CTE.FollowedId
ORDER BY Date", new { id }, UnitOfWork.Transaction, commandTimeout: 90).ToList();
        }

        public List<ContractExpense> GetContractExpenses(int contractId)
        {
            return UnitOfWork.Session.Query<ContractExpense>($@"
                SELECT ce.*
                FROM ContractExpenses1 ce WITH(NOLOCK)
                WHERE ce.ContractId = @contractId AND ce.DeleteDate IS NULL
                ORDER BY ce.Date", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public List<Contract> ListWithPrepaymentForTodayForEndPeriod(List<int> depoAccountSettingId)
        {
            var neededPercentPaymentTypes = new List<PercentPaymentType>
            {
                PercentPaymentType.EndPeriod
            };

            var neededContractStatuses = new List<ContractStatus>
            {
                ContractStatus.Signed
            };

            var isOffBalance = false;
            var today = DateTime.Now;
            return UnitOfWork.Session.Query<Contract>($@"
                SELECT c.* FROM Contracts c
                OUTER APPLY (
                    SELECT TOP 1 Balance FROM Accounts a 
                    WHERE a.ContractId = c.Id AND a.AccountSettingId in @depoAccountSettingId
                    AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                ) acc_bal
                WHERE c.PercentPaymentType IN @neededPercentPaymentTypes 
                    AND c.Status IN @neededContractStatuses
	                AND c.DeleteDate IS NULL
	                AND c.BuyoutDate IS NULL
                    AND c.IsOffBalance = @isOffBalance
                    AND c.NextPaymentDate <= @today
                    AND acc_bal.Balance > 0
                ", new { today, neededPercentPaymentTypes, neededContractStatuses, isOffBalance, depoAccountSettingId },
                UnitOfWork.Transaction).ToList();
        }

        public List<Contract> ListWithPrepaymentForTodayForAnnuity(List<int> depoAccountSettingId, List<int> penyAccountSettingId, List<int> penyProfitAccountSettingId,
            List<int> accountSettingId, List<int> profitSettingId)
        {
            var endPeriodPercentPaymentType = PercentPaymentType.EndPeriod;
            var isOffBalance = false;
            DateTime today = DateTime.Now.Date;
            var neededContractStatuses = new List<ContractStatus>
            {
                ContractStatus.Signed
            };

            return UnitOfWork.Session.Query<Contract>($@"
                SELECT c.* FROM Contracts c
                    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @depoAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_bal
                WHERE c.PercentPaymentType != @endPeriodPercentPaymentType 
                    AND c.Status IN @neededContractStatuses
	                AND c.DeleteDate IS NULL
	                AND c.BuyoutDate IS NULL
                    AND c.NextPaymentDate <= @today
                    AND c.IsOffBalance = @isOffBalance
                    AND acc_bal.Balance > 0
                UNION ALL
                SELECT c.* FROM Contracts c
                    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @depoAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_bal
				    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @penyAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_pa
				    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @penyProfitAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_pp
                WHERE c.PercentPaymentType != @endPeriodPercentPaymentType 
                    AND c.Status IN @neededContractStatuses
	                AND c.DeleteDate IS NULL
	                AND c.BuyoutDate IS NULL
                    AND c.NextPaymentDate > @today
                    AND c.IsOffBalance = @isOffBalance
                    AND acc_bal.Balance > 0
					AND (acc_pa.Balance < 0 OR acc_pp.Balance < 0)

                UNION ALL
                SELECT c.* FROM Contracts c
                    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @depoAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_bal
				    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @accountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) account
				    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @profitSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_profit
                WHERE c.PercentPaymentType != @endPeriodPercentPaymentType 
                    AND c.Status IN @neededContractStatuses
	                AND c.DeleteDate IS NULL
	                AND c.BuyoutDate IS NULL
                    AND c.NextPaymentDate >= @today
                    AND c.IsOffBalance = @isOffBalance
                    AND acc_bal.Balance > 0
					AND acc_bal.Balance >= (account.Balance * -1) + (acc_profit.Balance * -1)
                ", new
            {
                depoAccountSettingId,
                penyAccountSettingId,
                penyProfitAccountSettingId,
                today,
                neededContractStatuses,
                endPeriodPercentPaymentType,
                isOffBalance,
                accountSettingId,
                profitSettingId
            },
                UnitOfWork.Transaction).ToList();
        }

        public List<int> GetCreditLinesWhatCanPaySomethingNow()
        {
            string query = @"DECLARE @date DATE = dbo.GETASTANADATE();
        WITH contractBalance AS (
              SELECT ContractId
                     ,ISNULL([DEPO], 0) AS DEPO
                     ,ISNULL([ACCOUNT], 0) AS ACCOUNT
                     ,ISNULL([PROFIT], 0) AS PROFIT
                     ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
                     ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
                     ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
                     ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
                     ,ISNULL([EXPENSE], 0) AS EXPENSE
                     ,ISNULL(PROFIT_OFFBALANCE, 0) AS PROFIT_OFFBALANCE
                     ,ISNULL(OVERDUE_PROFIT_OFFBALANCE, 0) AS OVERDUE_PROFIT_OFFBALANCE
                     ,ISNULL(PENY_ACCOUNT_OFFBALANCE, 0) AS PENY_ACCOUNT_OFFBALANCE
                     ,ISNULL(PENY_PROFIT_OFFBALANCE, 0) AS PENY_PROFIT_OFFBALANCE
                     ,ISNULL(PENY_AMORTIZED_PROFIT, 0) AS PENY_AMORTIZED_PROFIT
                FROM (SELECT a.ContractId
                             ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                             ,acs.Code
                        FROM Accounts a
                        JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
                        JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
						JOIN Contracts contr on contr.Id = a.ContractId 
                       WHERE a.DeleteDate IS NULL
                         AND acs.Code IN ('DEPO', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE', 'PROFIT_OFFBALANCE', 'OVERDUE_PROFIT_OFFBALANCE', 
                         'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE', 'PENY_AMORTIZED_PROFIT')
						 AND contr.Status = 30 AND contr.DeleteDate is null AND contr.BuyoutDate is null and contr.IsOffBalance = 0
                         AND a.ContractId IN ( SELECT id FROM dogs.Tranches WHERE creditlineid IN 
             ( SELECT c.id FROM Contracts c
                 OUTER APPLY (
                     SELECT TOP 1 Balance FROM Accounts a 
                     WHERE a.ContractId = c.Id AND a.AccountSettingId IN (7)
                     AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                 ) acc_bal
                WHERE c.Status IN (30)
                   AND c.DeleteDate IS NULL
                   AND c.BuyoutDate IS NULL
                    AND c.ContractClass = 2
                    AND acc_bal.Balance > 1 ))
                     ) AS cb
               PIVOT (
                 MAX(cb.Balance)
                 FOR cb.Code IN ([DEPO], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE], [PROFIT_OFFBALANCE], [OVERDUE_PROFIT_OFFBALANCE], [PENY_ACCOUNT_OFFBALANCE], [PENY_PROFIT_OFFBALANCE], [PENY_AMORTIZED_PROFIT])
               ) AS pivotTable
            ),
            wrapperBalance AS (
            SELECT cb.ContractId
                   ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
                   ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT - ISNULL(accrued_profit.amount, 0)) AS RepaymentProfitAmount
                   ,cb.ACCOUNT AS AccountAmount
                   ,cb.PROFIT AS ProfitAmount
                   ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
                   ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
                   ,cb.PENY_ACCOUNT + cb.PENY_PROFIT AS PenyAmount
	                ,cb.PENY_ACCOUNT as PenyAccount
	                ,cb.PENY_PROFIT as PenyProfit
                   ,cb.EXPENSE AS ExpenseAmount
                   ,cb.DEPO AS PrepaymentBalance
                   ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
                   ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
                   ,PROFIT_OFFBALANCE AS ProfitOffBalance
                   ,[OVERDUE_PROFIT_OFFBALANCE] AS OverdueProfitOffBalance
                   ,[PENY_ACCOUNT_OFFBALANCE] AS PenyAccountOffBalance
                   ,[PENY_PROFIT_OFFBALANCE] AS PenyProfitOffBalance
                   ,[PENY_AMORTIZED_PROFIT] AS PenyAmortizedProfit
              FROM contractBalance cb
             OUTER APPLY (SELECT cps.DebtLeft, cps.Date
                            FROM ContractPaymentSchedule cps
                           WHERE ContractId = cb.ContractId
                             AND @date BETWEEN Date AND ISNULL(NextWorkingDate, Date)
                             AND ActualDate IS NULL
                             AND DeleteDate IS NULL
                   ) sch
            OUTER APPLY (SELECT TOP 1 ROUND(cps.PercentCost * DATEDIFF(DAY, sch.Date, @date) / IIF(cps.Period = 0, 30, cps.Period), 2) AS amount
                            FROM ContractPaymentSchedule cps
                           WHERE cps.ContractId = cb.ContractId
                             AND cps.DeleteDate IS NULL
                             AND cps.Date > sch.Date
                             AND EXISTS (SELECT * FROM dbo.CashOrders co JOIN dbo.BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.Id
                                          WHERE co.ContractId = cb.ContractId
                                            AND co.DeleteDate IS NULL
                                            AND co.OrderDate >= DATEADD(DAY, 1, sch.Date)
                                            AND co.OrderDate < DATEADD(DAY, 1, @date)
                                            AND co.OrderType = 30
                                            AND co.ApproveStatus = 10
                                            AND bos.Code = 'INTEREST_ACCRUAL')
                           ORDER BY cps.Date) accrued_profit
            )
            SELECT Distinct CreditLineId       
              FROM wrapperBalance
                          JOIN Contracts contr ON wrapperBalance.ContractId = contr.Id 
				              AND (RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount + PenyAmortizedProfit > 0   )
            	            JOIN dogs.Tranches ON contr.Id = dogs.Tranches.id
            ";
            var creditLInes = UnitOfWork.Session.Query<int>(query,
                new { }, UnitOfWork.Transaction).ToList();
            return creditLInes;
        }

        public List<int> GetCreditLinesWhatCanBuyOutAllTranchesNow()
        {
            string query = @"select CreditLineId from (select CreditLineId, SUM(SummaryBalance) as sumbalcredl from (select ContractId, SUM(Balance) as SummaryBalance from  (SELECT a.ContractId 
                             ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                             ,acs.Code
                        FROM Accounts a
                        JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
                        JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
						JOIN Contracts contr on contr.Id = a.ContractId 
                       WHERE a.DeleteDate IS NULL
                         AND acs.Code IN ('ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE', 'PROFIT_OFFBALANCE', 'OVERDUE_PROFIT_OFFBALANCE', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE')
						 AND contr.Status = 30 AND contr.DeleteDate is null AND contr.BuyoutDate is null and contr.IsOffBalance = 0
                         AND a.ContractId IN ( SELECT id FROM dogs.Tranches WHERE creditlineid IN 
             ( SELECT c.id FROM Contracts c
                 OUTER APPLY (
                     SELECT TOP 1 Balance FROM Accounts a 
                     WHERE a.ContractId = c.Id AND a.AccountSettingId IN (7)
                     AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                 ) acc_bal
                WHERE c.Status IN (30)
                   AND c.DeleteDate IS NULL
                   AND c.BuyoutDate IS NULL
                    AND c.ContractClass = 2
                    AND acc_bal.Balance > 1 ))) as ContrBala 
					group by ContrBala.ContractId) as SummaryBal
					Join dogs.Tranches tr on tr.Id = ContractId
					group by CreditLineId) as CreditLineSummaryDebt
		Join ( select ContractId as id, SUM(Balance) as CreditLineBalanceMinusExpense from (SELECT a.ContractId 
                             ,Balance
                             ,acs.Code
                        FROM Accounts a
                        JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
                        JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
						JOIN Contracts contr on contr.Id = a.ContractId 
                       WHERE a.DeleteDate IS NULL
                         AND acs.Code IN ('DEPO', 'EXPENSE')
						 AND contr.Status = 30 AND contr.DeleteDate is null AND contr.BuyoutDate is null and contr.IsOffBalance = 0
                         AND a.ContractId IN ( SELECT c.id FROM Contracts c
                 OUTER APPLY (
                     SELECT TOP 1 Balance FROM Accounts a 
                     WHERE a.ContractId = c.Id AND a.AccountSettingId IN (7)
                     AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                 ) acc_bal
                WHERE c.Status IN (30)
                   AND c.DeleteDate IS NULL
                   AND c.BuyoutDate IS NULL
                    AND c.ContractClass = 2
                    AND acc_bal.Balance > 1 )) as CrDepoAndExpense
					group by ContractId) as CreditLineSummaryFounds on CreditLineSummaryFounds.Id = CreditLineSummaryDebt.CreditLineId
					where CreditLineSummaryFounds.CreditLineBalanceMinusExpense >= CreditLineSummaryDebt.sumbalcredl
            ";
            var creditLInes = UnitOfWork.Session.Query<int>(query,
                new { }, UnitOfWork.Transaction).ToList();
            return creditLInes;
        }


        public List<Contract> ListWithPrepaymentForCreditLinesTodayForAnnuity(List<int> depoAccountSettingId)
        {
            var contractClass = ContractClass.CreditLine;
            var endPeriodPercentPaymentType = PercentPaymentType.EndPeriod;
            DateTime today = DateTime.Now.Date;
            var neededContractStatuses = new List<ContractStatus>
            {
                ContractStatus.Signed
            };

            return UnitOfWork.Session.Query<Contract>($@"
                SELECT c.* FROM Contracts c
                    OUTER APPLY (
                        SELECT TOP 1 Balance FROM Accounts a 
                        WHERE a.ContractId = c.Id AND a.AccountSettingId in @depoAccountSettingId
                        AND a.CloseDate IS NULL AND a.DeleteDate IS NULL
                    ) acc_bal
                WHERE c.PercentPaymentType != @endPeriodPercentPaymentType 
                    AND c.Status IN @neededContractStatuses
	                AND c.DeleteDate IS NULL
	                AND c.BuyoutDate IS NULL
                    AND c.ContractClass = 2
                    AND acc_bal.Balance > 0
            ", new
            {
                depoAccountSettingId,
                today,
                neededContractStatuses,
                endPeriodPercentPaymentType,
                contractClass
            },
                UnitOfWork.Transaction).ToList();
        }

        public void UpdateCrmInfo(int id, int crmId)
        {
            using (var transaction = BeginTransaction())
            {
                var query = @"
                UPDATE Contracts
                SET CrmId=@CrmId
                WHERE Id = @Id";

                UnitOfWork.Session.Execute(query, new { id, crmId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdateCrmPaymentInfo(int id, int crmPaymentId)
        {
            using (var transaction = BeginTransaction())
            {
                var query = @"
                UPDATE Contracts
                SET CrmPaymentId=@CrmPaymentId
                WHERE Id = @Id";

                UnitOfWork.Session.Execute(query, new { id, crmPaymentId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Contract Check(string contractNumber, string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(contractNumber)) throw new ArgumentNullException(nameof(contractNumber));
            if (string.IsNullOrWhiteSpace(identityNumber)) throw new ArgumentNullException(nameof(identityNumber));

            return UnitOfWork.Session.Query<Contract>(@"SELECT TOP 1 c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN Clients cl ON cl.Id = c.ClientId
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.ContractNumber = @contractNumber
   AND cl.IdentityNumber = @identityNumber",
                new { contractNumber, identityNumber }, UnitOfWork.Transaction).FirstOrDefault();
        }

        /// <summary>
        /// Получить количество договоров клиента с определенными статусами
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="contractStatuses">Статусы договоров</param>
        /// <returns></returns>
        public int GetCountOfClientContractsWithCertainStatuses(int clientId, IEnumerable<ContractStatus> contractStatuses)
        {
            if (contractStatuses == null)
                throw new ArgumentNullException(nameof(contractStatuses));

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(c.Id) FROM Contracts c
	                WHERE c.Status IN @contractStatuses AND c.ClientId = @clientId AND c.DeleteDate IS NULL
                ", new { clientId, contractStatuses }, UnitOfWork.Transaction);
        }

        /// <summary>
        /// Получить список договоров для рассылки просрочки
        /// </summary>
        /// <param name="date">Дата запроса</param>
        /// <param name="delayDays">Количество дней просрочки</param>
        /// <returns></returns>
        public IEnumerable<BaseNotificationModel> ListForDelayNotification(DateTime date, int delayDays)
        {
            return UnitOfWork.Session.Query<BaseNotificationModel>($@"
                SELECT 
                    c.Id AS ContractId, 
                    c.BranchId,
                    c.ClientId
                FROM Contracts c 
                WHERE c.DeleteDate IS NULL 
                    AND c.PercentPaymentType = {(int)PercentPaymentType.EndPeriod}
                    AND c.Status = {(int)ContractStatus.Signed}
                    AND CAST(c.MaturityDate as date) = DATEADD(day, -@delayDays, CONVERT(DATE, @date))
                    AND c.ContractClass != {(int)ContractClass.CreditLine}
                UNION ALL
                SELECT 
                    c.Id as ContractId,
                    c.BranchId,
                    c.ClientId
                FROM Contracts c 
                OUTER APPLY(SELECT TOP 1 Date 
                            FROM ContractPaymentSchedule 
                            WHERE ContractId=c.Id 
                                AND ActionId IS NULL 
                                AND Canceled IS NULL
                                AND DeleteDate IS NULL
                            ORDER BY DATE ASC) cps
                WHERE c.DeleteDate IS NULL
                    AND c.PercentPaymentType != {(int)PercentPaymentType.EndPeriod}
                    AND c.Status = {(int)ContractStatus.Signed}
                    AND CAST(cps.Date AS DATE) = DATEADD(day, -@delayDays, CONVERT(DATE, @date))
                    AND c.ContractClass != {(int)ContractClass.CreditLine}", new { date, delayDays }, UnitOfWork.Transaction);
        }

        /// <summary>
        /// Получить список договоров для рассылки просрочки по позициям
        /// </summary>
        /// <param name="date">Дата запроса</param>
        /// <param name="delayDays">Количество дней просрочки</param>
        /// <param name="collateralType"></param>
        /// <returns></returns>
        public IEnumerable<BaseNotificationModel> ListForDelayNotificationPosition(DateTime date, int delayDays, CollateralType collateralType)
        {
            return UnitOfWork.Session.Query<BaseNotificationModel>($@"SELECT 
			    c.Id as ContractId,
			    c.BranchId,
			    c.ClientId
		    FROM Contracts c
		    LEFT JOIN ClientsBlackList cbl ON cbl.ClientId = c.ClientId
		    LEFT JOIN BlackListReasons blb ON blb.Id = cbl.ReasonId AND blb.Code = 'DIED'
		    WHERE c.DeleteDate IS NULL
		      AND c.PercentPaymentType = {(int)PercentPaymentType.EndPeriod}
		      AND c.Status = {(int)ContractStatus.Signed}
	          AND CAST(c.MaturityDate AS DATE) = DATEADD(day, -@delayDays, CONVERT(DATE, @date))
		      AND c.ContractClass != {(int)ContractClass.CreditLine}
		      AND c.CollateralType = @collateralType
		      AND cbl.Id IS NULL
		    UNION
		    SELECT 
			    c.Id as ContractId,
			    c.BranchId,
			    c.ClientId
		    FROM Contracts c 
		    LEFT JOIN ClientsBlackList cbl ON cbl.ClientId = c.ClientId
		    LEFT JOIN BlackListReasons blb ON blb.Id = cbl.ReasonId AND blb.Code = 'DIED'
			    OUTER APPLY(SELECT TOP 1 Date 
						    FROM ContractPaymentSchedule 
						    WHERE ContractId=c.Id 
						      AND ActionId IS NULL 
						      AND Canceled IS NULL	
						      AND DeleteDate IS NULL
                            ORDER BY DATE ASC) cps
		    WHERE c.DeleteDate IS NULL
		      AND c.PercentPaymentType != {(int)PercentPaymentType.EndPeriod}
		      AND c.Status = {(int)ContractStatus.Signed}
		      AND CAST(cps.Date AS DATE) = DATEADD(day, -@delayDays, CONVERT(DATE, @date))
		      AND c.ContractClass != {(int)ContractClass.CreditLine}
		      AND c.CollateralType = @collateralType
		      AND cbl.Id IS NULL", new { date, delayDays, collateralType }, UnitOfWork.Transaction);
        }

        /// <summary>
        /// Получить список договоров для рассылки предстоящей оплаты
        /// </summary>
        /// <param name="date">Дата запроса</param>
        /// <param name="paymentDays">Количество дней до даты оплаты</param>
        /// <returns></returns>
        public IEnumerable<PaymentNotificationModel> ListForPaymentNotification(DateTime date, int paymentDays)
        {
            return UnitOfWork.Session.Query<PaymentNotificationModel>($@"WITH DiscreteContracts AS (SELECT 
	c.ClientId
	,c.MaturityDate Date
FROM Contracts c
WHERE c.DeleteDate IS NULL
	AND CAST(c.MaturityDate AS DATE) = DATEADD(DAY, {paymentDays}, @date)
	AND c.PercentPaymentType = {(int)PercentPaymentType.EndPeriod}
	AND c.Status = {(int)ContractStatus.Signed}
GROUP BY 
	c.ClientId
	,c.MaturityDate),
AnuitetContracts AS (SELECT
	c.ClientId
	,cps.Date
FROM ContractPaymentSchedule cps
	JOIN Contracts c ON cps.ContractId=c.Id
WHERE c.DeleteDate IS NULL
	AND cps.DeleteDate IS NULL
	AND cps.Canceled IS NULL
	AND cps.ActualDate IS NULL
    AND c.PercentPaymentType != {(int)PercentPaymentType.EndPeriod}
    AND c.Status = {(int)ContractStatus.Signed}
	AND CAST(cps.Date AS DATE) = DATEADD(DAY, {paymentDays}, @date)
	AND c.ContractClass != {(int)ContractClass.CreditLine}
GROUP BY c.ClientId, cps.Date),
FinalClients AS (
SELECT p.ClientId ,p.Date
FROM DiscreteContracts p
UNION
SELECT p.ClientId ,p.Date
FROM AnuitetContracts p)

SELECT fc.ClientId, fc.Date, t.BranchId 
FROM FinalClients fc
OUTER APPLY (SELECT TOP 1 BranchId FROM Contracts 
			WHERE ClientId = fc.ClientId
				AND Status = {(int)ContractStatus.Signed}
				AND DeleteDate IS NULL
			ORDER BY Id DESC) t", new { date, paymentDays }, UnitOfWork.Transaction);
        }

        /// <summary>
        /// Получить список договоров для рассылки последней оплаты по дискретам
        /// </summary>
        /// <param name="date">Дата запроса</param>
        /// <returns></returns>
        public IEnumerable<PaymentLastNotificationModel> ListForLastPaymentNotification(DateTime date)
        {
            return UnitOfWork.Session.Query<PaymentLastNotificationModel>($@"WITH Props AS
	              (
		              SELECT cps.Id, cps.ContractId, cps.Date, cps.DebtCost + cps.PercentCost as PaymentCost, c.BranchId, c.ClientId,
			              ROW_NUMBER() OVER (PARTITION BY cps.ContractId ORDER BY cps.Date DESC) AS RowNumber
		              , LAG(cps.id) OVER(PARTITION BY cps.ContractId ORDER BY cps.Date DESC) AS last_payment
		              FROM ContractPaymentSchedule cps
		              INNER JOIN Contracts c ON c.Id = cps.ContractId
		              WHERE c.PercentPaymentType = {(int)PercentPaymentType.EndPeriod}
		              AND c.Status = {(int)ContractStatus.Signed}
		              AND c.DeleteDate IS NULL
		              AND cps.DeleteDate IS NULL
	              )

	              SELECT 
			            cc.ContractId, 
			            cc.Date as PaymentDate, 
			            cc.DebtCost + cc.PercentCost as PaymentCost, 
			            p.BranchId, 
			            p.ClientId 
	              FROM Props p
	              INNER JOIN ContractPaymentSchedule cc ON cc.Id = p.last_payment 
	              WHERE p.RowNumber = 2
	              AND CAST(p.Date AS DATE) = @date
	              AND cc.ActualDate IS NULL", new { date }, UnitOfWork.Transaction);
        }

        public List<Contract> GetContractsAccordingProductTypeForClient(string productCode, int clientId)
        {
            return UnitOfWork.Session.Query<Contract>($@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN LoanProductTypes lps ON c.ProductTypeId = lps.Id
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE lps.Code = @code
   AND c.ClientId = @clientId
   AND c.DeleteDate IS NULL",
                new { code = productCode, clientId }, UnitOfWork.Transaction).ToList();
        }

        public List<int> GetContractsByParentContractDateForPenaltyAccrual(DateTime date)
        {
            var query = @"
                SELECT c.Id FROM Contracts c
                    JOIN Contracts cp ON cp.Id = c.ParentId
                    WHERE c.Status IN (30,50)
                        AND c.DeleteDate IS NULL
                        AND DAY(cp.ContractDate) = @day
				        AND MONTH(cp.ContractDate) = @month";

            var contracts = UnitOfWork.Session.Query<int>(query,
                 new { day = date.Day, month = date.Month }, UnitOfWork.Transaction).ToList();

            if (!DateTime.IsLeapYear(date.Year) && date.Day == 28 && date.Month == 2)
                contracts.AddRange(UnitOfWork.Session.Query<int>(query, new { day = 29, month = date.Month }, UnitOfWork.Transaction));

            return contracts;
        }

        public List<int> GetContractsOnDateForPenaltyAccrual(DateTime date)
        {
            var query = @"
                SELECT Id FROM Contracts
                      WHERE Status IN (30,50)
                          AND ParentId IS NULL
                          AND DeleteDate IS NULL
                          AND DAY(ContractDate) = @day
				          AND MONTH(ContractDate) = @month
                          AND ContractClass != 2";


            var contracts = UnitOfWork.Session.Query<int>(query, new { day = date.Day, month = date.Month }, UnitOfWork.Transaction).ToList();

            if (!DateTime.IsLeapYear(date.Year) && date.Day == 28 && date.Month == 2)
                contracts.AddRange(UnitOfWork.Session.Query<int>(query, new { day = 29, month = date.Month }, UnitOfWork.Transaction));

            return contracts;
        }

        public List<Contract> GetContractsForDecreasePenaltyRates(DateTime date)
        {
            return UnitOfWork.Session.Query<Contract>($@"SELECT c.*,
                                                           tr.CreditLineId,
                                                           tr.MaxCreditLineCost,
                                                           cai.DebtGracePeriod
                                                      FROM Contracts c
                                                      LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
                                                      LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
                                                      LEFT JOIN Groups g ON g.Id = c.BranchId
                                                     WHERE c.DeleteDate IS NULL
                                                       AND NOT EXISTS (SELECT 1 FROM ClientDeferments cd 
                                                                        WHERE cd.ContractId = c.Id 
	                                                                    AND cd.DeleteDate IS NULL
                                                                        AND (cd.Status = 10 OR (cd.Status = 20 AND @date BETWEEN cd.StartDate AND cd.EndDate)))
                                                       AND c.Status IN (30,50)
                                                       AND c.CollateralType <> 50
                                                       AND c.UsePenaltyLimit = 1
                                                       AND DATEDIFF(DAY, c.NextPaymentDate, @date) = @period
                ", new { date = date, period = Constants.NBRK_PENALTY_DECREASE_PERIOD_FROM }, UnitOfWork.Transaction).ToList();
        }

        public List<decimal> GetEsimatedCostsForPreAprovedAmount(int modelId, int year)
        {
            var query = @"SELECT c.EstimatedCost FROM Contracts c
                    JOIN ContractPositions cp ON cp.ContractId = c.Id
                    JOIN Cars cr ON cr.Id = cp.PositionId
                    WHERE cr.VehicleModelId = @modelId AND cr.ReleaseYear = @year
                    AND c.DeleteDate IS NULL";

            var list = UnitOfWork.Session.Query<decimal>(string.Concat(query, " AND c.ContractDate > DATEADD(MM, -3, dbo.GETASTANADATE())"), new { modelId, year }, UnitOfWork.Transaction).ToList();

            if (list is null || !list.Any())
                list = UnitOfWork.Session.Query<decimal>(query, new { modelId, year }, UnitOfWork.Transaction).ToList();

            if (list != null && list.Any())
                list.Sort();

            return list;
        }

        public List<Contract> GetChildrenContracts(int contractId)
        {
            return UnitOfWork.Session.Query<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.ClosedParentId = @contractId
   AND c.DeleteDate IS NULL",
                new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public int? GetClosedParentId(int contractId)
        {
            var query = @"SELECT ClosedParentId FROM Contracts c
                        WHERE c.Id = @contractId
                        AND c.DeleteDate IS NULL";

            return UnitOfWork.Session.Query<int>(query, new { contractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Contract> GetContractsByClientIdAndContractId(int clientId, int contractId, IEnumerable<ContractStatus> contractStatuses)
        {
            return UnitOfWork.Session.Query<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.ClientId = @clientId
   AND c.Id != @contractId
   AND c.Status IN @contractStatuses
   AND c.DeleteDate IS NULL",
                new { clientId, contractId, contractStatuses }, UnitOfWork.Transaction).ToList();
        }

        public List<Contract> GetContractsByClientId(int clientId, IEnumerable<ContractStatus> contractStatuses)
        {
            return UnitOfWork.Session.Query<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.ClientId = @clientId
   AND c.Status IN @contractStatuses
   AND c.DeleteDate IS NULL",
                new { clientId, contractStatuses }, UnitOfWork.Transaction).ToList();
        }

        public List<Contract> GetContractsByClientIdAndContractClases(int clientId, List<int> contractClases)
        {
            return UnitOfWork.Session.Query<Contract>(@"
            SELECT * FROM Contracts WHERE Contracts.ClientId = @clientId AND Contracts.ContractClass IN @contractClases
            AND Contracts.DeleteDate IS NULL",
                new { clientId, contractClases }, UnitOfWork.Transaction).ToList();
        }

        public async Task<List<Contract>> GetContractsByClientIdAndContractClasesAsync(int clientId, List<int> contractClases)
        {
            var contracts = await UnitOfWork.Session.QueryAsync<Contract>(@"
            SELECT * FROM Contracts 
            WHERE Contracts.ClientId = @clientId 
            AND Contracts.ContractClass IN @contractClases 
            AND Contracts.DeleteDate IS NULL",
                new { clientId, contractClases }, UnitOfWork.Transaction);

            return contracts.ToList();
        }

        public Contract GetContractPositions(int contractId)
        {
            var entity = UnitOfWork.Session.Query<Contract, User, Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod,
       u.*
  FROM Contracts c WITH(NOLOCK)
  JOIN Users u ON c.AuthorId = u.Id
  LEFT JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.Id = @contractId",
                (c, u) =>
                {
                    c.Author = u;
                    if (c.ProductTypeId.HasValue)
                    {
                        c.ProductType = _loanProductTypeRepository.Get(c.ProductTypeId.Value);
                    }
                    if (c.SettingId.HasValue)
                    {
                        c.Setting = _loanPercentRepository.Get(c.SettingId.Value);
                    }
                    return c;
                },
                new { contractId }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            entity.Client = _clientRepository.Get(entity.ClientId);

            if (entity.CollateralType == CollateralType.Car)
            {
                entity.Positions = UnitOfWork.Session.Query<ContractPosition, Car, VehicleMark, VehicleModel, Category, Application, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Cars.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Cars.Mark,
                        Cars.Model,
                        Cars.ReleaseYear,
                        Cars.TransportNumber,
                        Cars.MotorNumber,
                        Cars.BodyNumber,
                        Cars.TechPassportNumber,
                        Cars.TechPassportDate,
                        Cars.Color,
                        Cars.ParkingStatusId,
                        Cars.VehicleMarkId,
	                    Cars.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        Categories.*,
                        Applications.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Cars ON Cars.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Cars.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Cars.VehicleModelId
                    LEFT JOIN Applications ON Applications.AppId in (SELECT TOP 1 AppId from Applications WHERE PositionId = ContractPositions.PositionId ORDER BY AppId DESC)
                    WHERE ContractId = @contractId",
                (cp, p, m, mod, c, a) =>
                {
                    cp.Position = p;
                    p.VehicleMark = m;
                    p.VehicleModel = mod;
                    cp.Category = c;
                    if (cp.Position.ClientId.HasValue)
                        cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                    if (a != null)
                    {
                        cp.TurboCost = a.TurboCost;
                        cp.MotorCost = a.MotorCost;
                    }
                    return cp;
                }, new { contractId }, UnitOfWork.Transaction).ToList();
            }
            else if (entity.CollateralType == CollateralType.Realty)
            {
                entity.Positions = UnitOfWork.Session.Query<ContractPosition, Realty, Category, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Realties.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Realties.RealtyTypeId,
                        Realties.CadastralNumber,
                        Realties.Rca,
                        Categories.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Realties ON Realties.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    WHERE ContractId = @contractId",
                (cp, r, c) =>
                {
                    cp.Position = r;
                    cp.Category = c;
                    if (cp.Position.ClientId.HasValue)
                        cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                    return cp;
                }, new { contractId }, UnitOfWork.Transaction).ToList();
            }
            else if (entity.CollateralType == CollateralType.Machinery)
            {
                entity.Positions = UnitOfWork.Session.Query<ContractPosition, Machinery, VehicleMark, VehicleModel, Category, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Machineries.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Machineries.Mark,
                        Machineries.Model,
                        Machineries.ReleaseYear,
                        Machineries.TransportNumber,
                        Machineries.MotorNumber,
                        Machineries.BodyNumber,
                        Machineries.TechPassportNumber,
                        Machineries.TechPassportDate,
                        Machineries.Color,
                        Machineries.ParkingStatusId,
                        Machineries.VehicleMarkId,
	                    Machineries.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        Categories.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Machineries ON Machineries.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Machineries.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Machineries.VehicleModelId
                    WHERE ContractId = @contractId",
                (cp, p, m, mod, c) =>
                {
                    cp.Position = p;
                    p.VehicleMark = m;
                    p.VehicleModel = mod;
                    cp.Category = c;
                    return cp;
                }, new { contractId }, UnitOfWork.Transaction).ToList();
            }
            else
            {
                entity.Positions = UnitOfWork.Session.Query<ContractPosition, Position, Category, ContractPosition>(@"
                SELECT *
                FROM ContractPositions WITH(NOLOCK)
                JOIN Positions ON Positions.Id = ContractPositions.PositionId
                JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                WHERE ContractId = @contractId",
                (cp, p, c) =>
                {
                    cp.Position = p;
                    cp.Category = c;
                    return cp;
                }, new { contractId }, UnitOfWork.Transaction).ToList();
            }

            entity.Subjects = UnitOfWork.Session.Query<ContractLoanSubject, User, ContractLoanSubject>(@"SELECT sub.*, u.*
                FROM ContractLoanSubjects sub WITH(NOLOCK)
                LEFT JOIN Users u ON sub.AuthorId = u.Id
                WHERE sub.ContractId = @contractId",
                (sub, u) =>
                {
                    sub.Author = u;
                    sub.Client = _clientRepository.Get(sub.ClientId);
                    sub.Subject = _loanSubjectRepository.Get(sub.SubjectId);
                    return sub;
                }, new { contractId }, UnitOfWork.Transaction).ToList();

            return entity;
        }

        public async Task<int> GetCreditLineId(int contractId)
        {
            var query = @"select CreditLineId from dogs.Tranches where Id = @contractId";

            return UnitOfWork.Session.ExecuteScalar<int>(query, new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<int> GetTranchesCount(int creditLineContractId)
        {

            var query = @"select Count(*) from dogs.Tranches where CreditLineId = @creditLineContractId";

            return UnitOfWork.Session.ExecuteScalar<int>(query, new { creditLineContractId }, UnitOfWork.Transaction);
        }

        public async Task<int> GetActiveTranchesCount(int trancheContractId)
        {

            var query = @"select count(*) from dogs.Tranches tr
            inner join Contracts co on co.Id = tr.Id
            where tr.CreditLineId = (select CreditLineId from dogs.Tranches where Id = @trancheContractId)
            and co.Status = 30";

            return UnitOfWork.Session.ExecuteScalar<int>(query, new { trancheContractId }, UnitOfWork.Transaction);
        }

        public async Task<CreditLineSettings> GetCreditLineSettingsAsync(int Id)
        {
            var query = @"select * from dogs.CreditLines where Id = @Id";

            return (await UnitOfWork.Session.QueryAsync<CreditLineSettings>(query, new { Id }, UnitOfWork.Transaction)).FirstOrDefault();
        }

        public async Task<List<int>> GetTrancheIdsByCreditLine(int Id)
        {
            var query = @"select Id from dogs.Tranches where CreditLineId = @Id";

            return (await UnitOfWork.Session.QueryAsync<int>(query, new { Id }, UnitOfWork.Transaction)).ToList();//check
        }

        public async Task<int> GetCreditLineByTrancheId(int Id)
        {
            var query = @"select CreditLineId from dogs.Tranches where Id = @Id";

            return UnitOfWork.Session.Query<int>(query, new { Id }, UnitOfWork.Transaction).FirstOrDefault();//check
        }

        public async Task<List<Contract>> GetAllSignedTranches(int creditLineContractId, bool IsOnlySignedTranches = true)
        {
            var pre = "WHERE c.DeleteDate IS NULL AND tr.CreditLineId = @creditLineContractId";

            if (IsOnlySignedTranches)
                pre += " AND c.Status = 30";

            return UnitOfWork.Session.Query<Contract>($@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN dogs.Tranches tr ON c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 {pre}",
                new { creditLineContractId }, UnitOfWork.Transaction).ToList();
        }

        public IList<ContractListInfo> GetFilteredList(ListQuery listQuery, object query = null, bool withoutTranches = false)
        {
            var contracts = InternalGetFilteredList(listQuery, query);

            if (withoutTranches)
                return contracts;

            var creditLineIds = contracts
                .Where(x => x.ContractClass == ContractClass.CreditLine)
                .Select(x => x.Id)
                .ToList();

            if (!creditLineIds.Any())
                return contracts;

            var tranches = InternalGetFilteredList(listQuery, query, creditLineIds);

            contracts.ToList().ForEach(x => x.Tranches = tranches.Where(t => t.CreditLineId == x.Id).ToList());

            return contracts;
        }

        public int GetFilteredListCount(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var identityNumber = query?.Val<string>("IdentityNumber");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var carNumber = query?.Val<string>("CarNumber");
            var rca = query?.Val<string>("Rca");
            var isTransferred = query?.Val<bool?>("IsTransferred") ?? false;
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var clientId = query?.Val<int?>("ClientId");
            var settingId = query?.Val<int?>("SettingId");
            var allBranches = query?.Val<bool?>("AllBranches");

            var result = GetContractsPredicate(listQuery.Filter, beginDate, endDate,
                collateralType, displayStatus, isTransferred, clientId, settingId);

            var from = string.IsNullOrEmpty(result.Item2) ? "FROM Contracts c" : result.Item2;
            var predicate = result.Item1;

            var pre = string.Empty;
            if (!String.IsNullOrEmpty(identityNumber) || !String.IsNullOrEmpty(carNumber) || !String.IsNullOrEmpty(rca))
            {
                if (!String.IsNullOrEmpty(identityNumber))
                    pre += " AND cc.IdentityNumber = @identityNumber";
                if (!String.IsNullOrEmpty(carNumber))
                    pre += " AND cars.TransportNumber = @carNumber";
                if (!String.IsNullOrEmpty(rca))
                    pre += " AND realty.rca = @rca";
            }
            else
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }
            predicate += pre;

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(DISTINCT c.Id)
  {from}
  JOIN Clients cc on cc.Id = c.ClientId
  JOIN Groups g on g.Id = c.BranchId
  JOIN Users u on u.Id = c.AuthorId
  LEFT JOIN dogs.Tranches t on t.Id = c.Id
  LEFT JOIN dogs.Tranches t2 on t2.CreditLineId = c.Id
  LEFT JOIN Contracts c2 on c2.Id = t2.Id  
  LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
  LEFT JOIN LoanPercentSettings lps on lps.Id = c.SettingId
  LEFT JOIN ContractPositions cpp ON cpp.ContractId = c.Id
  LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
  LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
  LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id AND ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
  LEFT JOIN CollectionContractStatuses ccs2 ON ccs2.ContractId = c2.Id AND ccs2.IsActive = 1 AND ccs2.DeleteDate IS NULL
 {predicate}",
                new
                {
                    listQuery.Filter,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    identityNumber,
                    beginDate,
                    endDate,
                    collateralType,
                    carNumber,
                    rca,
                    ownerIds,
                    clientId,
                    settingId
                }, UnitOfWork.Transaction, commandTimeout: 1800);
        }

        public IList<ContractTrancheInfo> GetTranchesAndBalanceByCreditLineId(int creditLineId)
        {
            return UnitOfWork.Session.Query<ContractTrancheInfo>(@"WITH allPenBal AS (
  SELECT SUM(acc.Balance) as Balance
         ,acc.ContractId
    FROM Accounts acc, AccountSettings accSetting
   WHERE acc.AccountSettingId = accSetting.Id
     AND accSetting.Code in ('PENY_ACCOUNT', 'PENY_PROFIT', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE')
   GROUP BY acc.ContractId
)
SELECT c.Id
       ,c.ContractNumber
       ,c.ContractDate
       ,c.LoanCost
       ,(CASE
           WHEN c.DeleteDate IS NOT NULL THEN 60
           WHEN c.Status = 0 THEN 0
           WHEN c.Status = 20 THEN 5
           WHEN c.Status = 24 THEN 24
           WHEN c.Status = 30 AND c.InscriptionId IS NOT NULL AND i.Status != 20 THEN 25
		   WHEN c.Status = 30 AND c.NextPaymentDate < CAST(dbo.GETASTANADATE() AS DATE) THEN 20
           WHEN c.Status = 30 AND a.Balance < 0 THEN 20
           WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
           WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
           WHEN c.Status = 40 THEN 40
           WHEN c.Status = 50 THEN 50
           WHEN c.Status = 60 THEN 55
           ELSE 0
       END) AS Status
  FROM Contracts c
  JOIN dogs.Tranches t on t.Id = c.Id
  LEFT JOIN allPenBal a on a.ContractId = c.Id
  LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
  LEFT JOIN CollectionContractStatuses ccs on ccs.ContractId = c.Id
 WHERE t.CreditLineId = @creditLineId
   AND c.DeleteDate IS NULL
 ORDER BY c.ContractDate DESC, c.ContractNumber Desc",
                new { creditLineId }, UnitOfWork.Transaction).ToList();
        }

        public IList<ContractBalance> GetBalances(IList<int> contractIds)
        {
            if (!contractIds.Any())
                return new List<ContractBalance>();

            return UnitOfWork.Session.Query<ContractBalance>(@"DECLARE @date DATE = dbo.GETASTANADATE();
WITH contractBalance AS (
  SELECT ContractId
         ,ISNULL([DEPO], 0) AS DEPO
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
         ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
         ,ISNULL([EXPENSE], 0) AS EXPENSE
    FROM (SELECT a.ContractId
                 ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                 ,acs.Code
            FROM Accounts a
            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
           WHERE a.DeleteDate IS NULL
             AND acs.Code IN ('DEPO', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE')
             AND a.ContractId IN @contractIds
         ) AS cb
   PIVOT (
     MAX(cb.Balance)
     FOR cb.Code IN ([DEPO], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE])
   ) AS pivotTable
),
wrapperBalance AS (
SELECT cb.ContractId
       ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
       ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT - ISNULL(accrued_profit.amount, 0)) AS RepaymentProfitAmount
       ,cb.ACCOUNT AS AccountAmount
       ,cb.PROFIT AS ProfitAmount
       ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
       ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
       ,cb.PENY_ACCOUNT + cb.PENY_PROFIT AS PenyAmount
       ,cb.EXPENSE AS Expense
       ,cb.DEPO AS PrepaymentBalance
       ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
       ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
  FROM contractBalance cb
 OUTER APPLY (SELECT cps.DebtLeft, cps.Date
                FROM ContractPaymentSchedule cps
               WHERE ContractId = cb.ContractId
                 AND @date BETWEEN Date AND ISNULL(cps.NextWorkingDate, Date)
                 AND ActualDate IS NULL
                 AND DeleteDate IS NULL
       ) sch
OUTER APPLY (SELECT TOP 1 ROUND(cps.PercentCost * DATEDIFF(DAY, sch.Date, @date) / IIF(cps.Period = 0, 30, cps.Period), 2) AS amount
                FROM ContractPaymentSchedule cps
               WHERE cps.ContractId = cb.ContractId
                 AND cps.DeleteDate IS NULL
                 AND cps.Date > sch.Date
                 AND EXISTS (SELECT * FROM dbo.CashOrders co JOIN dbo.BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.Id
                              WHERE co.ContractId = cb.ContractId
                                AND co.DeleteDate IS NULL
                                AND co.OrderDate >= DATEADD(DAY, 1, sch.Date)
                                AND co.OrderDate < DATEADD(DAY, 1, @date)
                                AND co.OrderType = 30
                                AND co.ApproveStatus = 10
                                AND bos.Code = 'INTEREST_ACCRUAL')
               ORDER BY cps.Date) accrued_profit
)
SELECT ContractId
       ,AccountAmount
       ,ProfitAmount
       ,OverdueAccountAmount
       ,OverdueProfitAmount
       ,PenyAmount
       ,AccountAmount + OverdueAccountAmount AS TotalAcountAmount
       ,ProfitAmount + OverdueProfitAmount AS TotalProfitAmount
       ,Expense
       ,PrepaymentBalance
       ,RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount AS CurrentDebt
       ,IIF((RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) < 0, 0, RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) AS TotalRepaymentAmount
       ,IIF(RedemptionAmount - PrepaymentBalance < 0, 0, RedemptionAmount - PrepaymentBalance) AS TotalRedemptionAmount
  FROM wrapperBalance",
            new { contractIds }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<ContractBalance>> GetBalancesAsync(IList<int> contractIds)
        {
            if (!contractIds.Any())
                return new List<ContractBalance>();

            return await UnitOfWork.Session.QueryAsync<ContractBalance>(@"WITH contractBalance AS (
  SELECT ContractId
         ,ISNULL([DEPO], 0) AS DEPO
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
         ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
         ,ISNULL([EXPENSE], 0) AS EXPENSE
         ,ISNULL([CREDIT_LINE_LIMIT], 0) AS CREDIT_LINE_LIMIT
    FROM (SELECT a.ContractId
                 ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                 ,acs.Code
            FROM Accounts a
            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
           WHERE a.DeleteDate IS NULL
             AND acs.Code IN ('DEPO', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE', 'CREDIT_LINE_LIMIT')
             AND a.ContractId IN @contractIds
         ) AS cb
   PIVOT (
     MAX(cb.Balance)
     FOR cb.Code IN ([DEPO], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE], [CREDIT_LINE_LIMIT])
   ) AS pivotTable
),
wrapperBalance AS (
SELECT cb.ContractId
       ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
       ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT) AS RepaymentProfitAmount
       ,cb.ACCOUNT AS AccountAmount
       ,cb.PROFIT AS ProfitAmount
       ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
       ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
       ,cb.PENY_ACCOUNT + cb.PENY_PROFIT AS PenyAmount
       ,cb.PENY_ACCOUNT AS PenyAccount
       ,cb.PENY_PROFIT AS PenyProfit
       ,cb.EXPENSE AS Expense
       ,cb.DEPO AS PrepaymentBalance
       ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
       ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
       ,cb.CREDIT_LINE_LIMIT AS CreditLineLimit
  FROM contractBalance cb
 OUTER APPLY (SELECT cps.DebtLeft
                FROM ContractPaymentSchedule cps
               WHERE ContractId = cb.ContractId
                 AND Date = cast((select dbo.GETASTANADATE()) as date)
                 AND ActualDate IS NULL
                 AND DeleteDate IS NULL
       ) sch
)
SELECT ContractId
       ,AccountAmount
       ,ProfitAmount
       ,OverdueAccountAmount
       ,OverdueProfitAmount
       ,PenyAmount
       ,AccountAmount + OverdueAccountAmount AS TotalAcountAmount
       ,ProfitAmount + OverdueProfitAmount AS TotalProfitAmount
       ,Expense AS ExpenseAmount
       ,PrepaymentBalance
       ,PenyAccount
       ,PenyProfit
       ,RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount AS CurrentDebt
       ,IIF((RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) < 0, 0, RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) AS TotalRepaymentAmount
       ,IIF(RedemptionAmount - PrepaymentBalance < 0, 0, RedemptionAmount - PrepaymentBalance) AS TotalRedemptionAmount
       ,CreditLineLimit
  FROM wrapperBalance",
            new { contractIds }, UnitOfWork.Transaction);
        }

        public async Task<Contract> GetNonCreditLineByNumberAsync(string contractNumber)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON tr.Id = c.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.DeleteDate IS NULL
   AND c.ContractClass != 2
   AND c.ContractNumber = @contractNumber",
                new { contractNumber }, UnitOfWork.Transaction);
        }

        public async Task<Contract> GetCreditLineByNumberAsync(string contractNumber)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Contract>(@"SELECT c.*
  FROM Contracts c
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.DeleteDate IS NULL
   AND c.ContractClass = 2
   AND c.ContractNumber = @contractNumber",
                new { contractNumber }, UnitOfWork.Transaction);
        }

        public async Task<bool> HasPartialPaymentAsync(int id)
        {
            var result = await UnitOfWork.Session.ExecuteScalarAsync<bool?>(@$"SELECT TOP 1 1 FROM Contracts WHERE Id = @id and PartialPaymentParentId IS NOT NULL
UNION ALL
SELECT TOP 1 1 FROM ContractPaymentScheduleHistory WHERE ContractId = @id",
                new { id }, UnitOfWork.Transaction);

            return result.HasValue;
        }

        public async Task<IEnumerable<Contract>> GetListForOnlineByIinAsync(string iin)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod,
  FROM Clients cl
  JOIN Contracts c ON c.ClientId = cl.Id
  LEFT JOIN dogs.Tranches tr ON tr.Id = c.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE cl.IdentityNumber = @iin
   AND c.Status IN (20, 24, 25, 26, 27, 29, 30, 50)
   AND c.CreatedInOnline = 1",
                new { iin }, UnitOfWork.Transaction);
        }
        public async Task<IEnumerable<Contract>> GetHistoryForOnlineByIinAsync(string iin)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Clients cl
  JOIN Contracts c ON c.ClientId = cl.Id
  LEFT JOIN dogs.Tranches tr ON tr.Id = c.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE cl.IdentityNumber = @iin
   AND c.Status > 20
   AND c.CreatedInOnline = 1",
                new { iin }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Contract>> GetContractsByVinAsync(string vin)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT c.*
  FROM Contracts c
  JOIN ContractPositions cp ON cp.ContractId = c.Id
  JOIN Cars cr ON cr.Id = cp.PositionId
 WHERE c.DeleteDate IS NULL
   AND c.MaturityDate > GETDATE()
   AND cr.BodyNumber = @vin
   AND c.Status = 30",
                new { vin }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Contract>> GetTranchesByVinCode(string vin)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT ResultContracts.*
            FROM Cars
            JOIN Positions ON Positions.Id = Cars.Id 
            JOIN ContractPositions on ContractPositions.PositionId = Positions.Id 
            JOIN Contracts ON Contracts.Id = ContractPositions.ContractId
            JOIN dogs.Tranches ON dogs.Tranches.CreditLineId = Contracts.Id
            JOIN Contracts AS ResultContracts ON ResultContracts.id = Tranches.Id 
            WHERE Cars.BodyNumber = @vin 
            AND Contracts.DeleteDate is null", new { vin }, UnitOfWork.Transaction);
        }

        public async Task<string> GetWaitPayTypeOperationCode(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<string>(@"SELECT pt.OperationCode
  FROM PayOperations po
  JOIN PayTypes pt ON pt.Id = po.PayTypeId
 WHERE po.DeleteDate IS NULL
   AND po.Status IN (0)
   AND po.ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Contract>> GetTrancheListForOnlineByCreditLineIdAsync(int creditLineId)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  LEFT JOIN dogs.Tranches tr ON tr.Id = c.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE tr.CreditLineId = @creditLineId
   AND c.Status IN (20, 24, 25, 26, 27, 29, 30, 50)
   AND c.CreatedInOnline = 1",
                new { creditLineId }, UnitOfWork.Transaction);
        }

        public async Task<OverdueForCrm> GetOverdueForCrmAsync(string contractNumber)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<OverdueForCrm>(@"SELECT TOP 1 cl.IdentityNumber,
       ISNULL( (SELECT TOP 1 clc.Address FROM ClientContacts clc WHERE clc.ClientId = cl.Id AND clc.ContactTypeId = 1 AND clc.IsDefault = 1),
         (SELECT TOP 1 clc.Address FROM ClientContacts clc WHERE clc.ClientId = cl.Id AND clc.ContactTypeId = 1)) AS MobilePhone,
       c.Id AS ContractId,
       c.ContractNumber,
       c.NextPaymentDate,
       cps.DebtCost,
       cps.PercentCost,
       ISNULL(PenyAccount.Balance * (-1), 0) AS PenaltyCost,
       cps.DebtCost + PercentCost + ISNULL(cps.PenaltyCost, 0) AS TotalCost,
       CASE
         WHEN DATEADD(DAY, 1, c.NextPaymentDate) = cast(dbo.GETASTANADATE() as date)
           THEN 1
         ELSE 0
       END AS ExpiredToday
  FROM Contracts c
  JOIN LoanPercentSettings lps ON lps.Id = c.SettingId
  JOIN ContractPaymentSchedule cps ON cps.ContractId = c.Id AND cps.Date = c.NextPaymentDate AND cps.DeleteDate IS NULL
  JOIN Clients cl ON cl.Id = c.ClientId
  LEFT JOIN (SELECT a.ContractId, SUM(a.Balance) AS Balance
               FROM Accounts a
               JOIN AccountSettings acs ON a.AccountSettingId = acs.Id
              WHERE acs.Code IN ('PENY_ACCOUNT', 'PENY_PROFIT')
                AND a.Balance < 0
              GROUP BY a.ContractId) AS PenyAccount ON PenyAccount.ContractId = c.Id
 WHERE c.Status = 30
   AND lps.UseSystemType = 1
   AND (c.NextPaymentDate < cast(dbo.GETASTANADATE() as date) OR PenyAccount.ContractId IS NOT NULL)
   AND c.ContractNumber = @contractNumber
   AND c.ContractClass != 2",
                new { contractNumber }, UnitOfWork.Transaction);
        }


        private IList<ContractListInfo> InternalGetFilteredList(ListQuery listQuery, object query = null, IList<int> creditLineIds = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var identityNumber = query?.Val<string>("IdentityNumber");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var carNumber = query?.Val<string>("CarNumber");
            var rca = query?.Val<string>("Rca");
            var isTransferred = query?.Val<bool?>("IsTransferred") ?? false;
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var clientId = query?.Val<int?>("ClientId");
            var settingId = query?.Val<int?>("SettingId");
            var allBranches = query?.Val<bool?>("AllBranches");

            var from = "FROM Contracts c";
            var predicate = string.Empty;
            var offset = string.Empty;

            if (creditLineIds?.Any() ?? false)
            {
                predicate = "WHERE c.ContractClass = 3 AND t.CreditLineId IN @creditLineIds";
                offset = "OFFSET (0) ROWS FETCH NEXT 100 ROWS ONLY";
            }
            else
            {
                offset = "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";
                var result = GetContractsPredicate(listQuery.Filter, beginDate, endDate,
                    collateralType, displayStatus, isTransferred, clientId, settingId);

                predicate = result.Item1;

                if (!string.IsNullOrEmpty(result.Item2))
                    from = result.Item2;
            }

            var pre = string.Empty;
            if (!String.IsNullOrEmpty(identityNumber) || !String.IsNullOrEmpty(carNumber) || !String.IsNullOrEmpty(rca))
            {
                if (!String.IsNullOrEmpty(identityNumber))
                    pre += " AND cc.IdentityNumber = @identityNumber";
                if (!String.IsNullOrEmpty(carNumber))
                    pre += " AND cars.TransportNumber = @carNumber";
                if (!String.IsNullOrEmpty(rca))
                    pre += " AND realty.rca = @rca";
            }
            else if (!creditLineIds?.Any() ?? true)
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }
            predicate += pre;

            string sql = $@"
DECLARE @current_date DATE = CONVERT(DATE, dbo.GETASTANADATE());

WITH contractsList AS (
  SELECT DISTINCT
         c.Id
         ,c.ContractNumber
         ,c.ContractClass
         ,c.ContractDate
         ,c.MaturityDate
         ,c.LoanCost
         ,c.LoanPeriod
         ,c.LoanPercent
         ,c.DeleteDate
         ,c.Status
         ,c.InscriptionId
         ,cps.NextPaymentDate
         ,c.ProlongDate
         ,c.BuyoutDate
         ,c.PercentPaymentType
         ,c.Locked
         ,c.SettingId
         ,cc.Id AS ClientId
         ,cc.FullName AS ClientFullName
         ,cc.MaidenName
         ,g.DisplayName AS BranchName
         ,u.FullName AS AuthorFullName
         ,lps.Name AS ProductName
         ,lps.IsFloatingDiscrete AS IsFloatingDiscrete
         ,lps.PaymentPeriodType
         ,(SELECT SUM(acc.Balance)
             FROM Accounts acc, AccountSettings accSetting
            WHERE acc.ContractId = c.Id
              AND acc.AccountSettingId = accSetting.Id
              AND accSetting.Code in ('PENY_ACCOUNT', 'PENY_PROFIT', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE')
            GROUP BY acc.ContractId) AS allPenBal
         ,(SELECT CONCAT(' ', IIF(cars.Id IS NOT NULL, CONCAT_WS(' ', cars.Mark, cars.Model, REPLACE(cars.TransportNumber, ' ', '')), CONCAT(dv.Name,' ',realty.RCA)),',',CHAR(10)) 
	        FROM ContractPositions cpp 
	        LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
	        LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
            LEFT JOIN DomainValues dv ON dv.Id = realty.RealtyTypeId
	        WHERE cpp.ContractId = c.Id AND cpp.DeleteDate IS NULL 
	        order by cpp.Id DESC
	        FOR XML PATH('')) AS grnzCollat
         ,Encumbrance.Id AS EncumbranceId
         ,t.CreditLineId
         ,c.CollateralType
         ,i.Status AS InscriptionStatus
         ,ccs.CollectionStatusCode
         ,DATEDIFF(DAY,ccs.StartDelayDate,dbo.GetAstanaDate()) AS DelayDays
    {from}
    JOIN Clients cc on cc.Id = c.ClientId
    JOIN Groups g on g.Id = c.BranchId
    JOIN Users u on u.Id = c.AuthorId
    LEFT JOIN dogs.Tranches t on t.Id = c.Id
    LEFT JOIN dogs.Tranches t2 on t2.CreditLineId = c.Id
    LEFT JOIN Contracts c2 on c2.Id = t2.Id
    LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
    LEFT JOIN LoanPercentSettings lps on lps.Id = c.SettingId
    LEFT JOIN ContractPositions cpp ON cpp.ContractId = c.Id
    LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
    LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
    LEFT JOIN DomainValues dv ON dv.Id = realty.RealtyTypeId
    LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id AND ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
    LEFT JOIN CollectionContractStatuses ccs2 ON ccs2.ContractId = c2.Id AND ccs2.IsActive = 1 AND ccs2.DeleteDate IS NULL
    OUTER APPLY(
        SELECT ce.Id FROM ContractExpenses1 ce 
        LEFT JOIN Expenses e ON e.Id = ce.ExpenseId AND e.DeleteDate IS NULL
	    WHERE ce.ContractId = c.Id AND ce.DeleteDate IS NULL
	    AND e.ActionType = 50 AND e.Cost > 0) Encumbrance
    OUTER APPLY (SELECT TOP 1 ISNULL(cps.NextWorkingDate, cps.Date) as NextPaymentDate
               FROM ContractPaymentSchedule cps
              WHERE cps.ContractId = c.Id
                AND cps.DeleteDate IS NULL
                AND cps.ActualDate IS NULL
                AND cps.Canceled IS NULL
                AND cps.Date <= @current_date
              ORDER BY cps.Date) cps
   {predicate}
   GROUP BY c.Id, c.ContractNumber, c.ContractClass, c.ContractDate, c.MaturityDate, c.LoanCost, c.LoanPeriod, c.LoanPercent, c.DeleteDate, c.Status, c.InscriptionId, cps.NextPaymentDate,
            c.ProlongDate, c.BuyoutDate, c.PercentPaymentType, c.Locked, c.SettingId, cc.Id, cc.FullName, cc.MaidenName, g.DisplayName, u.FullName, lps.Name, lps.IsFloatingDiscrete,
            lps.PaymentPeriodType, t.CreditLineId, c.CollateralType, Encumbrance.Id, i.Status, ccs.CollectionStatusCode, DATEDIFF(DAY,ccs.StartDelayDate,dbo.GetAstanaDate()), g.Name
   ORDER BY c.ContractDate DESC, c.ContractNumber Desc
  {offset}
)
SELECT *
       ,(CASE
           WHEN c.DeleteDate IS NOT NULL THEN 60
           WHEN c.Status = 0 THEN 0
           WHEN c.Status = 20 THEN 5
           WHEN c.Status = 24 THEN 24
           WHEN c.Status = 30 AND ISNULL(c.NextPaymentDate, @current_date) < @current_date THEN 20
           WHEN c.Status = 30 AND c.allPenBal < 0 THEN 20
           WHEN c.Status = 30 AND c.MaturityDate >= @current_date AND c.ProlongDate IS NOT NULL THEN 30
           WHEN c.Status = 30 AND c.MaturityDate >= @current_date AND c.ProlongDate IS NULL THEN 10
           WHEN c.Status = 40 THEN 40
           WHEN c.Status = 50 THEN 50
           WHEN c.Status = 60 THEN 55
           ELSE 0
       END) AS DisplayStatus
       ,IIF(EncumbranceId > 0, 1, 0) AS HasEncumbrance
  FROM contractsList c
 ORDER BY c.ContractDate DESC, c.ContractNumber Desc";
            var returnList = UnitOfWork.Session.Query<ContractListInfo>(sql,
                new
                {
                    listQuery.Filter,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    identityNumber,
                    beginDate,
                    endDate,
                    collateralType,
                    carNumber,
                    rca,
                    creditLineIds,
                    ownerIds,
                    clientId,
                    settingId
                }, UnitOfWork.Transaction, commandTimeout: 1800).ToList();
            return returnList;
        }

        private (string, string) GetContractsPredicate(string value, DateTime? beginDate, DateTime? endDate, CollateralType? collateralType,
            ContractDisplayStatus? displayStatus, bool isTransferred, int? clientId, int? settingId)
        {

            var predicate = new List<string>
            {
                "WHERE c.ContractClass != 3"
            };

            if (collateralType.HasValue)
                predicate.Add("c.CollateralType = @collateralType");

            if (isTransferred)
                predicate.Add("EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)");
            else
                predicate.Add("NOT EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)");

            if (!string.IsNullOrEmpty(value))
            {

                if (value.LastIndexOf("-") is int index && index > 0 && SRegex.Regex.IsMatch(value.Substring(index, value.Length - index), "^-T\\d{3}$"))
                {
                    predicate.Add($"c.ContractNumber LIKE N'%{value.Substring(0, index)}%'");
                }
                else
                {
                    var fields = new List<string> { "c.ContractNumber", "cc.FullName", "cc.IdentityNumber", "cc.MobilePhone", "cars.TransportNumber" };
                    predicate.Add($"({string.Join("\r\nOR ", fields.Select(f => $"{f} LIKE N'%{value}%'").ToArray())})");
                }
            }

            if (clientId.HasValue)
                predicate.Add("c.ClientId = @clientId");

            if (settingId.HasValue)
                predicate.Add("c.SettingId = @settingId");

            if (displayStatus.HasValue)
            {
                if (displayStatus.Value != ContractDisplayStatus.Deleted)
                    predicate.Add("c.DeleteDate IS NULL");

                var result = GetContractsDisplayStatusPredicate(displayStatus.Value, beginDate, endDate);

                return (string.Join("\r\nAND ", predicate.Concat(result.Item1).ToArray()), result.Item2);
            }
            else
            {
                if (beginDate.HasValue)
                    predicate.Add("c.ContractDate >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.ContractDate <= @endDate");

                predicate.Add("c.DeleteDate IS NULL");
            }

            return (string.Join("\r\nAND ", predicate.ToArray()), string.Empty);
        }

        private (List<string>, string) GetContractsDisplayStatusPredicate(ContractDisplayStatus displayStatus, DateTime? beginDate, DateTime? endDate)
        {

            var fromStr = string.Empty;
            var predicate = new List<string>();

            var needContractActionsStatuses = new List<ContractDisplayStatus>
                {
                    ContractDisplayStatus.Prolong,
                    ContractDisplayStatus.BoughtOut,
                    ContractDisplayStatus.SoldOut,
                    ContractDisplayStatus.MonthlyPayment
                };

            if (needContractActionsStatuses.Contains(displayStatus))
            {
                fromStr = "FROM ContractActions ca\r\n  JOIN Contracts c ON ca.ContractId = c.Id";

                if (beginDate.HasValue)
                    predicate.Add("c.[ContractDate] >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.[ContractDate] <= @endDate");
            }
            else
            {
                if (beginDate.HasValue)
                    predicate.Add("c.ContractDate >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.ContractDate <= @endDate");
            }

            var statusPredicate = displayStatus switch
            {
                ContractDisplayStatus.AwaitForMoneySend => "c.Status = 20 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL",
                ContractDisplayStatus.BoughtOut => "c.Status = 40 AND ca.ActionType IN (20, 30, 40, 90)",
                ContractDisplayStatus.MonthlyPayment => "c.Status = 30 AND ca.ActionType = 80 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE())",
                ContractDisplayStatus.New => "c.Status = 0",
                ContractDisplayStatus.Open => "c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL",
                ContractDisplayStatus.Overdue => "c.Status = 30 AND (c.NextPaymentDate < CONVERT(DATE, dbo.GETASTANADATE()))",
                ContractDisplayStatus.SoftCollection => $"(ccs.FincoreStatusId = {(int)ContractDisplayStatus.SoftCollection} OR ccs2.FincoreStatusId = {(int)ContractDisplayStatus.SoftCollection} )",
                ContractDisplayStatus.HardCollection => $"(ccs.FincoreStatusId = {(int)ContractDisplayStatus.HardCollection} OR ccs2.FincoreStatusId = {(int)ContractDisplayStatus.HardCollection} )",
                ContractDisplayStatus.LegalCollection => $"(ccs.FincoreStatusId =  {(int)ContractDisplayStatus.LegalCollection}  OR ccs2.FincoreStatusId =  {(int)ContractDisplayStatus.LegalCollection} )",
                ContractDisplayStatus.LegalHardCollection => $"(ccs.FincoreStatusId =  {(int)ContractDisplayStatus.LegalHardCollection}  OR ccs2.FincoreStatusId =  {(int)ContractDisplayStatus.LegalHardCollection} )",
                ContractDisplayStatus.Prolong => "c.Status = 30 AND ca.ActionType = 10 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL",
                ContractDisplayStatus.Signed => "c.Status = 30",
                ContractDisplayStatus.SoldOut => "c.Status = 50 AND ca.ActionType = 60",
                ContractDisplayStatus.Deleted => "c.DeleteDate IS NOT NULL",
                _ => string.Empty,
            };

            predicate.Add(statusPredicate);

            return (predicate, fromStr);
        }

        public async Task<List<ContractPosition>> GetPositionsByContractIdAsync(int contractId)
        {
            var positions = new List<ContractPosition>();

            var contractPositions = await UnitOfWork.Session.QueryAsync<ContractPosition, Position, ContractPosition>(@"
                SELECT cp.*, p.* FROM ContractPositions cp
                JOIN Positions p ON p.Id = cp.PositionId
                WHERE cp.DeleteDate IS NULL
                AND cp.ContractId = @contractId",
            (cp, p) =>
            {
                cp.Position = p;
                return cp;
            },
            new { contractId }, UnitOfWork.Transaction);

            return FillPositions(contractPositions);
        }

        public List<ContractPosition> GetPositionsByContractId(int contractId)
        {
            var contractPositions = UnitOfWork.Session.Query<ContractPosition, Position, ContractPosition>(@"
                SELECT cp.*, p.* FROM ContractPositions cp
                JOIN Positions p ON p.Id = cp.PositionId
                WHERE cp.DeleteDate IS NULL
                AND cp.ContractId = @contractId",
            (cp, p) =>
            {
                cp.Position = p;
                return cp;
            },
            new { contractId }, UnitOfWork.Transaction);

            return FillPositions(contractPositions);
        }

        private List<ContractPosition> FillPositions(IEnumerable<ContractPosition> contractPositions)
        {
            var positions = new List<ContractPosition>();

            var contractPositionsList = contractPositions.ToList();

            foreach (var contractPosition in contractPositionsList)
            {
                if (contractPosition.Position.CollateralType == CollateralType.Car)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Car, VehicleMark, VehicleModel, Category, Application, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Cars.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Cars.Mark,
                        Cars.Model,
                        Cars.ReleaseYear,
                        Cars.TransportNumber,
                        Cars.MotorNumber,
                        Cars.BodyNumber,
                        Cars.TechPassportNumber,
                        Cars.TechPassportDate,
                        Cars.Color,
                        Cars.ParkingStatusId,
                        Cars.VehicleMarkId,
	                    Cars.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        Categories.*,
                        Applications.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Cars ON Cars.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Cars.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Cars.VehicleModelId
                    LEFT JOIN Applications ON Applications.AppId in (SELECT TOP 1 AppId from Applications WHERE PositionId = ContractPositions.PositionId ORDER BY AppId DESC)
                    WHERE ContractPositions.Id = @contractPositionId",
                    (cp, p, m, mod, c, a) =>
                    {
                        cp.Position = p;
                        p.VehicleMark = m;
                        p.VehicleModel = mod;
                        cp.Category = c;
                        if (cp.Position.ClientId.HasValue)
                            cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                        if (a != null)
                        {
                            cp.TurboCost = a.TurboCost;
                            cp.MotorCost = a.MotorCost;
                        }
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else if (contractPosition.Position.CollateralType == CollateralType.Realty)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Realty, Category, PositionEstimate, DomainValue, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Realties.*,
                        Positions.Name,
                        Positions.CollateralType,
                        Positions.ClientId,
                        Categories.*,
                        pe.*,
                        dv.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Realties ON Realties.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    LEFT JOIN PositionEstimates pe ON ContractPositions.EstimationId = pe.Id
                    LEFT JOIN DomainValues dv ON dv.Id = Realties.RealtyTypeId
                    WHERE ContractPositions.Id = @contractPositionId",
                    (cp, r, c, pe, dv) =>
                    {
                        r.Address = _realtyAddressRepository.Get(r.Id);
                        r.RealtyDocuments = _realtyDocumentsRepository.GetDocumentsForRealty(r.Id);
                        cp.Position = r;
                        r.RealtyType = dv;
                        cp.Category = c;
                        if (cp.EstimationId.HasValue)
                        {
                            cp.PositionEstimate = _positionEstimatesRepository.Get(cp.EstimationId.Value);
                        }
                        cp.PositionEstimateHistory = _positionEstimateHistoryRepository.ListEstimationHistoryForPosition(r.Id);
                        //cp.PositionEstimate = pe;
                        if (cp.Position.ClientId.HasValue)
                            cp.Position.Client = _clientRepository.Get(cp.Position.ClientId.Value);
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else if (contractPosition.Position.CollateralType == CollateralType.Machinery)
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Machinery, VehicleMark, VehicleModel, VehicleLiquidity, Category, ContractPosition>(@"
                    SELECT
                        ContractPositions.*,
                        Machineries.Id,
                        Positions.Name,
                        Positions.CollateralType,
                        Machineries.Mark,
                        Machineries.Model,
                        Machineries.ReleaseYear,
                        Machineries.TransportNumber,
                        Machineries.MotorNumber,
                        Machineries.BodyNumber,
                        Machineries.TechPassportNumber,
                        Machineries.TechPassportDate,
                        Machineries.Color,
                        Machineries.ParkingStatusId,
                        Machineries.VehicleMarkId,
	                    Machineries.VehicleModelId,
                        VehicleMarks.*,
	                    VehicleModels.*,
                        VehicleLiquid.*,
                        Categories.*
                    FROM ContractPositions WITH(NOLOCK)
                    JOIN Positions ON Positions.Id = ContractPositions.PositionId
                    JOIN Machineries ON Machineries.Id = ContractPositions.PositionId
                    JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                    JOIN VehicleMarks ON VehicleMarks.Id = Machineries.VehicleMarkId
                    JOIN VehicleModels ON VehicleModels.Id = Machineries.VehicleModelId
                    WHERE ContractPositions.Id = @contractPositionId",
                (cp, p, m, mod, vl, c) =>
                {
                    cp.Position = p;
                    p.VehicleMark = m;
                    p.VehicleModel = mod;
                    cp.Category = c;
                    return cp;
                }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
                else
                {
                    positions.Add(UnitOfWork.Session.Query<ContractPosition, Position, Category, ContractPosition>(@"
                        SELECT *
                        FROM ContractPositions WITH(NOLOCK)
                        JOIN Positions ON Positions.Id = ContractPositions.PositionId
                        JOIN Categories ON ContractPositions.CategoryId = Categories.Id
                        WHERE ContractPositions.Id = @contractPositionId", (cp, p, c) =>
                    {
                        cp.Position = p;
                        cp.Category = c;
                        return cp;
                    }, new { contractPositionId = contractPosition.Id }, UnitOfWork.Transaction).FirstOrDefault());
                }
            }

            return positions;
        }

        public void SetDebtGracePeriodForContract(int contractId, int debtGracePeriod)
        {
            UnitOfWork.Session.Execute(@"IF EXISTS (SELECT Id FROM [dogs].[ContractAdditionalInfo] WHERE Id = @contractId)
            BEGIN
                UPDATE [dogs].[ContractAdditionalInfo] SET DebtGracePeriod = @debtGracePeriod WHERE Id = @contractId
            END
            ELSE
            BEGIN
                INSERT INTO [dogs].[ContractAdditionalInfo] (Id, DebtGracePeriod) VALUES (@contractId, @debtGracePeriod)
            END
                ", new { contractId, debtGracePeriod }, UnitOfWork.Transaction);
        }

        public bool IsOnline(int contractId)
        {
            var result = UnitOfWork.Session.QueryFirstOrDefault<bool?>(@"SELECT c.CreatedInOnline
  FROM Contracts c
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE c.Id = @contractId",
                new { contractId }, UnitOfWork.Transaction);

            return result ?? false;
        }

        public async Task<Contract> GetFirstTranche(int creditLineId, List<ContractStatus> statusList = null)
        {
            var predicate = "WHERE c.DeleteDate IS NULL AND tr.CreditLineId = @creditLineId";

            if (statusList != null && statusList.Count > 0)
                predicate += " AND c.Status IN @statusList";

            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Contract>($@"SELECT TOP 1 c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN dogs.Tranches tr ON tr.Id = c.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 {predicate}
 ORDER BY c.Id ASC",
                new { creditLineId, statusList = statusList?.Select(x => (int)x) }, UnitOfWork.Transaction);
        }

        public List<Contract> GetContractsByIdentityNumber(string identityNumber)
        {
            return UnitOfWork.Session.Query<Contract>(@"SELECT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN Clients cl ON cl.Id = c.ClientId
  LEFT JOIN dogs.Tranches tr on c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE cl.IdentityNumber = @identityNumber
   AND c.DeleteDate IS NULL
   AND c.ContractClass != 2
   AND c.Status IN (30,50)
 ORDER BY c.Id DESC",
                new { identityNumber }, UnitOfWork.Transaction).ToList();
        }

        public List<CollectionOverdueContract> GetOverdueContractList(CollectionOverdueDays collectionOverdueDays)
        {
            var overdueDays = collectionOverdueDays;
            string query = $@"WITH cte AS (
                                            SELECT 
                                                ccs.ContractId, 
                                                c1.ContractNumber, 
                                                ccs.StartDelayDate, 
                                                ccs.CollectionStatusCode AS StatusBeforeCode,
                                                DATEDIFF(DAY, ccs.StartDelayDate, dbo.GETASTANADATE()) AS DelayDays,
                                                ISNULL(car1.ParkingStatusId, car2.ParkingStatusId) AS ParkingStatusId,
                                                c1.CollateralType
                                            FROM CollectionContractStatuses ccs
                                            LEFT JOIN Contracts c1 ON c1.Id = ccs.ContractId
                                            LEFT JOIN ContractPositions cp1 ON cp1.ContractId = c1.Id
                                            LEFT JOIN Cars car1 ON car1.Id = cp1.PositionId
                                            LEFT JOIN dogs.Tranches t ON t.Id = c1.Id
                                            LEFT JOIN Contracts c2 ON c2.Id = t.CreditLineId
                                            LEFT JOIN ContractPositions cp2 ON cp2.ContractId = c2.Id
                                            LEFT JOIN Cars car2 ON car2.Id = cp2.PositionId
                                            WHERE ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
                                          )
                                            SELECT DISTINCT 
                                                cte.ContractId, 
                                                cte.ContractNumber, 
                                                cte.StartDelayDate, 
                                                cte.StatusBeforeCode,
                                                CASE 
                                                    WHEN cte.StatusBeforeCode = 'LEGALHARD_COLLECTION' THEN 'LEGALHARD_COLLECTION'
		                                            WHEN cte.StatusBeforeCode = 'LEGAL_COLLECTION' THEN 'LEGAL_COLLECTION'
		                                            WHEN cte.CollateralType = 60 THEN
                                                        CASE
                                                            WHEN cte.DelayDays BETWEEN {overdueDays.SoftCollection} AND {overdueDays.LegalhardByRealestate - 1} THEN 'HARD_COLLECTION'
                                                            WHEN cte.DelayDays >= {overdueDays.LegalhardByRealestate} THEN 'LEGALHARD_COLLECTION'
                                                            ELSE NULL
                                                        END
		                                            WHEN cte.CollateralType != 60 THEN
                                                        CASE
                                                            WHEN cte.DelayDays BETWEEN {overdueDays.SoftCollection} AND {overdueDays.Legalhard - 1} THEN 'HARD_COLLECTION'
                                                            WHEN cte.DelayDays >= {overdueDays.Legalhard} THEN
                                                                CASE
                                                                    WHEN cte.ParkingStatusId = 1 THEN 'LEGALHARD_COLLECTION'
                                                                    WHEN cte.ParkingStatusId = 2 OR cte.ParkingStatusId IS NULL THEN 'LEGAL_COLLECTION'
                                                                    ELSE NULL
                                                                END
                                                            ELSE NULL
                                                        END
                                                    ELSE NULL
                                                END AS StatusAfterCode,
                                                cte.DelayDays,
                                                cte.CollateralType
                                            FROM cte
                                            WHERE cte.DelayDays >= {overdueDays.SoftCollection};";

            var result = UnitOfWork.Session.Query<CollectionOverdueContract>(query, UnitOfWork.Transaction, commandTimeout: 4000);
            return result.ToList();
        }

        public IEnumerable<CollectionClose> GetFrozenContractToClose()
        {
            return UnitOfWork.Session.Query<CollectionClose>(@$"SELECT  
	cd.ContractId
	,0 as ActionId
	,DATEDIFF(DAY, ccs.StartDelayDate, dbo.GETASTANADATE()) AS DelayDays
FROM ClientDeferments cd
LEFT JOIN Contracts c ON c.Id = cd.ContractId
LEFT JOIN Clients cl ON cl.Id = cd.ClientId
LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = cd.ContractId
WHERE cd.DeleteDate IS NULL
AND c.DeleteDate IS NULL
AND cl.DeleteDate IS NULL
AND ccs.DeleteDate IS NULL
AND ccs.IsActive = 1
AND cd.Status = {(int)RestructuringStatusEnum.Frozen}", UnitOfWork.Transaction);
        }

        public List<CollectionOverdueContract> GetStartOverdueContractList(int softOverdueDays)
        {
            string query = $@"SELECT distinct 
                                c.id ContractId, 
                                c.ContractNumber, 
                                DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) AS DelayDays,
                                CASE 
	                                WHEN (DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) > 0 
			                            AND DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) < {softOverdueDays}) THEN 'SOFT_COLLECTION' 
	                            END as StatusAfterCode,
	                            CCS.CollectionStatusCode,
	                            c.NextPaymentDate StartDelayDate
                            FROM Contracts c 
                            LEFT JOIN CollectionContractStatuses ccs on ccs.ContractId = c.id
                            INNER JOIN ContractPaymentSchedule cps on cps.ContractId = c.Id and c.NextPaymentDate = cps.Date
                            WHERE c.Status IN ({(int)ContractStatus.Signed})
                            AND DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) > 0
                            AND ((CCS.CollectionStatusCode != 
	                            CASE 
		                            WHEN (DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) > 0 
			                            AND DATEDIFF(DAY, c.NextPaymentDate, dbo.GETASTANADATE()) < {softOverdueDays}) THEN 'SOFT_COLLECTION'
	                            END) OR CCS.id IS NULL)
                            AND c.DeleteDate IS NULL
                            AND cps.ActualDate IS NULL
                            AND cps.DeleteDate IS NULL
                            AND cps.Canceled IS NULL
                            AND cps.Prolongated IS NULL
                            AND (cps.NextWorkingDate IS NULL OR
                                (c.NextPaymentDate < CAST(dbo.GETASTANADATE() AS DATE)
                            AND cps.NextWorkingDate < CAST(dbo.GETASTANADATE() AS DATE)))";

            var result = UnitOfWork.Session.Query<CollectionOverdueContract>(query, UnitOfWork.Transaction);
            return result.ToList();
        }

        public List<int> GetContractsForInscriptionOffBalanceAdditionService(DateTime inscriptionsOnOrAfter)
        {
            var contracts = UnitOfWork.Session.Query<int>(@"
SELECT c.Id
FROM Contracts c
JOIN Inscriptions i ON c.InscriptionId = i.Id
WHERE i.Date >= @startDate
AND (i.Status != 20)
AND (c.Status = 30 OR c.Status = 50)
AND c.UsePenaltyLimit = 1
AND c.IsOffBalance = 1
AND c.DeleteDate IS NULL
AND i.DeleteDate IS NULL
", new { startDate = inscriptionsOnOrAfter }, UnitOfWork.Transaction).ToList();
            return contracts;
        }

        public List<int> GetContractsForInscriptionOffBalanceAdditionService(DateTime inscriptionsOnOrAfter, int branchId)
        {
            var contracts = UnitOfWork.Session.Query<int>(@"
SELECT c.Id
FROM Contracts c
JOIN Inscriptions i ON c.InscriptionId = i.Id
WHERE i.Date >= @startDate
AND (i.Status != 20)
AND (c.Status = 30 OR c.Status = 50)
AND c.UsePenaltyLimit = 1
AND c.IsOffBalance = 1
AND c.BranchId = @branchId
AND c.DeleteDate IS NULL
AND i.DeleteDate IS NULL
", new { startDate = inscriptionsOnOrAfter, branchId }, UnitOfWork.Transaction).ToList();
            return contracts;
        }

        public async Task<IEnumerable<Contract>> FindListByIdentityNumberAsync(string iin)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(@"SELECT DISTINCT c.*,
       tr.CreditLineId,
       tr.MaxCreditLineCost,
       cai.DebtGracePeriod
  FROM Contracts c
  JOIN Clients cl ON cl.Id = c.ClientId
  LEFT JOIN dogs.Tranches tr on c.Id = tr.Id
  LEFT JOIN dogs.ContractAdditionalInfo cai ON cai.Id = c.Id
  LEFT JOIN Groups g ON g.Id = c.BranchId
 WHERE cl.IdentityNumber = @iin
   AND c.Status IN (20, 24, 25, 26, 27, 29, 30, 50)
   AND c.DeleteDate IS NULL
 ORDER BY c.Id DESC",
                new { iin }, UnitOfWork.Transaction);
        }

        private Inscription GetContractInscription(int inscriptionId, IUnitOfWork unitOfWork)
        {
            var parameters = new { id = inscriptionId };
            var sqlQuery = @"
                SELECT *
                FROM Inscriptions WITH(NOLOCK)
                LEFT JOIN Users ON Inscriptions.AuthorId=Users.Id
                WHERE Inscriptions.Id = @id AND Inscriptions.DeleteDate IS NULL";

            var inscription = unitOfWork.Session.Query<Inscription, User, Inscription>(
                sqlQuery,
                (i, u) =>
                {
                    i.Author = u;
                    return i;
                },
                parameters,
                transaction: unitOfWork.Transaction
            ).FirstOrDefault();

            if (inscription != null)
            {
                inscription.Actions = GetInscriptionActions(inscription.Id, unitOfWork);
                inscription.Rows = GetInscriptionRows(inscription.Id, unitOfWork);
            }

            return inscription;
        }

        private List<InscriptionAction> GetInscriptionActions(int inscriptionId, IUnitOfWork unitOfWork)
        {
            var parameters = new { InscriptionId = inscriptionId };
            var sqlQuery = @"
                SELECT *
                FROM InscriptionActions WITH(NOLOCK)
                LEFT JOIN Users ON InscriptionActions.AuthorId=Users.Id
                WHERE InscriptionId = @InscriptionId AND InscriptionActions.DeleteDate IS NULL";

            var actions = unitOfWork.Session.Query<InscriptionAction, User, InscriptionAction>(
                sqlQuery,
                (i, u) =>
                {
                    i.Author = u;
                    return i;
                },
                parameters,
                transaction: unitOfWork.Transaction
            ).ToList();

            if (!actions.Any())
            {
                return actions;
            }

            foreach (var action in actions)
            {
                action.Rows = GetActionInscriptionRows(action.Id, unitOfWork);
            }

            return actions;
        }

        private List<InscriptionRow> GetInscriptionRows(int inscriptionId, IUnitOfWork unitOfWork)
        {
            var parameters = new { InscriptionId = inscriptionId };
            var sqlQuery = @"
                SELECT *
                FROM InscriptionRows
                WHERE InscriptionId = @InscriptionId";

            var inscriptionRows = unitOfWork.Session.Query<InscriptionRow>(sqlQuery, parameters, transaction: unitOfWork.Transaction).ToList();

            return inscriptionRows;
        }

        private List<InscriptionRow> GetActionInscriptionRows(int actionId, IUnitOfWork unitOfWork)
        {
            var parameters = new { id = actionId };
            var sqlQuery = @"
                SELECT r.*, debit.*, credit.*
                FROM InscriptionRows r WITH(NOLOCK)
                LEFT JOIN Accounts debit ON debit.Id = r.DebitAccountId
                LEFT JOIN Accounts credit ON credit.Id = r.CreditAccountId
                WHERE InscriptionActionId = @id";

            return unitOfWork.Session.Query<InscriptionRow, Account, Account, InscriptionRow>(
                sqlQuery,
                (row, debit, credit) =>
                {
                    row.DebitAccount = debit;
                    row.CreditAccount = credit;
                    return row;
                },
                parameters,
                transaction: unitOfWork.Transaction // Используем внешнюю транзакцию
            ).ToList();
        }

        public IEnumerable<ContractPosition> GetByClientId(int clientId, CollateralType collateralType)
        {
            return UnitOfWork.Session.Query<ContractPosition, Car, ContractPosition>(@"SELECT DISTINCT cp.*, car.*
  FROM Positions p
  JOIN ContractPositions cp ON cp.PositionId = p.Id
  JOIN Contracts c ON c.Id = cp.ContractId
  JOIN Clients cl ON cl.Id = c.ClientId
  LEFT JOIN Cars car ON car.Id = p.Id
 WHERE cp.DeleteDate IS NULL
   AND c.DeleteDate IS NULL
   AND c.Status IN (20, 24, 25, 26, 27, 29, 30, 50)
   AND cl.Id = @clientId
   AND p.CollateralType = @collateralType",
                (cp, car) =>
                {
                    if (cp != null && car != null)
                        cp.Position = car;

                    return cp;
                },
                new { clientId, collateralType }, UnitOfWork.Transaction);
        }

        public IDictionary<DateTime, int> GetUpcomingPaymentsDateByCreditLineId(int creditLineId)
        {
            return UnitOfWork.Session.Query(@"SELECT DISTINCT
       cps.Date,
       DATEDIFF(DAY, CAST(dbo.GETASTANADATE() AS DATE), cps.Date) AS UpcomingDays
  FROM dogs.Tranches t
  JOIN Contracts c ON c.Id = t.Id
  JOIN ContractPaymentSchedule cps ON cps.ContractId = c.Id
 WHERE c.Status = 30
   AND cps.DeleteDate IS NULL
   AND cps.Date > dbo.GETASTANADATE()
   AND cps.ActionId IS NULL
   AND cps.ActualDate IS NULL
   AND t.CreditLineId = @creditLineId
 ORDER BY cps.Date ASC;",
                new { creditLineId }, UnitOfWork.Transaction)
                .ToDictionary(
                    row => (DateTime)row.Date,
                    row => (int)row.UpcomingDays
                );
        }


        public List<int> GetCreditLinesFromTranchesIds(List<int> tranchesIds)
        {
            return UnitOfWork.Session.Query<int>(@"SELECT Distinct Tranches.CreditLineId FROM dogs.Tranches WHERE dogs.Tranches.Id IN @tranchesIds",
                new { tranchesIds }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractLoanSubject> GetContractLoanSubjects(int contractId)
        {
            return UnitOfWork.Session.Query<ContractLoanSubject, User, ContractLoanSubject>(@"SELECT sub.*,
       u.*
  FROM ContractLoanSubjects sub WITH(NOLOCK)
  LEFT JOIN Users u ON sub.AuthorId = u.Id
 WHERE sub.ContractId = @contractId
   AND DeleteDate IS NULL",
                (sub, u) =>
                {
                    sub.Author = u;
                    sub.Client = _clientRepository.Get(sub.ClientId);
                    sub.Subject = _loanSubjectRepository.Get(sub.SubjectId);
                    return sub;
                },
                new { contractId }, UnitOfWork.Transaction)
                .ToList();
        }

        public async Task<ContractReadyToMoneySendListView> GetContractsReadyToMoneySendList(ContractReadyToMoneySendListQuery query, int offset = 0, int limit = 20, bool isCashIssue = false, int? branchId = null)
        {
            ContractReadyToMoneySendListView view = new ContractReadyToMoneySendListView();
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("Contracts.ContractDate AS ContractDate");
            builder.Select("Contracts.ContractNumber AS ContractNumber");
            builder.Select("Clients.FullName AS ClientName");
            builder.Select("LoanPercentSettings.Name AS ProductName");
            builder.Select("LoanPercentSettings.LoanPercent * 30 AS 'Percent'");
            builder.Select("Contracts.LoanCost AS LoanCost");
            builder.Select("Contracts.MaturityDate AS MaturityDate");
            builder.Select("Contracts.Status AS StatusId");
            builder.Select("Groups.DisplayName AS BranchName");
            builder.Select("Users.FullName AS AuthorName");
            builder.Select("Positions.Name AS CarNumber");
            builder.Select("Clients.IdentityNumber AS IdentityNumber");
            builder.Select("Contracts.Id AS ContractId");
            builder.Select("Contracts.Id AS Id");
            builder.Select("ao.EncumbranceRegistered AS EncumbranceRegistered");
            builder.Select("ca.PayTypeId AS PayTypeId");
            builder.Select("pt.Name AS PayTypeName");
            #endregion

            #region Joins

            builder.Join(@"Clients ON Clients.Id = Contracts.ClientId");
            builder.Join(@"LoanPercentSettings ON LoanPercentSettings.Id = Contracts.SettingId");
            builder.Join(@"Groups ON Groups.Id = Contracts.BranchId");
            builder.Join(@"Users ON Users.Id = Contracts.AuthorId");
            builder.Join(@"dogs.Tranches AS Tranches ON Tranches.Id = Contracts.Id");
            builder.Join("Contracts AS CreditLine ON CreditLine.Id = Tranches.CreditLineId");
            builder.Join("ContractPositions ON ContractPositions.ContractId = CreditLine.Id");
            builder.Join("Positions ON Positions.Id = ContractPositions.PositionId");
            builder.Join("ApplicationsOnline ao ON ao.ContractId = Contracts.Id");
            builder.LeftJoin($"ContractActions ca ON ca.ContractId = Contracts.Id AND ca.ActionType = {(int)ContractActionType.Sign}");
            builder.LeftJoin($"PayTypes pt ON pt.Id = ca.PayTypeId");

            #endregion

            #region Where

            builder.Where("Contracts.Status IN (20, 24)");
            builder.Where("Contracts.CreatedInOnline = 1");
            builder.Where("ca.DeleteDate is null");

            if (!string.IsNullOrEmpty(query.CarNumber))
            {
                builder.Where("Positions.Name = @CarNumber", new { CarNumber = query.CarNumber });
            }

            if (!string.IsNullOrEmpty(query.IIN))
            {
                builder.Where("Clients.IdentityNumber = @IdentityNumber", new { IdentityNumber = query.IIN });
            }

            if (isCashIssue && branchId.HasValue)
            {
                builder.Where($"ao.CashIssueBranchId = {branchId.Value} AND ao.IsCashIssue = 1");
            }

            if (query.IsEncumbranceRegistered.HasValue && query.IsEncumbranceRegistered.Value)
            {
                builder.Where("ao.EncumbranceRegistered = 1");
            }

            #endregion

            builder.OrderBy("Contracts.Id Desc");


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM Contracts 
            /**join**/ /**leftjoin**/ /**where**/ /**orderby**/ OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var counter = builder.AddTemplate($@"SELECT COUNT(*) FROM Contracts 
            /**join**/ /**leftjoin**/ /**where**/");

            view.Count = await UnitOfWork.Session.QueryFirstAsync<int>(counter.RawSql, selector.Parameters);
            view.Items = await UnitOfWork.Session.QueryAsync<ContractReadyToMoneySendListItemView>(selector.RawSql, selector.Parameters);
            return view;
        }

        public void UpdateMaturityDate(Contract contract)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Contracts SET MaturityDate = @MaturityDate WHERE Id = @Id",
                    contract, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdateLoanPeriod(Contract contract)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Contracts SET LoanPeriod = @LoanPeriod WHERE Id = @Id",
                    contract, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<DateTime?> GetNearestTranchePaymentDateOfCreditLine(int creditLineId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<DateTime?>(@"SELECT TRANCHE.NextPaymentDate, Contracts.id FROM Contracts
                LEFT JOIN dogs.Tranches ON dogs.Tranches.CreditLineId = Contracts.Id
                JOIN Contracts AS TRANCHE ON TRANCHE.Id = dogs.Tranches.Id 
                WHERE Contracts.id = @creditLineId
                AND TRANCHE.DeleteDate IS NULL
                AND TRANCHE.Status = 30
                ORDER BY TRANCHE.NextPaymentDate ASC",
                new { creditLineId }, UnitOfWork.Transaction);
        }

        public async Task UpdatePeriodType(int periodTypeId, int contractId)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync($"UPDATE Contracts SET PeriodTypeId = @periodTypeId WHERE Id = @contractId", new { periodTypeId, contractId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task UpdateContractMarketing(Contract contract)
        {
            using var transaction = BeginTransaction();
            await UnitOfWork.Session.ExecuteAsync(@"UPDATE Contracts 
                        SET AttractionChannelId = @AttractionChannelId,
	                        LoanPurposeId = @LoanPurposeId,
	                        BusinessLoanPurposeId = @BusinessLoanPurposeId,
                            OkedForIndividualsPurposeId = @OkedForIndividualsPurposeId,
                            TargetPurposeId = @TargetPurposeId,
	                        OtherLoanPurpose = @OtherLoanPurpose
                        WHERE Id = @Id", contract, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public async Task<BaseListView<BaseContractInfoOnlineView>> GetBaseContractInfo(int limit = 20,
            int offset = 0,
            int? clientId = null,
            bool? isActive = null,
            int? contractClass = null, 
            int? contractId = null,
            int? creditLineId = null)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("DISTINCT Contracts.Id");
            builder.Select("Tranches2.CreditLineId");
            builder.Select("Contracts.ContractNumber");
            builder.Select("Contracts.NextPaymentDate");
            builder.Select("Contracts.ContractDate");
            builder.Select("Contracts.MaturityDate");
            builder.Select("Contracts.BuyoutDate");
            builder.Select("Contracts.ContractClass");
            builder.Select("Contracts.LoanPeriod");
            builder.Select("Contracts.BranchId");
            builder.Select("CASE WHEN Contracts.Status = 30 THEN 1 ELSE 0 END AS IsActive");
            builder.Select(@"CASE WHEN (Contracts.ContractClass <> 2 AND Contracts.NextPaymentDate < cast(dbo.GETASTANADATE() as date))
                                OR (MIN(Contracts2.NextPaymentDate) OVER (PARTITION BY Contracts.Id) < cast(dbo.GETASTANADATE() as date) AND Contracts.ContractClass = 2) THEN 1
                                ELSE 0
                                END AS IsOverdue");
            builder.Select(@"CASE WHEN(Contracts.ContractClass != 2 AND Contracts.IsOffBalance = 1) OR
                                (SUM(CAST(Contracts2.IsOffBalance AS INT)) OVER (PARTITION BY Contracts.Id)>0 AND Contracts.ContractClass = 2) THEN 1
                                ELSE 0
                                END AS IsOffBalance");
            builder.Select("Contracts.CollateralType");
            builder.Select("LoanPercentSettings.ScheduleType");
            builder.Select(
                "CASE WHEN Contracts.PercentPaymentType = 20 THEN 1 ELSE 0 END AS IsShortDiscrete");

            builder.Select(@"CASE WHEN (LegalCaseContractsStatus.IsActive=1 AND LegalCaseContractsStatus.DeleteDay IS NULL AND Contracts.ContractClass != 2)
                    OR (SUM(CAST(LegalCaseContractsStatus2.IsActive AS INT)) OVER (PARTITION BY Contracts.Id)>0 AND LegalCaseContractsStatus2.DeleteDay IS NULL AND Contracts.ContractClass = 2)
                    THEN 1
                    ELSE 0
                    END AS InLegalCollection");

            #endregion

            #region Join

            builder.LeftJoin("LoanPercentSettings ON Contracts.SettingId = LoanPercentSettings.Id");
            builder.LeftJoin("dogs.Tranches ON dogs.Tranches.CreditLineId = Contracts.Id");
            builder.LeftJoin("Contracts Contracts2 ON dogs.Tranches.Id=Contracts2.Id");
            builder.LeftJoin(@"(SELECT ContractId, DeleteDay, MAX(CAST(IsActive AS INT)) IsActive FROM LegalCaseContractsStatus GROUP BY ContractId, DeleteDay) LegalCaseContractsStatus
                                ON LegalCaseContractsStatus.ContractId=Contracts.Id");
            builder.LeftJoin("(SELECT ContractId, DeleteDay, MAX(CAST(IsActive AS INT)) IsActive FROM LegalCaseContractsStatus GROUP BY ContractId, DeleteDay) LegalCaseContractsStatus2 ON LegalCaseContractsStatus2.ContractId=dogs.Tranches.Id");
            builder.LeftJoin("dogs.Tranches Tranches2 ON Tranches2.Id=Contracts.Id");
            
            #endregion

            #region Where

            builder.Where("Contracts.DeleteDate IS NULL");
            builder.Where("Contracts.STATUS >= 30");
            builder.Where("Contracts.BranchId != 570");// https://dev.azure.com/tasfinance/FC/_workitems/edit/18307
            if (clientId != null)
            {
                builder.Where("Contracts.ClientId  = @clientId", new { clientId = clientId });
            }

            if (creditLineId != null)
            {
                builder.Where("Tranches2.CreditLineId  = @creditLineId", new { creditLineId = creditLineId });
            }
            if (isActive != null)
            {
                if (isActive.Value)
                {
                    builder.Where("Contracts.Status = 30");
                }
                else
                {
                    builder.Where("Contracts.Status != 30");
                }
            }

            if (contractClass != null)
            {
                builder.Where("Contracts.ContractClass  = @ContractClass", new { ContractClass = contractClass });
            }

            if (contractId != null)
            {
                builder.Where("Contracts.Id  = @contractId", new { contractId = contractId });
            }


            #endregion

            builder.OrderBy("Contracts.Id Desc");

            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM Contracts 
            /**leftjoin**/ /**where**/ /**orderby**/ OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");

            var counter = builder.AddTemplate($@"SELECT COUNT(DISTINCT Contracts.Id) FROM Contracts 
            /**join**/ /**leftjoin**/ /**where**/");


            return new BaseListView<BaseContractInfoOnlineView>
            {
                Count = await UnitOfWork.Session.QueryFirstAsync<int>(counter.RawSql, selector.Parameters),
                Limit = limit,
                List = await UnitOfWork.Session.QueryAsync<BaseContractInfoOnlineView>(selector.RawSql,
                    selector.Parameters),
                Offset = offset
            };
        }

        public async Task<IEnumerable<CreditLineLimitInfoView>> GetCreditLineInfos(List<int> contractIds)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("Contracts.Id AS ContractId");
            builder.Select("Accounts.Balance AS AvailableCreditLineLimit");
            builder.Select("Contracts.LoanCost AS InitialCreditLineLimit");
            builder.Select("Contracts.LoanCost - Accounts.Balance AS UsedCreditLineLimit");

            #endregion


            #region Join

            builder.LeftJoin(@"Accounts ON Accounts.ContractId = Contracts.Id AND Accounts.Code = '9990'");

            #endregion

            #region Where

            builder.Where("Contracts.Id IN @contractIds", new { contractIds = contractIds });

            #endregion


            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM Contracts 
            /**leftjoin**/ /**where**/ /**orderby**/ ");

            return await UnitOfWork.Session.QueryAsync<CreditLineLimitInfoView>(selector.RawSql, selector.Parameters);
        }
    }
}
