using eventBitTestClient.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace eventBitTestClient.Controllers
{
    public class SyncController : ApiController
    {
        private string X_AUTH_CLAIMS;
        // GET: api/Sync
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        #region Helpers
        private async Task<string> GetEntityResponse(string id, string eventName, double? since)
        {
            if (string.IsNullOrEmpty(X_AUTH_CLAIMS))
                X_AUTH_CLAIMS = Request.Headers.GetValues("X-AUTH-CLAIMS").First();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.experienteventbit.com/webapi/API/Event/" + eventName + "/" + id);

            client.DefaultRequestHeaders.Add("X-AUTH-CLAIMS", X_AUTH_CLAIMS);

            HttpResponseMessage response = client.GetAsync("?include=-&max=1000&since=" + since ?? "").Result;

            string newXAuthHeader = response.Headers.GetValues("X-AUTH-CLAIMS").First();

            X_AUTH_CLAIMS = newXAuthHeader;

            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        public static void CopyPropertyValues(object source, object destination)
        {
            var destProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                foreach (var destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name &&
                destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        destProperty.SetValue(destination, sourceProperty.GetValue(
                            source, new object[] { }), new object[] { });

                        break;
                    }
                }
            }
        }
        #endregion

        //private void FillEntityFromJObject

        // GET: api/Sync/5
        public async Task<HttpResponseMessage> Get(string id)
        {
            eventBitEntities entities = new eventBitEntities();

            string eventName = "INF999";
            double rowStamp = 0;
            EntityState entityState;


            while (true)
            {

                //Get EntityState
                entityState = entities.EntityStates.FirstOrDefault(x => x.ShowCode == eventName && x.EntityID == id);
                if (entityState == null)
                {
                    entityState = new EntityState();
                    entityState.EntityID = id;
                    entityState.ShowCode = eventName;
                    entities.EntityStates.Add(entityState);
                }


                string json = await GetEntityResponse(id, eventName, entityState.sysRowStampNumMax);

                dynamic d = JsonConvert.DeserializeObject(json);

                Enumerable.Count(d);

                if (((JArray)d).Count <= 0)
                    break;

                //Sync Data Here
                //There has to be a way I can get this to be more generic.
                switch (id)
                {
                    case "Booth":
                        {
                            foreach (JObject data in d)
                            {
                                EntBooth jsonEnt = data.ToObject<EntBooth>();

                                var ent = entities.EntBooths.FirstOrDefault(x => x.BoothID == jsonEnt.BoothID && x.sysEventID == jsonEnt.sysEventID);

                                if (ent == null)                            
                                    entities.EntBooths.Add(jsonEnt);                                
                                else                                
                                    CopyPropertyValues(jsonEnt, ent);                      

                            }
                            entityState.sysRowStampNumMax = entities.EntBooths.Max(m => m.sysRowStampNum);
                        }
                        break;
                    case "Category":
                        {
                            foreach (JObject data in d)
                            {
                                var jsonEnt = data.ToObject<EntCategory>();

                                var ent = entities.EntCategories.FirstOrDefault(x => x.CategoryID == jsonEnt.CategoryID && x.sysEventID == jsonEnt.sysEventID);

                                if (ent == null)
                                    entities.EntCategories.Add(jsonEnt);
                                else
                                    CopyPropertyValues(jsonEnt, ent);

                            }
                            entityState.sysRowStampNumMax = entities.EntCategories.Max(m => m.sysRowStampNum);
                        }
                        break;
                }
                //Save all added parts
                entities.SaveChanges();
            }
            
            return new HttpResponseMessage()
            {
                Content = new StringContent(X_AUTH_CLAIMS, System.Text.Encoding.UTF8, "application/json")
            };
        }

        // POST: api/Sync
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Sync/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Sync/5
        public void Delete(int id)
        {
        }
    }
}
