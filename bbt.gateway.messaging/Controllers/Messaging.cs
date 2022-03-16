using bbt.gateway.common.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class Messaging : ControllerBase
    {
        private readonly ILogger<Messaging> _logger;
        private readonly OtpSender _otpSender;

        public Messaging(ILogger<Messaging> logger,OtpSender otpSender)
        {
            _logger = logger;
            _otpSender = otpSender;
        }

        
        [SwaggerOperation(
            Summary = "Send templated Sms message",
            Description = "Templates are defined in dEngage"
            )]
        [HttpPost("sms/templated") ]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(SendSmsResponse))]
        [SwaggerResponse(460, "Given template is not found on dEngage", typeof(void))]
        [SwaggerResponse(465, "Sim card is changed.", typeof(void))]
        [SwaggerResponse(466, "Operator is changed.", typeof(void))]

        public IActionResult SendTemplatedSms([FromBody] SendTemplatedSmsRequest data)
        {
            return Ok();
        }


        [SwaggerOperation(
           Summary = "Send Sms message",
           Description = "Send given content directly."
           )]
        [HttpPost("sms/message")]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(SendSmsResponse))]
        [SwaggerResponse(465, "Sim card is changed.", typeof(void))]
        [SwaggerResponse(466, "Operator is changed.", typeof(void))]
        [SwaggerResponse(460, "Has Blacklist Record.", typeof(void))]
        public async Task<IActionResult> SendMessageSms([FromBody] SendMessageSmsRequest data)
        {
            
            if (ModelState.IsValid)
            {
                if(data.ContentType == MessageContentType.Otp)
                {
                    var res = await _otpSender.SendMessage(data);
                    return Ok(res);
                }
                return Ok();
            }
            else 
            {
                return BadRequest(ModelState);
            }
            

        }

        [SwaggerOperation(
           Summary = "Check Sms Message Status",
           Description = "Check Otp Sms Delivery Status."
           )]
        [HttpPost("sms/check-message")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> CheckMessageStatus([FromBody] CheckSmsRequest data)
        {

            if (ModelState.IsValid)
            {
                var res = await _otpSender.CheckMessage(data);
                return Ok(res);
            }
            else
            {
                return BadRequest(ModelState);
            }


        }

        [SwaggerOperation(
           Summary = "Send templated Email message",
           Description = "Templates are defined in dEngage"
           )]
        [HttpPost("/messaging/email/templated")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(SendEmailResponse))]
        [SwaggerResponse(460, "Given template is not found on dEngage", typeof(void))]
        public IActionResult SendTemplatedEmail([FromBody] SendTemplatedEmailRequest data)
        {
            return Ok();
        }


        [SwaggerOperation(
           Summary = "Send Email message",
           Description = "Send given content directly."
           )]
        [HttpPost("/messaging/email/message")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(SendEmailResponse))]
        public IActionResult SendMessageEmail([FromBody] SendMessageEmailRequest data)
        {
            return Ok();
        }

        //[SwaggerOperation(
        //   Summary = "Send Push Notification",
        //   Description = "Send push notification to device."
        //   )]
        //[HttpPost("/messaging/push-notification")]
        //[SwaggerResponse(200, "Push notification was sent successfully", typeof(SendEmailResponse))]
        //public IActionResult SendPushNotification([FromBody] SendPushNotification data)
        //{
        //    return Ok();
        //}

    }
}
