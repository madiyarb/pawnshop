using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Web.Models.Comment
{
    public enum CommentEntityType : short
    {
        [Display(Name = "Другое")]
        Other = 0,

        [Display(Name = "Заявка")]
        Application = 1,
    }
}
