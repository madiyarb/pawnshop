using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.PrintFormInfo;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintFormInformationController : Controller
    {
        public PrintFormInformationController()
        {
            
        }

        [HttpGet("contracts/{id}")]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetApplicationOnline([FromServices] ClientRepository clientRepository, 
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] ApplicationOnlineCarRepository applicationOnlineCarRepository,
            [FromServices] ApplicationsOnlineEstimationRepository applicationsOnlineEstimationRepository,
            [FromServices] ContractRepository contractRepository,
            [FromRoute] int id)
        {
            var application = applicationOnlineRepository.GetByContractId(id);

            if (application == null)
            {
                return NotFound();
            }
            int clientId = application.ClientId;

            var info = new PrintFormOpenCreditLineQuestionnaire
            {
                ClientInfo = await clientRepository.GetClientInfoForPrintForm(clientId),
                DocumentInfo = await clientRepository.GetClientDocumentInfoForPrintForm(clientId),
                ContactInfo = await clientRepository.GetClientContractInfoForPrintForm(clientId),
                AddressInfo = new PrintFormOpenCreditLineQuestionnaireClientAddressInfo(await clientRepository.GetAddressInfoForPrintForm(clientId)),
                FamilyInfo = await clientRepository.GetClientFamilyInfoForPrintForm(clientId),
                EmploymentInfo = await clientRepository.GetClientEmploymentInfoForPrintForm(clientId),
                IncomeInfo = await clientRepository.GetClientIncomeInfoForPrintForm(clientId),
                AdditionalContacts = await clientRepository.GetClientAdditionalContactsForPrintForm(clientId),
                ExpenseInfo = await clientRepository.GetClientExpenseInfoForPrintForm(clientId),
                ConditionInfo = await applicationOnlineRepository.GetClientIncomeInfoForPrintForm(application.Id),
                CollateralInfo = await applicationOnlineCarRepository.GetClientCollateralInfoForPrintForm(application.ApplicationOnlinePositionId),
                EstimationInfo = new PrintFormOpenCreditLineQuestionnaireEstimationInfo(await applicationsOnlineEstimationRepository.GetLastByApplicationIdAsync(application.Id)),
                CreateDate = (await contractRepository.GetOnlyContractAsync(id)).ContractDate
            };
            return Ok(info);
        }
    }
}
