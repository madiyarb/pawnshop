using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class LoanPercentSettingInsuranceCompanyRepository : RepositoryBase, IRepository<LoanPercentSettingInsuranceCompany>
    {
        public LoanPercentSettingInsuranceCompanyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(LoanPercentSettingInsuranceCompany entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO LoanPercentSettingInsuranceCompanies(SettingId, InsuranceCompanyId, InsurancePeriod, CreateDate, AuthorId, MaxPremium, PremiumAccuracy)
                        VALUES(@SettingId, @InsuranceCompanyId, @InsurancePeriod, @CreateDate, @AuthorId, @MaxPremium, @PremiumAccuracy)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void UpdateChangeableFileds(LoanPercentSettingInsuranceCompany entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE LoanPercentSettingInsuranceCompanies SET InsurancePeriod = @InsurancePeriod, MaxPremium = @MaxPremium, 
                                                                    PremiumAccuracy = @PremiumAccuracy, AuthorId = @AuthorId
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(LoanPercentSettingInsuranceCompany entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE LoanPercentSettingInsuranceCompanies SET SettingId = @SettingId, InsuranceCompanyId = @InsuranceCompanyId, 
                    InsurancePeriod = @InsurancePeriod, MaxPremium = @MaxPremium, PremiumAccuracy = @PremiumAccuracy
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanPercentSettingInsuranceCompany Get(int id)
        {
            /*return UnitOfWork.Session.QuerySingleOrDefault<LoanPercentSettingInsuranceCompany>(@"
                SELECT * 
                    FROM LoanPercentSettingInsuranceCompanies
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);*/

            return UnitOfWork.Session.Query<LoanPercentSettingInsuranceCompany, LoanPercentSetting, Group, LoanPercentSettingInsuranceCompany>(@"
                SELECT lpsi.*, lps.*, g.*
                FROM LoanPercentSettingInsuranceCompanies lpsi
                JOIN LoanPercentSettings lps ON lps.Id=lpsi.SettingId
                JOIN Groups g ON g.Id=lps.BranchId
                WHERE lpsi.Id = @id",
                (lpsi, lps, g) =>
                {
                    lpsi.Setting = lps;
                    lps.Branch = g;
                    return lpsi;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public LoanPercentSettingInsuranceCompany Find(object query)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");
            var settingId = query?.Val<int?>("SettingId");
            var condition = "WHERE DeleteDate IS NULL";

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;
            condition += settingId.HasValue ? " AND SettingId = @settingId" : string.Empty;

            return UnitOfWork.Session.QueryFirstOrDefault<LoanPercentSettingInsuranceCompany>($@"
                SELECT * 
                    FROM LoanPercentSettingInsuranceCompanies
                    {condition}
                       ", new { insuranceCompanyId, settingId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE LoanPercentSettingInsuranceCompanies SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<LoanPercentSettingInsuranceCompany> List(ListQuery listQuery, object query = null)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");
            var settingId = query?.Val<int?>("SettingId");

            var condition = "WHERE lp.DeleteDate IS NULL";

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;
            condition += settingId.HasValue ? " AND SettingId = @settingId" : string.Empty;

            return UnitOfWork.Session.Query<LoanPercentSettingInsuranceCompany, Client, ClientLegalForm, LoanPercentSetting, Group, LoanPercentSettingInsuranceCompany>($@"
                 SELECT lp.*, cl.*, lf.*, lps.*, g.*
                    FROM LoanPercentSettingInsuranceCompanies lp
                    LEFT JOIN LoanPercentSettings lps ON lps.Id=lp.SettingId
                    LEFT JOIN Groups g ON g.Id=lps.BranchId
                    LEFT JOIN Clients cl ON lp.InsuranceCompanyId = cl.Id
                    LEFT JOIN ClientLegalForms lf ON cl.LegalFormId = lf.Id {condition}", 
                (lp, cl, lf, lps, g) =>
                {
                    lp.InsuranceCompany = cl;
                    lp.InsuranceCompany.LegalForm = lf;
                    lp.Setting = lps;
                    lps.Branch = g;
                    return lp;
                } , new { insuranceCompanyId, settingId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");
            var settingId = query?.Val<int?>("SettingId");

            var condition = "WHERE DeleteDate IS NULL";
            condition += settingId.HasValue ? " AND SettingId = @settingId" : string.Empty;

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM LoanPercentSettingInsuranceCompanies {condition}", new { insuranceCompanyId, settingId },
                    UnitOfWork.Transaction);
        }

        public List<LoanPercentSettingInsuranceCompany> InsuranceCompaniesList()
        {
            return UnitOfWork.Session.Query<LoanPercentSettingInsuranceCompany, Client, LoanPercentSettingInsuranceCompany>($@"
                 SELECT distinct lp.InsuranceCompanyId, cl.*
                    FROM LoanPercentSettingInsuranceCompanies lp
                    JOIN Clients cl ON lp.InsuranceCompanyId = cl.Id",
                (lp, cl) =>
                {
                    lp.InsuranceCompany = cl;
                    return lp;
                },
                UnitOfWork.Transaction).ToList();
        }
    }
}