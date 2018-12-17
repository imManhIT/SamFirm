using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
namespace SamFirm
{
    class ConnectMysql
    {       
        //private string datasource = "localhost";
        //private string port = "3306";
        //private string database = "firmware";
        //private string user = "root";
        //private string pass = "toor";
        public MySqlConnection Initialize()
        {
            //Console.Write(strInput);
            //MySqlConnection conn = new MySqlConnection("datasource=" + this.datasource
            //    + ";port=" + this.port
            //    + ";database=" + this.database
            //    + ";User Id=" + this.user
            //    + ";password=" + this.pass);
            string strInput = File.ReadAllText("DBConfig.txt");
            string[] str = strInput.Split('|');
            MySqlConnection conn = new MySqlConnection("datasource=" + str[0].Split('=')[1]
                + ";port=" + str[1].Split('=')[1]
                + ";database=" + str[2].Split('=')[1]
                + ";User Id=" + str[3].Split('=')[1]
                + ";password=" + str[4].Split('=')[1]);
            return conn;
        }
    }
}
