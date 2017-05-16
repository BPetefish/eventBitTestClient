using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace eventBitTestClient.Controllers
{
    public class SnapshotController : ApiController
    {
        // GET: api/Snapshot
        public async Task<HttpResponseMessage> Get()
        {
            string reqAuth = Request.Headers.GetValues("X-AUTH-CLAIMS").First();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.experienteventbit.com/webapi/API/Event/INF999/TrackingData");

            client.DefaultRequestHeaders.Add("X-AUTH-CLAIMS", reqAuth);

            HttpResponseMessage response = client.GetAsync("").Result;

            string newXAuthHeader = response.Headers.GetValues("X-AUTH-CLAIMS").First();

            var data = await response.Content.ReadAsStringAsync();

            var d = JsonConvert.DeserializeObject<TrackedData>(data);

            ProcessTrackedData(d);

            return new HttpResponseMessage()
            {
                Content = new StringContent(newXAuthHeader, System.Text.Encoding.UTF8, "application/json")
            };
        }

        const string DIR_PATH = @"C:\Users\xFish\Documents\ChunkURITest\";
        private void ProcessTrackedData(TrackedData d)
        {
            foreach (Table t in d.Tables)
            {
                //Make Sure My Tables Are Correct
                CreateUpdateTrackingTables(t);

                //DownloadFiles
                for (int i = 0; i < t.ChunkURIs.Count; i++)
                {
                    WebClient Client = new WebClient();
                    Client.DownloadFile(t.ChunkURIs[i], DIR_PATH + t.TableName + "_" + i + ".gz");
                }

                //Load Files Into Database
                DirectoryInfo dir = new DirectoryInfo(DIR_PATH);
                var files = dir.GetFiles(t.TableName + "*");
                foreach (FileInfo file in files)
                {
                    List<string> chunckData = Decompress(file);
                    //Batch by 1000
                    var batchedData = splitList<string>(chunckData, 1000);
                    //Create Insert Statements
                    foreach (var bd in batchedData)
                    {
                        string insert = CreateInsertStatement(t.OrderedColumnSchema, t, bd).TrimEnd(',');
                        RunInsertStatement(insert);
                    }                     
                }
            }
        }

        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        private string CreateInsertStatement(List<string> columns, Table t, List<string> data)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Insert Into " + t.TableName);
            sb.Append("(");
            for (int i = 0; i < t.OrderedColumnSchema.Count; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(t.OrderedColumnSchema[i]);
            }
            sb.Append(")");
            sb.Append(" Values");
            foreach (string row in data) {

                string[] rowData = row.Split('|');

                sb.Append(" (");
                for (int i = 0; i < rowData.Length; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                   
                    //EVerything is a string.
                    sb.Append("'" + rowData[i] + "'");
                }
                sb.Append("),");
            }

            return sb.ToString();
        }

        public List<string> Decompress(FileInfo fileToDecompress)
        {
            List<string> dataLines = new List<string>();
            using (FileStream reader = File.OpenRead(fileToDecompress.FullName))
            using (GZipStream zip = new GZipStream(reader, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
                while (!unzip.EndOfStream)
                    dataLines.Add(unzip.ReadLine());

            return dataLines;
        }

        private void CreateUpdateTrackingTables(Table t)
        {
            using (SqlConnection con = new SqlConnection("Data Source=98.204.41.162,49172;Initial Catalog=eventBit;Persist Security Info=True;User ID=Petefish;Password=Pete8901"))
            {
                var commandStr = "If exists (select name from sysobjects where name = '{0}') drop table {0} CREATE TABLE {0}{1}";

                commandStr = string.Format(commandStr, t.TableName, CreateTableColumns(t.OrderedColumnSchema));

                using (SqlCommand command = new SqlCommand(commandStr, con))
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

        private void RunInsertStatement(string insert)
        {

            using (SqlConnection con = new SqlConnection("Data Source=98.204.41.162,49172;Initial Catalog=eventBit;Persist Security Info=True;User ID=Petefish;Password=Pete8901"))
            {                         

                using (SqlCommand command = new SqlCommand(insert, con))
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

        private string CreateTableColumns(List<string> columns)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");
            foreach (string c in columns)
            {
                if (sb.Length > 1)
                    sb.Append(",");
                sb.Append(c + " varchar(max)");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
