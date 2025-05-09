using FluentValidation;

namespace Pawnshop.Web.Models.ClientsMobilePhoneContacts
{
    public sealed class ClientsMobilePhoneContactBinding
    {
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
    }


    public sealed class ClientsMobilePhoneContactsBindingValidator : AbstractValidator<ClientsMobilePhoneContactBinding>
    {
        public ClientsMobilePhoneContactsBindingValidator()
        {
            RuleFor(b => b.PhoneNumber)
                .NotEmpty();
            RuleFor(b => b.Name)
                .NotEmpty();
        }
    }
}
