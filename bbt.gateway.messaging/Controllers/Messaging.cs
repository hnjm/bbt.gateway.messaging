using bbt.gateway.common.Models;
using bbt.gateway.messaging.Workers;
using bbt.gateway.messaging.Workers.OperatorGateway;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class Messaging : ControllerBase
    {
        private readonly OtpSender _otpSender;
        private readonly ITransactionManager _transactionManager;
        private readonly dEngageSender _dEngageSender;
        public Messaging(OtpSender otpSender,ITransactionManager transactionManager, dEngageSender dEngageSender)
        {
            _transactionManager = transactionManager;
            _otpSender = otpSender;
            _dEngageSender = dEngageSender;
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
                await _transactionManager.GetCustomerInfoByPhone(data.Phone);
                if(data.ContentType == MessageContentType.Otp)
                {
                    _transactionManager.Phone = data.Phone;
                    _transactionManager.TransactionType = TransactionType.Otp;
                    _transactionManager.LogState();
                    var res = await _otpSender.SendMessage(data);
                    return Ok(res);
                }
                if (data.ContentType == MessageContentType.Private)
                {
                    //await _operatordEngage.SendSms(data.Phone, SmsTypes.Fast, data.Content, null, null);
                }
                return Ok();
            }
            else 
            {
                _transactionManager.LogError("Model State is Not Valid | " + 
                    string.Join("|",ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
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
        [HttpPost("messaging/email/templated")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(SendEmailResponse))]
        [SwaggerResponse(460, "Given template is not found on dEngage", typeof(void))]
        public async Task<IActionResult> SendTemplatedEmail([FromBody] SendTemplatedEmailRequest data)
        {
            //var response = await _dEngageSender.SendTemplatedMail(data);
            return Ok();
        }


        [SwaggerOperation(
           Summary = "Send Email message",
           Description = "Send given content directly."
           )]
        [HttpPost("messaging/email/message")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(SendEmailResponse))]
        public async Task<IActionResult> SendMessageEmail([FromBody] SendMessageEmailRequest data)
        {
            //await _operatordEngage.SendMail(data.Email,data.Subject,data.Content,null,null);
            return Ok();
        }

        [SwaggerOperation(
           Summary = "Send Push Notification",
           Description = "Send push notification to device."
           )]
        [HttpPost("messaging/push-notification")]
        [SwaggerResponse(200, "Push notification was sent successfully", typeof(SendPushNotificationResponse))]
        public IActionResult SendPushNotification([FromBody] SendMessagePushNotificationRequest data)
        {
            return Ok();
        }

        [SwaggerOperation(
           Summary = "Send Templated Push Notification",
           Description = "Send templated push notification to device."
           )]
        [HttpPost("messaging/push-notification/templated")]
        [SwaggerResponse(200, "Push notification was sent successfully", typeof(SendPushNotificationResponse))]
        public IActionResult SendTemplatedPushNotification([FromBody] SendTemplatedPushNotificationRequest data)
        {
            return Ok();
        }

    }
}
