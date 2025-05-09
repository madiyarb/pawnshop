using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.CardTopUp.GetTransactionStatusCode
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    public partial class getTransactionStatusCodeResponseReturn
    {

        //private additionalInformation[] additionalInformationField;

        private long? amountAuthorisedField;

        private long? amountRefundedField;

        private long? amountRequestedField;

        private long? amountSettledField;

        private object authCodeField;

        private object bankRRNField;

        //private goods goodsField;

        private string issuerBankField;

        private string merchantLocalDateTimeField;

        private string merchantOnlineAddressField;

        private long? orderIdField;

        private string purchaserEmailField;

        private string purchaserNameField;

        private string purchaserPhoneField;

        private long? rspCodeField;

        private object rspCodeDescField;

        private string? sinkNodeField;

        private long? transactionCurrencyCodeField;

        private string transactionStatusField;

        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute("additionalInformation", Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        //public additionalInformation[] additionalInformation
        //{
        //    get
        //    {
        //        return this.additionalInformationField;
        //    }
        //    set
        //    {
        //        this.additionalInformationField = value;
        //    }
        //}

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? amountAuthorised
        {
            get
            {
                return this.amountAuthorisedField;
            }
            set
            {
                this.amountAuthorisedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? amountRefunded
        {
            get
            {
                return this.amountRefundedField;
            }
            set
            {
                this.amountRefundedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? amountRequested
        {
            get
            {
                return this.amountRequestedField;
            }
            set
            {
                this.amountRequestedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? amountSettled
        {
            get
            {
                return this.amountSettledField;
            }
            set
            {
                this.amountSettledField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object authCode
        {
            get
            {
                return this.authCodeField;
            }
            set
            {
                this.authCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object bankRRN
        {
            get
            {
                return this.bankRRNField;
            }
            set
            {
                this.bankRRNField = value;
            }
        }

        ///// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        //public goods goods
        //{
        //    get
        //    {
        //        return this.goodsField;
        //    }
        //    set
        //    {
        //        this.goodsField = value;
        //    }
        //}

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string issuerBank
        {
            get
            {
                return this.issuerBankField;
            }
            set
            {
                this.issuerBankField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string merchantLocalDateTime
        {
            get
            {
                return this.merchantLocalDateTimeField;
            }
            set
            {
                this.merchantLocalDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string merchantOnlineAddress
        {
            get
            {
                return this.merchantOnlineAddressField;
            }
            set
            {
                this.merchantOnlineAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? orderId
        {
            get
            {
                return this.orderIdField;
            }
            set
            {
                this.orderIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string purchaserEmail
        {
            get
            {
                return this.purchaserEmailField;
            }
            set
            {
                this.purchaserEmailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string purchaserName
        {
            get
            {
                return this.purchaserNameField;
            }
            set
            {
                this.purchaserNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string purchaserPhone
        {
            get
            {
                return this.purchaserPhoneField;
            }
            set
            {
                this.purchaserPhoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? rspCode
        {
            get
            {
                return this.rspCodeField;
            }
            set
            {
                this.rspCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public object rspCodeDesc
        {
            get
            {
                return this.rspCodeDescField;
            }
            set
            {
                this.rspCodeDescField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string? sinkNode
        {
            get
            {
                return this.sinkNodeField;
            }
            set
            {
                this.sinkNodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public long? transactionCurrencyCode
        {
            get
            {
                return this.transactionCurrencyCodeField;
            }
            set
            {
                this.transactionCurrencyCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string transactionStatus
        {
            get
            {
                return this.transactionStatusField;
            }
            set
            {
                this.transactionStatusField = value;
            }
        }
    }

}
