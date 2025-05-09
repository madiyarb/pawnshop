using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Core;
using Pawnshop.Data.Models.Transfers;
using Pawnshop.Web.Models.List;


namespace Pawnshop.Web.Controllers.Api
{
    public class TransferController : Controller
    {
        private readonly TransferRepository _transferRepository;
        private readonly ISessionContext _sessionContext;

        public TransferController(TransferRepository transferRepository, ISessionContext sessionContext)
        {
            _transferRepository = transferRepository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public Transfer Save([FromBody] Transfer transfer)
        {
            using (var transaction = _transferRepository.BeginTransaction())
            {
                if (transfer.Id > 0)
                {
                    transfer.Status = TransferStatus.Success;
                    _transferRepository.Update(transfer);
                }
                else
                {
                    transfer.UserId = _sessionContext.UserId;
                    _transferRepository.Insert(transfer);
                }

                transaction.Commit();
            }

            return transfer;
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public ListModel<Transfer> List([FromBody] ListQuery listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            

            return new ListModel<Transfer>
            {
                List = _transferRepository.List(listQuery),
                Count = _transferRepository.Count(listQuery)
            };
        }

        [HttpPost]
        [Authorize(Permissions.ContractTransfer)]
        public void Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _transferRepository.Delete(id);
        }

    }

}
