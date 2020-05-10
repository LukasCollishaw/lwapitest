using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Couchbase.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TicketAdminService.Controllers
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

        [HttpGet("count")]
        public async Task<JObject> GetNumberOfTickets()
        {
            var query = QueryRequest.Create("SELECT COUNT(*) AS size FROM `tickets`");
            var result = await _bucket.QueryAsync<dynamic>(query);

            int ret = 0;
            if (result.Success)
            {
                ret = result.Rows[0].size;
            }

            return JObject.FromObject(new { count = ret });
        }

        [HttpPost("invalidate/{id}")]
        public async Task<IActionResult> InvalidateTicket(string id)
        {
            var result = await _bucket.GetAsync<dynamic>(id);

            if (result.Value == null) return NotFound();

            await _bucket.UpsertAsync(id, new { valid = false });

            return Ok();
        }
    }
}
