using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.CardTopUp.GetTransactionStatusCode
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://beans.common.cnp.processing.kz/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://beans.common.cnp.processing.kz/xsd", IsNullable = false)]
    public partial class goods
    {

        private ushort amountField;

        private ushort currencyCodeField;

        private object merchantsGoodsIDField;

        private string nameOfGoodsField;

        /// <remarks/>
        public ushort amount
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
        public ushort currencyCode
        {
            get
            {
                return this.currencyCodeField;
            }
            set
            {
                this.currencyCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public object merchantsGoodsID
        {
            get
            {
                return this.merchantsGoodsIDField;
            }
            set
            {
                this.merchantsGoodsIDField = value;
            }
        }

        /// <remarks/>
        public string nameOfGoods
        {
            get
            {
                return this.nameOfGoodsField;
            }
            set
            {
                this.nameOfGoodsField = value;
            }
        }
    }

}
