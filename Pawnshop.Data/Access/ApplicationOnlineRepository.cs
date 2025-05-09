using Dapper;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Helpers;
using Pawnshop.Data.Models.ApplicationOnlineLog;
using Pawnshop.Data.Models.ApplicationsOnline.Bindings;
using Pawnshop.Data.Models.ApplicationsOnline.Views;
using Pawnshop.Data.Models.ApplicationsOnline;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.PrintFormInfo;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineRepository : RepositoryBase
    {
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationOnlineRepository(IUnitOfWork unitOfWork,
            IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationOnline Get(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationsOnline.*");
            builder.Select("ApplicationOnlineRejectionReasons.Code AS RejectionReasonCode");
            builder.Select("ApplicationOnlineInsurances.Id AS ApplicationOnlineInsuranceId");
            builder.LeftJoin(
                "ApplicationOnlineRejectionReasons on ApplicationOnlineRejectionReasons.id  = ApplicationsOnline.RejectionReasonId");
            builder.LeftJoin(
                "ApplicationOnlineInsurances ON ApplicationsOnline.id  = ApplicationOnlineInsurances.ApplicationOnlineId AND ApplicationOnlineInsurances.DeleteDate IS NULL");

            builder.Where("ApplicationsOnline.id = @id",
                new { id });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationsOnline /**leftjoin**/ /**where**/ ");
            return UnitOfWork.Session.Query<ApplicationOnline>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ApplicationOnline GetByContractId(int contractId)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationsOnline.*");
            builder.Select("ApplicationOnlineRejectionReasons.Code AS RejectionReasonCode");
            builder.Select("ApplicationOnlineInsurances.Id AS ApplicationOnlineInsuranceId");
            builder.LeftJoin(
                "ApplicationOnlineRejectionReasons on ApplicationOnlineRejectionReasons.id  = ApplicationsOnline.RejectionReasonId");
            builder.LeftJoin(
                "ApplicationOnlineInsurances ON ApplicationsOnline.id  = ApplicationOnlineInsurances.ApplicationOnlineId AND ApplicationOnlineInsurances.DeleteDate IS NULL");

            builder.Where("ApplicationsOnline.ContractId = @contractId",
                new { contractId });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationsOnline /**leftjoin**/ /**where**/ ");
            return UnitOfWork.Session.Query<ApplicationOnline>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ApplicationOnline Get(int contractId)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationsOnline.*");
            builder.Select("ApplicationOnlineRejectionReasons.Code AS RejectionReasonCode");
            builder.Select("ApplicationOnlineInsurances.Id AS ApplicationOnlineInsuranceId");
            builder.LeftJoin(
                "ApplicationOnlineRejectionReasons on ApplicationOnlineRejectionReasons.id  = ApplicationsOnline.RejectionReasonId");
            builder.LeftJoin(
                "ApplicationOnlineInsurances ON ApplicationsOnline.id  = ApplicationOnlineInsurances.ApplicationOnlineId AND ApplicationOnlineInsurances.DeleteDate IS NULL");

            builder.Where("ApplicationsOnline.ContractId = @contractId",
                new { contractId });
            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationsOnline /**leftjoin**/ /**where**/ ");
            return UnitOfWork.Session.Query<ApplicationOnline>(builderTemplate.RawSql, builderTemplate.Parameters).FirstOrDefault();
        }

        public ApplicationOnline GetByApplicationOnlinePositionId(Guid id)
        {
            return UnitOfWork.Session.Query<ApplicationOnline>(@"Select * from ApplicationsOnline where ApplicationOnlinePositionId = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ApplicationOnlineView> GetView(Guid id)
        {
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select("ApplicationsOnline.Id");
            builder.Select("ApplicationsOnline.CreationAuthorId");
            builder.Select("ApplicationsOnline.LastChangeAuthorId");
            builder.Select("CreationUser.FullName AS CreationAuthor");
            builder.Select("UpdateUser.FullName AS LastChangeAuthor");
            builder.Select("ApplicationsOnline.CreateDate");
            builder.Select("ApplicationsOnline.UpdateDate");
            builder.Select("ApplicationsOnline.Status AS StatusCode");
            builder.Select("ApplicationsOnline.ApplicationNumber");
            builder.Select("ApplicationsOnline.ApplicationAmount");
            builder.Select("ApplicationsOnline.LoanTerm");
            builder.Select("ApplicationsOnline.Stage");
            builder.Select("ApplicationsOnline.ProductId");
            builder.Select("Product.Name AS Product");
            builder.Select("ApplicationsOnline.ApplicationSource");
            builder.Select("ApplicationsOnline.LoanPurposeId");
            builder.Select("ApplicationsOnline.BusinessLoanPurposeId");
            builder.Select("ApplicationsOnline.OkedForIndividualsPurposeId");
            builder.Select("ApplicationsOnline.TargetPurposeId");
            builder.Select("ApplicationsOnline.AttractionChannelId");
            builder.Select("ApplicationsOnline.Branchid");
            builder.Select("ApplicationsOnline.ApplicationNumber AS ApplicationNumber");
            builder.Select("g.DisplayName AS BranchName");
            builder.Select("ApplicationsOnline.MaximumAvailableLoanTerm AS MaximumAvailableLoanTerm");
            builder.Select("ApplicationsOnline.ApplicationOnlinePositionId");
            builder.Select("ApplicationsOnline.ClientId");
            builder.Select("ApplicationsOnline.Similarity");
            builder.Select("ApplicationsOnline.ResponsibleManagerId");
            builder.Select("ApplicationsOnline.CreditLineId");
            builder.Select("ApplicationsOnline.ContractId");
            builder.Select("ApplicationsOnline.FirstPaymentDate");
            builder.Select("ApplicationsOnline.RejectReasonComment");
            builder.Select("ApplicationsOnline.Type");
            builder.Select("ApplicationOnlineRejectionReasons.InternalReason AS RejectReason");
            builder.Select("Contracts.ContractNumber AS ContractNumber");
            builder.Select("ResponsibleManagerUser.FullName AS ResponsibleManagerName");
            builder.Select("ResponsibleVerificator.FullName AS ResponsibleVerificatorName");
            builder.Select("ResponsibleAdmin.FullName AS ResponsibleAdminName");
            builder.Select("ApplicationsOnline.ContractBranchId AS ContractBranchId");
            builder.Select("ContractGroup.DisplayName AS ContractBranchName");
            builder.Select("ApplicationsOnline.EncumbranceRegistered");
            builder.Select("ApplicationsOnline.PartnerCode AS ApplicationPartnerCode");
            #endregion

            #region Join
            builder.LeftJoin("Users AS CreationUser ON CreationUser.Id = ApplicationsOnline.CreationAuthorId");
            builder.LeftJoin("Users AS UpdateUser ON UpdateUser.id = ApplicationsOnline.LastChangeAuthorId");
            builder.LeftJoin("LoanPercentSettings AS Product ON Product.Id = ApplicationsOnline.ProductId");
            builder.LeftJoin("Groups AS g ON g.Id = ApplicationsOnline.Branchid");
            builder.LeftJoin("ApplicationOnlineRejectionReasons on ApplicationOnlineRejectionReasons.Id = ApplicationsOnline.RejectionReasonId");
            builder.LeftJoin("Contracts on Contracts.Id = ApplicationsOnline.ContractId");
            builder.LeftJoin(
                "Users AS ResponsibleManagerUser ON  ResponsibleManagerUser.id = ApplicationsOnline.ResponsibleManagerId");
            builder.LeftJoin(
                "Users AS ResponsibleVerificator ON  ResponsibleVerificator.id = ApplicationsOnline.ResponsibleVerificatorId");
            builder.LeftJoin(
                "Users AS ResponsibleAdmin ON  ResponsibleAdmin.id = ApplicationsOnline.ResponsibleAdminId");
            builder.LeftJoin("Groups AS ContractGroup ON ContractGroup.Id = ApplicationsOnline.ContractBranchId");

            #endregion

            #region Where

            builder.Where("ApplicationsOnline.id = @id", new { id = id });

            #endregion

            var selector = builder.AddTemplate($@"Select /**select**/ from ApplicationsOnline 
            /**leftjoin**/ /**where**/ ");

            ApplicationOnlineView view = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<ApplicationOnlineView>(selector.RawSql, selector.Parameters);

            if (view == null)
                return null;
            view.FillStatus();
            return view;
        }

        public async Task<ApplicationOnline> Insert(ApplicationOnline applicationOnline, int? userId = null)
        {
            await _service.LogApplicationOnline(new ApplicationOnlineLogData
            {
                ApplicationAmount = applicationOnline.ApplicationAmount,
                ApplicationId = applicationOnline.Id,
                LoanTerm = applicationOnline.LoanTerm,
                ProductId = applicationOnline.ProductId
            }, _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.QuerySingleOrDefaultAsync(@"
                INSERT INTO ApplicationsOnline ( ApplicationAmount, ApplicationNumber, ApplicationOnlineInsuranceId, ApplicationOnlinePositionId, BranchId, ClientId, CreateDate, 
                  CreationAuthorId, CreditLineId, Id, IncomeAmount, ExpenseAmount, LastChangeAuthorId, ListId, LoanTerm, MaximumAvailableLoanTerm, ProductId, Stage, Status, UpdateDate,
                  CorrectedIncomeAmount, CorrectedExpenseAmount, FirstPaymentDate, Type, AttractionChannelId, EncumbranceRegistered, PartnerCode, SignType ) 
                VALUES ( @ApplicationAmount, NEXT VALUE FOR ApplicationNumberSequence, @ApplicationOnlineInsuranceId, @ApplicationOnlinePositionId, @BranchId, @ClientId, @CreateDate,
                  @CreationAuthorId, @CreditLineId, @Id, @IncomeAmount, @ExpenseAmount, @LastChangeAuthorId, @ListId, @LoanTerm, @MaximumAvailableLoanTerm, @ProductId, @Stage, @Status, @UpdateDate,
                  @IncomeAmount, @ExpenseAmount, @FirstPaymentDate, @Type , @AttractionChannelId, @EncumbranceRegistered, @PartnerCode, @SignType )
                SELECT SCOPE_IDENTITY()", applicationOnline, UnitOfWork.Transaction);
                transaction.Commit();
                Guid id = applicationOnline.Id;
                return UnitOfWork.Session.Query<ApplicationOnline>(@"Select ApplicationsOnline.*, ApplicationOnlineInsurances.Id AS ApplicationOnlineInsuranceId from ApplicationsOnline
                Left JOIN ApplicationOnlineInsurances ON ApplicationsOnline.id  = ApplicationOnlineInsurances.ApplicationOnlineId
                AND ApplicationOnlineInsurances.DeleteDate IS NULL
                WHERE 
                ApplicationsOnline.id = @id",
                    new { id }, UnitOfWork.Transaction).FirstOrDefault();
            }
        }

        public async Task<ApplicationOnlineListView> GetList(GetApplicationOnlineListBinding binding, int offset, int limit, int? clientId = null, int? branchId = null)
        {
            if (!binding.ClientId.HasValue && clientId != null)
            {
                binding.ClientId = clientId;
            }
            SqlBuilder builder = new SqlBuilder();

            #region Select

            builder.Select(@"CASE WHEN ApplicationOnlineInsurances.Id IS NOT NULL 
                AND ApplicationOnlineInsurances.DeleteDate IS NULL THEN 1 ELSE 0 END AS Insurance");
            builder.Select("ApplicationOnlineInsurances.InsurancePremium AS InsurancePremium");
            builder.Select(@"CASE WHEN ApplicationOnlineInsurances.Id IS NOT NULL 
            AND ApplicationOnlineInsurances.DeleteDate IS NULL THEN ApplicationOnlineInsurances.TotalLoanAmount 
            ELSE ApplicationsOnline.ApplicationAmount END AS TotalApplicationAmount");
            builder.Select("ApplicationsOnline.Id AS ApplicationId");
            builder.Select("Clients.IdentityNumber AS IIN");
            builder.Select("Clients.Name AS Name ");
            builder.Select("Clients.Surname AS Surname ");
            builder.Select("Clients.Patronymic AS Patronymic ");
            builder.Select("ClientContacts.Address AS PhoneNumber");
            builder.Select("Clients.PartnerCode AS PartnerCode");
            builder.Select("ApplicationsOnline.Status AS ApplicationStatus");
            builder.Select("ApplicationsOnline.ResponsibleManagerId AS Manager");
            builder.Select(
                @"CASE WHEN ApplicationOnlineInsurances.Id IS NOT NULL 
                AND ApplicationOnlineInsurances.DeleteDate IS NULL THEN ApplicationOnlineInsurances.AmountForCustomer 
                ELSE ApplicationsOnline.ApplicationAmount END AS LoanCost");
            builder.Select("ApplicationsOnline.ProductId AS ProductId");
            builder.Select("ApplicationsOnline.Stage AS Stage");
            builder.Select("ApplicationsOnline.CreateDate AS CreateDate");
            builder.Select("ApplicationsOnline.UpdateDate AS UpdateDate");
            builder.Select("ApplicationsOnline.RejectionReasonId AS RejectReason");
            builder.Select("ApplicationsOnline.LoanTerm AS LoanTerm");
            builder.Select("ApplicationOnlineCars.Mark AS CarMark");
            builder.Select("ApplicationOnlineCars.Model AS CarModel");
            builder.Select("ApplicationOnlineCars.ReleaseYear AS CarYear");
            builder.Select("ApplicationOnlineCars.TransportNumber AS CarNumber");
            builder.Select("ApplicationOnlinePositions.EstimatedCost AS MarketCost");
            builder.Select("ApplicationNumber as ApplicationNumber");
            builder.Select("ApplicationOnlineEstimations.Status AS EstimationStatus");
            builder.Select("ApplicationOnlineRejectionReasons.InternalReason as RejectReason");
            builder.Select("LoanPercentSettings.LoanPercent* LoanPercentSettings.ContractPeriodFromType AS LoanPercent");
            builder.Select("ApplicationsOnline.ClientId as ClientId");
            builder.Select("Users.FullName as ResposableManagerName");
            builder.Select("Contracts.ContractNumber as ContractNumber");
            builder.Select("SalesManagerUser.Id AS SalesManagerId");
            builder.Select("SalesManagerUser.FullName AS SalesManager");
            builder.Select("ApplicationsOnline.PartnerCode AS ApplicationPartnerCode");

            #endregion
            #region Join

            builder.Join(@"Clients ON Clients.Id = ApplicationsOnline.ClientId");
            builder.Join(@"ApplicationOnlineCars ON ApplicationsOnline.ApplicationOnlinePositionId = ApplicationOnlineCars.Id");
            builder.Join(@"LoanPercentSettings ON LoanPercentSettings.Id = ApplicationsOnline.ProductId");
            builder.Join("ApplicationOnlinePositions ON ApplicationsOnline.ApplicationOnlinePositionId = ApplicationOnlinePositions.Id");

            #endregion
            #region LeftJoin

            builder.LeftJoin(
                @"ApplicationOnlineInsurances ON ApplicationOnlineInsurances.id =
                    (SELECT TOP 1 Id  FROM ApplicationOnlineInsurances 
                    WHERE ApplicationOnlineId = ApplicationsOnline.id  
                    ORDER BY CreateDate DESC)");
            builder.LeftJoin(@"ApplicationOnlineEstimations ON ApplicationOnlineEstimations.Id =
                   (SELECT TOP 1 Id
				   FROM ApplicationOnlineEstimations
				   WHERE ApplicationOnlineId = ApplicationsOnline.id
				   ORDER BY UpdateDate DESC)");
            builder.LeftJoin(
                @"ClientContacts ON ClientContacts.ClientId = ApplicationsOnline.ClientId
                    AND ClientContacts.IsDefault = 1 
                    AND ClientContacts.DeleteDate IS NULL");
            builder.LeftJoin(
                "ApplicationOnlineRejectionReasons on ApplicationsOnline.RejectionReasonId = ApplicationOnlineRejectionReasons.id");
            builder.LeftJoin(@"Users ON Users.Id = ApplicationsOnline.ResponsibleManagerId");
            builder.LeftJoin("Contracts ON Contracts.id = ApplicationsOnline.ContractId");
            builder.LeftJoin(
                @"ApplicationOnlineStatusChangeHistories ON ApplicationOnlineStatusChangeHistories.id = 
                    (SELECT TOP 1 Id 
                    FROM ApplicationOnlineStatusChangeHistories
                    WHERE ApplicationOnlineStatusChangeHistories.ApplicationOnlineId = ApplicationsOnline.id
                    AND ApplicationOnlineStatusChangeHistories.Status IN ('Consideration', 'Created', 'Verification')
                    ORDER BY CreateDate DESC)");
            builder.LeftJoin("Users AS SalesManagerUser ON SalesManagerUser.id = ApplicationOnlineStatusChangeHistories.UserId");
            builder.LeftJoin(@"BranchesPartnerCodes ON BranchesPartnerCodes.PartnerCode = Clients.PartnerCode");
            builder.LeftJoin(
                "BranchesPartnerCodes AS ApplicationOnlineBranchCodes ON ApplicationOnlineBranchCodes.PartnerCode = ApplicationsOnline.PartnerCode");

            #endregion
            #region Where

            if (branchId.HasValue)
            {
                builder.Where(@"((
   (Clients.PartnerCode IS NULL AND ApplicationsOnline.PartnerCode IS NULL)
   OR (BranchesPartnerCodes.Id IS NULL AND ApplicationOnlineBranchCodes.Id is NULL) 
   OR DATEADD(MINUTE, 10, ApplicationsOnline.CreateDate) < dbo.GETASTANADATE() 
   OR (BranchesPartnerCodes.BranchId = @BranchId AND ApplicationsOnline.PartnerCode IS NULL)
   OR (ApplicationsOnline.PartnerCode IS NOT NULL AND ApplicationOnlineBranchCodes.BranchId = @BranchId) )
OR ApplicationsOnline.Status IN ('Verification', 'RequisiteCheck', 'BiometricPassed'))", new { BranchId = branchId.Value });
            }

            if (binding.ClientId.HasValue)
            {
                builder.Where("ApplicationsOnline.ClientId = @ClientId", new { ClientId = binding.ClientId });
            }

            if (binding.Insurance.HasValue)
            {
                builder.Where("Insurance = ", new { Insurance = binding.Insurance.Value });
            }

            if (!string.IsNullOrEmpty(binding.EstimationStatus))
            {
                builder.Where($"ApplicationOnlineEstimations.Status = @EstimationStatus", new { EstimationStatus = binding.EstimationStatus });
            }

            if (binding.InsurancePremium.HasValue)
            {
                builder.Where($"ApplicationOnlineInsurances.InsurancePremium = {binding.InsurancePremium.Value}");
            }

            if (binding.TotalApplicationAmount.HasValue)
            {
                builder.Where($"ApplicationOnlineInsurances.TotalLoanAmount = {binding.TotalApplicationAmount.Value}");
            }

            if (binding.Percent.HasValue)
            {
                builder.Where($"ABS(LoanPercentSettings.LoanPercent * LoanPercentSettings.ContractPeriodFromType  - " +
                            $"{binding.Percent.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} ) < 0.01");
            }

            if (binding.ApplicationId.HasValue)
            {
                builder.Where($"ApplicationsOnline.Id = '{binding.ApplicationId}'");
            }

            if (!string.IsNullOrEmpty(binding.IIN))
            {
                builder.Where($"Clients.IdentityNumber Like N'%{binding.IIN}%'");
            }

            if (!string.IsNullOrEmpty(binding.Name))
            {
                builder.Where($"Clients.Name Like N'%{binding.Name}%'");
            }

            if (!string.IsNullOrEmpty(binding.Surname))
            {
                builder.Where($"Clients.Surname Like N'%{binding.Surname}%'");
            }

            if (!string.IsNullOrEmpty(binding.Patronymic))
            {
                builder.Where($"Clients.Patronymic Like N'%{binding.Patronymic}%'");
            }

            if (!string.IsNullOrEmpty(binding.PhoneNumber))
            {
                builder.Where($"ClientContacts.Address Like '%{binding.PhoneNumber}%'");
            }

            if (binding.ApplicationStatus != null && binding.ApplicationStatus.Any())
            {
                string stautuses = "";
                foreach (var status in binding.ApplicationStatus)
                {
                    stautuses += $"'{status}',";
                }

                stautuses = stautuses.Substring(0, stautuses.Length - 1);
                builder.Where($"ApplicationsOnline.Status IN ( {stautuses} )");
            }

            if (!string.IsNullOrEmpty(binding.Manager))
            {
                builder.Where($"ApplicationsOnline.ResponsibleManagerId = '{binding.Manager}'");
            }

            if (binding.LoanCost.HasValue)
            {
                builder.Where($"ApplicationsOnline.ApplicationAmount = {binding.LoanCost}");
            }

            if (binding.ProductId.HasValue)
            {
                builder.Where($"ApplicationsOnline.ProductId = {binding.ProductId}");
            }

            if (!string.IsNullOrEmpty(binding.PartnerCode))
            {
                builder.Where($"Clients.PartnerCode = '{binding.PartnerCode}'");
            }

            if (!string.IsNullOrEmpty(binding.ApplicationPartnerCode))
            {
                builder.Where($"ApplicationsOnline.PartnerCode = '{binding.ApplicationPartnerCode}'");
            }

            if (binding.Stage.HasValue)
            {
                builder.Where($"ApplicationsOnline.Stage = {binding.Stage}");
            }

            if (binding.CreateDateBefore.HasValue)
            {
                builder.Where($"ApplicationsOnline.CreateDate <= '{binding.CreateDateBefore.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
            }

            if (binding.CreateDateAfter.HasValue)
            {
                builder.Where($"ApplicationsOnline.CreateDate >= '{binding.CreateDateAfter.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
            }

            if (binding.UpdateDateBefore.HasValue)
            {
                builder.Where($"ApplicationsOnline.UpdateDate <= '{binding.UpdateDateBefore.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
            }

            if (binding.UpdateDateAfter.HasValue)
            {
                builder.Where($"ApplicationsOnline.UpdateDate >= '{binding.UpdateDateAfter.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
            }

            if (binding.RejectReason.HasValue)
            {
                builder.Where($"ApplicationsOnline.RejectionReasonId = {binding.RejectReason}");
            }

            if (!string.IsNullOrEmpty(binding.CarMark))
            {
                builder.Where($"ApplicationOnlineCars.Mark = '{binding.CarMark}'");
            }

            if (!string.IsNullOrEmpty(binding.CarModel))
            {
                builder.Where($"ApplicationOnlineCars.Model =  '{binding.CarModel}'");
            }

            if (binding.CarYear.HasValue)
            {
                builder.Where($"ApplicationOnlineCars.ReleaseYear = {binding.CarYear}");
            }

            if (!string.IsNullOrEmpty(binding.CarNumber))
            {
                builder.Where($"ApplicationOnlineCars.BodyNumber Like '%{binding.CarNumber}%'");
            }

            if (binding.MarketCost.HasValue)
            {
                builder.Where($"ApplicationOnlinePositions.EstimatedCost = {binding.MarketCost}");
            }

            if (!string.IsNullOrEmpty(binding.ApplicationNumber))
            {
                builder.Where($"ApplicationsOnline.ApplicationNumber = '{binding.ApplicationNumber}'");
            }

            if (!string.IsNullOrEmpty(binding.ContractNumber))
            {
                builder.Where($"Contracts.ContractNumber = '{binding.ContractNumber}'");
            }

            if (binding.SalesManagerId.HasValue)
            {
                builder.Where($"SalesManagerUser.id = {binding.SalesManagerId}");
            }

            if (binding.MinutesLeftAfterUpdate.HasValue)
            {
                builder.Where(
                    $"DATEDIFF(MINUTE, ApplicationsOnline.UpdateDate, dbo.GETASTANADATE()) >= {binding.MinutesLeftAfterUpdate}");
            }
            #endregion

            builder.OrderBy("ApplicationsOnline.CreateDate Desc");

            var listView = new ApplicationOnlineListView();

            var selector = builder.AddTemplate($@"SELECT /**select**/ FROM ApplicationsOnline 
            /**join**/ /**leftjoin**/ /**where**/ /**orderby**/ OFFSET ({offset}) ROWS FETCH NEXT {limit} ROWS ONLY");
            var counter = builder.AddTemplate($@"SELECT COUNT(*) FROM ApplicationsOnline 
            /**join**/ /**leftjoin**/ /**where**/");

            listView.Count = await UnitOfWork.Session.QueryFirstAsync<int>(counter.RawSql, selector.Parameters);
            listView.List = await UnitOfWork.Session.QueryAsync<ApplicationOnlineListItemView>(selector.RawSql, selector.Parameters);
            listView.FillAdditionalInfo();

            return listView;

        }

        public async Task Update(ApplicationOnline applicationOnline)
        {
            var query = $@"
                UPDATE ApplicationsOnline SET
                LastChangeAuthorId = {applicationOnline.LastChangeAuthorId} ,
                UpdateDate = '{applicationOnline.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}' ,
                ApplicationAmount = {applicationOnline.ApplicationAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} ,
                LoanTerm = {applicationOnline.LoanTerm} ,
                Stage = {applicationOnline.Stage} ,
                ProductId = {applicationOnline.ProductId} , 
                Status = N'{applicationOnline.Status}' ,
                RejectReasonComment = N'{applicationOnline.RejectReasonComment} '";

            List<string> nullableQuery = new List<string>();

            #region fill query nullable params

            if (applicationOnline.RejectionReasonId.HasValue)
            {
                nullableQuery.Add($"RejectionReasonId = {applicationOnline.RejectionReasonId}");
            }

            if (applicationOnline.BranchId.HasValue)
            {
                nullableQuery.Add($"BranchId = {applicationOnline.BranchId}");
            }

            if (applicationOnline.Similarity.HasValue)
            {
                nullableQuery.Add($"Similarity = {applicationOnline.Similarity.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (applicationOnline.MaximumAvailableLoanTerm.HasValue)
            {
                nullableQuery.Add($"MaximumAvailableLoanTerm = {applicationOnline.MaximumAvailableLoanTerm}");
            }

            if (applicationOnline.ContractId.HasValue)
            {
                nullableQuery.Add($"ContractId = {applicationOnline.ContractId}");
            }

            if (applicationOnline.CreditLineId.HasValue)
            {
                nullableQuery.Add($"CreditLineId = {applicationOnline.CreditLineId}");
            }

            if (applicationOnline.LoanPurposeId.HasValue)
            {
                nullableQuery.Add($"LoanPurposeId = {applicationOnline.LoanPurposeId}");
            }

            var businessLoanPurpose = applicationOnline.BusinessLoanPurposeId.HasValue ? applicationOnline.BusinessLoanPurposeId.ToString() : "null";
            nullableQuery.Add($"BusinessLoanPurposeId = {businessLoanPurpose}");
            
            var okedForIndividualsPurpose = applicationOnline.OkedForIndividualsPurposeId.HasValue ? applicationOnline.OkedForIndividualsPurposeId.ToString() : "null";
            nullableQuery.Add($"OkedForIndividualsPurposeId = {okedForIndividualsPurpose}");
            
            var targetPurpose = applicationOnline.TargetPurposeId.HasValue ? applicationOnline.TargetPurposeId.ToString() : "null";
            nullableQuery.Add($"TargetPurposeId = {targetPurpose}");

            if (applicationOnline.AttractionChannelId.HasValue)
            {
                nullableQuery.Add($"AttractionChannelId = {applicationOnline.AttractionChannelId}");
            }

            if (applicationOnline.ResponsibleManagerId.HasValue)
            {
                nullableQuery.Add($"ResponsibleManagerId = {applicationOnline.ResponsibleManagerId}");
            }
            else
            {
                nullableQuery.Add($"ResponsibleManagerId = null");
            }

            if (applicationOnline.CorrectedIncomeAmount.HasValue)
            {
                nullableQuery.Add($"CorrectedIncomeAmount = {applicationOnline.CorrectedIncomeAmount.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (applicationOnline.CorrectedExpenseAmount.HasValue)
            {
                nullableQuery.Add($"CorrectedExpenseAmount = {applicationOnline.CorrectedExpenseAmount.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            }

            if (applicationOnline.FirstPaymentDate.HasValue)
            {
                nullableQuery.Add($"FirstPaymentDate = '{applicationOnline.FirstPaymentDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}'");
            }

            if (applicationOnline.ContractBranchId.HasValue)
            {
                nullableQuery.Add($"ContractBranchId = {applicationOnline.ContractBranchId}");
            }
            else
            {
                nullableQuery.Add($"ContractBranchId = null");
            }

            if (applicationOnline.ResponsibleVerificatorId.HasValue)
            {
                nullableQuery.Add($"ResponsibleVerificatorId = {applicationOnline.ResponsibleVerificatorId}");
            }

            if (applicationOnline.ResponsibleAdminId.HasValue)
            {
                nullableQuery.Add($"ResponsibleAdminId = {applicationOnline.ResponsibleAdminId}");
            }

            #endregion

            if (nullableQuery.Any())
            {
                query += " ,";
            }

            for (int i = 0; i < nullableQuery.Count; i++)
            {
                if (i == nullableQuery.Count - 1)
                {
                    query += nullableQuery[i];
                }
                else
                {
                    query += nullableQuery[i] + " ,";
                }
            }

            query += $@" 
                WHERE Id = '{applicationOnline.Id}'
                SELECT SCOPE_IDENTITY()";

            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.QuerySingleOrDefaultAsync(query, new { }, UnitOfWork.Transaction);

                transaction.Commit();
            }

            await _service.LogApplicationOnline(new ApplicationOnlineLogData
            {
                ApplicationAmount = applicationOnline.ApplicationAmount,
                ApplicationId = applicationOnline.Id,
                LoanTerm = applicationOnline.LoanTerm,
                ProductId = applicationOnline.ProductId
            }, _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public List<ApplicationOnline> GetListOnEstimateByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ApplicationOnline>(@"SELECT * FROM ApplicationsOnline WHERE Status = 'OnEstimation' AND ClientId = @clientId",
                new { clientId }, UnitOfWork.Transaction)
                .ToList();
        }

        public List<ApplicationOnlineDelayApproved> GetDelayApprovedIds()
        {
            return UnitOfWork.Session.Query<ApplicationOnlineDelayApproved>(@"SELECT ao.Id,
       ao.ClientId,
       ao.UpdateDate
  FROM ApplicationsOnline ao
  LEFT JOIN OnlineTasks ot ON ot.ApplicationId = ao.Id AND ot.Type = 'DelayApplication'
 WHERE ao.Status = 'Approved'
   AND DATEDIFF(MINUTE, ao.UpdateDate, dbo.GETASTANADATE()) >= 30
   AND ot.Id IS NULL",
                null, UnitOfWork.Transaction)
                .ToList();
        }

        public List<ApplicationOnline> GetIdListForDecline()
        {
            return UnitOfWork.Session.Query<ApplicationOnline>(@"SELECT *
  FROM ApplicationsOnline
 WHERE UpdateDate <= DATEADD(DAY, -3, dbo.GETASTANADATE())
   AND Status NOT IN ('ContractConcluded', 'Declined', 'Canceled')",
                null, UnitOfWork.Transaction)
                .ToList();
        }

        public async Task<IEnumerable<AdditionalContactLink>> GetLinkApplicationsByAdditionalContactPhoneNumber(string phoneNumber)
        {
            return await UnitOfWork.Session.QueryAsync<AdditionalContactLink>(@"SELECT ao.Id AS ApplicationOnlineId,
       ao.ApplicationNumber,
       c.FullName AS ClientFullName,
       cac.ContactOwnerTypeId,
       dv.Name AS ContactOwnerTypeName
  FROM ClientAdditionalContacts cac
  JOIN ApplicationsOnline ao ON ao.ClientId = cac.ClientId
  JOIN Clients c ON c.Id = ao.ClientId
  JOIN DomainValues dv ON dv.Id = cac.ContactOwnerTypeId
 WHERE cac.DeleteDate IS NULL
   AND cac.PhoneNumber = @phoneNumber",
                   new { phoneNumber }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<ApplicationOnlineByClientIdView>> GetListByClientId(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<ApplicationOnlineByClientIdView>(@"SELECT ao.Id,
       ao.ApplicationNumber,
       c.Id AS ContractId,
       c.ContractNumber
  FROM ApplicationsOnline ao
  LEFT JOIN Contracts c ON c.Id = ao.ContractId
 WHERE ao.ClientId = @clientId",
                new { clientId }, UnitOfWork.Transaction);
        }

        public async Task SetCashIssueBranch(ApplicationOnline applicationOnline)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.QuerySingleOrDefaultAsync(@"UPDATE ApplicationsOnline
   SET IsCashIssue = @IsCashIssue,
       CashIssueBranchId = @CashIssueBranchId,
       LastChangeAuthorId = @LastChangeAuthorId,
       UpdateDate = @UpdateDate
 WHERE Id = @Id",
                    applicationOnline, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task ChangeEncumbranceRegisteredState(ApplicationOnline applicationOnline)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.QuerySingleOrDefaultAsync(@"UPDATE ApplicationsOnline
   SET EncumbranceRegistered = @EncumbranceRegistered,
       LastChangeAuthorId = @LastChangeAuthorId,
       UpdateDate = @UpdateDate
 WHERE Id = @Id",
                    applicationOnline, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<ApplicationOnline> GetOnlyApplicationOnline(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationsOnline.*");

            builder.Where("ApplicationsOnline.id = @id",
                new { id });

            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationsOnline /**where**/ ");
            return UnitOfWork.Session.Query<ApplicationOnline>(builderTemplate.RawSql, builderTemplate.Parameters).FirstOrDefault();
        }

        public async Task<PrintFormOpenCreditLineQuestionnaireConditionInfo> GetClientIncomeInfoForPrintForm(Guid id)
        {
            var builder = new SqlBuilder();
            builder.Select("ApplicationsOnline.ApplicationAmount");
            builder.Select("ApplicationsOnline.LoanTerm");
            builder.Select("DomainValues.Name AS LoanPurposeRus");
            builder.Select("DomainValues.NameAlt AS LoanPurposeKaz");
            builder.Select("AttractionChannels.Name AS AttractionChannelRus");
            builder.Select("AttractionChannels.NameAlt AS AttractionChannelKaz");

            builder.LeftJoin("DomainValues ON DomainValues.Id = ApplicationsOnline.LoanPurposeId");
            builder.LeftJoin("AttractionChannels ON AttractionChannels.Id = ApplicationsOnline.AttractionChannelId");


            builder.Where("ApplicationsOnline.id = @id",
                new { id });

            var builderTemplate = builder.AddTemplate("Select /**select**/ from ApplicationsOnline /**leftjoin**/ /**where**/ /**orderby**/  ");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PrintFormOpenCreditLineQuestionnaireConditionInfo>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }
    }
}
