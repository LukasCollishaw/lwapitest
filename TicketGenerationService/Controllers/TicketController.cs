using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TicketGenerationService.Controllers
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

        [HttpPut("generate")]
        public async Task<JObject> Put()
        {
            var ticketId = Guid.NewGuid().ToString();

            await _bucket.UpsertAsync(ticketId, new { valid = true, created = DateTime.Now });

            return JObject.FromObject(new { id = ticketId });
        }
    }
}
