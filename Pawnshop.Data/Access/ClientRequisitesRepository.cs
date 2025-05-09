using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.ApplicationOnlineHistoryLogger;
using Pawnshop.Data.Helpers;
using Pawnshop.Data.Models.ClientRequisiteLogItems;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.Views;
using Pawnshop.Data.Models.JetPay;

namespace Pawnshop.Data.Access
{
    public sealed class ClientRequisitesRepository : RepositoryBase
    {
        private readonly IApplicationOnlineHistoryLoggerService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientRequisitesRepository(IUnitOfWork unitOfWork,
            IApplicationOnlineHistoryLoggerService service,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public ClientRequisite Get(int id)
        {
            return UnitOfWork.Session.Query<ClientRequisite, JetPayCardPayoutInformation, ClientRequisite>(@"SELECT cr.*,
       jt.*
  FROM ClientRequisites cr
  LEFT JOIN JetPayCardPayoutInformations jt ON jt.ClientRequisiteId = cr.Id
 WHERE cr.Id = @id
   AND cr.DeleteDate IS NULL",
                (cr, jt) =>
                {
                    cr.JetPayCardInfo = jt;
                    return cr;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ClientRequisiteListView GetClientRequisites(int clientId)
        {
            ClientRequisiteListView clientRequisiteList = new ClientRequisiteListView();
            List<ClientRequisiteGeneralView> clientRequisites =
            UnitOfWork.Session.Query<ClientRequisiteGeneralView>(@"SELECT ClientRequisites.*, Clients.FullName AS BankName, 
                Clients.BankIdentifierCode AS BankCode
  FROM ClientRequisites 
  LEFT JOIN Clients ON Clients.Id = ClientRequisites.BankId
 WHERE ClientRequisites.ClientId = @clientId AND ClientRequisites.DeleteDate IS NULL",//and DeleteDate is null
                new { clientId }, UnitOfWork.Transaction).ToList();

            clientRequisiteList.ClientRequisiteBills = clientRequisites.Where(clientRequisite => clientRequisite.RequisiteTypeId == 1)
                .Select(clientRequisite =>
                    new ClientRequisiteBillView
                    {
                        Id = clientRequisite.Id,
                        BankName = clientRequisite.BankName,
                        BankCode = clientRequisite.BankCode,
                        BankId = clientRequisite.BankId,
                        AuthorId = clientRequisite.AuthorId,
                        ClientId = clientRequisite.ClientId,
                        CreateDate = clientRequisite.CreateDate,
                        IsDefault = clientRequisite.IsDefault,
                        Note = clientRequisite.Note,
                        Value = clientRequisite.Value
                    }).ToList();
            clientRequisiteList.ClientRequisiteCards = clientRequisites.Where(clientRequisite => clientRequisite.RequisiteTypeId == 2)
                .Select(clientRequisite =>
                    new ClientRequisiteCardView
                    {
                        Id = clientRequisite.Id,
                        AuthorId = clientRequisite.AuthorId,
                        ClientId = clientRequisite.ClientId,
                        CreateDate = clientRequisite.CreateDate,
                        IsDefault = clientRequisite.IsDefault,
                        Note = clientRequisite.Note,
                        CardExpiryDate = clientRequisite.CardExpiryDate,
                        CardHolderName = clientRequisite.CardHolderName,
                        Value = clientRequisite.Value
                    }).ToList();

            return clientRequisiteList;
        }

        public void Insert(ClientRequisite clientRequisite)
        {
            using (var transaction = BeginTransaction())
            {
                clientRequisite.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ClientRequisites (ClientId, RequisiteTypeId, BankId, Note, AuthorId, CreateDate, IsDefault, Value, CardExpiryDate, CardHolderName, CardType)
                VALUES (@ClientId, @RequisiteTypeId, @BankId, @Note, @AuthorId, @CreateDate, @IsDefault, @Value, @CardExpiryDate, @CardHolderName, @CardType)

SELECT SCOPE_IDENTITY()",
                    clientRequisite, UnitOfWork.Transaction);

                transaction.Commit();
            }

            _service.LogClientRequisiteData(new ClientRequisiteLogData(clientRequisite),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public void SetAllOldClientRequisitesToNotDefault(int clientId)
        {
            using (var transaction = BeginTransaction())
            {
                string query = $@"
                UPDATE ClientRequisites 
                SET ClientRequisites.IsDefault = 0
                WHERE ClientRequisites.ClientId = {clientId};";
                UnitOfWork.Session.QuerySingleOrDefault(query, new { clientId }
                    , UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientRequisite GetByValue(string value)
        {
            return UnitOfWork.Session.Query<ClientRequisite>(@"SELECT * FROM ClientRequisites
 WHERE ClientRequisites.Value = @value AND DeleteDate is null",//and DeleteDate is null
                new { value }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Update(ClientRequisite clientRequisite)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"UPDATE ClientRequisites
   SET BankId = @BankId,
       Note = @Note,
       AuthorId = @AuthorId, 
       DeleteDate = @DeleteDate, 
       Value = @Value, 
       CardExpiryDate = @CardExpiryDate, 
       CardHolderName = @CardHolderName,
       IsDefault = @IsDefault,
       CardType = @CardType
 WHERE Id = @Id;", clientRequisite
                    , UnitOfWork.Transaction);

                transaction.Commit();
            }
            _service.LogClientRequisiteData(new ClientRequisiteLogData(clientRequisite),
                _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? Constants.ADMINISTRATOR_IDENTITY);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientRequisites SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<IEnumerable<ClientRequisite>> GetListByClientIdAsync(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<ClientRequisite>(@"SELECT *
  FROM ClientRequisites
 WHERE ClientId = @clientId
   AND DeleteDate IS NULL",
                new { clientId }, UnitOfWork.Transaction);
        }
    }
}
