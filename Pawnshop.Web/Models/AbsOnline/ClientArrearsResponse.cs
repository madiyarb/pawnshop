using System.Collections.Generic;
using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class ClientArrearsResponse
    {
        /// <summary>
        /// Параметр шины <b><u>code</u></b>
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Параметр шины <b><u>message</u></b>
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Параметр шины <b><u>identityNumber</u></b>
        /// </summary>
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>surname</u></b>
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>name</u></b>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Параметр шины <b><u>patronymic</u></b>
        /// </summary>
        public string Patronymic { get; set; }

        /// <summary>
        /// Параметр шины <b><u>fullname</u></b>
        /// </summary>
        public string Fullname { get; set; }

        /// <summary>
        /// Параметр шины <b><u>isMale</u></b>
        /// </summary>
        public bool IsMale { get; set; }

        /// <summary>
        /// Параметр шины <b><u>birthDay</u></b>
        /// </summary>
        public DateTime BirthDay { get; set; }

        /// <summary>
        /// Параметр шины <b><u>mobilePhone</u></b>
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Параметр шины <b><u>legalForm</u></b>
        /// </summary>
        public string LegalForm { get; set; }

        /// <summary>
        /// Параметр шины <b><u>isResident</u></b>
        /// </summary>
        public bool IsResident { get; set; }

        /// <summary>
        /// Параметр шины <b><u>isPEP</u></b>
        /// </summary>
        public bool IsPEP { get; set; }

        /// <summary>
        /// Параметр шины <b><u>citizenship</u></b>
        /// </summary>
        public string Citizenship { get; set; }

        /// <summary>
        /// Параметр шины <b><u>clientId</u></b>
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contracts</u></b>
        /// </summary>
        public IList<ContractArrears> Contracts { get; set; } = new List<ContractArrears>();
    }
}
