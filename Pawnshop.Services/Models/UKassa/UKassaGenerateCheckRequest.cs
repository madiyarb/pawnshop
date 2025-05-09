using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaGenerateCheckRequest
    {
        public int operation_type { get; set; }
        public List<Payment> payments { get; set; }
        public int kassa { get; set; }
        public List<Item> items { get; set; }
        public decimal total_amount { get; set; }
        public decimal change { get; set; }
        public bool currency { get; set; }
        public int check_type { get; set; }
        public int tax { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string email { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string phone { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool skip_idempotence_error { get; set; }
    }

    public class Payment
    {
        public int payment_type { get; set; }
        public decimal total { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public bool is_catalog { get; set; }
        public string catalog { get; set; }
        public int section { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal sum { get; set; }
        public decimal total { get; set; }
        public decimal discount { get; set; }
        public int discount_type { get; set; }
        public decimal discount_value { get; set; }
        public int markup { get; set; }
        public bool is_nds { get; set; }
        public decimal nds_percent { get; set; }
        public int tax { get; set; }
        public bool is_discount_storno { get; set; }
        public bool is_markup_storno { get; set; }
        public bool is_storno { get; set; }
        public string id { get; set; }
        public int quantity_type { get; set; }
        public string currency { get; set; }
        public bool excise_enabled { get; set; }
        public string excise_mark { get; set; }
        public string excise_uid { get; set; }
        public string excise_series { get; set; }
        public int excise_type { get; set; }
        public bool mark_enabled { get; set; }
        public string mark_code { get; set; }
    }

}