using eventBitTestClient.Data;
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

        private HttpResponseHelper RH = new HttpResponseHelper();
        private SQLDataHelper SDQ = new SQLDataHelper();

        // GET: api/Snapshot
        [Route("api/Snapshot/{eventName}")]
        public async Task<HttpResponseMessage> Get(string eventName)
        {
            eventBitEntities entities = new eventBitEntities();

            //Grab my X-AUTH from header.
            string reqAuth = Request.Headers.GetValues("X-AUTH-CLAIMS").First();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.experienteventbit.com/webapi/API/Event/");

            client.DefaultRequestHeaders.Add("X-AUTH-CLAIMS", reqAuth);

            HttpResponseMessage response = client.GetAsync(eventName + "/TrackingData").Result;

            string newXAuthHeader = response.Headers.GetValues("X-AUTH-CLAIMS").First();

            var data = await response.Content.ReadAsStringAsync();

            //Check for error
            HttpResponseMessage r = new HttpResponseMessage();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var d = JsonConvert.DeserializeObject<TrackedData>(data);

                SnapshotState ssState;
                //Check my snapshot id.
                ssState = entities.SnapshotStates.FirstOrDefault(x => x.ShowCode == eventName);
                
                if (ssState != null)
                {
                    if (ssState.UniqueIdentifier == d.UniqueIdentifier)
                        return RH.OK(newXAuthHeader, "Snapshot unique identifier matches currently loaded snapshot, no new snapshot available.");
                } else
                {
                    ssState = new SnapshotState();
                    ssState.ShowCode = eventName;
                    entities.SnapshotStates.Add(ssState);
                }

                ProcessTrackedData(d, eventName);

                ssState.UniqueIdentifier = d.UniqueIdentifier;
                entities.SaveChanges();

                return RH.OK(newXAuthHeader, "Snapshot sync complete.");
            }
            else
            {
                return RH.BadRequest(newXAuthHeader, data);
            }


        }

        const string DIR_PATH = @"C:\Users\xFish\Documents\ChunkURITest\";
        private void ProcessTrackedData(TrackedData d, string eventName)
        {
            foreach (Table t in d.Tables)
            {
                //Make Sure My Tables Are Correct
                CreateUpdateTrackingTables(t, eventName);

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
                        string insert = CreateInsertStatement(t.OrderedColumnSchema, t, bd, eventName).TrimEnd(',');
                        RunInsertStatement(insert);
                    }
                }
            }
        }

        #region File Helpers
        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
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
        #endregion

        #region SQL Helpers
        private void CreateUpdateTrackingTables(Table t, string eventName)
        {
            var commandStr = "If exists (select name from sysobjects where name = '{0}') drop table {0} CREATE TABLE {0}{1}";

            commandStr = string.Format(commandStr, eventName + "_" + t.TableName, CreateTableColumns(t.OrderedColumnSchema));

            SDQ.ExecuteNonQuery(commandStr);
        }

        private void RunInsertStatement(string insert)
        {
            SDQ.ExecuteNonQuery(insert);
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

        private string CreateInsertStatement(List<string> columns, Table t, List<string> data, string eventName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Insert Into " + eventName + "_" + t.TableName);
            sb.Append("(");
            for (int i = 0; i < t.OrderedColumnSchema.Count; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(t.OrderedColumnSchema[i]);
            }
            sb.Append(")");
            sb.Append(" Values");
            foreach (string row in data)
            {

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
        #endregion
    }
}
