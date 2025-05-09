using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.OuterServiceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class OuterServiceSettingRepository : RepositoryBase, IRepository<OuterServiceSetting>
    {
        public OuterServiceSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(OuterServiceSetting entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO OuterServiceSettings(ServiceCompanyId, Login, Password, URL, ControllerURL, AuthTypeId, ServiceTypeId, AuthorId, CreateDate, DeleteDate)
                VALUES(@ServiceCompanyId, @Login, @Password, @URL, @ControllerURL, @AuthTypeId, @ServiceTypeId, @AuthorId, @CreateDate, @DeleteDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }
        
        public void Update(OuterServiceSetting entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE OuterServiceSettings
                SET 
                    ServiceCompanyId = @ServiceCompanyId,
                    Login = @Login, 
                    Password = @Password,
                    URL = @URL, 
                    ControllerURL = @ControllerURL,
                    AuthTypeId = @AuthTypeId,
                    ControllerURL = @ControllerURL,
                    ServiceTypeId = @ServiceTypeId,
                    CreateDate = @CreateDate,
                    DeleteDate = @DeleteDate                    
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE OuterServiceSettings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public OuterServiceSetting Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<OuterServiceSetting>(@"
                SELECT * 
                    FROM OuterServiceSettings
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public OuterServiceSetting Find(object query)
        {
            var code = query?.Val<string>("Code");

            var condition = "WHERE oss.DeleteDate IS NULL AND serviceType.DomainCode = @domainCode";

            condition += !string.IsNullOrEmpty(code) ? " AND serviceType.Code = @code" : string.Empty;

            return UnitOfWork.Session.Query<OuterServiceSetting>($@"
                SELECT *
                FROM OuterServiceSettings oss
                    LEFT JOIN DomainValues serviceType ON oss.ServiceTypeId = serviceType.Id
                {condition}",
                new { code, domainCode = Constants.OUTER_SERVICE_TYPES_DOMAIN_VALUE },
                UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<OuterServiceSetting> List(ListQuery listQuery, object query = null)
        {
            var serviceCompanyId = query?.Val<int?>("ServiceCompanyId");

            var condition = "WHERE oss.DeleteDate IS NULL";

            condition += serviceCompanyId.HasValue ? " AND oss.ServiceCompanyId = @serviceCompanyId" : string.Empty;

            return UnitOfWork.Session.Query<OuterServiceSetting, DomainValue, DomainValue, OuterServiceSetting>($@"
                SELECT *
                FROM OuterServiceSettings oss
                    LEFT JOIN DomainValues authType ON oss.AuthTypeId = authType.Id
                    LEFT JOIN DomainValues serviceType ON oss.ServiceTypeId = serviceType.Id
                {condition}",
                (oss, authType, serviceType) =>
                {
                    oss.AuthType = authType;
                    oss.ServiceType = serviceType;

                    return oss;
                },
                new { serviceCompanyId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var serviceCompanyId = query?.Val<int?>("ServiceCompanyId");

            var condition = "WHERE oss.DeleteDate IS NULL";

            condition += serviceCompanyId.HasValue ? " AND oss.ServiceCompanyId = @serviceCompanyId" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SSELECT count(*)
                    FROM OuterServiceSettings oss
                        LEFT JOIN DomainValues authType ON oss.AuthTypeId = authType.Id
                        LEFT JOIN DomainValues serviceType ON oss.ServiceTypeId = serviceType.Id
                    {condition}",
                    new { serviceCompanyId },
                    UnitOfWork.Transaction);
        }
    }
}
