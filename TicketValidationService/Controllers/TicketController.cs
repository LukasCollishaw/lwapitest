using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TicketValidationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly IBucket _bucket;

        public TicketController(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("tickets");
        }

        [HttpGet]
        [Route("validate/{id}")]
        public async Task<ActionResult<JObject>> Get(string id)
        {
            var ticket = await _bucket.GetAsync<dynamic>(id);

            if (!ticket.Success) return NotFound();
            
            return Ok(JObject.FromObject(new { valid = ticket.Value.valid }));
        }
    }
}
