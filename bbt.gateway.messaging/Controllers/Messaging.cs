using bbt.gateway.messaging.Models;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [HttpPost("/messaging/sms/templated") ]
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
        [HttpPost("/messaging/sms/message")]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(SendSmsResponse))]
        [SwaggerResponse(465, "Sim card is changed.", typeof(void))]
        [SwaggerResponse(466, "Operator is changed.", typeof(void))]
        public async Task<IActionResult> SendMessageSms([FromBody] SendMessageSmsRequest data)
        {
 
            var res = await _otpSender.SendMessage(data);
            return Ok(res);

        }

        /* For Future development

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
        */

       
     
    }
}
