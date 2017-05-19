using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace eventBitTestClient.Controllers
{
    public class HttpResponseHelper
    {
        public HttpResponseHelper() { }

        public HttpResponseMessage BadRequest(string header = "", string content = "")
        {
            HttpResponseMessage badReq = new HttpResponseMessage();

            badReq.StatusCode = HttpStatusCode.BadRequest;
            badReq.Headers.Add("X-AUTH-CLAIMS", header);
            //Just go ahead and return this content that I got.
            badReq.Content = new StringContent(content);
            return badReq;
        }

        public HttpResponseMessage OK(string header = "", string content = "")
        {
            HttpResponseMessage r = new HttpResponseMessage();
            r.StatusCode = HttpStatusCode.OK;
            r.Headers.Add("X-AUTH-CLAIMS", header);
            r.Content = new StringContent(content);
            return r;
        }


      
    }
}