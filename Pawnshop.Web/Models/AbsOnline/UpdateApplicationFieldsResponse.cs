namespace Pawnshop.Web.Models.AbsOnline
{
    public class UpdateApplicationFieldsResponse
    {
        /// <summary>
        /// параметр шины <b><u>application_id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// параметр шины <b><u>instance_id</u></b>
        /// </summary>
        public int? InstanceId { get; set; }

        /// <summary>
        /// параметр шины <b><u>error</u></b>
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// параметр шины <b><u>message</u></b>
        /// </summary>
        public string Message { get; set; }
    }
}
