using bbt.gateway.common.Models.v2;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace bbt.gateway.messaging.Controllers.v2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class Messaging : ControllerBase
    {
        private readonly OtpSender _otpSender;
        private readonly ITransactionManager _transactionManager;
        private readonly dEngageSender _dEngageSender;
        public Messaging(OtpSender otpSender, ITransactionManager transactionManager, dEngageSender dEngageSender)
        {
            _transactionManager = transactionManager;
            _otpSender = otpSender;
            _dEngageSender = dEngageSender;
        }

        [SwaggerOperation(
            Summary = "Send Templated Sms Message",
            Description = ""
            + "<div>To Send Sms With Template Which Defined On dEngage Use This Method</div>"
            + "<div>Sender,Template,Phone,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Must Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>Given Template Must Be Defined On Both Of dEngage Tenants(On And Burgan) With Same Content Name</div>"
            + "<div>Template Params Must Be Set As JsonString Which Serialized From Dynamic Fields That Given Template Contains"
            + "<br />Example : Let's Assume template content is 'Welcome {%=$Current.Name%} {%=$Current.Surname%}.' "
            + "<br />In This Case Template Params Must Be Set To {\"Name\":\"Actual Name\",\"Surname\":\"Actual Surname\"}</div>"
            + "<div>TemplateParams Field Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : {\"Password\",\"&lt;Mask&gt;123456&lt;/Mask&gt;\"}</div>"
            + "",
            Tags = new[] { "Sms" }
            )]
        [HttpPost("sms/templated")]
        [SwaggerRequestExample(typeof(TemplatedSmsRequest), typeof(TemplatedSmsRequestExampleFilter))]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(404, "Not Found", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(TemplatedSmsResponse))]
        [SwaggerResponse(450, "Given template is not found on dEngage", typeof(void))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]
        public async Task<IActionResult> SendTemplatedSms([FromBody] TemplatedSmsRequest data)
        {
            await Task.CompletedTask;
            return Ok();
        }

        [SwaggerOperation(
           Summary = "Send Sms message",
           Description = ""
            + "<div>To Send Sms With Plain Text Use This Method</div>"
            + "<div>Sender,SmsType,Content,Phone,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Should Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>You can use advantages of headers by creating header from Header Management Services"
            + "<br /> When message services are called, we get customer info(BusinessLine,BranchCode)"
            + "<br /> Then try to find header matches with (BusinessLine[BL],BranchCode[BC],SmsType[ST])"
            + "<br /> Match Order by priority is (BL-BC-ST)-(BC-ST)-(BL-ST)-(ST)"
            + "<br /> If any header is matches, we add matched header prefix and suffix to beginning of message and end of the message</div>"
            + "<div>Content Field Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : \"Content\" : \"Your password is &lt;Mask&gt;1000&lt;/Mask&gt;.\"</div>"
            + "",
           Tags = new[] { "Sms" }
           )]
        [HttpPost("sms/message")]
        //[SwaggerRequestExample(typeof(TemplatedSmsRequest), typeof(TemplatedSmsRequestExampleFilter))]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(SmsResponse))]
        [SwaggerResponse(200, "Sms was sent successfully", typeof(OtpResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(SmsResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(SmsResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(SmsResponse))]
        [SwaggerResponse(404, "Not Found", typeof(SmsResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(SmsResponse))]
        [SwaggerResponse(450, "Given template is not found on dEngage", typeof(void))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(460, "Has Blacklist Record", typeof(OtpResponse))]
        [SwaggerResponse(461, "Sim Change", typeof(OtpResponse))]
        [SwaggerResponse(462, "Operator Change", typeof(OtpResponse))]
        [SwaggerResponse(463, "Rejected By Operator", typeof(OtpResponse))]
        [SwaggerResponse(464, "Not Subscriber", typeof(OtpResponse))]
        [SwaggerResponse(465, "Client Error", typeof(OtpResponse))]
        [SwaggerResponse(466, "Server Error", typeof(OtpResponse))]
        [SwaggerResponse(467, "Maximum Characters Count Exceed", typeof(OtpResponse))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]

        public async Task<IActionResult> SendMessageSms([FromBody] SmsRequest data)
        {
            await Task.CompletedTask;
            return null;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Mock")
            {
                return Ok(new SmsResponse()
                {
                    Status = dEngageResponseCodes.Success,
                    TxnId = Guid.NewGuid(),
                });
            }

            if (data.Phone == null)
            {
                data.Phone = new Phone
                {
                    CountryCode = _transactionManager.CustomerRequestInfo.MainPhone.CountryCode,
                    Prefix = _transactionManager.CustomerRequestInfo.MainPhone.Prefix,
                    Number = _transactionManager.CustomerRequestInfo.MainPhone.Number,
                };
            }

            if (ModelState.IsValid)
            {
                if (data.SmsType == SmsTypes.Otp)
                {
                    //return Ok(await _otpSender.SendMessage(data));
                }
                else
                {
                    //return Ok(await _dEngageSender.SendSms(data));
                }
            }
            else
            {
                _transactionManager.LogError("Model State is Not Valid | " +
                    string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }


        }

        [SwaggerOperation(
           Summary = "Send templated Email message",
           Description = ""
            + "<div>To Send E-Mail With Template Which Defined On dEngage Use This Method</div>"
            + "<div>Sender,Template,E-Mail,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Must Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>Given Template Must Be Defined On Both Of dEngage Tenants(On And Burgan) With Same Content Name</div>"
            + "<div>Template Params Must Be Set As JsonString Which Serialized From Dynamic Fields That Given Template Contains"
            + "<br />Example : Let's Assume template content is 'Welcome {%=$Current.Name%} {%=$Current.Surname%}.' "
            + "<br />In This Case Template Params Must Be Set To {\"Name\":\"Actual Name\",\"Surname\":\"Actual Surname\"}</div>"
            + "<div>TemplateParams Field Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : {\"Password\",\"&lt;Mask&gt;123456&lt;/Mask&gt;\"}</div>"
            + "<div>If You Want Send Attachments With Mail You Can Use Attachments Field"
            + "<br />Attachments.Name must be set Filename and Attachment.Data must be set to Base64 String Encoded From File Byte Array</div>"
            + "",
           Tags = new[] { "E-Mail" }
           )]
        [HttpPost("email/templated")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(TemplatedMailResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(TemplatedMailResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(TemplatedMailResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(TemplatedMailResponse))]
        [SwaggerResponse(404, "Not Found", typeof(TemplatedMailResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(TemplatedMailResponse))]
        [SwaggerResponse(450, "Given template is not found on dEngage", typeof(void))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]
        public async Task<IActionResult> SendTemplatedEmail([FromBody] TemplatedMailRequest data)
        {
            await Task.CompletedTask;
            if (data.Email == null)
            {
                data.Email = _transactionManager.CustomerRequestInfo.MainEmail;
            }
            //var response = await _dEngageSender.SendTemplatedMail(data);
            return Ok();
        }


        [SwaggerOperation(
           Summary = "Send Email message",
           Description = ""
            + "<div>To Send E-Mail With Plain Text or Html Use This Method</div>"
            + "<div>Sender,From,Subject,Content,Email,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Should Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>From Field Must Be Contains Just FromName Not Domain."
            + "<br/> System Will Decide Domain Part Depends On Customer Type" 
            + "Example : (noreply | Correct Usage) - (noreply@burgan.com.tr | Wrong Usage) </div>"
            + "<div>Content and Subject Fields Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : \"Content\" : \"Your password is &lt;Mask&gt;1000&lt;/Mask&gt;.\"</div>"
            + "",
           Tags = new[] { "E-Mail" }
           )]
        [HttpPost("email/message")]
        [SwaggerResponse(200, "Email was sent successfully", typeof(MailResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(MailResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(MailResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(MailResponse))]
        [SwaggerResponse(404, "Not Found", typeof(MailResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(MailResponse))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]
        public async Task<IActionResult> SendMessageEmail([FromBody] MailRequest data)
        {
            await Task.CompletedTask;
            return null;
            if (data.Email == null)
            {
                data.Email = _transactionManager.CustomerRequestInfo.MainEmail;
            }
            //var response = await _dEngageSender.SendMail(data);
            //return Ok(response);
        }

        [SwaggerOperation(
           Summary = "Send Push Notification",
           Description = ""
            + "<div>To Send Push Notification With Plain Text Use This Method</div>"
            + "<div>Sender,Content,CitizenshipNo,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Should Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>Content Field Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : \"Content\" : \"Your password is &lt;Mask&gt;1000&lt;/Mask&gt;.\"</div>"
            + "",
            Tags = new[] {"Push Notification"}
           )]
        [HttpPost("push-notification/message")]
        [SwaggerResponse(200, "Push notification was sent successfully", typeof(PushResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(PushResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(PushResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(PushResponse))]
        [SwaggerResponse(404, "Not Found", typeof(PushResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(PushResponse))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]
        public async Task<IActionResult> SendPushNotification([FromBody] PushRequest data)
        {
            await Task.CompletedTask;
            return null;
            //var response = await _dEngageSender.SendPushNotification(data);
            //return Ok(response);
        }

        [SwaggerOperation(
           Summary = "Send Templated Push Notification",
           Description = ""
            + "<div>To Send Push Notification With Template Which Defined On dEngage Use This Method</div>"
            + "<div>Sender,Template,CitizenshipNo,Process Fields are Mandatory</div>"
            + "<div>When Customer Type(Burgan/On) is Not Known, Sender Field Must Be Set To AutoDetect"
            + " <br />Otherwise This Field Must Be Set Burgan or On</div>"
            + "<div>Given Template Must Be Defined On Both Of dEngage Tenants(On And Burgan) With Same Content Name</div>"
            + "<div>Template Params Must Be Set As JsonString Which Serialized From Dynamic Fields That Given Template Contains"
            + "<br />Example : Let's Assume template content is 'Welcome {%=$Current.Name%} {%=$Current.Surname%}.' "
            + "<br />In This Case Template Params Must Be Set To {\"Name\":\"Actual Name\",\"Surname\":\"Actual Surname\"}</div>"
            + "<div>TemplateParams Field Will Be Logged After This Method Called. If You Need To Masking Critical Information You Should Surround"
            + " Critical Information with &lt;Mask&gt;&lt;/Mask&gt; . "
            + "<br />Example : {\"Password\",\"&lt;Mask&gt;123456&lt;/Mask&gt;\"}</div>"
            + "",
           Tags = new[] { "Push Notification" }
           )]
        [HttpPost("push-notification/templated")]
        [SwaggerResponse(200, "Push notication was sent successfully", typeof(TemplatedPushResponse))]
        [SwaggerResponse(400, "Bad Request", typeof(TemplatedPushResponse))]
        [SwaggerResponse(401, "Unauthorized", typeof(TemplatedPushResponse))]
        [SwaggerResponse(403, "Not Allowed", typeof(TemplatedPushResponse))]
        [SwaggerResponse(404, "Not Found", typeof(TemplatedPushResponse))]
        [SwaggerResponse(429, "Too Many Request", typeof(TemplatedPushResponse))]
        [SwaggerResponse(450, "Given template is not found on dEngage", typeof(void))]
        [SwaggerResponse(451, "Customer Not Found.", typeof(void))]
        [SwaggerResponse(500, "Internal Server Error. Get Contact With Integration", typeof(void))]

        public async Task<IActionResult> SendTemplatedPushNotification([FromBody] TemplatedPushRequest data)
        {
            await Task.CompletedTask;
            return null;
            //var response = await _dEngageSender.SendTemplatedPushNotification(data);
            //return Ok(response);
        }

    }
}
