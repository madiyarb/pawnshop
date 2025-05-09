namespace Pawnshop.Web.Models.AbsOnline
{
    public class ChangeClientIinRequest
    {
        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>new_uin</u></b>
        /// </summary>
        public string NewIIN { get; set; }
    }
}
