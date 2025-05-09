using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;

namespace Pawnshop.Web.Models.Auth
{
    public class SignInModel : ILoggable
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public object Format()
        {
            return $"Username: {Username}";
        }
    }
}