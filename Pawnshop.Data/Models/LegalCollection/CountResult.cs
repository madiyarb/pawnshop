using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class CountResult<T>
    {
        private IEnumerable<T> _data;
        private int _totalCount;

        public IEnumerable<T> Data
        {
            get => _data ?? Enumerable.Empty<T>();
            set => _data = value;
        }

        public int TotalCount
        {
            get => _totalCount;
            set => _totalCount = value >= 0 ? value : 0;
        }

        public CountResult(IEnumerable<T> data, int totalCount)
        {
            _data = data;
            _totalCount = totalCount >= 0 ? totalCount : 0;
        }
    }
}