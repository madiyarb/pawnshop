namespace Pawnshop.Data.Models.Crm
{
    public class CrmStatus
    {
        public int Id;
        public int BitrixId;
        public string CrmName;
        public string DisplayName;

        public string GetCrmStage(int crmCategory)
        {
            return $@"C{crmCategory}:{CrmName}";
        }
    }
}
