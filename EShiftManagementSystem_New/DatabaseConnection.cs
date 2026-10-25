using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EShiftManagementSystem_New
{
    internal class DatabaseConnection
    {
        private string connectionString =
            "Server=localhost;" +
            "Port=3306;" +
            "Database=eshift_db;" +
            "Uid=root;" +
            "Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
