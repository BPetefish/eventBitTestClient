using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eventBitTestClient.Data
{
    public class SQLDataHelper
    {
        const string HOME_CON_STRING = "Data Source=98.204.41.162,49172;Initial Catalog=eventBit;Persist Security Info=True;User ID=Petefish;Password=Pete8901";
        public void ExecuteNonQuery(string query)
        {
            using (SqlConnection con = new SqlConnection(HOME_CON_STRING))
            {
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
    }
}