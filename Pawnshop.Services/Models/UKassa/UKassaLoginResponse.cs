using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaLoginResponse
    {
        public string email { get; set; }
        public string full_name { get; set; }
        public string auth_token { get; set; }
        public int id { get; set; }
        public UserKassas user_kassas { get; set; }
        public string photo { get; set; }
        public string user_partner { get; set; }
        public string user_rmni { get; set; }
        public bool is_available_statistic { get; set; }
        public bool is_api_readable { get; set; }
        public IntegrationToken integration_token { get; set; }
    }

    public class UserKassas
    {
        public List<KassaLogin> kassa { get; set; }
        public List<string> permissions { get; set; }
        public int max_check_sum { get; set; }
        public List<int> roles { get; set; }
        public bool filled { get; set; }
        public int id { get; set; }
        public CompanyLogin company { get; set; }
    }

    public class CompanyLogin
    {
        public string name { get; set; }
        public string bin_iin { get; set; }
        public int nds_type { get; set; }
    }

    public class KassaLogin
    {
        public string name { get; set; }
        public int id { get; set; }
        public string factory_number { get; set; }
        public bool is_activated { get; set; }
        public bool usn_sync { get; set; }
    }

    public class IntegrationToken
    {
        public string token { get; set; }
        public string timeout { get; set; }
        public string _operator { get; set; }
    }

}
