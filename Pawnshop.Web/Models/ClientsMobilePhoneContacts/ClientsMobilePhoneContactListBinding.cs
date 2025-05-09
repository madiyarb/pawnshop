using FluentValidation;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.ClientsMobilePhoneContacts
{
    public sealed class ClientsMobilePhoneContactListBinding
    {
        public List<ClientsMobilePhoneContactBinding> MobileContactsList { get; set; }
    }

    public sealed class ClientsMobilePhoneContactListBindingValidator : AbstractValidator<ClientsMobilePhoneContactListBinding>
    {
        public ClientsMobilePhoneContactListBindingValidator()
        {
            RuleFor(b => b.MobileContactsList)
                .NotNull()
                .Must(contacts => contacts.Count > 0 );
        }
    }
}
