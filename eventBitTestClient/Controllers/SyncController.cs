using eventBitTestClient.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net;

namespace eventBitTestClient.Controllers
{
    public class SyncController : ApiController
    {
        private HttpResponseHelper RH = new HttpResponseHelper();
        private SQLDataHelper SDQ = new SQLDataHelper();

        // GET: api/Sync
        public IEnumerable<string> Get()
        {
            //eventBitEntities entities = new eventBitEntities();
            //string[] entCol = entities.GetType().GetProperties().Where(x => x.Name.StartsWith("Ent")).Select(x => x.Name).ToArray();
            return new string[] {"Booth",
                                "BoothCategory",
                                "Category",
                                "Company",
                                "CompanyAltName",
                                "CompanyBooth",
                                "CompanyCategory",
                                "Facility",
                                "FieldDetail",
                                "FieldDetailCategory",
                                "FieldDetailPick",
                                "FieldDetailPickCategory",
                                "Location",
                                "LocationProduct",
                                "LocationSchedule",
                                "Map",
                                "MapBooth",
                                "Person",
                                "PersonCategory",
                                "PersonCompany",
                                "PersonFieldDetailPick",
                                "PersonPurchase",
                                "PersonRegistration",
                                "PersonReservation",
                                "Product",
                                "ProductCategory"
                                 };
            // return entCol;
        }


        #region Helpers
        private async Task<EntityCallResponse> GetEntityResponse(string id, string eventName, double? since)
        {

            string authHeader = Request.Headers.GetValues("X-AUTH-CLAIMS").First();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.experienteventbit.com/webapi/API/Event/" + eventName + "/" + id);

            client.DefaultRequestHeaders.Add("X-AUTH-CLAIMS", authHeader);

            HttpResponseMessage response = client.GetAsync("?include=-&max=1000&since=" + since ?? "").Result;

            string newXAuthHeader = response.Headers.GetValues("X-AUTH-CLAIMS").First();

            var json = await response.Content.ReadAsStringAsync();

            return new EntityCallResponse() {Content = json, StatusCode = response.StatusCode, X_AUTH_CLAIMS = newXAuthHeader, RequestString = client.BaseAddress.AbsoluteUri + "?include=-&max=1000&since=" + since ?? "" };
        }

        public class EntityCallResponse
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Content { get; set; }
            public string X_AUTH_CLAIMS { get; set; }
            public string RequestString { get; set; }
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

        [Route("api/Sync/Events")]
        public async Task<HttpResponseMessage> GetEvents()
        {
            string authHeader = Request.Headers.GetValues("X-AUTH-CLAIMS").First();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://dev.experienteventbit.com/webapi/API/Event/");

            client.DefaultRequestHeaders.Add("X-AUTH-CLAIMS", authHeader);

            HttpResponseMessage response = client.GetAsync("").Result;

            string newXAuthHeader = response.Headers.GetValues("X-AUTH-CLAIMS").First();

            var json = await response.Content.ReadAsStringAsync();
            return RH.OK(newXAuthHeader, json);

        }

        // GET: api/Sync/5
        [Route("api/Sync/{id}/{eventName}")]
        public async Task<HttpResponseMessage> Get(string id, string eventName)
        {

            EntityState entityState;

            //Clear up data structures
            using (eventBitEntities entities = new eventBitEntities())
            {

                entityState = entities.EntityStates.FirstOrDefault(x => x.ShowCode == eventName && x.EntityID == id);
                if (entityState == null)
                {
                    entityState = new EntityState();
                    entityState.EntityID = id;
                    entityState.ShowCode = eventName;
                }

                EntityCallResponse resp = await GetEntityResponse(id, eventName, entityState.sysRowStampNumMax);

                //Error Check This Out.
                if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                {
                    return RH.BadRequest(resp.X_AUTH_CLAIMS, resp.Content);
                }

                var d = JsonConvert.DeserializeObject(resp.Content);

                ResponseDTO rDTO = new ResponseDTO();
                rDTO.Count = ((JArray)d).Count;
                rDTO.LastSince = resp.RequestString;

                //Sync Data Here
                //There has to be a way I can get this to be more generic.
                if (rDTO.Count > 0)
                {
                    try
                    {
                        //There has to be a way to make this SUPER generic. 
                        //Currently a messy switch. At least it makes it easy to debug.
                        //ProcessDataToEntities(entities, entityState, d, id);
                        int sysEventId = 0;

                        GenericSwitchToAssignTypes(d, id, out sysEventId);

                        UpdateEntitySyncLog(id, eventName, sysEventId);
                    }
                    catch (Exception e)
                    {
                        return RH.BadRequest(resp.X_AUTH_CLAIMS, e.Message.ToString());
                    }
                }

                return RH.OK(resp.X_AUTH_CLAIMS, JsonConvert.SerializeObject(rDTO));
            }

        }

