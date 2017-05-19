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
using System.Linq.Expressions;
using System.Data.SqlClient;

namespace eventBitTestClient.Controllers
{
    public class SyncController : ApiController
    {
        private string X_AUTH_CLAIMS;

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


        // GET: api/Sync/5
        [Route("api/Sync/{id}/{eventName}")]
        public async Task<HttpResponseMessage> Get(string id, string eventName)
        {
            eventBitEntities entities = new eventBitEntities();
            HttpResponseMessage r = new HttpResponseMessage();

            EntityState entityState;


            entityState = entities.EntityStates.FirstOrDefault(x => x.ShowCode == eventName && x.EntityID == id);
            if (entityState == null)
            {
                entityState = new EntityState();
                entityState.EntityID = id;
                entityState.ShowCode = eventName;
                //entities.EntityStates.Add(entityState);
            }


            string json = await GetEntityResponse(id, eventName, entityState.sysRowStampNumMax);

            dynamic d = JsonConvert.DeserializeObject(json);

            //if (d.status)
            //{

            //}

            ResponseDTO rDTO = new ResponseDTO();
            rDTO.Count = ((JArray)d).Count;
            //if (((JArray)d).Count <= 0)
            //    break;

            //Sync Data Here
            //There has to be a way I can get this to be more generic.
            if (rDTO.Count > 0) {
                try
                {
                    //There has to be a way to make this SUPER generic. 
                    //Currently a messy switch. At least it makes it easy to debug.
                    //ProcessDataToEntities(entities, entityState, d, id);
                    int sysEventId = 0;

                    GenericSwitchToAssignTypes(entities, d, id, out sysEventId);

                    UpdateEntitySyncLog(id, eventName, sysEventId);
                }
                catch (Exception e)
                {

                }
            }



            r.StatusCode = HttpStatusCode.OK;
            r.Headers.Add("X-AUTH-CLAIMS", X_AUTH_CLAIMS);
            r.Content = new StringContent(JsonConvert.SerializeObject(rDTO));
            return r;
        }

        private class ResponseDTO
        {
            public int Count { get; set; }
        }

        private void ProcessDataToEntitiesGeneric<T>(eventBitEntities entities, dynamic d, string id, out int sysEventID) where T : class
        {
            // var ent = entities.Set<t.GetType()>();
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
            //Expression Tree? Maybe?
  
        }

        //I May want to send responses back so I can give the user a snapshot of whats going on.
        //Of course I can always just fake the loading bar.
        private void GenericSwitchToAssignTypes(eventBitEntities entities, dynamic d, string id, out int sysEventID)
        {
            sysEventID = 0;
            switch (id)
            {
                #region 1) Booth
                case "Booth":
                    {
                        ProcessDataToEntitiesGeneric<EntBooth>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 2) BoothCategory
                case "BoothCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntBoothCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 3) Category
                case "Category":
                    {
                        ProcessDataToEntitiesGeneric<EntCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 4) Company
                case "Company":
                    {
                        ProcessDataToEntitiesGeneric<EntCompany>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 5) CompanyAltName
                case "CompanyAltName":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyAltName>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 6) CompanyBooth
                case "CompanyBooth":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyBooth>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 7) CompanyCategory
                case "CompanyCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntCompanyCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 8) Facility
                case "Facility":
                    {
                        ProcessDataToEntitiesGeneric<EntFacility>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 9) FieldDetail
                case "FieldDetail":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetail>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 10) FieldDetailCategory
                case "FieldDetailCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 11) FieldDetailPick
                case "FieldDetailPick":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailPick>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 12) FieldDetailPickCategory
                case "FieldDetailPickCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntFieldDetailPickCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 13) Location
                case "Location":
                    {
                        ProcessDataToEntitiesGeneric<EntLocation>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 14) LocationProduct
                case "LocationProduct":
                    {
                        ProcessDataToEntitiesGeneric<EntLocationProduct>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 15) LocationSchedule
                case "LocationSchedule":
                    {
                        ProcessDataToEntitiesGeneric<EntLocationSchedule>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 16) Map
                case "Map":
                    {
                        ProcessDataToEntitiesGeneric<EntMap>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 17) MapBooth
                case "MapBooth":
                    {
                        ProcessDataToEntitiesGeneric<EntMapBooth>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 18) Person
                case "Person":
                    {
                        ProcessDataToEntitiesGeneric<EntPerson>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 19) PersonCategory
                case "PersonCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonCategory>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 20) PersonCompany
                case "PersonCompany":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonCompany>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 21) PersonFieldDetailPick
                case "PersonFieldDetailPick":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonFieldDetailPick>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 22) PersonPurchase
                case "PersonPurchase":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonPurchase>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 23) PersonRegistration
                case "PersonRegistration":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonRegistration>(entities, d, id, out sysEventID);
                    }
                    break;
                #endregion
                #region 24) PersonReservation
                case "PersonReservation":
                    {
                        ProcessDataToEntitiesGeneric<EntPersonReservation>(entities, d, id, out sysEventID);
                    }

                    break;
                #endregion
                #region 25) Product
                case "Product":
                    {
                        ProcessDataToEntitiesGeneric<EntProduct>(entities, d, id, out sysEventID);
                    }

                    break;
                #endregion
                #region 26) ProductCategory
                case "ProductCategory":
                    {
                        ProcessDataToEntitiesGeneric<EntProductCategory>(entities, d, id, out sysEventID);
                    }

                    break;
                    #endregion

            }
        }


        private void UpdateEntitySyncLog(string entityId, string showCode, int sysEventId)
        {
            using (SqlConnection con = new SqlConnection("Data Source=98.204.41.162,49172;Initial Catalog=eventBit;Persist Security Info=True;User ID=Petefish;Password=Pete8901"))
            {
                var commandStr = "exec InsertUpdateEntityState '{0}', '{1}', {2}";

                commandStr = string.Format(commandStr, entityId, showCode, sysEventId);

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
    }
}
