using bbt.gateway.common.Models;
using Microsoft.AspNetCore.Components;

namespace bbt.gateway.messaging.ui.Pages
{
    public partial class MessageDetails : ComponentBase
    {
        [Parameter] public Transaction Txn { get; set; }

        private ICollection<OtpResponseLog> responseLogs;
        private ICollection<OtpTrackingLog> trackingsLogs;
        private string message { get; set; }
        private string template { get; set; }
        private string templateParams { get; set; }

        protected override void OnInitialized()
        {
            message = Txn.OtpRequestLog == null ? "" : (Txn.OtpRequestLog.Content ?? "");  
            responseLogs = GetOtpResponseLogs();
            trackingsLogs = GetOtpTrackingLogs();
        }

        private string GetTemplate()
        {
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                if (Txn.SmsRequestLog == null)
                    return "";
                return Txn.SmsRequestLog.TemplateId ?? "";
            }
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                if (Txn.MailRequestLog == null)
                    return "";
                return Txn.MailRequestLog.TemplateId ?? "";
            }
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedPush)
            {
                if (Txn.PushNotificationRequestLog == null)
                    return "";
                return Txn.PushNotificationRequestLog.TemplateId ?? "";
            }
            return "";
        }

        private string GetTemplateParams()
        {
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                if (Txn.SmsRequestLog == null)
                    return "";
                return Txn.SmsRequestLog.TemplateParams ?? "";
            }
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                if (Txn.MailRequestLog == null)
                    return "";
                return Txn.MailRequestLog.TemplateParams ?? "";
            }
            if (Txn.TransactionType == TransactionType.TransactionalTemplatedPush)
            {
                if (Txn.PushNotificationRequestLog == null)
                    return "";
                return Txn.PushNotificationRequestLog.TemplateParams ?? "";
            }
            return "";
        }

        private ICollection<OtpResponseLog> GetOtpResponseLogs()
        {
            if (Txn.OtpRequestLog == null)
                return new List<OtpResponseLog>();
            return Txn.OtpRequestLog.ResponseLogs ?? new List<OtpResponseLog>();
        }

        private ICollection<OtpTrackingLog> GetOtpTrackingLogs()
        {
            if (Txn.OtpRequestLog == null)
                return new List<OtpTrackingLog>();
            if (Txn.OtpRequestLog.ResponseLogs == null)
                return new List<OtpTrackingLog>();

            if (Txn.OtpRequestLog.ResponseLogs.Count == 1)
            {
                return Txn.OtpRequestLog.ResponseLogs.First().TrackingLogs ?? new List<OtpTrackingLog>();
            }
            else
            {
                if (Txn.OtpRequestLog.ResponseLogs.FirstOrDefault(l => l.ResponseCode == SendSmsResponseStatus.Success) == null)
                { 
                    return new List<OtpTrackingLog>();
                }

                return Txn.OtpRequestLog.ResponseLogs.FirstOrDefault(l => l.ResponseCode == SendSmsResponseStatus.Success).TrackingLogs ?? new List<OtpTrackingLog>();
            }
        }

        private string GetOperator()
        {
            if (Txn.TransactionType == TransactionType.Otp)
            {
                if (Txn.OtpRequestLog != null)
                {
                    return Txn.OtpRequestLog.PhoneConfiguration.Operator.ToString() ?? "unknown";
                }
            }
            if (Txn.TransactionType == TransactionType.TransactionalMail || Txn.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                if (Txn.MailRequestLog != null)
                {
                    return Txn.MailRequestLog.Operator.ToString();
                }
            }
            if (Txn.TransactionType == TransactionType.TransactionalSms || Txn.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                if (Txn.SmsRequestLog != null)
                {
                    return Txn.SmsRequestLog.Operator.ToString();
                }
            }
            if (Txn.TransactionType == TransactionType.TransactionalPush || Txn.TransactionType == TransactionType.TransactionalTemplatedPush)
            {
                if (Txn.PushNotificationRequestLog != null)
                {
                    return Txn.PushNotificationRequestLog.Operator.ToString();
                }
            }

            return "unknown";
        }
    }
}
