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
            //eventBitEntities entities = new eventBitEntities();
            //string[] entCol = entities.GetType().GetProperties().Where(x => x..StartsWith("Ent")).Select(x => x.Name).ToArray();
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
                    //There has to be a way to make this SUPER generic. 
                    //Currently a messy switch. At least it makes it easy to debug.
                    ProcessDataToEntities(entities, entityState, d, id);
                    //ProcessDataToEntitiesGeneric<EntBooth>(entities, entityState, d, id);
                }
                catch (Exception e)
                {

                }
            }


            r.StatusCode = HttpStatusCode.OK;
            r.Headers.Add("X-AUTH-CLAIMS", X_AUTH_CLAIMS);
            return r;
        }


        private void ProcessDataToEntitiesGeneric<T>(eventBitEntities entities, EntityState entityState, dynamic d, string id) where T : class
        {
            // var ent = entities.Set<t.GetType()>();
            //var table = entities.Set<T>();

        //    var ent = table.FirstOrDefault(x => x.BoothId = 100)
        }

        //I May want to send responses back so I can give the user a snapshot of whats going on.
        //Of course I can always just fake the loading bar.
        private void ProcessDataToEntities(eventBitEntities entities, EntityState entityState, dynamic d, string id)
        {
            switch (id)
            {
                #region 1) Booth
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
                #endregion
                #region 2) BoothCategory
                case "BoothCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntBoothCategory jsonEnt = data.ToObject<EntBoothCategory>();

                            var ent = entities.EntBoothCategories.FirstOrDefault(x => x.BoothCategoryID == jsonEnt.BoothCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntBoothCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntBoothCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntBoothCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 3) Category
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
                #endregion
                #region 4) Company
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
                #endregion
                #region 5) CompanyAltName
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
                #endregion
                #region 6) CompanyBooth
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
                #endregion
                #region 7) CompanyCategory
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
                #endregion
                #region 8) Facility
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
                #endregion
                #region 9) FieldDetail
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
                #endregion
                #region 10) FieldDetailCategory
                case "FieldDetailCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntFieldDetailCategory jsonEnt = data.ToObject<EntFieldDetailCategory>();

                            var ent = entities.EntFieldDetailCategories.FirstOrDefault(x => x.FieldDetailCategoryID == jsonEnt.FieldDetailCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntFieldDetailCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 11) FieldDetailPick
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
                #endregion
                #region 12) FieldDetailPickCategory
                case "FieldDetailPickCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntFieldDetailPickCategory jsonEnt = data.ToObject<EntFieldDetailPickCategory>();

                            var ent = entities.EntFieldDetailPickCategories.FirstOrDefault(x => x.FieldDetailPickCategoryID == jsonEnt.FieldDetailPickCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntFieldDetailPickCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailPickCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntFieldDetailPickCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 13) Location
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
                #endregion
                #region 14) LocationProduct
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
                #endregion
                #region 15) LocationSchedule
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
                #endregion
                #region 16) Map
                case "Map":
                    {
                        foreach (JObject data in d)
                        {
                            EntMap jsonEnt = data.ToObject<EntMap>();

                            var ent = entities.EntMaps.FirstOrDefault(x => x.MapID == jsonEnt.MapID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntMaps.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntMaps.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntMaps.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 17) MapBooth
                case "MapBooth":
                    {
                        foreach (JObject data in d)
                        {
                            EntMapBooth jsonEnt = data.ToObject<EntMapBooth>();

                            var ent = entities.EntMapBooths.FirstOrDefault(x => x.MapBoothID == jsonEnt.MapBoothID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntMapBooths.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntMapBooths.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntMapBooths.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 18) Person
                case "Person":
                    {
                        foreach (JObject data in d)
                        {
                            EntPerson jsonEnt = data.ToObject<EntPerson>();

                            var ent = entities.EntPersons.FirstOrDefault(x => x.PersonID == jsonEnt.PersonID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersons.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersons.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersons.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 19) PersonCategory
                case "PersonCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonCategory jsonEnt = data.ToObject<EntPersonCategory>();

                            var ent = entities.EntPersonCategories.FirstOrDefault(x => x.PersonCategoryID == jsonEnt.PersonCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonCategories.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 20) PersonCompany
                case "PersonCompany":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonCompany jsonEnt = data.ToObject<EntPersonCompany>();

                            var ent = entities.EntPersonCompanies.FirstOrDefault(x => x.PersonCompanyID == jsonEnt.PersonCompanyID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonCompanies.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonCompanies.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonCompanies.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 21) PersonFieldDetailPick
                case "PersonFieldDetailPick":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonFieldDetailPick jsonEnt = data.ToObject<EntPersonFieldDetailPick>();

                            var ent = entities.EntPersonFieldDetailPicks.FirstOrDefault(x => x.PersonFieldDetailPickID == jsonEnt.PersonFieldDetailPickID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonFieldDetailPicks.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonFieldDetailPicks.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonFieldDetailPicks.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 22) PersonPurchase
                case "PersonPurchase":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonPurchase jsonEnt = data.ToObject<EntPersonPurchase>();

                            var ent = entities.EntPersonPurchases.FirstOrDefault(x => x.PersonPurchaseID == jsonEnt.PersonPurchaseID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonPurchases.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonPurchases.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonPurchases.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 23) PersonRegistration
                case "PersonRegistration":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonRegistration jsonEnt = data.ToObject<EntPersonRegistration>();

                            var ent = entities.EntPersonRegistrations.FirstOrDefault(x => x.PersonRegistrationID == jsonEnt.PersonRegistrationID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonRegistrations.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonRegistrations.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonRegistrations.Max(m => m.sysRowStampNum);
                        }
                    }
                    break;
                #endregion
                #region 24) PersonReservation
                case "PersonReservation":
                    {
                        foreach (JObject data in d)
                        {
                            EntPersonReservation jsonEnt = data.ToObject<EntPersonReservation>();

                            var ent = entities.EntPersonReservations.FirstOrDefault(x => x.PersonReservationID == jsonEnt.PersonReservationID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntPersonReservations.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonReservations.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntPersonReservations.Max(m => m.sysRowStampNum);
                        }
                    }

                    break;
                #endregion
                #region 25) Product
                case "Product":
                    {
                        foreach (JObject data in d)
                        {
                            EntProduct jsonEnt = data.ToObject<EntProduct>();

                            var ent = entities.EntProducts.FirstOrDefault(x => x.ProductID == jsonEnt.ProductID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntProducts.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntProducts.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntProducts.Max(m => m.sysRowStampNum);
                        }
                    }

                    break;
                #endregion
                #region 26) ProductCategory
                case "ProductCategory":
                    {
                        foreach (JObject data in d)
                        {
                            EntProductCategory jsonEnt = data.ToObject<EntProductCategory>();

                            var ent = entities.EntProductCategories.FirstOrDefault(x => x.ProductCategoryID == jsonEnt.ProductCategoryID && x.sysEventID == jsonEnt.sysEventID);

                            if (ent == null)
                                entities.EntProductCategories.Add(jsonEnt);
                            else
                                CopyPropertyValues(jsonEnt, ent);

                        }
                        try
                        {
                            entityState.sysRowStampNumMax = entities.EntProductCategories.Local.Max(m => m.sysRowStampNum);
                        }
                        catch (Exception e)
                        {
                            entityState.sysRowStampNumMax = entities.EntProductCategories.Max(m => m.sysRowStampNum);
                        }
                    }

                    break;
                    #endregion

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
