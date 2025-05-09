using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaReportResponse
    {
        public string closed_at { get; set; }
        public DuplicatedZ duplicated { get; set; }
        public string created_at { get; set; }
    }

    public class DuplicatedZ
    {
        public Date_Time date_time { get; set; }
        public List<Section> sections { get; set; }
        public Revenue revenue { get; set; }
        public CashSum cash_sum { get; set; }
        public Kassa kassa { get; set; }
        public List<TotalResult> total_result { get; set; }
        public List<Operation> operations { get; set; }
        public List<MoneyPlacement> money_placements { get; set; }
        public List<TicketOperation> ticket_operations { get; set; }
        public List<NonNullableSum> start_shift_non_nullable_sums { get; set; }
        public int shift_number { get; set; }
        public Company company { get; set; }
        public List<NonNullableSum> non_nullable_sums { get; set; }
    }

    public class Date_Time
    {
        public Time time { get; set; }
        public Date date { get; set; }
    }

    public class Time
    {
        public int minute { get; set; }
        public int hour { get; set; }
        public int second { get; set; }
    }

    public class Date
    {
        public int month { get; set; }
        public int year { get; set; }
        public int day { get; set; }
    }

    public class Revenue
    {
        public bool is_negative { get; set; }
        public Sum sum { get; set; }
    }

    public class CashSum
    {
        public string bills { get; set; }
        public int coins { get; set; }
    }

    public class Section
    {
        public List<Operation> operations { get; set; }
        public string section_code { get; set; }
    }

    public class Operation
    {
        public int count { get; set; }
        public Sum sum { get; set; }
        public int operation { get; set; }
    }

    public class TotalResult
    {
        public int count { get; set; }
        public Sum sum { get; set; }
        public int operation { get; set; }
    }

    public class MoneyPlacement
    {
        public OperationsSum operations_sum { get; set; }
        public int operations_total_count { get; set; }
        public int operations_count { get; set; }
        public int operation { get; set; }
    }

    public class OperationsSum
    {
        public string bills { get; set; }
        public int coins { get; set; }
    }

    public class TicketOperation
    {
        public Sum tickets_sum { get; set; }
        public int tickets_total_count { get; set; }
        public int tickets_count { get; set; }
        public int operation { get; set; }
        public List<PaymentZ> payments { get; set; }
    }

    public class PaymentZ
    {
        public int payment { get; set; }
        public Sum sum { get; set; }
    }

    public class NonNullableSum
    {
        public Sum sum { get; set; }
        public int operation { get; set; }
    }

    public class Sum
    {
        public string bills { get; set; }
        public int coins { get; set; }
    }

}
