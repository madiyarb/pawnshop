using Pawnshop.Data.Models.Localizations;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web.Models.Clients
{
    public sealed class ClientIsTrustableCheckView
    {
        public bool IsTrustable { get; set; }

        public List<LocalizationView>? Description { get; set; }

        public ClientIsTrustableCheckView() { }

        public ClientIsTrustableCheckView(bool isTrustable, List<Localization> description = null)
        {
            IsTrustable = isTrustable;

            if (!isTrustable)
            {
                Description = description?
                    .Select(x => new LocalizationView
                    {
                        Language = x.Language,
                        Value = x.Value
                    })
                    .ToList();
            }
        }
    }
}
