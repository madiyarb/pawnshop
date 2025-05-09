using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries.PrintTemplate;
using Pawnshop.Data.Models.Dictionaries.PrintTemplates;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class PrintTemplateRepository : RepositoryBase, IRepository<PrintTemplate>
    {
        private readonly DomainValueRepository _domainValueRepository;
        public PrintTemplateRepository(IUnitOfWork unitOfWork, DomainValueRepository domainValueRepository) : base(unitOfWork)
        {
            _domainValueRepository = domainValueRepository;
        }

        public void Insert(PrintTemplate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PrintTemplates ( Name, NameAlt, Code, AuthorId, CreateDate, HasNumber, CategoryId, SubCategoryId ) VALUES ( @Name, @NameAlt, @Code, @AuthorId, @CreateDate, @HasNumber, @CategoryId, @SubCategoryId )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
        public void InsertConfig(PrintTemplateCounterConfig entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PrintTemplateCounterConfigs ( TemplateId, NumberFormat, RelatesOnOrganization, RelatesOnBranch, RelatesOnCollateralType, RelatesOnProductType, RelatesOnYear, RelatesOnScheduleType, BeginFrom ) VALUES ( @TemplateId, @NumberFormat, @RelatesOnOrganization, @RelatesOnBranch, @RelatesOnCollateralType, @RelatesOnProductType, @RelatesOnYear, @RelatesOnScheduleType, @BeginFrom )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(PrintTemplate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE PrintTemplates SET Name = @Name, NameAlt = @NameAlt, Code = @Code, HasNumber = @HasNumber, CategoryId = @CategoryId, SubCategoryId = @SubCategoryId WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void UpdateConfig(PrintTemplateCounterConfig entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@$"UPDATE PrintTemplateCounterConfigs SET NumberFormat = @NumberFormat, RelatesOnOrganization = @RelatesOnOrganization,
RelatesOnBranch = @RelatesOnBranch, RelatesOnCollateralType = @RelatesOnCollateralType, RelatesOnProductType = @RelatesOnProductType, RelatesOnYear = @RelatesOnYear,
RelatesOnScheduleType = @RelatesOnScheduleType, BeginFrom = @BeginFrom WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void UpdateCounter(PrintTemplateCounter entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@$"
IF NOT EXISTS (SELECT Id FROM PrintTemplateCounters WHERE Id = @Id)
BEGIN
    INSERT INTO PrintTemplateCounters ( ConfigId, Counter, OrganizationId, BranchId, CollateralType, ProductTypeId, Year, ScheduleType )
    VALUES ( @ConfigId, @Counter, @OrganizationId, @BranchId, @CollateralType, @ProductTypeId, @Year, @ScheduleType )
END
ELSE
BEGIN
UPDATE PrintTemplateCounters SET ConfigId = @ConfigId, Counter = @Counter, OrganizationId = @OrganizationId, BranchId = @BranchId, CollateralType = @CollateralType, ProductTypeId = @ProductTypeId, Year = @Year, ScheduleType = @ScheduleType WHERE Id = @id
END", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE PrintTemplates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PrintTemplate Get(int id)
        {
            return UnitOfWork.Session.Query<PrintTemplate, User, PrintTemplate>(@"
SELECT ls.*, u.*
  FROM PrintTemplates ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
WHERE ls.Id = @id", (h, u) =>
            {
                h.Author = u;
                if (h.CategoryId.HasValue)
                    h.Category = _domainValueRepository.Get(h.CategoryId.Value);
                if (h.SubCategoryId.HasValue)
                    h.SubCategory = _domainValueRepository.Get(h.SubCategoryId.Value);
                return h;
            },new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public PrintTemplateCounterConfig GetConfigByTemplate(int templateId)
        {
            return UnitOfWork.Session.Query<PrintTemplateCounterConfig>(@"
SELECT *
  FROM PrintTemplateCounterConfigs c
WHERE c.TemplateId = @templateId", new { templateId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        private PrintTemplateCounterConfig GetConfig(int id)
        {
            return UnitOfWork.Session.Query<PrintTemplateCounterConfig>(@"
SELECT *
  FROM PrintTemplateCounterConfigs c
WHERE c.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public int Next(PrintTemplateCounterFilter query)
        {
            var config = GetConfig(query.ConfigId);
            var counter = FindCounter(query, config) ?? new PrintTemplateCounter(query, config);
            counter.Counter++;
            UpdateCounter(counter);
            return counter.Counter;
        }

        public PrintTemplate Find(object query)
        {
            throw new NotImplementedException();
        }

        public PrintTemplateCounter FindCounter(PrintTemplateCounterFilter filter, PrintTemplateCounterConfig config)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var condition = "WHERE ConfigId = @ConfigId";
            condition += config.RelatesOnCollateralType ? " AND CollateralType = @CollateralType" : string.Empty;
            condition += config.RelatesOnBranch ? " AND BranchId = @BranchId" : string.Empty;
            condition += config.RelatesOnOrganization ? " AND OrganizationId = @OrganizationId" : string.Empty;
            condition += config.RelatesOnProductType ? " AND ProductTypeId = @ProductTypeId" : string.Empty;
            condition += config.RelatesOnYear ? " AND Year = @Year" : string.Empty;
            condition += config.RelatesOnScheduleType ? " AND ScheduleType = @ScheduleType" : string.Empty;

            return UnitOfWork.Session.Query<PrintTemplateCounter>($@"
SELECT TOP 1 *
FROM PrintTemplateCounters
{condition}", filter, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<PrintTemplate> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PrintTemplate, User, PrintTemplate>($@"
SELECT ls.*, u.*
  FROM PrintTemplates ls
LEFT JOIN Users u ON u.Id = ls.AuthorId
{condition} {order} {page}", (h, u) =>
            {
                h.Author = u;
                if(h.CategoryId.HasValue)
                    h.Category = _domainValueRepository.Get(h.CategoryId.Value);
                if(h.SubCategoryId.HasValue)
                    h.SubCategory = _domainValueRepository.Get(h.SubCategoryId.Value);
                return h;
            },new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "ls.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM PrintTemplates ls
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(q)
FROM (
SELECT COUNT(*)
FROM LoanRequiredSubjects
WHERE SubjectId = @id
SELECT COUNT(*) as q
FROM ContractPrintTemplates
WHERE SubjectId = @id) as t", new { id });
        }
    }
}