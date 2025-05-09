using Pawnshop.Services.CardCashOut.GetCashOutTransactionStatus;

namespace Pawnshop.Services.CardCashOut.CompleteCashOutTransaction
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://kz.processing.cnp.merchant_ws/xsd")]
    public partial class completeCashOutTransactionResponseReturn
    {

        //private additionalInformation[] additionalInformationField;

        private object altCardIssuerCountryField;

        //private object altMaskedCardNumberField;

        private int? amountField;

        private object? authCodeField;

        private string? cardIssuerCountryField;

        private object? errorDescriptionField;

        private object? feeField;

        private string? maskedCardNumberField;

        private string? merchantLocalDateTimeField;

        private string? merchantOnlineAddressField;

        private object? receiverEmailField;

        private string? receiverNameField;

        private object? receiverPhoneField;

        private byte? rspCodeField;

        private object? senderEmailField;

        private string? senderNameField;

        private object? senderPhoneField;

        private bool? successField;

        private ushort? transactionCurrencyCodeField;

        private string? transactionStatusField;

        private object? userIpAddressField;

        private string? verified3DField;

        ///// <remarks/>
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
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object altCardIssuerCountry
        {
            get
            {
                return this.altCardIssuerCountryField;
            }
            set
            {
                this.altCardIssuerCountryField = value;
            }
        }

        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        //public object altMaskedCardNumber
        //{
        //    get
        //    {
        //        return this.altMaskedCardNumberField;
        //    }
        //    set
        //    {
        //        this.altMaskedCardNumberField = value;
        //    }
        //}

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public int? amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
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
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string cardIssuerCountry
        {
            get
            {
                return this.cardIssuerCountryField;
            }
            set
            {
                this.cardIssuerCountryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object errorDescription
        {
            get
            {
                return this.errorDescriptionField;
            }
            set
            {
                this.errorDescriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string maskedCardNumber
        {
            get
            {
                return this.maskedCardNumberField;
            }
            set
            {
                this.maskedCardNumberField = value;
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
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object receiverEmail
        {
            get
            {
                return this.receiverEmailField;
            }
            set
            {
                this.receiverEmailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string receiverName
        {
            get
            {
                return this.receiverNameField;
            }
            set
            {
                this.receiverNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = true)]
        public object receiverPhone
        {
            get
            {
                return this.receiverPhoneField;
            }
            set
            {
                this.receiverPhoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public byte? rspCode
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
        public object senderEmail
        {
            get
            {
                return this.senderEmailField;
            }
            set
            {
                this.senderEmailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string senderName
        {
            get
            {
                return this.senderNameField;
            }
            set
            {
                this.senderNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public object senderPhone
        {
            get
            {
                return this.senderPhoneField;
            }
            set
            {
                this.senderPhoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public bool? success
        {
            get
            {
                return this.successField;
            }
            set
            {
                this.successField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public ushort? transactionCurrencyCode
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

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public object userIpAddress
        {
            get
            {
                return this.userIpAddressField;
            }
            set
            {
                this.userIpAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd")]
        public string verified3D
        {
            get
            {
                return this.verified3DField;
            }
            set
            {
                this.verified3DField = value;
            }
        }
    }
}
