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
            HttpResponseMessage r = new HttpResponseMessage();

            string eventName = "IFT999";
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
                try
                {
                    ProcessDataToEntities(entities, entityState, d, id);
                } catch (Exception e)
                {

                }
            }


            r.StatusCode = HttpStatusCode.OK;
            r.Headers.Add("X-AUTH-CLAIMS", X_AUTH_CLAIMS);
            return r;
        }

        private void ProcessDataToEntities(eventBitEntities entities, EntityState entityState, dynamic d, string id)
        {
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
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntBooths.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntBooths.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "Category":
                    {
                        foreach (JObject data in d)
                        {
                            EntCategory jsonEnt = data.ToObject<EntCategory>();

                            var ent = entities.EntCategories.FirstOrDefault(x => x.CategoryID == jsonEnt.CategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "Company":
                    {
                        foreach (JObject data in d)
                        {
                            EntCompany jsonEnt = data.ToObject<EntCompany>();

                            var ent = entities.EntCompanies.FirstOrDefault(x => x.CompanyID == jsonEnt.CompanyID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntCompanies.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanies.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanies.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "CompanyAltName":
                    {
                        foreach (JObject data in d)
                        {
                            EntCompanyAltName jsonEnt = data.ToObject<EntCompanyAltName>();

                            var ent = entities.EntCompanyAltNames.FirstOrDefault(x => x.CompanyAltNameID == jsonEnt.CompanyAltNameID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntCompanyAltNames.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyAltNames.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyAltNames.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "CompanyBooth":
                    {
                        foreach (JObject data in d)
                        {
                            EntCompanyBooth jsonEnt = data.ToObject<EntCompanyBooth>();

                            var ent = entities.EntCompanyBooths.FirstOrDefault(x => x.CompanyBoothID == jsonEnt.CompanyBoothID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntCompanyBooths.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyBooths.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyBooths.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "CompanyCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntCompanyCategory jsonEnt = data.ToObject<EntCompanyCategory>();

                            var ent = entities.EntCompanyCategories.FirstOrDefault(x => x.CompanyCategoryID == jsonEnt.CompanyCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntCompanyCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntCompanyCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                    case "Facility":
                    {
                        foreach (JObject data in d)
                        {
                            EntFacility jsonEnt = data.ToObject<EntFacility>();

                            var ent = entities.EntFacilities.FirstOrDefault(x => x.FacilityID == jsonEnt.FacilityID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntFacilities.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntFacilities.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntFacilities.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "FieldDetail":
                    {
                        foreach (JObject data in d)
                        {
                            EntFieldDetail jsonEnt = data.ToObject<EntFieldDetail>();

                            var ent = entities.EntFieldDetails.FirstOrDefault(x => x.FieldDetailID == jsonEnt.FieldDetailID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntFieldDetails.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetails.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetails.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "FieldDetailPick":
                    {
                        foreach (JObject data in d)
                        {
                            EntFieldDetailPick jsonEnt = data.ToObject<EntFieldDetailPick>();

                            var ent = entities.EntFieldDetailPicks.FirstOrDefault(x => x.FieldDetailPickID == jsonEnt.FieldDetailPickID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntFieldDetailPicks.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailPicks.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailPicks.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "Location":
                    {
                        foreach (JObject data in d)
                        {
                            EntLocation jsonEnt = data.ToObject<EntLocation>();

                            var ent = entities.EntLocations.FirstOrDefault(x => x.LocationID == jsonEnt.LocationID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntLocations.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntLocations.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntLocations.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "LocationProduct":
                    {
                        foreach (JObject data in d)
                        {
                            EntLocationProduct jsonEnt = data.ToObject<EntLocationProduct>();

                            var ent = entities.EntLocationProducts.FirstOrDefault(x => x.LocationProductID == jsonEnt.LocationProductID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntLocationProducts.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntLocationProducts.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntLocationProducts.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                case "LocationSchedule":
                    {
                        foreach (JObject data in d)
                        {
                            EntLocationSchedule jsonEnt = data.ToObject<EntLocationSchedule>();

                            var ent = entities.EntLocationSchedules.FirstOrDefault(x => x.LocationScheduleID == jsonEnt.LocationScheduleID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntLocationSchedules.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntLocationSchedules.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntLocationSchedules.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;

            }
            //Save all added parts
            try
            {
                entities.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }
    }
}
