using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ClientSUSNStatuses;
using Pawnshop.Data.Models.ClientSUSNStatuses.Views;
using Pawnshop.Data.Models.SUSNRequests;

namespace Pawnshop.Data.Access
{
    public sealed class ClientSUSNStatusesRepository : RepositoryBase
    {
        public ClientSUSNStatusesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task Insert(ClientSUSNStatus clientSusnStatus)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.InsertAsync(clientSusnStatus, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<ClientSUSNStatusesView> GetStatusesView(int clientId, Guid requestId)
        {
            ClientSUSNStatusesView view = new ClientSUSNStatusesView();
            var builder = new SqlBuilder();
            builder.Select("SUSNStatuses.Name");
            builder.Select("SUSNStatuses.NameKz");
            builder.Select("SUSNStatuses.Code"); 
            builder.Select("SUSNStatuses.Permanent");
            builder.Select("SUSNStatuses.Decline");
            builder.Select("ClientSUSNStatuses.CreateDate");
            builder.Join(
                "SUSNStatuses ON  SUSNStatuses.Id = ClientSUSNStatuses.SUSNStatusId");
            builder.Where("ClientSUSNStatuses.ClientId = @clientId", new { clientId });
            builder.Where("ClientSUSNStatuses.SUSNRequestId = @SUSNRequestId", new { SUSNRequestId = requestId });
            builder.OrderBy("SUSNStatuses.Code");
            var selector = builder.AddTemplate($"Select /**select**/ from ClientSUSNStatuses /**join**/ /**where**/ /**orderby**/ ");
            var counter =
                builder.AddTemplate(
                    $"Select count(*) from ClientSUSNStatuses /**join**/ /**where**/");

            view.Count = await UnitOfWork.Session.QuerySingleAsync<int>(counter.RawSql,
                counter.Parameters);

            if (view.Count == 0)
                return null;

            view.List = (await UnitOfWork.Session.QueryAsync<ClientSUSNStatusView>(selector.RawSql,
                selector.Parameters)).ToList();
            view.AnySUSNStatus = true;
            return view;

        }
    }
}
