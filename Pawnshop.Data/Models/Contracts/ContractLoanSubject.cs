using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractLoanSubject : IEntity
    {
        public int Id { get; set; }
        [RequiredId(ErrorMessage = "Вид субъекта не выбран")]
        public int SubjectId { get; set; }
        public LoanSubject Subject { get; set; }
        public int ContractId { get; set; }
        [RequiredId(ErrorMessage = "Субъектом нужно выбрать определенного клиента")]
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
