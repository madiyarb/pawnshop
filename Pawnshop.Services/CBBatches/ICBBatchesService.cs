using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.FillCBBatchesManually;
using Pawnshop.Data.Models.OuterServiceSettings;
using System.Collections.Generic;

namespace Pawnshop.Services.CBBatches
{
    public interface ICBBatchesService
    {
        List<int> CreateCBBatches(FillCBBatchesManuallyRequest req, int userId);
        CBBatchStatus SetBatchStatusSCB(string messege);
        CBBatch GetBatchById(int id);
        string GetBatchStatusRequestSCB(string userId, int packageId);
        string GetBatchStatusRequestFCB(OuterServiceSetting config, int batchId);
        void CheckConnectionToSCB(OuterServiceSetting config);
        void CheckConnectionToFCB(OuterServiceSetting config);
    }
}
