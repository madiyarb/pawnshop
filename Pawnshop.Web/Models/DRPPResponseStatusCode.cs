namespace Pawnshop.Web.Models
{
    public enum DRPPResponseStatusCode
    {
        ClientNotFound,
        ApplicationNotFound,
        VerificationRequestNotFound,
        UnspecifiedPawnshopApplicationException,
        UnspecifiedDatabaseProblems,
        UnspecifiedProblem,
        CreationApplicationWIthoutNumber,
        BadClient,
        BadData,
        RequisiteOwnedByOtherClient,
        RequisiteNumberCheckFailed,
        BankCodeNotFound,
        NoMoreAttemptsLeft,
        RejectionReasonNotFound,
        ApplicationInWrongStatus,
        FileCreationFailed,
        WrongIIN,
        WrongPhone,
        ContractNotFound,
        FileNotFound,
        ContractIsOffBalance
    }
}
