using System;
using System.Collections.Generic;
using System.Data;
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

        public DataTable GetEntityDataTable(string tableName, double eventId)
        {
            using (SqlConnection con = new SqlConnection(HOME_CON_STRING))
            {

                string q = "SELECT TOP 50 * FROM {0} where sysEventId = {1}";
                q = string.Format(q, tableName, eventId);

                con.Open();

                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            return dt;
                        }
                    } // reader closed and disposed up here

                } // command disposed here
                
            }

            return null;
        }

        public int GetRowCountForEntity(string tableName, double eventId)
        {
            using (SqlConnection con = new SqlConnection(HOME_CON_STRING))
            {

                string q = "SELECT Count(*) as Count FROM {0} where sysEventId = {1}";
                q = string.Format(q, tableName, eventId);

                con.Open();

                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            var dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                                return (int)dt.Rows[0]["Count"];

                            return 0;
                        }
                    } // reader closed and disposed up here

                } // command disposed here

            }

            return 0;
        }
    }
}