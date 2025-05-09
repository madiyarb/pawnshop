using System;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ManualUpdate;

namespace Pawnshop.Services.ManualUpdate
{
    public class ManualUpdateService : IManualUpdateService
    {
        private readonly ISessionContext _sessionContext;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ManualUpdateExecuteRepository _manualUpdateExecuteRepository;
        private readonly ManualUpdateHistoryRepository _manualUpdateHistoryRepository;

        public ManualUpdateService(ManualUpdateExecuteRepository manualUpdateExecuteRepository,
            ManualUpdateHistoryRepository manualUpdateHistoryRepository, ISessionContext sessionContext, IUnitOfWork unitOfWork)
        {
            _manualUpdateExecuteRepository = manualUpdateExecuteRepository;
            _manualUpdateHistoryRepository = manualUpdateHistoryRepository;
            _sessionContext = sessionContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<ManualUpdateModel> SendUpdate(ManualUpdateRequest manualUpdate)
        {
            ValidateQuery(manualUpdate.UpdateQuery);
            ValidateQuery(manualUpdate.SelectQuery);
            var manualUpdateResponse = new ManualUpdateModel()
            {
                UserId = _sessionContext.UserId,
                CreateDate = DateTime.Now,
                SelectQuery = manualUpdate.SelectQuery,
                UpdateQuery = manualUpdate.UpdateQuery,
                CategoryId = manualUpdate.CategoryId,
                Status = ManualUpdateStatus.Error
            };

            using (var transaction = _unitOfWork.BeginTransaction())
            {
                try
                {
                    var selectQueryResult = _manualUpdateExecuteRepository.GetDynamic(manualUpdate);
                    await _manualUpdateExecuteRepository.Insert(manualUpdate);
                    var updatedQueryResult = _manualUpdateExecuteRepository.GetDynamic(manualUpdate);
                    if (selectQueryResult == updatedQueryResult)
                    {
                        throw new ApplicationException("Изменения не были применены. Запросы до и после идентичны");
                    }
                    manualUpdateResponse.SelectResult = selectQueryResult;
                    manualUpdateResponse.UpdateResult = updatedQueryResult;
                    manualUpdateResponse.Status = ManualUpdateStatus.Success;

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            _manualUpdateHistoryRepository.Insert(manualUpdateResponse);
            return manualUpdateResponse;
        }

        private static void ValidateQuery(string query)
        {
            string lowerQuery = query.ToLower();

            if (!lowerQuery.Contains("where") && !lowerQuery.Contains("insert"))
            {
                throw new ApplicationException("В запросе нет фильтра WHERE.");
            }

            if (lowerQuery.Contains("delete"))
            {
                if (!lowerQuery.Contains("deleteddate"))
                {
                    throw new ApplicationException("Удаление запрещено.");
                }
            }

            if (lowerQuery.Contains("*"))
            {
                throw new ApplicationException("Нельзя использовать *");
            }

            if (lowerQuery.Contains("drop"))
            {
                throw new ApplicationException("Нельзя использовать DROP");
            }
        }
    }
}