using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.CardTopUp.GetTransactionStatusCode
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private getTransactionStatusCodeResponse getTransactionStatusCodeResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
        public getTransactionStatusCodeResponse getTransactionStatusCodeResponse
        {
            get
            {
                return this.getTransactionStatusCodeResponseField;
            }
            set
            {
                this.getTransactionStatusCodeResponseField = value;
            }
        }
    }
}
