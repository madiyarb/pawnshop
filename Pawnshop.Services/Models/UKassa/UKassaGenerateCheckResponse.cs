using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaGenerateCheckResponse
    {
        public int id { get; set; }
        public User user { get; set; }
        public List<TransactionItems> transaction_items { get; set; }
        public List<TransactionPayments> transaction_payments { get; set; }
        public decimal total_nds { get; set; }
        public decimal discount_total { get; set; }
        public int shift { get; set; }
        public string created_at { get; set; }
        public string nds_percent { get; set; }
        public string html_code { get; set; }
        public string html_code_kz { get; set; }
        public int kassa_id { get; set; }
        public string fixed_check { get; set; }
        public int type { get; set; }
        public decimal total_amount { get; set; }
        public decimal total_fully { get; set; }
        public decimal discount { get; set; }
        public decimal markup { get; set; }
        public bool is_offline { get; set; }
        public bool fixed_offline { get; set; }
        public Duplicated duplicated { get; set; }
        public string link { get; set; }
        public int nds_type { get; set; }
        public bool canceled { get; set; }
        public bool currency { get; set; }
        public int check_number { get; set; }
        public int check_type { get; set; }
        public string serial_number { get; set; }
        public bool unlimited_packet { get; set; }
        public string cati { get; set; }
        public string rrn { get; set; }
        public int kassa_mode { get; set; }
        public string return_check_pk { get; set; }
    }

    public class User
    {
        public string full_name { get; set; }
    }

    public class Duplicated
    {
        public Company company { get; set; }
        public Kassa kassa { get; set; }
        public string user { get; set; }
    }

    public class TransactionItems
    {
        public string name { get; set; }
        public decimal price { get; set; }
        public decimal total { get; set; }
        public decimal total_fully { get; set; }
        public decimal discount { get; set; }
        public decimal discount_value { get; set; }
        public decimal quantity { get; set; }
        public int quantity_type { get; set; }
        public bool is_nds { get; set; }
        public bool is_discount_storno { get; set; }
        public bool is_markup_storno { get; set; }
        public decimal markup { get; set; }
        public string catalog { get; set; }
        public bool custom_cancelled { get; set; }
        public string quantity_name { get; set; }
        public bool storno { get; set; }
        public int nds_percent { get; set; }
        public string excise_stamp { get; set; }
        public string mark_code { get; set; }
        public string return_position_pk { get; set; }
        public string code_hatch { get; set; }
        public string barcode { get; set; }
        public string vendor { get; set; }
        public string code { get; set; }
        public string iiko_id { get; set; }
    }

    public class TransactionPayments
    {
        public int id { get; set; }
        public string method_name { get; set; }
        public string bank_account { get; set; }
        public int payment_type { get; set; }
        public decimal total { get; set; }
        public decimal change { get; set; }
        public decimal amount { get; set; }
        public int transaction { get; set; }
    }

}
