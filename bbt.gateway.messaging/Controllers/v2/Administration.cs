using bbt.gateway.common.Extensions;
using bbt.gateway.common.Models;
using bbt.gateway.common.Models.v2;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly CodecSender _codecSender;
        private readonly dEngageSender _dEngageSender;
        private readonly OtpSender _otpSender;
        public Administration(HeaderManager headerManager, OperatorManager operatorManager,
            IRepositoryManager repositoryManager, ITransactionManager transactionManager,
            CodecSender codecSender,dEngageSender dEngageSender,OtpSender otpSender)
        {
            _headerManager = headerManager;
            _operatorManager = operatorManager;
            _repositoryManager = repositoryManager;
            _transactionManager = transactionManager;
            _codecSender = codecSender;
            _dEngageSender = dEngageSender;
            _otpSender = otpSender;
        }

        [SwaggerOperation(
           Summary = "Check Fast Sms Message Status",
           Description = "Check Fast Sms Delivery Status."
           )]
        [HttpPost("sms/check-message")]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<IActionResult> CheckMessageStatus([FromBody] CheckFastSmsRequest data)
        {

            if (ModelState.IsValid)
            {
                if (data.Operator == common.Models.OperatorType.Codec)
                {
                    var res = await _codecSender.CheckSms(data);
                    return Ok(res);
                }
                if (data.Operator == common.Models.OperatorType.dEngageOn ||
                    data.Operator == common.Models.OperatorType.dEngageBurgan)
                {
                    var res = await _dEngageSender.CheckSms(data);
                    return Ok(res);
                }
                return BadRequest("Unknown Operator");
            }
            else
            {
                return BadRequest(ModelState);
            }


        }

        [SwaggerOperation(
           Summary = "Check Fast Sms Message Status",
           Description = "Check Fast Sms Delivery Status."
           )]
        [HttpPost("otp/check-message")]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<IActionResult> CheckOtpMessageStatus([FromBody] common.Models.v2.CheckSmsRequest data)
        {

            if (ModelState.IsValid)
            {
                var res = await _otpSender.CheckMessage(data.MapTo<common.Models.CheckSmsRequest>());
                return Ok(res);

            }
            else
            {
                return BadRequest(ModelState);
            }


        }

        [SwaggerOperation(Summary = "Returns content headers configuration",
            Tags = new[] { "Header Management" })]
        [HttpGet("headers")]
        [SwaggerResponse(200, "Headers is returned successfully", typeof(Header[]))]
        public async Task<IActionResult> GetHeaders([FromQuery][Range(0, 100)] int page = 0, [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            return Ok(await _headerManager.Get(page, pageSize));
        }

        [SwaggerOperation(Summary = "Save or update header configuration",
            Tags = new[] { "Header Management" })]
        [HttpPost("headers")]
        [SwaggerRequestExample(typeof(HeaderRequest), typeof(AddHeaderRequestExampleFilter))]
        [SwaggerResponse(200, "Header is saved successfully", typeof(Header[]))]
        public async Task<IActionResult> SaveHeader([FromBody] Header data)
        {
            await _headerManager.Save(data);
            return Ok();
        }

        [SwaggerOperation(Summary = "Deletes header configuration",
            Tags = new[] { "Header Management" })]
        [HttpDelete("headers/{id}")]
        [SwaggerResponse(200, "Header is deleted successfully", typeof(void))]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            await _headerManager.Delete(id);
            return Ok();
        }

        [SwaggerOperation(Summary = "Returns operator configurations",
            Tags = new[] { "Operator Management" })]
        [HttpGet("operators")]
        [SwaggerResponse(200, "Operators was returned successfully", typeof(Operator[]))]
        public async Task<IActionResult> GetOperators()
        {
            return Ok(await _operatorManager.Get());
        }

        [SwaggerOperation(Summary = "Updated operator configuration",
            Tags = new[] { "Operator Management" })]
        [HttpPost("operators")]
        [SwaggerResponse(200, "operator has saved successfully", typeof(void))]
        public async Task<IActionResult> SaveOperator([FromBody] Operator data)
        {
            await _operatorManager.Save(data);
            return Ok();
        }


        [SwaggerOperation(Summary = "Returns phone activities",
            Tags = new[] { "Phone Management" })]
        [HttpGet("phone-monitor/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(PhoneConfiguration))]

        public async Task<IActionResult> GetPhoneMonitorRecords(int countryCode, int prefix, int number, int count)
        {
            return Ok(await _repositoryManager.PhoneConfigurations.GetWithRelatedLogsAndBlacklistEntriesAsync(countryCode, prefix, number, count));
        }


        [SwaggerOperation(Summary = "Returns phone blacklist records",
            Tags = new[] { "Phone Management" })]
        [HttpGet("blacklists/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(BlackListEntry))]

        public async Task<IActionResult> GetPhoneBlacklistRecords(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            return Ok(await _repositoryManager.BlackListEntries
                .GetWithLogsAsync(countryCode, prefix, number, page, pageSize));
        }

        [SwaggerOperation(Summary = "Adds phone to blacklist records",
            Tags = new[] { "Phone Management" })]
        [HttpPost("blacklists")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        public async Task<IActionResult> AddPhoneToBlacklist([FromBody] AddPhoneToBlacklistRequest data)
        {
            Guid newOtpBlackListEntryId = Guid.NewGuid();

            var config = (await _repositoryManager.PhoneConfigurations
                .FindAsync(c => c.Phone.CountryCode == data.Phone.CountryCode && c.Phone.Prefix == data.Phone.Prefix && c.Phone.Number == data.Phone.Number))
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

                await _repositoryManager.PhoneConfigurations.AddAsync(config);
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

            await _repositoryManager.BlackListEntries.AddAsync(newOtpBlackListEntry);

            return Created("", newOtpBlackListEntryId);
        }

        [SwaggerOperation(Summary = "Resolve blacklist item",
            Tags = new[] { "Phone Management" })]
        [HttpPatch("blacklists/{blacklist-entry-id}/resolve")]
        [SwaggerResponse(201, "Record was updated successfully", typeof(void))]
        [SwaggerResponse(404, "Record not found", typeof(void))]
        public async Task<IActionResult> ResolveBlacklistItem([FromRoute(Name = "blacklist-entry-id")] Guid entryId, [FromBody] ResolveBlacklistEntryRequest data)
        {
            var config = await _repositoryManager.BlackListEntries.FirstOrDefaultAsync(b => b.Id == entryId);
            if (config == null)
                return NotFound(entryId);
            var resolvedAt = DateTime.Now;
            config.ResolvedBy = data.ResolvedBy;
            config.Status = BlacklistStatus.Resolved;
            config.ResolvedAt = resolvedAt;

            //Update Old System
            var oldBlacklistEntry = await _repositoryManager.DirectBlacklists.FirstOrDefaultAsync(b => b.SmsId == config.SmsId);
            if (oldBlacklistEntry != null)
            {
                oldBlacklistEntry.VerifyDate = resolvedAt;
                oldBlacklistEntry.IsVerified = true;
                oldBlacklistEntry.VerifiedBy = data.ResolvedBy.Identity;
                oldBlacklistEntry.Explanation = "Messaging Gateway Tarafından Onaylandı.";
                await _repositoryManager.SaveSmsBankingChangesAsync();
            }

            await _repositoryManager.SaveChangesAsync();

            return StatusCode(201);
        }

        [SwaggerOperation(Summary = "Deletes phone activities",
            Tags = new[] { "Phone Management" })]
        [HttpDelete("phone-monitor/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(204, "Records was deleted successfully", typeof(void))]

        public async Task<IActionResult> GetPhoneMonitorRecords(int countryCode, int prefix, int number)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Prod" &&
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                var phoneConfigurations = await _repositoryManager.PhoneConfigurations.FindAsync(p =>
                p.Phone.CountryCode == countryCode &&
                p.Phone.Prefix == prefix &&
                p.Phone.Number == number
            );

                foreach (var phoneConfiguration in phoneConfigurations)
                {
                    await _repositoryManager.PhoneConfigurations.DeletePhoneConfiguration(phoneConfiguration.Id);
                }


                return NoContent();
            }
            return Forbid();   
        }

        [SwaggerOperation(Summary = "Adds phone to whitelist",
            Description = "<div>Phone Number Has To Be Added To Whitelist To Receives Sms On Test Environment</div>",
            Tags = new[] { "Whitelist Management" })]
        [HttpPost("whitelist/phone")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        [SwaggerResponse(400, "Phone Number Field Is Mandatory", typeof(void))]
        [SwaggerResponse(409, "Phone Number Is Already Exists In Whitelist", typeof(void))]
        public async Task<IActionResult> AddPhoneToWhitelist([FromBody] AddPhoneToWhitelistRequest data)
        {
            if (data.Phone == null)
            {
                return BadRequest("Phone Number Field Is Mandatory");
            }

            if ((await _repositoryManager.Whitelist.FindAsync(w => (w.Phone.CountryCode == data.Phone.CountryCode)
             && (w.Phone.Prefix == data.Phone.Prefix)
             && (w.Phone.Number == data.Phone.Number))).FirstOrDefault() != null)
                return BadRequest("Phone Number Is Already Exist");

            var whitelistRecord = new WhiteList()
            {
                CreatedBy = data.CreatedBy.MapTo<common.Models.Process>(),
                IpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? HttpContext.Connection.RemoteIpAddress.ToString(),
                Phone = data.Phone.MapTo<common.Models.Phone>()
            };

            await _repositoryManager.Whitelist.AddAsync(whitelistRecord);
            await _repositoryManager.SaveChangesAsync();
            
            return Created("", whitelistRecord.Id);
        }

        [SwaggerOperation(Summary = "Adds mail to whitelist",
            Description = "<div>Mail Address Has To Be Added To Whitelist To Receives E-Mail On Test Environment</div>",
            Tags = new[] { "Whitelist Management" })]
        [HttpPost("whitelist/email")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        [SwaggerResponse(400, "Phone Number Field Is Mandatory", typeof(void))]
        [SwaggerResponse(409, "Phone Number Is Already Exists In Whitelist", typeof(void))]
        public async Task<IActionResult> AddMailToWhitelist([FromBody] AddMailToWhitelistRequest data)
        {
            if (data.Email == null)
            {
                return BadRequest("Email Field Is Mandatory");
            }

            if ((await _repositoryManager.Whitelist.FindAsync(w => w.Mail == data.Email)).FirstOrDefault() != null)
                return BadRequest("Email Is Already Exist");

            var whitelistRecord = new WhiteList()
            {
                CreatedBy = data.CreatedBy.MapTo<common.Models.Process>(),
                IpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? HttpContext.Connection.RemoteIpAddress.ToString(),
                Mail = data.Email
            };

            await _repositoryManager.Whitelist.AddAsync(whitelistRecord);
            await _repositoryManager.SaveChangesAsync();

            return Created("", whitelistRecord.Id);
        }

        [SwaggerOperation(Summary = "Adds Citizenshipno to whitelist",
            Description = "<div>Citizenshipno Has To Be Added To Whitelist To Receives Pushs On Test Environment</div>",
            Tags = new[] { "Whitelist Management" })]
        [HttpPost("whitelist/push")]
        [SwaggerResponse(201, "Record was created successfully", typeof(void))]
        [SwaggerResponse(400, "Citizenshipno Field Is Mandatory", typeof(void))]
        [SwaggerResponse(409, "Citizenshipno Is Already Exists In Whitelist", typeof(void))]
        public async Task<IActionResult> AddCitizenshipnoToWhitelist([FromBody] AddCitizenshipnoToWhitelistRequest data)
        {
            if (data.CitizenshipNo == null)
            {
                return BadRequest("Citizenshipno Field Is Mandatory");
            }

            if ((await _repositoryManager.Whitelist.FindAsync(w => w.ContactId == data.CitizenshipNo)).FirstOrDefault() != null)
                return BadRequest("Citizenshipno Is Already Exist");

            var whitelistRecord = new WhiteList()
            {
                CreatedBy = data.CreatedBy.MapTo<common.Models.Process>(),
                IpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? HttpContext.Connection.RemoteIpAddress.ToString(),
                ContactId = data.CitizenshipNo
            };

            await _repositoryManager.Whitelist.AddAsync(whitelistRecord);
            await _repositoryManager.SaveChangesAsync();

            return Created("", whitelistRecord.Id);
        }

        [SwaggerOperation(Summary = "Deletes Phone Number From Whitelist configuration",
            Tags = new[] { "Whitelist Management" })]
        [HttpDelete("whitelist/phone")]
        [SwaggerResponse(200, "Whitelist record is deleted successfully", typeof(void))]
        public async Task<IActionResult> DeletePhoneFromWhitelist(common.Models.v2.Phone phone)
        {
            var recordsToDelete = await _repositoryManager.Whitelist.FindAsync(w => (w.Phone.CountryCode == phone.CountryCode)
              && (w.Phone.Prefix == phone.Prefix)
              && (w.Phone.Number == phone.Number));

            if(!recordsToDelete.Any())
                return BadRequest("There is no record for given phone number");

            foreach (WhiteList whitelist in recordsToDelete)
            {
                _repositoryManager.Whitelist.Remove(whitelist);
            }

            await _repositoryManager.SaveChangesAsync();
            return Ok();
        }


        [SwaggerOperation(Summary = "Deletes Mail Address From Whitelist configuration",
            Tags = new[] { "Whitelist Management" })]
        [HttpDelete("whitelist/mail")]
        [SwaggerResponse(200, "Whitelist record is deleted successfully", typeof(void))]
        public async Task<IActionResult> DeleteMailFromWhitelist(string Mail)
        {
            var recordsToDelete = await _repositoryManager.Whitelist.FindAsync(w => w.Mail == Mail);

            if (!recordsToDelete.Any())
                return BadRequest("There is no record for given mail address");

            foreach (WhiteList whitelist in recordsToDelete)
            {
                _repositoryManager.Whitelist.Remove(whitelist);
            }

            await _repositoryManager.SaveChangesAsync();
            return Ok();
        }

        [SwaggerOperation(Summary = "Deletes Citizenshipno From Whitelist configuration",
            Tags = new[] { "Whitelist Management" })]
        [HttpDelete("whitelist/push")]
        [SwaggerResponse(200, "Whitelist record is deleted successfully", typeof(void))]
        public async Task<IActionResult> DeletePushFromWhitelist(string CitizenshipNo)
        {
            var recordsToDelete = await _repositoryManager.Whitelist.FindAsync(w => w.ContactId == CitizenshipNo);

            if (!recordsToDelete.Any())
                return BadRequest("There is no record for given citizenshipNo");

            foreach (WhiteList whitelist in recordsToDelete)
            {
                _repositoryManager.Whitelist.Remove(whitelist);
            }

            await _repositoryManager.SaveChangesAsync();
            return Ok();
        }

        [SwaggerOperation(Summary = "Returns phone's whitelist status",
            Tags = new[] { "Whitelist Management" })]
        [HttpGet("whitelist/check/phone")]
        [SwaggerResponse(200, "Phone is in whitelist", typeof(void))]
        [SwaggerResponse(404, "Phone is not in whitelist", typeof(void))]
        public async Task<IActionResult> CheckPhone(int CountryCode,int Prefix,int Number)
        {
            if ((await _repositoryManager.Whitelist.FindAsync(w => (w.Phone.CountryCode == CountryCode)
               && (w.Phone.Prefix == Prefix)
               && (w.Phone.Number == Number))).FirstOrDefault() != null)
                return Ok();
            else
                return NotFound();
        }

        [SwaggerOperation(Summary = "Returns E-mail's whitelist status",
            Tags = new[] { "Whitelist Management" })]
        [HttpGet("whitelist/check/email")]
        [SwaggerResponse(200, "E-Mail is in whitelist", typeof(void))]
        [SwaggerResponse(404, "E-Mail is not in whitelist", typeof(void))]
        public async Task<IActionResult> CheckMail(string email)
        {
            if ((await _repositoryManager.Whitelist.FindAsync(w => w.Mail == email)).FirstOrDefault() != null)
                return Ok();
            else
                return NotFound();
        }

        [SwaggerOperation(Summary = "Returns CitizenshipNo's whitelist status",
            Tags = new[] { "Whitelist Management" })]
        [HttpGet("whitelist/check/push")]
        [SwaggerResponse(200, "CitizensipNo is in whitelist", typeof(void))]
        [SwaggerResponse(404, "CitizensipNo is not in whitelist", typeof(void))]
        public async Task<IActionResult> CheckPush(string CitizenshipNo)
        {
            if ((await _repositoryManager.Whitelist.FindAsync(w => w.ContactId == CitizenshipNo)).FirstOrDefault() != null)
                return Ok();
            else
                return NotFound();
        }

        [SwaggerOperation(Summary = "Returns phones otp sending logs",
            Tags = new[] { "Phone Management" })]
        [HttpGet("otp-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(OtpRequestLog[]))]
        public async Task<IActionResult> GetOtpLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            return Ok(await _repositoryManager.OtpRequestLogs
                .GetWithResponseLogsAsync(countryCode, prefix, number, page, pageSize));
        }

        [SwaggerOperation(Summary = "Returns phones sms sending logs",
            Tags = new[] { "Phone Management" })]
        [HttpGet("sms-log/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(SmsResponseLog[]))]
        public async Task<IActionResult> GetSmsLog(int countryCode, int prefix, int number, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {

            return Ok((await _repositoryManager.SmsRequestLogs
                .FindAsync(c => c.PhoneConfiguration.Phone.CountryCode == countryCode && c.PhoneConfiguration.Phone.Prefix == prefix && c.PhoneConfiguration.Phone.Number == number))
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray());
        }

        [SwaggerOperation(Summary = "Returns Generated Template Message Associated With Transaction",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transaction/{txnId}/generatedMessage")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(GeneratedMessage))]

        public async Task<IActionResult> GetPhoneBlacklistRecords(Guid txnId)
        {
            var transaction = await _repositoryManager.Transactions.GetWithIdAsNoTrackingAsync(txnId);
            if (transaction == null)
                return NotFound();

            if (transaction.TransactionType != TransactionType.TransactionalTemplatedSms &&
               transaction.TransactionType != TransactionType.TransactionalTemplatedMail &&
               transaction.TransactionType != TransactionType.TransactionalTemplatedPush)
                return BadRequest("Transaction Type is Not Templated");

            if (transaction.TransactionType == TransactionType.TransactionalTemplatedSms)
                return Ok(new GeneratedMessage { Content = transaction.SmsRequestLog.content });

            if (transaction.TransactionType == TransactionType.TransactionalTemplatedMail)
                return Ok(new GeneratedMessage { Content = transaction.MailRequestLog.content });

            if (transaction.TransactionType == TransactionType.TransactionalTemplatedPush)
                return Ok(new GeneratedMessage { Content = transaction.PushNotificationRequestLog.Content });

            return BadRequest();
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/phone/{countryCode}/{prefix}/{number}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public async Task<IActionResult> GetTransactionsWithPhone(int countryCode, int prefix, int number, int smsType,DateTime startDate,DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (smsType == 1)
            {
                var res = await _repositoryManager.Transactions.GetOtpMessagesWithPhoneAsync(countryCode, prefix, number, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (smsType == 2)
            {
                var res = await _repositoryManager.Transactions.GetTransactionalSmsMessagesWithPhoneAsync(countryCode, prefix, number, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new TransactionsDto());
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/mail/{email}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public async Task<IActionResult> GetTransactionsWithEmail(string mail, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            var res = await _repositoryManager.Transactions.GetMailMessagesWithMailAsync(mail, startDate, endDate, page, pageSize);
            return Ok(new TransactionsDto { Transactions = res.Item1 ,Count = res.Item2});
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/customer/{customerNo}/{messageType}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public async Task<IActionResult> GetTransactionsWithCustomerNo(ulong customerNo,int messageType,int smsType, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (messageType == 1)
            {
                if (smsType == 1)
                {
                    var res = await _repositoryManager.Transactions.GetOtpMessagesWithCustomerNoAsync(customerNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1,Count = res.Item2});
                }
                if (smsType == 2)
                {
                    var res = await _repositoryManager.Transactions.GetTransactionalSmsMessagesWithCustomerNoAsync(customerNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                    
                return Ok(new TransactionsDto());
            }
            if (messageType == 2)
            {
                var res = await _repositoryManager.Transactions.GetMailMessagesWithCustomerNoAsync(customerNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (messageType == 3)
            {
                var res = await _repositoryManager.Transactions.GetPushMessagesWithCustomerNoAsync(customerNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new TransactionsDto());
        }

        [SwaggerOperation(Summary = "Returns transactions info",
            Tags = new[] { "Transaction Management" })]
        [HttpGet("transactions/citizen/{citizenshipNo}/{messageType}")]
        [SwaggerResponse(200, "Records was returned successfully", typeof(Transaction[]))]
        public async Task<IActionResult> GetTransactionsWithCitizenshipNo(string citizenshipNo, int messageType, int smsType, DateTime startDate, DateTime endDate, [Range(0, 100)] int page = 0, [Range(1, 100)] int pageSize = 20)
        {
            if (messageType == 1)
            {
                if (smsType == 1)
                {
                    var res = await _repositoryManager.Transactions.GetOtpMessagesWithCitizenshipNoAsync(citizenshipNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                    
                if (smsType == 2)
                {
                    var res = await _repositoryManager.Transactions.GetTransactionalSmsMessagesWithCitizenshipNoAsync(citizenshipNo, startDate, endDate, page, pageSize);
                    return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
                }
                return Ok(new TransactionsDto());
            }
            if (messageType == 2)
            {
                var res = await _repositoryManager.Transactions.GetMailMessagesWithCitizenshipNoAsync(citizenshipNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }
            if (messageType == 3)
            {
                var res = await _repositoryManager.Transactions.GetPushMessagesWithCitizenshipNoAsync(citizenshipNo, startDate, endDate, page, pageSize);
                return Ok(new TransactionsDto { Transactions = res.Item1, Count = res.Item2 });
            }

            return Ok(new List<Transaction>());
        }

        [SwaggerOperation(Summary = "Returns report for CreditReport",
            Tags = new[] { "Report Management" })]
        [HttpPost("CreditReport")]
        [SwaggerResponse(200, "Report is returned successfully", typeof(FileContentResult))]
        public async Task<FileContentResult> GetReport(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            string fileContent = await reader.ReadToEndAsync();
            using var stringReader = new StringReader(fileContent);
            using var stringReader2 = new StringReader(fileContent);
            var reportDate = await stringReader.ReadLineAsync();
            reportDate = reportDate.Trim();

            Dictionary<string, int> repeatedLinesOrder = new();

            string resultContent = String.Empty;
            resultContent += reportDate + "\r\n";
            
            string? line = await stringReader.ReadLineAsync();
            while (line != null)
            {
                var lineArray = line.Trim().Split("|");
                if (repeatedLinesOrder.ContainsKey(GetKeyName(lineArray)))
                {
                    repeatedLinesOrder[GetKeyName(lineArray)]++;
                }
                else
                {
                    repeatedLinesOrder.Add(
                            GetKeyName(lineArray),
                            0);
                }
                line = await stringReader.ReadLineAsync();
            }

            await stringReader2.ReadLineAsync();
            line = await stringReader2.ReadLineAsync();
            while (line != null)
            {
                var lineArray = line.Trim().Split("|");

                DateTime dt = DateTime.Now;
                var transactions = await _repositoryManager.Transactions.GetReportTransaction(
                    Convert.ToInt32(lineArray[1].Substring(5)),
                    reportDate,
                    lineArray[2]
                );
                Console.WriteLine("Sql Execution Time : " + (DateTime.Now - dt).TotalMilliseconds);
                
                if (transactions == null || transactions?.Count() == 0)
                {
                    resultContent += line+"|Rapor Bulunamadı\r\n";
                }
                if (transactions?.Count() == 1)
                {
                    var smsResponseLog = GetSmsResponseLog(transactions.FirstOrDefault());
                    
                    resultContent += line+
                        await GetReportLine(
                            GetSmsResponseLog(transactions.FirstOrDefault()),
                            transactions.FirstOrDefault().SmsRequestLog);
                }
                if (transactions?.Count() > 1)
                {
                    resultContent += line +
                        await GetReportLine(
                            GetSmsResponseLog(
                                transactions.OrderByDescending(t => t.CreatedAt).ElementAt(repeatedLinesOrder[GetKeyName(lineArray)]--)),
                                transactions.FirstOrDefault().SmsRequestLog);
                }

                line = await stringReader2.ReadLineAsync();
            }


            return File(Encoding.UTF8.GetBytes(resultContent), "application/octet-stream","rapor.csv");
        }

        private SmsResponseLog? GetSmsResponseLog(Transaction transaction)
        {
            if (transaction.SmsRequestLog == null)
                return null;
            if (transaction.SmsRequestLog.ResponseLogs == null)
                return null;
            if (transaction.SmsRequestLog.ResponseLogs.Count() == 0)
                return null;
            return transaction.SmsRequestLog.ResponseLogs.FirstOrDefault();
        }

        private string GetKeyName(string[] array)
        {
            return $"{array[1].Trim()}_{array[2].Trim()}".Trim();
        }

        private async Task<string> GetReportLine(SmsResponseLog smsResponseLog,SmsRequestLog smsRequestLog)
        {
            if (smsResponseLog != null)
            {
                Console.WriteLine($"CreatedAt : {smsResponseLog.CreatedAt} | StatusQueryId : {smsResponseLog.StatusQueryId}");
                SmsTrackingLog? trackingLog  = null;
                if (smsResponseLog.Operator == OperatorType.Codec)
                {
                    trackingLog = await _codecSender.CheckSms(new CheckFastSmsRequest
                    {
                        Operator = smsResponseLog.Operator,
                        SmsRequestLogId = smsRequestLog.Id,
                        StatusQueryId = smsResponseLog.StatusQueryId
                    });
                }
                if (smsResponseLog.Operator == OperatorType.dEngageOn ||
                    smsResponseLog.Operator == OperatorType.dEngageBurgan)
                {
                    trackingLog = await _dEngageSender.CheckSms(new CheckFastSmsRequest
                    {
                        Operator = smsResponseLog.Operator,
                        SmsRequestLogId = smsRequestLog.Id,
                        StatusQueryId = smsResponseLog.StatusQueryId
                    });
                }

                if (trackingLog != null)
                {
                        return $"|{smsResponseLog.CreatedAt.ToString(new System.Globalization.CultureInfo("tr-TR"))}|{trackingLog.Status}|{trackingLog.StatusReason}\r\n";

                }
                
                return "|Rapor Bulunamadı\r\n";
            }

            return "|Rapor Bulunamadı\r\n";
            
        }

    }
}
