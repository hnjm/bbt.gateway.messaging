using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Middlewares
{
    public class CustomerInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private MiddlewareRequest _middlewareRequest;
        private ITransactionManager _transactionManager;
        private IRepositoryManager _repositoryManager;
        private Transaction _transaction;
        private RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        
        public CustomerInfoMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new();
        }

        public async Task InvokeAsync(HttpContext context,ITransactionManager transactionManager,IRepositoryManager repositoryManager)
        {
            _transactionManager = transactionManager;
            _repositoryManager = repositoryManager;
            
            try
            {
                context.Request.EnableBuffering();

                var ipAdress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress.ToString();

                _transactionManager.Ip = ipAdress;

                await using var requestStream = _recyclableMemoryStreamManager.GetStream();

                await context.Request.Body.CopyToAsync(requestStream);
                var body = ReadStreamInChunks(requestStream);

                // Reset the request body stream position so the next middleware can read it
                context.Request.Body.Position = 0;

                _transaction = new Transaction()
                {
                    Id = _transactionManager.TxnId,
                    Request = body,
                    IpAdress = _transactionManager.Ip,
                };
                _repositoryManager.Transactions.Add(_transaction);
                _repositoryManager.SaveChanges();

                _middlewareRequest = JsonConvert.DeserializeObject<MiddlewareRequest>(body);

                _transaction.CreatedBy = _middlewareRequest.Process;
                _transaction.Mail = _middlewareRequest.Email;
                _transaction.Phone = _middlewareRequest.Phone;
        
                _repositoryManager.SaveChanges();
                
                SetTransaction(context);

                await GetCustomerDetail();

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Prod")
                {
                    CheckWhitelist();
                }

                var originalStream = context.Response.Body;
                await using var responseBody = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBody;

                // Call the next delegate/middleware in the pipeline.
                await _next(context);
                _transactionManager.LogState();
                
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalStream);

                _transaction.TransactionType = _transactionManager.TransactionType;
                _transaction.OtpRequestLog = _transactionManager.OtpRequestLog;
                _transaction.SmsRequestLog = _transactionManager.SmsRequestLog;
                _transaction.MailRequestLog = _transactionManager.MailRequestLog;
                if (_transactionManager.TransactionType == TransactionType.Otp)
                {
                    _transaction.Response = response.MaskOtpContent();
                }
                else
                {
                    _transaction.Response = response.MaskFields();
                }
                

                _repositoryManager.SaveChanges();
            }
            catch (FormatException ex)
            {
                _transaction.TransactionType = _transactionManager.TransactionType;
                _transaction.OtpRequestLog = _transactionManager.OtpRequestLog;
                _transaction.SmsRequestLog = _transactionManager.SmsRequestLog;
                _transaction.MailRequestLog = _transactionManager.MailRequestLog;
                _transaction.PushNotificationRequestLog = _transactionManager.PushNotificationRequestLog;
                _transaction.Response = "Customer No Should Be Valid Long Value | Detail :" + ex.ToString();
                _repositoryManager.SaveChanges();

                _transactionManager.LogState();
                _transactionManager.LogError("Customer No Should Be Valid Long Value | Detail :" + ex.ToString());

                context.Response.StatusCode = 500;
            }
            catch (BadHttpRequestException ex)
            {
                _transaction.TransactionType = _transactionManager.TransactionType;
                _transaction.OtpRequestLog = _transactionManager.OtpRequestLog;
                _transaction.SmsRequestLog = _transactionManager.SmsRequestLog;
                _transaction.MailRequestLog = _transactionManager.MailRequestLog;
                _transaction.PushNotificationRequestLog = _transactionManager.PushNotificationRequestLog;
                _transaction.Response = "An Error Occured | Detail :" + ex.ToString();
                _repositoryManager.SaveChanges();

                _transactionManager.LogState();
                _transactionManager.LogError("An Error Occured | Detail :" + ex.ToString());

                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                _transaction.TransactionType = _transactionManager.TransactionType;
                _transaction.OtpRequestLog = _transactionManager.OtpRequestLog;
                _transaction.SmsRequestLog = _transactionManager.SmsRequestLog;
                _transaction.MailRequestLog = _transactionManager.MailRequestLog;
                _transaction.PushNotificationRequestLog = _transactionManager.PushNotificationRequestLog;
                _transaction.Response = "An Error Occured | Detail :" + ex.ToString();
                _repositoryManager.SaveChanges();

                _transactionManager.LogState();
                _transactionManager.LogError("An Error Occured | Detail :"+ex.ToString());

                context.Response.StatusCode = 500;
            }
        }

        private void CheckWhitelist()
        {
            if (_transactionManager.TransactionType == TransactionType.Otp)
            {
                if (_repositoryManager.Whitelist.Find(w => 
                (w.Phone.CountryCode == _middlewareRequest.Phone.CountryCode
                && w.Phone.Prefix == _middlewareRequest.Phone.Prefix
                && w.Phone.Number == _middlewareRequest.Phone.Number
                )).FirstOrDefault() == null)
                {
                    throw new BadHttpRequestException("İletişime geçmek istediğiniz numaranın non-prod ortamlarda gönderim izni yoktur. " +
                        "/Whitelist endpoint ile whitelist tablosuna eklemeniz gerekmektedir.",403);
                }
            }

            if (_transactionManager.TransactionType == TransactionType.TransactionalSms ||
                _transactionManager.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                if (_repositoryManager.Whitelist.Find(w =>
                (w.Phone.CountryCode == _middlewareRequest.Phone.CountryCode
                && w.Phone.Prefix == _middlewareRequest.Phone.Prefix
                && w.Phone.Number == _middlewareRequest.Phone.Number
                )).FirstOrDefault() == null)
                {
                    throw new BadHttpRequestException("İletişime geçmek istediğiniz numaranın non-prod ortamlarda gönderim izni yoktur. " +
                        "/Whitelist endpoint ile whitelist tablosuna eklemeniz gerekmektedir.", 403);
                }
            }

            if (_transactionManager.TransactionType == TransactionType.TransactionalMail ||
                _transactionManager.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                if (_repositoryManager.Whitelist.Find(w => w.Mail == _middlewareRequest.Email).FirstOrDefault()
                    == null)
                {
                    throw new BadHttpRequestException("İletişime geçmek istediğiniz mail adresinin non-prod ortamlarda gönderim izni yoktur. " +
                        "/Whitelist endpoint ile whitelist tablosuna eklemeniz gerekmektedir.", 403);
                }
            }
        }

        private async Task GetCustomerDetail()
        {
            if (_middlewareRequest.CustomerNo != null && _middlewareRequest.CustomerNo > 0)
            {
                await GetCustomerInfo();
                if (_middlewareRequest.Phone == null)
                {
                    _middlewareRequest.Phone = _transactionManager.CustomerRequestInfo.MainPhone;
                }
                if (_middlewareRequest.Email == null)
                {
                    _middlewareRequest.Email = _transactionManager.CustomerRequestInfo.MainEmail;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_middlewareRequest.ContactId))
                {
                    await GetCustomerInfoByCitizenshipNumber();
                    if (_middlewareRequest.Phone == null)
                    {
                        _middlewareRequest.Phone = _transactionManager.CustomerRequestInfo.MainPhone;
                    }
                    if (_middlewareRequest.Email == null)
                    {
                        _middlewareRequest.Email = _transactionManager.CustomerRequestInfo.MainEmail;
                    }
                }
            }

            if (_middlewareRequest.Phone != null)
            {
                await GetCustomerInfoByPhone(_transactionManager.CustomerRequestInfo.CustomerNo);
            }
            else
            {
                if (!string.IsNullOrEmpty(_middlewareRequest.Email))
                {
                    await GetCustomerInfoByEmail(_transactionManager.CustomerRequestInfo.CustomerNo);
                }
                else
                { 
                
                }
            }
        }

        private void SetTransaction(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            if (path.Contains("sms") && !path.Contains("check"))
            {
                if (_middlewareRequest.ContentType == MessageContentType.Otp)
                {
                    SetTransactionAsOtp();
                }
                else
                {
                    if (path.Contains("templated"))
                    {
                        SetTransactionAsTemplatedSms();
                    }
                    else
                    {
                        SetTransactionAsSms();
                    }
                }
            }

            if (path.Contains("email"))
            {

                if (path.Contains("templated"))
                {
                    SetTransactionAsTemplatedMail();
                }
                else
                {
                    SetTransactionAsMail();
                }

            }

            if (path.Contains("push"))
            {

                if (path.Contains("templated"))
                {
                    SetTransactionAsTemplatedPushNotification();
                }
                else
                {
                    SetTransactionAsPushNotification();
                }

            }
        }
        private void SetTransactionAsOtp()
        {
            _transactionManager.TransactionType = TransactionType.Otp;
            _transactionManager.OtpRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.OtpRequestInfo.Content = _middlewareRequest.Content?.MaskOtpContent();
            _transactionManager.OtpRequestInfo.Phone = _middlewareRequest.Phone;
        }

        private void SetTransactionAsSms()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalSms;
            _transactionManager.SmsRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.SmsRequestInfo.Content = _middlewareRequest.Content?.MaskFields();
            _transactionManager.SmsRequestInfo.Phone = _middlewareRequest.Phone;
        }

        private void SetTransactionAsTemplatedSms()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalTemplatedSms;
            _transactionManager.SmsRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.SmsRequestInfo.TemplateId = _middlewareRequest.TemplateId;
            _transactionManager.SmsRequestInfo.TemplateParams = _middlewareRequest.TemplateParams?.MaskFields();
            _transactionManager.SmsRequestInfo.Phone = _middlewareRequest.Phone;
        }

        private void SetTransactionAsMail()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalMail;
            _transactionManager.MailRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.MailRequestInfo.Content = _middlewareRequest.Content?.MaskFields();
            _transactionManager.MailRequestInfo.Email = _middlewareRequest.Email;
        }

        private void SetTransactionAsTemplatedMail()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalTemplatedMail;
            _transactionManager.MailRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.MailRequestInfo.TemplateId = _middlewareRequest.TemplateId;
            _transactionManager.MailRequestInfo.TemplateParams = _middlewareRequest.TemplateParams?.MaskFields();
            _transactionManager.MailRequestInfo.Email = _middlewareRequest.Email;
        }

        private void SetTransactionAsPushNotification()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalPush;
            _transactionManager.PushRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.PushRequestInfo.ContactId = _middlewareRequest.ContactId;
            _transactionManager.PushRequestInfo.TemplateId = _middlewareRequest.TemplateId;
            _transactionManager.PushRequestInfo.TemplateParams = _middlewareRequest.TemplateParams?.MaskFields();
            _transactionManager.PushRequestInfo.CustomParameters = _middlewareRequest.CustomParameters?.MaskFields();
        }

        private void SetTransactionAsTemplatedPushNotification()
        {
            _transactionManager.TransactionType = TransactionType.TransactionalTemplatedPush;
            _transactionManager.PushRequestInfo.Process = _middlewareRequest.Process;
            _transactionManager.PushRequestInfo.ContactId = _middlewareRequest.ContactId;
            _transactionManager.PushRequestInfo.TemplateId = _middlewareRequest.TemplateId;
            _transactionManager.PushRequestInfo.TemplateParams = _middlewareRequest.TemplateParams?.MaskFields();
            _transactionManager.PushRequestInfo.CustomParameters = _middlewareRequest.CustomParameters?.MaskFields();
        }

        private async Task GetCustomerInfoByCitizenshipNumber()
        {
            await _transactionManager.GetCustomerInfoByCitizenshipNumber(_middlewareRequest.ContactId);
            await _transactionManager.GetCustomerInfoByCustomerNo(_transactionManager.CustomerRequestInfo.CustomerNo.Value);
        }

        private async Task GetCustomerInfoByPhone(ulong? CustomerNo = null)
        {
            var phoneConfiguration = _repositoryManager.PhoneConfigurations.FirstOrDefault(p =>
                            p.Phone.CountryCode == _middlewareRequest.Phone.CountryCode
                            && p.Phone.Prefix == _middlewareRequest.Phone.Prefix
                            && p.Phone.Number == _middlewareRequest.Phone.Number);
            if (phoneConfiguration == null)
            {
                if(CustomerNo == null)
                    await _transactionManager.GetCustomerInfoByPhone(_middlewareRequest.Phone);

                phoneConfiguration = new PhoneConfiguration()
                {
                    CustomerNo = CustomerNo ?? _transactionManager.CustomerRequestInfo.CustomerNo,
                    Phone = _middlewareRequest.Phone,

                };

                phoneConfiguration.Logs = new List<PhoneConfigurationLog>() {
                                    new PhoneConfigurationLog()
                                    {
                                        Action = "Initialize",
                                        Type = "Add",
                                        Phone = phoneConfiguration,
                                        CreatedBy = _middlewareRequest.Process,
                                    },
                                };

                _repositoryManager.PhoneConfigurations.Add(phoneConfiguration);
                _repositoryManager.SaveChanges();

                if (_middlewareRequest.ContentType == MessageContentType.Otp)
                    _transactionManager.OtpRequestInfo.PhoneConfiguration = phoneConfiguration;
                else
                    _transactionManager.SmsRequestInfo.PhoneConfiguration = phoneConfiguration;

            }
            else
            {
                _transactionManager.CustomerRequestInfo.CustomerNo = CustomerNo ?? (phoneConfiguration.CustomerNo ?? null);
                if (CustomerNo == null && phoneConfiguration.CustomerNo != null)
                    await _transactionManager.GetCustomerInfoByCustomerNo((ulong)phoneConfiguration.CustomerNo);

                if (_middlewareRequest.ContentType == MessageContentType.Otp)
                    _transactionManager.OtpRequestInfo.PhoneConfiguration = phoneConfiguration;
                else
                    _transactionManager.SmsRequestInfo.PhoneConfiguration = phoneConfiguration;
            }

        }

        private async Task GetCustomerInfoByEmail(ulong? CustomerNo = null)
        {
            if (!string.IsNullOrEmpty(_middlewareRequest.Email))
            {
                var mailConfiguration = _repositoryManager.MailConfigurations.FirstOrDefault(m => m.Email == _middlewareRequest.Email);
                if (mailConfiguration == null)
                {
                    if(CustomerNo == null)
                        await _transactionManager.GetCustomerInfoByEmail(_middlewareRequest.Email);
                    mailConfiguration = new MailConfiguration()
                    {
                        CustomerNo = CustomerNo ?? _transactionManager.CustomerRequestInfo.CustomerNo,
                        Email = _middlewareRequest.Email,

                    };

                    mailConfiguration.Logs = new List<MailConfigurationLog>();
                    var mailConfigurationLog = new MailConfigurationLog()
                    {
                        Action = "Initialize",
                        Type = "Add",
                        Mail = mailConfiguration,
                        CreatedBy = _middlewareRequest.Process,
                    };

                    mailConfiguration.Logs.Add(mailConfigurationLog);

                    _repositoryManager.MailConfigurations.Add(mailConfiguration);
                    _repositoryManager.SaveChanges();

                    _transactionManager.MailRequestInfo.MailConfiguration = mailConfiguration;
                }
                else
                {
                    _transactionManager.CustomerRequestInfo.CustomerNo = CustomerNo ?? (mailConfiguration.CustomerNo ?? null);
                    if (CustomerNo == null && mailConfiguration.CustomerNo != null)
                        await _transactionManager.GetCustomerInfoByCustomerNo((ulong)mailConfiguration.CustomerNo);

                    _transactionManager.MailRequestInfo.MailConfiguration = mailConfiguration;
                }
            }
        }

        private async Task GetCustomerInfo()
        {
            await _transactionManager.GetCustomerInfoByCustomerNo(_middlewareRequest.CustomerNo.Value);
        }

        private  string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }
    }

    

    public static class CustomerInfoMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomerInfoMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseWhen(context => (context.Request.Path.Value.IndexOf("/Messaging") != -1
            && context.Request.Path.Value.IndexOf("/sms/check") == -1
            ), builder =>
            {
                builder.UseMiddleware<CustomerInfoMiddleware>();
            });
        }
    }
}
