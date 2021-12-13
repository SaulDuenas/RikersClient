using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using WebServiceTest.DTOs;
using WebServiceTest.Models;

namespace WebServiceTest.Controllers
{
    public class TicketController : ApiController
    {

        /*
        // Get api/files
        [HttpGet]
        public IEnumerable<File> Get()
        {
            Request.Headers.Add("Accept", "text/json");
            return File.FileList;
        }

    */

        [HttpGet]
        public IHttpActionResult Get()
        {
            Request.Headers.Add("Accept", "text/json");
            return Ok(InBoundFileMock.FileList);
        }


        [HttpPost]
        public IHttpActionResult Post([FromBody]InBoundFileDTO file)
        {

            InBoundFileMock.FileList.Insert(0,file); 

            return Ok();
         
        }

    }
}
