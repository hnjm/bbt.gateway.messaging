using bbt.gateway.messaging.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Administration : ControllerBase
    {
        private readonly ILogger<Administration> _logger;

        public Administration(ILogger<Administration> logger)
        {
            _logger = logger;
        }

        [SwaggerOperation(
            Summary = "Returns content headers configuration",
            Description = "Returns content headers configuration"
            )]
        [HttpGet("/admin/headers")]
        [SwaggerResponse(200, "Headers was returned successfully", typeof(Header[]))]

        public IActionResult GetHeaders([FromQuery] int page = 0, [FromQuery] int pageSize = 20)
        {
            return Ok();
        }

        [SwaggerOperation(
           Summary = "Save header configuration",
           Description = "Save header configuration"
           )]
        [HttpPost("/admin/headers")]
        [SwaggerResponse(200, "Header has saved successfully", typeof(Header[]))]

        public IActionResult SaveHeader([FromBody] Header data)
        {
            return Ok();
        }

        [SwaggerOperation(
           Summary = "Deletes header configuration",
           Description = "Deletes header configuration"
           )]
        [HttpDelete("/admin/headers/{id}")]
        [SwaggerResponse(200, "Header has deleted successfully", typeof(Header[]))]

        public IActionResult DeleteHeader([FromQuery] Guid id)
        {
            return Ok();
        }


        [SwaggerOperation(
          Summary = "Returns blacklist records and sms log for specific phone",
          Description = "Returns blacklist records for specific phone"
          )]
        [HttpGet("/admin/phone-monitor/{countryCode}/{prefix}/{suffix}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(PhoneConfiguration))]

        public IActionResult GetPhoneMonitorRecords(int countryCode, int prefix, int suffix, [FromQuery] int smsSentLogSize = 20)
        {
            return Ok();
        }

    }
}
