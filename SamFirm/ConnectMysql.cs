using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
namespace SamFirm
{
    class ConnectMysql
    {       
        private string datasource = "localhost";
        private string port = "3306";
        private string database = "firmware";
        private string user = "root";
        private string pass = "toor";
        public MySqlConnection Initialize()
        {
            MySqlConnection conn = new MySqlConnection("datasource=" + this.datasource 
                + ";port=" + this.port 
                + ";database=" + this.database 
                + ";User Id=" + this.user 
                + ";password=" + this.pass);
            return conn;
        }
    }
}
