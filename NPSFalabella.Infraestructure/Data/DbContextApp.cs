using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NPSFalabella.Infraestructure.Data
{
    public class DbContextApp
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DbContextApp(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("AppConnection");
        }
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);


    }
}
