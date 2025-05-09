namespace Pawnshop.Web.Models.AbsOnline
{
    public class ChangeClientPhoneRequest
    {
        /// <summary>
        /// Параметр шины <b><u>uin</u></b>
        /// </summary>
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>old_tel</u></b>
        /// </summary>
        public string OldPhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tel</u></b>
        /// </summary>
        public string NewPhone { get; set; }
    }
}