        private class ResponseDTO
        {
            public int Count { get; set; }
            public string LastSince { get; set; }
        }

        private void ProcessDataToEntitiesGeneric<T>(dynamic d, string id, out int sysEventID) where T : class
        {
            // var ent = entities.Set<t.GetType()>();
            //I want to make sure I dispose of this here
            using (eventBitEntities entities = new eventBitEntities())
            {
                var table = entities.Set<T>();

                //I pass in show code but everything is based on sysEventId??
                sysEventID = 0;
                foreach (JObject data in d)
                {
                    var jsonEnt = data.ToObject<T>();

                    int pKey = Convert.ToInt32(jsonEnt.GetType().GetProperty(id + "ID").GetValue(jsonEnt, null));

                    if (sysEventID == 0)
                    {
                        sysEventID = Convert.ToInt32(jsonEnt.GetType().GetProperty("sysEventID").GetValue(jsonEnt, null));
                    }

                    var ent = table.Find(pKey);

                    if (ent == null)
                        table.Add(jsonEnt);
                    else
                        CopyPropertyValues(jsonEnt, ent);

                }
                entities.SaveChanges();
            }
            //Expression Tree? Maybe?

        }

        //I May want to send responses back so I can give the user a snapshot of whats going on.
        //Of course I can always just fake the loading bar.
        private void GenericSwitchToAssignTypes(dynamic d, string id, out int sysEventID)
        {
            sysEventID = 0;
            switch (id)
            {
                #region 1) Booth
                case "Booth":
                    {
                        ProcessDataToEntitiesGeneric<EntBooth>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 2) BoothCategory
                case "BoothCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntBoothCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 3) Category
                case "Category":
                    {
                        ProcessDataToEntitiesGeneric<EntCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 4) Company
                case "Company":
                    {
                        ProcessDataToEntitiesGeneric<EntCompany>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 5) CompanyAltName
                case "CompanyAltName":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyAltName>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 6) CompanyBooth
                case "CompanyBooth":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyBooth>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 7) CompanyCategory
                case "CompanyCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 8) Facility
                case "Facility":
                    {
                        ProcessDataToEntitiesGeneric<EntFacility>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 9) FieldDetail
                case "FieldDetail":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetail>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 10) FieldDetailCategory
                case "FieldDetailCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 11) FieldDetailPick
                case "FieldDetailPick":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailPick>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 12) FieldDetailPickCategory
                case "FieldDetailPickCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailPickCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 13) Location
                case "Location":
                    {
                        ProcessDataToEntitiesGeneric<EntLocation>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 14) LocationProduct
                case "LocationProduct":
                    {
                        ProcessDataToEntitiesGeneric<EntLocationProduct>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 15) LocationSchedule
                case "LocationSchedule":
                    {
                        ProcessDataToEntitiesGeneric<EntLocationSchedule>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 16) Map
                case "Map":
                    {
                        ProcessDataToEntitiesGeneric<EntMap>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 17) MapBooth
                case "MapBooth":
                    {
                        ProcessDataToEntitiesGeneric<EntMapBooth>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 18) Person
                case "Person":
                    {
                        ProcessDataToEntitiesGeneric<EntPerson>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 19) PersonCategory
                case "PersonCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonCategory>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 20) PersonCompany
                case "PersonCompany":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonCompany>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 21) PersonFieldDetailPick
                case "PersonFieldDetailPick":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonFieldDetailPick>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 22) PersonPurchase
                case "PersonPurchase":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonPurchase>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 23) PersonRegistration
                case "PersonRegistration":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonRegistration>(d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 24) PersonReservation
                case "PersonReservation":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonReservation>(d, id, out sysEventID);
                    }

                    break;
                #endregion
                #region 25) Product
                case "Product":
                    {
                        ProcessDataToEntitiesGeneric<EntProduct>(d, id, out sysEventID);
                    }

                    break;
                #endregion
                #region 26) ProductCategory
                case "ProductCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntProductCategory>(d, id, out sysEventID);
                    }

                    break;
                #endregion

            }
        }


        private void UpdateEntitySyncLog(string entityId, string showCode, int sysEventId)
        {
            var commandStr = "exec InsertUpdateEntityState '{0}', '{1}', {2}";

            commandStr = string.Format(commandStr, entityId, showCode, sysEventId);

            SDQ.ExecuteNonQuery(commandStr);
        }
    }
}
