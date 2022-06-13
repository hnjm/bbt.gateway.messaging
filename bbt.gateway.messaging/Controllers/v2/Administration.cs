using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace bbt.gateway.messaging.Controllers.v2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class Administration : ControllerBase
    {
        private readonly HeaderManager _headerManager;
        private readonly OperatorManager _operatorManager;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ITransactionManager _transactionManager;
        public Administration(HeaderManager headerManager, OperatorManager operatorManager,
            IRepositoryManager repositoryManager, ITransactionManager transactionManager)
        {
            _headerManager = headerManager;
            _operatorManager = operatorManager;
            _repositoryManager = repositoryManager;
            _transactionManager = transactionManager;
        }

        [SwaggerOperation(Summary = "Returns content headers configuration",
            Tags = new[] { "Header Management" })]
        [HttpGet("headers")]
        [SwaggerResponse(200, "Headers is returned successfully", typeof(Header[]))]
        public IActionResult GetHeaders([FromQuery][Range(0, 100)] int page = 0, [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            return Ok(_headerManager.Get(page, pageSize));
        }

        [SwaggerOperation(Summary = "Save or update header configuration",
            Tags = new[] { "Header Management" })]
        [HttpPost("headers")]
        [SwaggerResponse(200, "Header is saved successfully", typeof(Header[]))]
        public IActionResult SaveHeader([FromBody] Header data)
        {
            _headerManager.Save(data);
            return Ok();
        }

        [SwaggerOperation(Summary = "Deletes header configuration",
            Tags = new[] { "Header Management" })]
        [HttpDelete("headers/{id}")]
        [SwaggerResponse(200, "Header is deleted successfully", typeof(Header[]))]
        public IActionResult DeleteHeader([FromQuery] Guid id)
        {
            _headerManager.Delete(id);
            return Ok();
        }

        [SwaggerOperation(Summary = "Returns operator configurations",
            Tags = new[] { "Operator Management" })]
        [HttpGet("operators")]
        [SwaggerResponse(200, "Operators was returned successfully", typeof(Operator[]))]
        public IActionResult GetOperators()
        {
            return Ok(_operatorManager.Get());
        }

        [SwaggerOperation(Summary = "Updated operator configuration",
            Tags = new[] { "Operator Management" })]
        [HttpPost("operators")]
        [SwaggerResponse(200, "operator has saved successfully", typeof(void))]
        public IActionResult SaveOperator([FromBody] Operator data)
        {
            _operatorManager.Save(data);
            return Ok();
        }


        [SwaggerOperation(Summary = "Returns phone activities",
            Tags = new[] { "Phone Management" })]
        [HttpGet("phone-monitor/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(PhoneConfiguration))]

        public IActionResult GetPhoneMonitorRecords(int countryCode, int prefix, int number, int count)
        {
            return Ok(_repositoryManager.PhoneConfigurations.GetWithRelatedLogsAndBlacklistEntries(countryCode, prefix, number, count)
                .ToArray());
        }


        [SwaggerOperation(Summary = "Returns phone blacklist records",
            Tags = new[] { "Phone Management" })]
        [HttpGet("blacklists/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(BlackListEntry))]

        public IActionResult GetPhoneBlacklistRecords(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            return Ok(_repositoryManager.BlackListEntries
                .getWithLogs(countryCode, prefix, number, page, pageSize)
                .ToArray());
        }

        [SwaggerOperation(Summary = "Adds phone to blacklist records",
            Tags = new[] { "Phone Management" })]
        [HttpPost("blacklists")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        public IActionResult AddPhoneToBlacklist([FromBody] AddPhoneToBlacklistRequest data)
        {
            Guid newOtpBlackListEntryId = Guid.NewGuid();

            var config = _repositoryManager.PhoneConfigurations
                .Find(c => c.Phone.CountryCode == data.Phone.CountryCode && c.Phone.Prefix == data.Phone.Prefix && c.Phone.Number == data.Phone.Number)
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

                _repositoryManager.PhoneConfigurations.Add(config);
            }

            var newOtpBlackListEntry = new BlackListEntry
            {
                Id = newOtpBlackListEntryId,
                PhoneConfiguration = config,
                PhoneConfigurationId = config.Id,
                Reason = data.Reason,
                Source = data.Source,
                ValidTo = DateTime.UtcNow.AddDays(data.Days),
                CreatedBy = data.Process
            };

            _repositoryManager.BlackListEntries.Add(newOtpBlackListEntry);
            _repositoryManager.SaveChanges();


            return Created("", newOtpBlackListEntryId);
        }

        [SwaggerOperation(Summary = "Resolve blacklist item",
            Tags = new[] { "Phone Management" })]
        [HttpPatch("blacklists/{blacklist-entry-id}/resolve")]
        [SwaggerResponse(201, "Record was updated successfully", typeof(void))]
        [SwaggerResponse(404, "Record not found", typeof(void))]
        public IActionResult ResolveBlacklistItem([FromRoute(Name = "blacklist-entry-id")] Guid entryId, [FromBody] ResolveBlacklistEntryRequest data)
        {
            var config = _repositoryManager.BlackListEntries.FirstOrDefault(b => b.Id == entryId);
            if (config == null)
                return NotFound(entryId);
            var resolvedAt = DateTime.Now;
            config.ResolvedBy = data.ResolvedBy;
            config.Status = BlacklistStatus.Resolved;
            config.ResolvedAt = resolvedAt;

            //Update Old System
            var oldBlacklistEntry = _repositoryManager.DirectBlacklists.FirstOrDefault(b => b.SmsId == config.SmsId);
            if (oldBlacklistEntry != null)
            {
                oldBlacklistEntry.VerifyDate = resolvedAt;
                oldBlacklistEntry.IsVerified = true;
                oldBlacklistEntry.VerifiedBy = data.ResolvedBy.Identity;
                oldBlacklistEntry.Explanation = "Messaging Gateway Tarafından Onaylandı.";
                _repositoryManager.SaveSmsBankingChanges();
            }

            _repositoryManager.SaveChanges();

            return StatusCode(201);
        }

        [SwaggerOperation(Summary = "Adds phone or mail to whitelist records",
            Tags = new[] { "Whitelist Management" })]
        [HttpPost("whitelist")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        public IActionResult AddPhoneToWhitelist([FromBody] AddWhitelistRequest data)
        {
            if (string.IsNullOrEmpty(data.Email) && data.Phone == null)
            {
                return NotFound();
            }

            var whitelistRecord = new WhiteList()
            {
                CreatedBy = data.CreatedBy,
                IpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? HttpContext.Connection.RemoteIpAddress.ToString()
            };

            if (data.Phone != null)
            {
                whitelistRecord.Phone = data.Phone;
            }
            if (!string.IsNullOrEmpty(data.Email))
            {
                whitelistRecord.Mail = data.Email;
            }

            _repositoryManager.Whitelist.Add(whitelistRecord);
            _repositoryManager.SaveChanges();

            return Created("", whitelistRecord.Id);
        }

        [SwaggerOperation(Summary = "Returns phones otp sending logs",
            Tags = new[] { "Phone Management" })]
        [HttpGet("otp-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(OtpRequestLog[]))]
        public IActionResult GetOtpLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            return Ok(_repositoryManager.OtpRequestLogs
                .GetWithResponseLogs(countryCode, prefix, number, page, pageSize)
                .ToArray());
        }

        [SwaggerOperation(Summary = "Returns phones sms sending logs",
            Tags = new[] { "Phone Management" })]
        [HttpGet("sms-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(SmsResponseLog[]))]
        public IActionResult GetSmsLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {

            return Ok(_repositoryManager.SmsRequestLogs
                .Find(c => c.PhoneConfiguration.Phone.CountryCode == countryCode && c.PhoneConfiguration.Phone.Prefix == prefix && c.PhoneConfiguration.Phone.Number == number)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray());
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/phone/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public IActionResult GetTransactionsWithPhone(int countryCode, int prefix, int number, int smsType,DateTime startDate,DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (smsType == 1)
            {
                var res = _repositoryManager.Transactions.GetOtpMessagesWithPhone(countryCode, prefix, number, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (smsType == 2)
            {
                var res = _repositoryManager.Transactions.GetTransactionalSmsMessagesWithPhone(countryCode, prefix, number, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new TransactionsDto());
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/mail/{email}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public IActionResult GetTransactionsWithEmail(string mail, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            var res = _repositoryManager.Transactions.GetMailMessagesWithMail(mail, startDate, endDate, page, pageSize);
            return Ok(new TransactionsDto { Transactions = res.Item1 ,Count = res.Item2});
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/customer/{customerNo}/{messageType}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public IActionResult GetTransactionsWithCustomerNo(ulong customerNo,int messageType,int smsType, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (messageType == 1)
            {
                if (smsType == 1)
                {
                    var res = _repositoryManager.Transactions.GetOtpMessagesWithCustomerNo(customerNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1,Count = res.Item2});
                }
                if (smsType == 2)
                {
                    var res = _repositoryManager.Transactions.GetTransactionalSmsMessagesWithCustomerNo(customerNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                    
                return Ok(new TransactionsDto());
            }
            if (messageType == 2)
            {
                var res = _repositoryManager.Transactions.GetMailMessagesWithCustomerNo(customerNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (messageType == 3)
            {
                var res = _repositoryManager.Transactions.GetPushMessagesWithCustomerNo(customerNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new TransactionsDto());
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/citizen/{citizenshipNo}/{messageType}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public IActionResult GetTransactionsWithCitizenshipNo(string citizenshipNo, int messageType, int smsType, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (messageType == 1)
            {
                if (smsType == 1)
                {
                    var res = _repositoryManager.Transactions.GetOtpMessagesWithCitizenshipNo(citizenshipNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                    
                if (smsType == 2)
                {
                    var res = _repositoryManager.Transactions.GetTransactionalSmsMessagesWithCitizenshipNo(citizenshipNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                return Ok(new TransactionsDto());
            }
            if (messageType == 2)
            {
                var res = _repositoryManager.Transactions.GetMailMessagesWithCitizenshipNo(citizenshipNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (messageType == 3)
            {
                var res = _repositoryManager.Transactions.GetPushMessagesWithCitizenshipNo(citizenshipNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new List<Transaction>());
        }

    }
}
