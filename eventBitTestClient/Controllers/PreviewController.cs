using eventBitTestClient.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace eventBitTestClient.Controllers
{
    public class PreviewController : ApiController
    {
        // GET: api/Preview
        [Route("api/Preview/{showCode}/{entityId}")]
        public object Get(string showCode, string entityId)
        {
            PreviewDTO pDTO = new PreviewDTO();
            SQLDataHelper SQLDA = new SQLDataHelper();

            eventBitEntities entities = new eventBitEntities();

            var entityState = entities.EntityStates.FirstOrDefault(x => x.ShowCode == showCode && x.EntityID == entityId);

            DataTable dt = new DataTable();

            if (entityState != null)
            {
                dt = SQLDA.GetEntityDataTable(entityId, (entityState.sysEventId ?? 0));

                pDTO.Columns = dt.Columns.Cast<DataColumn>().Where(x => !x.ColumnName.StartsWith("sys"))
                                     .Select(x => Char.ToLowerInvariant(x.ColumnName[0]) + x.ColumnName.Substring(1))
                                     .ToArray();

                pDTO.SysRowStampNumMax = entityState.sysRowStampNumMax.ToString();

                pDTO.RowCount = SQLDA.GetRowCountForEntity(entityId, (entityState.sysEventId ?? 0));
            }

            pDTO.Data = dt;

            return pDTO;
        }

        public class PreviewDTO
        {
            public int RowCount { get; set; }
            public string SysRowStampNumMax { get; set; }
            public DataTable Data { get; set; }
            public string[] Columns { get; set; }
        }
    }
}
