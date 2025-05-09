using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients.Profiles
{
    public class ClientQuestionnaireFilledStatusDto
    {
        private int _requiredAdditionalContactsCount;
        public bool IsProfileFilled { get; set; }
        public bool IsExpenseFilled { get; set; }
        public bool AreEmploymentsFilled { get; set; }
        public bool AreAssetsFilled { get; set; }
        public int AdditionalContactsCount { get; set; }
        public int RequiredAdditionalContactsCount
        {
            get { return _requiredAdditionalContactsCount; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException($"{nameof(RequiredAdditionalContactsCount)} должен быть больше нуля");

                _requiredAdditionalContactsCount = value;
            }
        }
        public bool AreAdditionalContactsFilled
        {
            get
            {
                return AdditionalContactsCount >= RequiredAdditionalContactsCount;
            }
        }

        public bool IsCodeWordFilled { get; set; }
    }
}
