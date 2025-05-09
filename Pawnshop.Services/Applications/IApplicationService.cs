using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Applications
{
    public interface IApplicationService
    {
        bool Save(ApprovedContract model);
        SameContractList GetSameContracts(MobileAppModel mobileAppModel);
        bool SendCarOrClientToBlackList(ModelForBlackList model);
        List<string> CheckForBlackList(MobileAppModel mobileAppModel);
        List<VehicleMark> GetVehicleMarks();
        List<VehicleModel> GetVehicleModelsByMarkId(int markId);
        Task<List<ContractDataForAddition>> GetContractDataForAdditionAsync(string bodyNumber, string identityNumber, string techPassportNumber);
        ContractDataForAddition GetContractDataForAddition(string bodyNumber);
        Task<List<ContractDataForTranche>> GetContractDataForTrancheAsync(string bodyNumber, string identityNumber, string techPassportNumber);
        void Validate(ApprovedContract model);
        ListModel<Application> ListWithCount(ListQueryModel<ApplicationListQueryModel> listQuery);
        ApplicationModel Get(int id);
        // Был перемешен в ApplicationController
        // Task<int> CreateContractFromApplication(ContractModel model, int branchId, string numberCode, int userId);
        bool IsCreateAdditionWithInsurance(AdditionDetails model, int branchId, int userId);
        ApplicationDetails GetApplicationDetailsByContractId(int contractId);
        Application GetApplicationByParentContractId(int parentContractId);
        ApplicationDetails GetApplicationDetailsByApplicationId(int applicationId);
        void RejectApplication(int id);
        void CompleteApplication(int id);
        void ValidateLoanAmounts(decimal validatedAmount, ApplicationModel applicationModel, ContractClass contractClass);
        void UpdateApplicationDetailsChildContractId(ApplicationDetails applicationDetails);
        void ValidateAndCompleteApplication(int? parentContractId, int? childContractId, Contract contract, InsurancePoliceRequest latestPoliceRequest);
        void CancelApplicationByContractId(int contractId);
        void SavePartialSign(Contract contract, ApplicationDetails applicationDetails);
        Application IsAdditionFromApplication(Contract parentContract);
        ContractPosition[] ChangePositionEstimatedCostFromApplication(Application application, Contract parentContract, bool isAddition = false, int? positionEstimatedCost = null);
        List<Application> GetApplicationsForReject();
        void RejectApplicationAndDeleteContract(Application application, DateTime date);
        void ValidateApplicationForAdditionCost(int contractId, decimal additionCost);
        ApplicationModel GetApplicationModelForAddition(int? parentContractId);
        ContractPartialSign GetContractPartialSign(int contractId);
        int ChangeStatusToNew(int contractId);
        string CheckIdentityDocument(string identityNumber, string documentNumber);
        TrancheLimit GetLimitByCategory(int trancheId);
        int GetBitrixId(int contractId);
    }
}
