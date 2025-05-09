using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Pawnshop.Core.Options;

namespace Pawnshop.Data
{
    public class ReportContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ReportContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("ReportDatabase");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}