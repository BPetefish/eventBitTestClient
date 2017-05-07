using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace eventBitTestClient.Controllers
{
    [RoutePrefix("src/api")]
    public class PullController : ApiController
    {
        // GET: api/Pull
        [HttpGet]
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

            return new HttpResponseMessage()
            {
                Content = new StringContent(newXAuthHeader, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }

    public class XAUTH
    {
        public int EntityID { get; set; }
        public int EntityType { get; set; }
        public double Expires { get; set; }
        public double Issued { get; set; }
        public string Token { get; set; }
    }

    public class Table
    {
        public string TableName { get; set; }
        public List<string> OrderedColumnSchema { get; set; }
        public List<string> ChunkURIs { get; set; }
    }

    public class TrackedData
    {
        public string UniqueIdentifier { get; set; }
        public List<Table> Tables { get; set; }
    }
}
