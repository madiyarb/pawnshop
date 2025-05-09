using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Localizations;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class LocalizationRepository : RepositoryBase
    {
        public LocalizationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public LocalizationItem Get(int id)
        {
            var items = new Dictionary<int, LocalizationItem>();

            return UnitOfWork.Session.Query<LocalizationItem, Localization, LocalizationItem>(@"SELECT li.*,
       l.*
  FROM LocalizationItems li
  JOIN Localizations l ON l.LocalizationItemId = li.Id
 WHERE li.Id = @id",
                (item, localization) =>
                {
                    if (!items.TryGetValue(item.Id, out LocalizationItem itemEntry))
                    {
                        itemEntry = item;
                        itemEntry.Localizations = new List<Localization> { };
                        items.Add(itemEntry.Id, itemEntry);
                    }

                    itemEntry.Localizations.Add(localization);
                    return itemEntry;
                },
                new { id }, UnitOfWork.Transaction)
                .Distinct()
                .FirstOrDefault();
        }

        public LocalizationItem GetByCode(string code)
        {
            var items = new Dictionary<int, LocalizationItem>();

            return UnitOfWork.Session.Query<LocalizationItem, Localization, LocalizationItem>(@"SELECT li.*,
       l.*
  FROM LocalizationItems li
  JOIN Localizations l ON l.LocalizationItemId = li.Id
 WHERE li.Code = @code",
                (item, localization) =>
                {
                    if (!items.TryGetValue(item.Id, out LocalizationItem itemEntry))
                    {
                        itemEntry = item;
                        itemEntry.Localizations = new List<Localization> { };
                        items.Add(itemEntry.Id, itemEntry);
                    }

                    itemEntry.Localizations.Add(localization);
                    return itemEntry;
                },
                new { code }, UnitOfWork.Transaction)
                .Distinct()
                .FirstOrDefault();
        }
    }
}
