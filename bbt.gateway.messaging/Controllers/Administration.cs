using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace bbt.gateway.messaging.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Administration : ControllerBase
    {
        private readonly HeaderManager _headerManager;
        private readonly OperatorManager _operatorManager;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<Administration> _logger;

        public Administration(ILogger<Administration> logger,HeaderManager headerManager,OperatorManager operatorManager,
            DatabaseContext databaseContext)
        {
            _logger = logger;
            _headerManager = headerManager;
            _operatorManager = operatorManager;
            _databaseContext = databaseContext;
        }

        [SwaggerOperation(Summary = "Returns content headers configuration")]
        [HttpGet("/admin/headers")]
        [SwaggerResponse(200, "Headers is returned successfully", typeof(Header[]))]
        public IActionResult GetHeaders([FromQuery][Range(0, 100)] int page = 0, [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            return Ok(_headerManager.Get(page, pageSize));
        }

        [SwaggerOperation(Summary = "Save or update header configuration")]
        [HttpPost("/admin/headers")]
        [SwaggerResponse(200, "Header is saved successfully", typeof(Header[]))]
        public IActionResult SaveHeader([FromBody] Header data)
        {
            _headerManager.Save(data);
            return Ok();
        }

        [SwaggerOperation(Summary = "Deletes header configuration")]
        [HttpDelete("/admin/headers/{id}")]
        [SwaggerResponse(200, "Header is deleted successfully", typeof(Header[]))]
        public IActionResult DeleteHeader([FromQuery] Guid id)
        {
            _headerManager.Delete(id);
            return Ok();
        }

        [SwaggerOperation(Summary = "Returns operator configurations")]
        [HttpGet("/admin/operators")]
        [SwaggerResponse(200, "Operators was returned successfully", typeof(Operator[]))]
        public IActionResult GetOperators()
        {
            return Ok(_operatorManager.Get());
        }

        [SwaggerOperation(Summary = "Updated operator configuration")]
        [HttpPost("/admin/operators")]
        [SwaggerResponse(200, "operator has saved successfully", typeof(void))]
        public IActionResult SaveOperator([FromBody] Operator data)
        {
            _operatorManager.Save(data);
            return Ok();
        }


        [SwaggerOperation(Summary = "Returns phone activities")]
        [HttpGet("/admin/phone-monitor/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(PhoneConfiguration))]

        public IActionResult GetPhoneMonitorRecords(int countryCode, int prefix, int number)
        {
            PhoneConfiguration[] returnValue = null;
            
            returnValue = _databaseContext.PhoneConfigurations
                .Where(c => c.Phone.CountryCode == countryCode && c.Phone.Prefix == prefix && c.Phone.Number == number)
                .Include(c => c.BlacklistEntries.Take(10).OrderBy(l => l.CreatedAt))
                .Include(c => c.OtpLogs.Take(10).OrderBy(l => l.CreatedAt))
                .Include(c => c.Logs.Take(10).OrderBy(l => l.CreatedAt))
                .Include(c => c.SmsLogs.Take(10).OrderBy(l => l.CreatedAt))
                .ToArray();

            return Ok(returnValue);
        }


        [SwaggerOperation(Summary = "Returns phone blacklist records")]
        [HttpGet("/admin/blacklists/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(BlackListEntry))]

        public IActionResult GetPhoneBlacklistRecords(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            BlackListEntry[] returnValue = null;

            returnValue = _databaseContext.BlackListEntries
                .Where(c => c.PhoneConfiguration.Phone.CountryCode == countryCode && c.PhoneConfiguration.Phone.Prefix == prefix && c.PhoneConfiguration.Phone.Number == number)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray(); 

            return Ok(returnValue);
        }

        [SwaggerOperation(Summary = "Adds phone to blacklist records")]
        [HttpPost("/admin/blacklists")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        public IActionResult AddPhoneToBlacklist([FromBody] AddPhoneToBlacklistRequest data)
        {
            Guid newOtpBlackListEntryId = Guid.NewGuid();

           
            var config = _databaseContext.PhoneConfigurations
                .Where(c => c.Phone.CountryCode == data.Phone.CountryCode && c.Phone.Prefix == data.Phone.Prefix && c.Phone.Number == data.Phone.Number)
                .FirstOrDefault();

            if (config == null)
            {
                config = new PhoneConfiguration
                {
                    Phone = data.Phone,
                    Logs = new List<PhoneConfigurationLog>(),
                    BlacklistEntries = new List<BlackListEntry>()
                };

                config.Logs.Add(new PhoneConfigurationLog
                {
                    Type = "Initialization",
                    Action = "Blacklist Entry",
                    CreatedBy = data.Process,
                    RelatedId = newOtpBlackListEntryId
                });

                _databaseContext.Add(config);
            }

            var newOtpBlackListEntry = new BlackListEntry
            {
                Id = newOtpBlackListEntryId,
                PhoneConfigurationId = config.Id,
                Reason = data.Reason,
                Source = data.Source,
                ValidTo = DateTime.Now.AddDays(data.Days),
                CreatedBy = data.Process
            };

            _databaseContext.Add(newOtpBlackListEntry);
            _databaseContext.SaveChanges();
            

            return Created("", newOtpBlackListEntryId);
        }

        [SwaggerOperation(Summary = "Resolve blacklist item")]
        [HttpPatch("/admin/blacklists/{blacklist-entry-id}/resolve")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        public IActionResult ResolveBlacklistItem([FromRoute(Name = "blacklist-entry-id")] Guid entryId, [FromBody] ResolveBlacklistEntryRequest data)
        {
            
            var config = _databaseContext.BlackListEntries.FirstOrDefault(b => b.Id == entryId);
            config.ResolvedBy = data.ResolvedBy;
            config.Status = BlacklistStatus.Resolved;
            _databaseContext.SaveChanges();
            
            return Ok();
        }

        [SwaggerOperation(Summary = "Returns phones otp sending logs")]
        [HttpGet("/admin/otp-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(OtpRequestLog[]))]
        public IActionResult GetOtpLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            OtpRequestLog[] returnValue = null;

            returnValue = _databaseContext.OtpRequestLogs
                .Where(c => c.PhoneConfiguration.Phone.CountryCode == countryCode && c.PhoneConfiguration.Phone.Prefix == prefix && c.PhoneConfiguration.Phone.Number == number)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray();

            return Ok(returnValue);
        }

        [SwaggerOperation(Summary = "Returns phones sms sending logs")]
        [HttpGet("/admin/sms-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(SmsLog[]))]
        public IActionResult GetSmsLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            SmsLog[] returnValue = null;

           
            returnValue = _databaseContext.SmsLogs
                .Where(c => c.PhoneConfiguration.Phone.CountryCode == countryCode && c.PhoneConfiguration.Phone.Prefix == prefix && c.PhoneConfiguration.Phone.Number == number)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray();

            return Ok(returnValue);
        }

    }
}
