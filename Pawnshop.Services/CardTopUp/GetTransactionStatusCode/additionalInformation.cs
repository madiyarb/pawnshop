using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.CardTopUp.GetTransactionStatusCode
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://beans.common.cnp.processing.kz/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = false)]
    public partial class additionalInformation
    {

        private string keyField;

        private string valueField;

        /// <remarks/>
        public string key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

}
