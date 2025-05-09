using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Localizations;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class LoanPercentRepository : RepositoryBase, IRepository<LoanPercentSetting>
    {
        private readonly LoanPercentSettingInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly LoanSettingRateRepository _loanSettingRateRepository;
        private readonly ISessionContext _sessionContext;
        private readonly LoanSettingProductTypeLTVRepository _loanSettingProductTypeLTVRepository;

        public LoanPercentRepository(IUnitOfWork unitOfWork, LoanPercentSettingInsuranceCompanyRepository insuranceCompanyRepository,
            LoanSettingRateRepository loanSettingRateRepository,
            ISessionContext sessionContext, LoanSettingProductTypeLTVRepository loanSettingProductTypeLTVRepository) : base(unitOfWork)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _sessionContext = sessionContext;
            _loanSettingRateRepository = loanSettingRateRepository;
            _loanSettingProductTypeLTVRepository = loanSettingProductTypeLTVRepository;
        }

        public void Insert(LoanPercentSetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanPercentSettings
( OrganizationId, BranchId, CollateralType, CardType, LoanCostFrom, LoanCostTo, LoanPeriod, MinLoanPeriod, LoanPercent,
Name, NameAlt, ScheduleType, ContractPeriodFrom, ContractPeriodFromType, ContractPeriodTo, ContractPeriodToType, DebtPeriod, DebtPeriodType,
PaymentPeriod, PaymentPeriodType, IsActual, IsProduct, AdditionAvailable, InitialFeeRequired, ProductTypeId, PartialPaymentRequiredSum,
PartialPaymentRequiredPercent, CategoryId, CurrencyId, ParentId, Note, ContractTypeId, PeriodTypeId, PaymentOrderSchema, AvailableDateFrom,
AvailableDateTill, IsInsuranceAvailable, UsePenaltyLimit, IsKdnRequired, DebtGracePeriodFrom, DebtGracePeriodFromType, DebtGracePeriodTo, DebtGracePeriodToType, IsFloatingDiscrete, IsLiquidityOn, IsInsuranceAdditionalLimitOn )

VALUES ( @OrganizationId, @BranchId, @CollateralType, @CardType, @LoanCostFrom, @LoanCostTo, @LoanPeriod, @MinLoanPeriod, @LoanPercent,
@Name, @NameAlt, @ScheduleType, @ContractPeriodFrom, @ContractPeriodFromType, @ContractPeriodTo, @ContractPeriodToType, @DebtPeriod, @DebtPeriodType,
@PaymentPeriod, @PaymentPeriodType, @IsActual, @IsProduct, @AdditionAvailable, @InitialFeeRequired, @ProductTypeId, @PartialPaymentRequiredSum,
@PartialPaymentRequiredPercent, @CategoryId, @CurrencyId, @ParentId, @Note, @ContractTypeId, @PeriodTypeId, @PaymentOrderSchema, @AvailableDateFrom,
@AvailableDateTill, @IsInsuranceAvailable, @UsePenaltyLimit, @IsKdnRequired, @DebtGracePeriodFrom, @DebtGracePeriodFromType, @DebtGracePeriodTo, @DebtGracePeriodToType, @IsFloatingDiscrete, @IsLiquidityOn, @IsInsuranceAdditionalLimitOn )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                if (entity.RequiredSubjects != null)
                    foreach (var subject in entity.RequiredSubjects)
                    {
                        subject.SettingId = entity.Id;
                        InsertSubject(subject);
                    }

                if (entity.PrintTemplates != null)
                    foreach (var template in entity.PrintTemplates)
                    {
                        InsertTemplate(template, entity.Id);
                    }

                if (entity.Restrictions != null)
                    foreach (var restriction in entity.Restrictions)
                    {
                        restriction.SettingId = entity.Id;
                        InsertRestriction(restriction);
                    }

                if (entity.InsuranceCompanies != null)
                    foreach (var insuranceCompany in entity.InsuranceCompanies)
                    {
                        insuranceCompany.SettingId = entity.Id;
                        insuranceCompany.CreateDate = DateTime.Now;
                        insuranceCompany.AuthorId = _sessionContext.UserId;
                        _insuranceCompanyRepository.Insert(insuranceCompany);
                    }

                if (entity.LoanSettingRates != null)
                {
                    foreach (var loanSettingRate in entity.LoanSettingRates)
                    {
                        loanSettingRate.ProductSettingId = entity.Id;
                        loanSettingRate.CreateDate = DateTime.Now;
                        loanSettingRate.AuthorId = _sessionContext.UserId;
                    }

                    _loanSettingRateRepository.InsertOrUpdate(entity.LoanSettingRates);
                }

                transaction.Commit();
            }
        }

        private void InsertSubject(LoanRequiredSubject subject)
        {
            subject.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanRequiredSubjects
( SubjectId, SettingId, Max, Min )
VALUES ( @SubjectId, @SettingId, @Max, @Min )
SELECT SCOPE_IDENTITY()", subject, UnitOfWork.Transaction);
        }

        private void InsertTemplate(PrintTemplate template, int settingId)
        {
            UnitOfWork.Session.Execute(@"
INSERT INTO LoanSettingTemplates
( SettingId, TemplateId )
VALUES ( @settingId, @templateId )
SELECT SCOPE_IDENTITY()", new { settingId, templateId = template.Id }, UnitOfWork.Transaction);
        }

        private void InsertRestriction(LoanPercentSettingRestriction restriction)
        {
            restriction.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanPercentSettingRestrictions
( SettingId, LoanCostFrom, LoanCostTo, LoanPercent, PenaltyPercent, HasPeriods )
VALUES ( @SettingId, @LoanCostFrom, @LoanCostTo, @LoanPercent, @PenaltyPercent, @HasPeriods )
SELECT SCOPE_IDENTITY()", restriction, UnitOfWork.Transaction);

            if (restriction.HasPeriods)
            {
                foreach (var period in restriction.Periods)
                {
                    period.RestrictionId = restriction.Id;
                    InsertResprictionPeriod(period);
                }
            }
        }

        private void InsertResprictionPeriod(LoanPercentSettingPeriod period)
        {
            period.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanPercentSettingPeriods
( RestrictionId, PaymentNumber, LoanPercent, PenaltyPercent )
VALUES ( @RestrictionId, @PaymentNumber, @LoanPercent, @PenaltyPercent )
SELECT SCOPE_IDENTITY()", period, UnitOfWork.Transaction);
        }

        public void Update(LoanPercentSetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE LoanPercentSettings
SET OrganizationId = @OrganizationId, BranchId = @BranchId, CollateralType = @CollateralType, CardType = @CardType,
    LoanCostFrom = @LoanCostFrom, LoanCostTo = @LoanCostTo, LoanPeriod = @LoanPeriod,
    MinLoanPeriod = @MinLoanPeriod, LoanPercent = @LoanPercent,
    Name = @Name, NameAlt = @NameAlt, ScheduleType = @ScheduleType, ContractPeriodFrom = @ContractPeriodFrom, ContractPeriodFromType = @ContractPeriodFromType,
    ContractPeriodTo = @ContractPeriodTo, ContractPeriodToType = @ContractPeriodToType,
    DebtPeriod = @DebtPeriod, DebtPeriodType = @DebtPeriodType, PaymentPeriod = @PaymentPeriod, PaymentPeriodType = @PaymentPeriodType,
    IsActual = @IsActual, IsProduct = @IsProduct, AdditionAvailable = @AdditionAvailable, InitialFeeRequired = @InitialFeeRequired,
    ProductTypeId = @ProductTypeId, PartialPaymentRequiredSum = @PartialPaymentRequiredSum, PartialPaymentRequiredPercent = @PartialPaymentRequiredPercent,
    CategoryId = @CategoryId, CurrencyId = @CurrencyId, ParentId = @ParentId, Note = @Note, ContractTypeId = @ContractTypeId, PeriodTypeId = @PeriodTypeId, 
    PaymentOrderSchema = @PaymentOrderSchema, AvailableDateFrom = @AvailableDateFrom, AvailableDateTill = @AvailableDateTill, 
    IsInsuranceAvailable = @IsInsuranceAvailable, UsePenaltyLimit = @UsePenaltyLimit, IsKdnRequired = @IsKdnRequired,
    DebtGracePeriodFrom = @DebtGracePeriodFrom, DebtGracePeriodFromType = @DebtGracePeriodFromType, DebtGracePeriodTo = @DebtGracePeriodTo, DebtGracePeriodToType = @DebtGracePeriodToType, IsFloatingDiscrete = @IsFloatingDiscrete,
    IsLiquidityOn = @IsLiquidityOn, IsInsuranceAdditionalLimitOn = @IsInsuranceAdditionalLimitOn
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                //удаление Субъектов, если есть сущетсвующие субъекты для данного продукта
                var existingSubjects = RequiredSubjects(entity.Id);
                if (existingSubjects.Any())
                {
                    if (entity.RequiredSubjects.Any())
                    {
                        foreach (var subject in existingSubjects)
                        {
                            if (!entity.RequiredSubjects.Exists(inSubject => inSubject.Id == subject.Id))
                            {
                                UnitOfWork.Session.Execute("DELETE FROM LoanRequiredSubjects WHERE Id = @id", new { subject.Id }, UnitOfWork.Transaction);
                            }
                        }
                    }
                    else
                    {
                        UnitOfWork.Session.Execute("DELETE FROM LoanRequiredSubjects WHERE SettingId = @id", new { entity.Id }, UnitOfWork.Transaction);
                    }
                }

                if (entity.RequiredSubjects != null)
                    foreach (var subject in entity.RequiredSubjects)
                    {
                        if (subject.Id == 0)
                        {
                            subject.SettingId = entity.Id;
                            InsertSubject(subject);
                        }
                        else
                        {
                            UpdateSubject(subject);
                        }
                    }

                UnitOfWork.Session.Execute("DELETE FROM LoanSettingTemplates WHERE SettingId = @id", new { entity.Id }, UnitOfWork.Transaction);

                if (entity.PrintTemplates != null)
                    foreach (var template in entity.PrintTemplates)
                    {
                        InsertTemplate(template, entity.Id);
                    }

                if (entity.Restrictions != null)
                    foreach (var restriction in entity.Restrictions)
                    {
                        if (restriction.Id == 0)
                        {
                            restriction.SettingId = entity.Id;
                            InsertRestriction(restriction);
                        }
                        else
                        {
                            UpdateRestriction(restriction);
                        }
                    }

                var insuranceCompaniesToDelete = Get(entity.Id).InsuranceCompanies.Where(w => !entity.InsuranceCompanies.Any(x => w.Id == x.Id));

                foreach (var insuranceCompany in insuranceCompaniesToDelete)
                    _insuranceCompanyRepository.Delete(insuranceCompany.Id);

                if (entity.InsuranceCompanies != null)
                    foreach (var insuranceCompany in entity.InsuranceCompanies)
                    {
                        if (insuranceCompany.Id == 0)
                        {
                            insuranceCompany.SettingId = entity.Id;
                            insuranceCompany.CreateDate = DateTime.Now;
                            insuranceCompany.AuthorId = _sessionContext.UserId;
                            _insuranceCompanyRepository.Insert(insuranceCompany);
                        }
                        else
                            _insuranceCompanyRepository.Update(insuranceCompany);
                    }

                var ratesToDelete = Get(entity.Id).LoanSettingRates.Where(w => !entity.LoanSettingRates.Any(x => w.Id == x.Id));

                foreach (var rate in ratesToDelete)
                    _loanSettingRateRepository.Delete(rate.Id);

                if (entity.LoanSettingRates != null)
                {
                    foreach (var rate in entity.LoanSettingRates)
                    {
                        if (rate.Id == 0)
                        {
                            rate.ProductSettingId = entity.Id;
                            rate.CreateDate = DateTime.Now;
                            rate.AuthorId = _sessionContext.UserId;
                        }
                    }

                    _loanSettingRateRepository.InsertOrUpdate(entity.LoanSettingRates);
                }

                transaction.Commit();
            }
        }

        private void UpdateSubject(LoanRequiredSubject subject)
        {
            UnitOfWork.Session.Execute(@"
UPDATE LoanRequiredSubjects
SET SubjectId = @SubjectId, SettingId = @SettingId, Max = @Max, Min = @Min
WHERE Id = @Id", subject, UnitOfWork.Transaction);
        }

        private void UpdateRestriction(LoanPercentSettingRestriction restriction)
        {
            UnitOfWork.Session.Execute(@"
UPDATE LoanPercentSettingRestrictions
SET SettingId = @SettingId, LoanCostFrom = @LoanCostFrom, LoanCostTo = @LoanCostTo, LoanPercent = @LoanPercent, PenaltyPercent = @PenaltyPercent, HasPeriods = @HasPeriods
WHERE Id = @Id", restriction, UnitOfWork.Transaction);

            if (restriction.HasPeriods)
            {
                UnitOfWork.Session.Execute("DELETE FROM LoanPercentSettingPeriods WHERE RestrictionId = @id", new { restriction.Id }, UnitOfWork.Transaction);
                foreach (var period in restriction.Periods)
                {
                    period.RestrictionId = restriction.Id;
                    InsertResprictionPeriod(period);
                }
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE LoanPercentSettings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public LoanPercentSetting Get(int id)
        {
            var setting = UnitOfWork.Session.Query<LoanPercentSetting, Group, LoanProductType, Category, Currency, LoanPercentSetting>(@"
SELECT l.*, g.*, p.*, cat.*, cur.*
FROM LoanPercentSettings l
LEFT JOIN Groups g ON l.BranchId = g.Id
LEFT JOIN LoanProductTypes p ON p.Id = l.ProductTypeId
LEFT JOIN Categories cat ON cat.Id = l.CategoryId
LEFT JOIN Currencies cur ON cur.Id = l.CurrencyId
WHERE l.Id = @id", (l, g, p, cat, cur) =>
            {
                l.Branch = g;
                l.ProductType = p;
                l.Category = cat;
                l.Currency = cur;
                return l;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (setting is null)
                throw new PawnshopApplicationException("Настройки продукта не найдены");

            setting.PrintTemplates = PrintTemplates(id);

            setting.RequiredSubjects = RequiredSubjects(id);

            setting.InsuranceCompanies = _insuranceCompanyRepository.List(new ListQuery(), new { SettingId = setting.Id });

            setting.LoanSettingRates = _loanSettingRateRepository.List(new ListQuery(), new { SettingId = setting.Id });

            if (setting.ParentId.HasValue)
                setting.Parent = Get(setting.ParentId.Value);

            setting.ProductTypeLTVs = _loanSettingProductTypeLTVRepository.ListForLoanSetting(id);

            return setting;
        }

        private List<PrintTemplate> PrintTemplates(int settingId)
        {
            return UnitOfWork.Session.Query<PrintTemplate, DomainValue, DomainValue, PrintTemplate>(@"
SELECT t.*, cat.*, subCat.*
FROM PrintTemplates t
JOIN LoanSettingTemplates st ON t.Id = st.TemplateId AND st.SettingId = @settingId
LEFT JOIN DomainValues cat ON cat.Id = t.CategoryId
LEFT JOIN DomainValues subCat ON subCat.Id = t.SubCategoryId",
(t, cat, subCat) =>
{
    t.Category = cat;
    t.SubCategory = subCat;
    return t;
},
new { settingId }, UnitOfWork.Transaction).ToList();
        }

        private List<LoanRequiredSubject> RequiredSubjects(int settingId)
        {
            return UnitOfWork.Session.Query<LoanRequiredSubject, LoanSubject, LoanRequiredSubject>(@"
SELECT rs.*, s.*
FROM LoanSubjects s
JOIN LoanRequiredSubjects rs ON rs.SubjectId = s.Id AND rs.SettingId = @settingId", (rs, s) =>
            {
                rs.Subject = s;
                return rs;
            }, new { settingId }, UnitOfWork.Transaction).ToList();
        }

        public LoanPercentSetting Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var branchId = query?.Val<int>("BranchId");
            var collateralType = query?.Val<CollateralType>("CollateralType");
            var cardType = query?.Val<CardType>("CardType");
            var loanCost = query?.Val<decimal?>("LoanCost");
            var loanPeriod = query?.Val<int>("LoanPeriod");
            var isProduct = query?.Val<bool?>("IsProduct");
            var isActual = query?.Val<bool?>("IsActual");
            var isKdnRequired = query?.Val<bool?>("IsKdnRequired");

            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));
            if (!collateralType.HasValue) throw new ArgumentNullException(nameof(collateralType));
            if (!cardType.HasValue) throw new ArgumentNullException(nameof(cardType));

            var condition = @"WHERE l.DeleteDate IS NULL
                                AND(l.BranchId IS NULL OR l.BranchId = @branchId)
                                AND(l.CollateralType = @collateralType OR l.CollateralType = 0)
                                AND(l.CardType = @cardType OR l.CardType = 0)";

            if (loanCost.HasValue)
                condition += " AND (@loanCost BETWEEN l.LoanCostFrom AND l.LoanCostTo)";
            if (loanPeriod.HasValue)
                condition += " AND (l.LoanPeriod = @loanPeriod OR l.LoanPeriod = 0)";

            if (isProduct.HasValue)
                condition += " AND l.IsProduct = @isProduct";

            if (isActual.HasValue)
                condition += " AND l.IsActual  = @isActual";

            if (isKdnRequired.HasValue)
                condition += " AND l.IsKdnRequired = @isKdnRequired";

            return UnitOfWork.Session.Query<LoanPercentSetting, Group, LoanProductType, Category, Currency, LoanPercentSetting>(@$"
                SELECT TOP 1 l.*, g.*, p.*, cat.*, cur.*
                    FROM LoanPercentSettings l
                        LEFT JOIN Groups g ON l.BranchId = g.Id
                        LEFT JOIN LoanProductTypes p ON p.Id = l.ProductTypeId
                        LEFT JOIN Categories cat ON cat.Id = l.CategoryId
                        LEFT JOIN Currencies cur ON cur.Id = l.CurrencyId
                            {condition}
                            ORDER BY l.CollateralType DESC,
                                l.CardType DESC,
                                l.LoanCostFrom DESC,
                                l.LoanCostTo DESC,
                                l.LoanPeriod DESC",
                (l, g, p, cat, cur) =>
                {
                    l.Branch = g;
                    l.ProductType = p;
                    l.Category = cat;
                    l.Currency = cur;
                    l.RequiredSubjects = RequiredSubjects(l.Id);
                    l.PrintTemplates = PrintTemplates(l.Id);
                    l.InsuranceCompanies = _insuranceCompanyRepository.List(new ListQuery(), new { SettingId = l.Id });
                    l.LoanSettingRates = _loanSettingRateRepository.List(new ListQuery(), new { SettingId = l.Id });
                    l.ProductTypeLTVs = _loanSettingProductTypeLTVRepository.ListForLoanSetting(l.Id);

                    return l;
                }, new
                {
                    branchId,
                    collateralType,
                    cardType,
                    loanCost,
                    loanPeriod,
                    isProduct,
                    isActual,
                    isKdnRequired
                }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<LoanPercentSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query?.Val<int?>("OrganizationId");
            if (!organizationId.HasValue) throw new ArgumentNullException(nameof(organizationId));
            var branchId = query?.Val<int?>("BranchId");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var cardType = query?.Val<CardType?>("CardType");
            var loanPeriod = query?.Val<int?>("LoanPeriod");
            var isProduct = query?.Val<bool?>("IsProduct");
            var isActual = query?.Val<bool?>("IsActual");
            var isKdnRequired = query?.Val<bool?>("IsKdnRequired");
            var contractClass = query?.Val<ContractClass?>("ContractClass");

            var pre = "l.DeleteDate IS NULL";
            /*pre += " AND l.UseSystemType != 1";  //Отображать (TSO ...) продукты.*/
            pre += organizationId.HasValue ? " AND l.OrganizationId = @organizationId" : string.Empty;
            pre += branchId.HasValue ? " AND (l.BranchId IS NULL OR l.BranchId = @branchId)" : string.Empty;
            pre += collateralType.HasValue ? " AND (l.CollateralType = @collateralType OR l.CollateralType = 0)" : string.Empty;
            pre += cardType.HasValue ? " AND (l.CardType = @cardType OR l.CardType = 0 OR l.CardType IS NULL)" : string.Empty;
            pre += isProduct.HasValue ? " AND l.IsProduct = @isProduct" : string.Empty;
            pre += isActual.HasValue ? " AND l.IsActual  = @isActual" : string.Empty;
            pre += isKdnRequired.HasValue ? " AND l.IsKdnRequired = @isKdnRequired" : string.Empty;
            pre += contractClass.HasValue ? " AND l.ContractClass = @ContractClass" : string.Empty;

            var condition = listQuery.Like(pre, "g.DisplayName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "l.CollateralType",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<LoanPercentSetting, Group, LoanProductType, Category, Currency, LoanPercentSetting>($@"
SELECT l.*, g.*, p.*, cat.*, cur.*
FROM LoanPercentSettings l
LEFT JOIN Groups g ON l.BranchId = g.Id
LEFT JOIN LoanProductTypes p ON p.Id = l.ProductTypeId
LEFT JOIN Categories cat ON cat.Id = l.CategoryId
LEFT JOIN Currencies cur ON cur.Id = l.CurrencyId
{condition} {order} {page}",
            (l, g, p, cat, cur) =>
            {
                l.Branch = g;
                l.ProductType = p;
                l.Category = cat;
                l.Currency = cur;
                l.RequiredSubjects = RequiredSubjects(l.Id);
                l.PrintTemplates = PrintTemplates(l.Id);
                l.InsuranceCompanies = _insuranceCompanyRepository.List(new ListQuery(), new { SettingId = l.Id });
                l.LoanSettingRates = _loanSettingRateRepository.List(new ListQuery(), new { SettingId = l.Id });
                l.Restrictions = new List<LoanPercentSettingRestriction>();
                l.ProductTypeLTVs = _loanSettingProductTypeLTVRepository.ListForLoanSetting(l.Id);

                return l;
            },
            new
            {
                organizationId,
                branchId,
                collateralType,
                cardType,
                loanPeriod,
                isProduct,
                isActual,
                isKdnRequired,
                contractClass,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var organizationId = query?.Val<int?>("OrganizationId");
            if (!organizationId.HasValue) throw new ArgumentNullException(nameof(organizationId));
            var branchId = query?.Val<int?>("BranchId");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var cardType = query?.Val<CardType?>("CardType");
            var loanPeriod = query?.Val<int?>("LoanPeriod");
            var isProduct = query?.Val<bool?>("IsProduct");
            var isActual = query?.Val<bool?>("IsActual");
            var isKdnRequired = query?.Val<bool?>("IsKdnRequired");
            var contractClass = query?.Val<string>("ContractClass");

            var pre = "l.DeleteDate IS NULL";
            pre += organizationId.HasValue ? " AND l.OrganizationId = @organizationId" : string.Empty;
            pre += branchId.HasValue ? " AND l.BranchId = @branchId" : string.Empty;
            pre += collateralType.HasValue ? " AND (l.CollateralType = @collateralType OR l.CollateralType = 0)" : string.Empty;
            pre += cardType.HasValue ? "  AND (l.CardType = @cardType OR l.CardType = 0)" : string.Empty;
            pre += isProduct.HasValue ? "  AND l.IsProduct = @isProduct" : string.Empty;
            pre += isActual.HasValue ? "  AND l.IsActual  = @isActual" : string.Empty;
            pre += isKdnRequired.HasValue ? "  AND l.IsKdnRequired = @isKdnRequired" : string.Empty;

            var condition = listQuery.Like(pre, "g.DisplayName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM LoanPercentSettings l
LEFT JOIN Groups g ON l.BranchId = g.Id
LEFT JOIN LoanProductTypes p ON p.Id = l.ProductTypeId
LEFT JOIN Categories cat ON cat.Id = l.CategoryId
LEFT JOIN Currencies cur ON cur.Id = l.CurrencyId
{condition}", new
            {
                organizationId,
                branchId,
                collateralType,
                cardType,
                loanPeriod,
                isProduct,
                isActual,
                isKdnRequired,
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public async Task<List<LoanPercentSetting>> GetChild(int id)
        {
            return UnitOfWork.Session.Query<LoanPercentSetting, Group, LoanProductType, Category, Currency, LoanPercentSetting>($@"
SELECT l.*, g.*, p.*, cat.*, cur.*
FROM LoanPercentSettings l
LEFT JOIN Groups g ON l.BranchId = g.Id
LEFT JOIN LoanProductTypes p ON p.Id = l.ProductTypeId
LEFT JOIN Categories cat ON cat.Id = l.CategoryId
LEFT JOIN Currencies cur ON cur.Id = l.CurrencyId
 where l.ParentId = @id",
            (l, g, p, cat, cur) =>
            {
                l.Branch = g;
                l.ProductType = p;
                l.Category = cat;
                l.Currency = cur;
                l.RequiredSubjects = RequiredSubjects(l.Id);
                l.PrintTemplates = PrintTemplates(l.Id);
                l.InsuranceCompanies = _insuranceCompanyRepository.List(new ListQuery(), new { SettingId = l.Id });
                l.LoanSettingRates = _loanSettingRateRepository.List(new ListQuery(), new { SettingId = l.Id });
                l.Restrictions = new List<LoanPercentSettingRestriction>();

                return l;
            },
            new
            {
                id
            }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<LoanPercentSetting>> GetOnlyListAsync(object query)
        {
            var predicate = "WHERE DeleteDate IS NULL AND IsActual = 1 AND ContractClass != 2";

            var useSystemType = query?.Val<UseSystemType?>("UseSystemType");

            if (useSystemType.HasValue)
            {
                predicate += useSystemType switch
                {
                    UseSystemType.OFFLINE => $" AND UseSystemType != {(int)UseSystemType.ONLINE}",
                    UseSystemType.ONLINE => $" AND UseSystemType != {(int)UseSystemType.OFFLINE}",
                    _ => "",
                };
            }

            return await UnitOfWork.Session.QueryAsync<LoanPercentSetting>($@"SELECT TOP 10 *
  FROM LoanPercentSettings
{predicate}
 ORDER BY Id DESC",
                new { useSystemType }, UnitOfWork.Transaction);
        }

        public async Task<LoanPercentSetting> GetOnlyAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<LoanPercentSetting>(@"SELECT *
  FROM LoanPercentSettings
 WHERE DeleteDate IS NULL
   AND Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<LoanPercentSetting>> FindListByProductTypeCode(string productTypeCode)
        {
            var products = await UnitOfWork.Session.QueryAsync<LoanPercentSetting>(@"SELECT lps.*
  FROM LoanPercentSettings lps
  JOIN LoanProductTypes lpt ON lpt.Id = lps.ProductTypeId
 WHERE lps.DeleteDate IS NULL
   AND lps.IsActual = 1
   AND lpt.Code = @productTypeCode",
                new { productTypeCode }, UnitOfWork.Transaction);

            foreach (var product in products)
            {
                if (product.TitleId.HasValue)
                {
                    product.Title = UnitOfWork.Session.Query<Localization>(@"SELECT * FROM Localizations WHERE LocalizationItemId = @itemId",
                        new { itemId = product.TitleId }, UnitOfWork.Transaction)
                        .ToList();
                }

                if (product.DescriptionId.HasValue)
                {
                    product.Description = UnitOfWork.Session.Query<Localization>(@"SELECT * FROM Localizations WHERE LocalizationItemId = @itemId",
                        new { itemId = product.DescriptionId }, UnitOfWork.Transaction)
                        .ToList();
                }
            }

            return products;
        }

        public LoanPercentSetting GetParent(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<LoanPercentSetting>(@"SELECT p.*
  FROM LoanPercentSettings p
  JOIN LoanPercentSettings c ON c.ParentId = p.Id
 WHERE p.DeleteDate IS NULL
   AND p.IsActual = 1
   AND c.DeleteDate IS NULL
   AND c.IsActual = 1
   AND c.Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public IEnumerable<LoanPercentSetting> GetListForOnline(ContractClass contractClass)
        {
            return UnitOfWork.Session.Query<LoanPercentSetting>($@"SELECT *
  FROM LoanPercentSettings
 WHERE DeleteDate IS NULL
   AND UseSystemType != 0
   AND ContractClass = @contractClass",
                new { contractClass }, UnitOfWork.Transaction);
        }
    }
}
