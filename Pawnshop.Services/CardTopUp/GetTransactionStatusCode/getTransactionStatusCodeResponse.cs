using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.CardTopUp.GetTransactionStatusCode
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd", IsNullable = false)]
    public partial class getTransactionStatusCodeResponse
    {

        private getTransactionStatusCodeResponseReturn returnField;

        /// <remarks/>
        public getTransactionStatusCodeResponseReturn @return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }
}
