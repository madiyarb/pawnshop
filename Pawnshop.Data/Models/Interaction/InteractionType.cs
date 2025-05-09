using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Interaction
{
    public enum InteractionType : short
    {
        [Display(Name = "Чат")]
        CHAT = 0,
        [Display(Name = "СМС")]
        SMS = 10,
        [Display(Name = "Входящий звонок")]
        CALL_INCOMING = 20,
        [Display(Name = "Исходящий звонок")]
        CALL_OUTGOING = 21,
    }
}
