using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ClientDocumentTypeRepository : RepositoryBase, IRepository<ClientDocumentType>
    {
        private readonly DomainValueRepository _domainValueRepository;
        public ClientDocumentTypeRepository(IUnitOfWork unitOfWork, DomainValueRepository domainValueRepository) : base(unitOfWork)
        {
            _domainValueRepository = domainValueRepository;
        }

        public void Insert(ClientDocumentType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ClientDocumentTypes(Name, NameKaz, Disabled, IsIndividual, HasSeries, NumberPlaceholder, SeriesPlaceholder, DatePlaceholder, ProviderPlaceholder, BirthPlacePlaceholder, DateExpirePlaceholder, NumberMask, Code, NumberMaskError, CBId)
VALUES(@Name, @NameKaz, @Disabled, @IsIndividual, @HasSeries, @NumberPlaceholder, @SeriesPlaceholder, @DatePlaceholder, @ProviderPlaceholder, @BirthPlacePlaceholder, @DateExpirePlaceholder, @NumberMask, @Code, @NumberMaskError, @CBId)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientDocumentType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ClientDocumentTypes SET Name = @Name, NameKaz = @NameKaz, Disabled = @Disabled, IsIndividual = @IsIndividual, HasSeries = @HasSeries, NumberPlaceholder = @NumberPlaceholder, SeriesPlaceholder = @SeriesPlaceholder, DatePlaceholder = @DatePlaceholder, ProviderPlaceholder = @ProviderPlaceholder, BirthPlacePlaceholder = @BirthPlacePlaceholder, DateExpirePlaceholder = @DateExpirePlaceholder, NumberMask = @NumberMask, Code = @Code, NumberMaskError = @NumberMaskError, CBId = @CBId
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ClientDocumentType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientDocumentType>(@"
                SELECT * 
                FROM ClientDocumentTypes
                WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<ClientDocumentType> GetByCode(string code)
        {
            var parameters = new { Code = code };
            var sqlQuery = @"
                SELECT TOP 1 * FROM ClientDocumentTypes
                WHERE Disabled = 0
                AND Code = @Code";

            return await UnitOfWork.Session
                .QueryFirstAsync<ClientDocumentType>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public ClientDocumentType Find(object query)
        {
            var code = query.Val<string>("Code");

            return UnitOfWork.Session.QuerySingleOrDefault<ClientDocumentType>(@"
                SELECT * 
                FROM ClientDocumentTypes
                WHERE Code=@code", new { code }, UnitOfWork.Transaction);
        }

        public List<ClientDocumentType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<ClientDocumentType>(@"SELECT * FROM ClientDocumentTypes", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ClientDocumentTypes", UnitOfWork.Transaction);
        }

        public List<ClientDocumentType> ListRealtyDocumentTypes()
        {
            var subTypeId = _domainValueRepository.GetByCodeAndDomainCode(Constants.REALTY_DOCUMENT, Constants.CLIENT_DOCUMENT_SUB_TYPE).Id; 
            return UnitOfWork.Session.Query<ClientDocumentType>(@"SELECT * FROM ClientDocumentTypes WHERE SubTypeId = @subTypeId AND Disabled = 0 ORDER BY Name ASC", new { subTypeId }, UnitOfWork.Transaction).ToList();
        }

        public List<ClientDocumentType> ListDocumentTypesForEstimationService()
        {
            return UnitOfWork.Session.Query<ClientDocumentType>(@"SELECT * FROM ClientDocumentTypes 
                    WHERE Code in ('PASSPORTKZ', 'IDENTITYCARD', 'RESIDENCE')",  UnitOfWork.Transaction).ToList();
        }
    }
}
