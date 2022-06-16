using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Exceptions;
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
        private ITransactionManager _transactionManager;
        private IRepositoryManager _repositoryManager;
        
        public CustomerInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,ITransactionManager transactionManager,IRepositoryManager repositoryManager)
        {
            _transactionManager = transactionManager;
            _repositoryManager = repositoryManager;
            
            await GetCustomerDetail();

            // Call the next delegate/middleware in the pipeline.
            await _next(context);   
        }
       
        private async Task GetCustomerDetail()
        {
            if (_transactionManager.Transaction.CustomerNo > 0)
            {
                await GetCustomerInfo();
            }
            else
            {
                if (!string.IsNullOrEmpty(_transactionManager.Transaction.CitizenshipNo))
                {
                    await GetCustomerInfoByCitizenshipNumber();
                }
            }

            if (_transactionManager.Transaction.Phone != null)
            {
                await GetCustomerInfoByPhone();
            }
            else
            {
                if (!string.IsNullOrEmpty(_transactionManager.Transaction.Mail))
                {
                    await GetCustomerInfoByEmail();
                }
                else
                {
                    throw new WorkflowException("Request should have at least one of those : (CustomerNo,Phone,Email,ContactId)",System.Net.HttpStatusCode.NotFound);
                }
            }
        }

        private async Task GetCustomerInfoByCitizenshipNumber()
        {
            await _transactionManager.GetCustomerInfoByCitizenshipNumber();
            await _transactionManager.GetCustomerInfoByCustomerNo();
        }

        private async Task GetCustomerInfoByPhone()
        {
            var phoneConfiguration = _repositoryManager.PhoneConfigurations.FirstOrDefault(p =>
                            p.Phone.CountryCode == _transactionManager.Transaction.Phone.CountryCode
                            && p.Phone.Prefix == _transactionManager.Transaction.Phone.Prefix
                            && p.Phone.Number == _transactionManager.Transaction.Phone.Number);

            if (phoneConfiguration == null)
            {
                if(_transactionManager.Transaction.CustomerNo == 0)
                    await _transactionManager.GetCustomerInfoByPhone();

                phoneConfiguration = new PhoneConfiguration()
                {
                    CustomerNo = _transactionManager.Transaction.CustomerNo,
                    Phone = _transactionManager.Transaction.Phone,
                };

                phoneConfiguration.Logs = new List<PhoneConfigurationLog>() {
                                    new PhoneConfigurationLog()
                                    {
                                        Action = "Initialize",
                                        Type = "Add",
                                        Phone = phoneConfiguration,
                                        CreatedBy = _transactionManager.Transaction.CreatedBy,
                                    },
                                };

                _repositoryManager.PhoneConfigurations.Add(phoneConfiguration);
            }
            else
            {
                if (_transactionManager.Transaction.CustomerNo > 0)
                {
                    phoneConfiguration.CustomerNo = _transactionManager.Transaction.CustomerNo;
                }
                else
                {
                    if (phoneConfiguration.CustomerNo != null && phoneConfiguration.CustomerNo > 0)
                    {
                        _transactionManager.Transaction.CustomerNo = phoneConfiguration.CustomerNo.GetValueOrDefault();
                        await _transactionManager.GetCustomerInfoByCustomerNo();
                    }
                    else
                    {
                        await _transactionManager.GetCustomerInfoByPhone();
                        phoneConfiguration.CustomerNo = _transactionManager.CustomerRequestInfo.CustomerNo;
                    }
                }                
            }

            if (_transactionManager.Transaction.TransactionType == TransactionType.Otp)
                _transactionManager.OtpRequestInfo.PhoneConfiguration = phoneConfiguration;
            else
                _transactionManager.SmsRequestInfo.PhoneConfiguration = phoneConfiguration;
        }

        private async Task GetCustomerInfoByEmail()
        {
            
            var mailConfiguration = _repositoryManager.MailConfigurations.FirstOrDefault(m => m.Email == _transactionManager.Transaction.Mail);
            if (mailConfiguration == null)
            {
                if(_transactionManager.Transaction.CustomerNo == 0)
                    await _transactionManager.GetCustomerInfoByEmail();
                mailConfiguration = new MailConfiguration()
                {
                    CustomerNo = _transactionManager.Transaction.CustomerNo,
                    Email = _transactionManager.Transaction.Mail,

                };

                mailConfiguration.Logs = new List<MailConfigurationLog>();
                var mailConfigurationLog = new MailConfigurationLog()
                {
                    Action = "Initialize",
                    Type = "Add",
                    Mail = mailConfiguration,
                    CreatedBy = _transactionManager.Transaction.CreatedBy,
                };

                mailConfiguration.Logs.Add(mailConfigurationLog);

                _repositoryManager.MailConfigurations.Add(mailConfiguration);
            }
            else
            {
                if (_transactionManager.Transaction.CustomerNo > 0)
                {
                    mailConfiguration.CustomerNo = _transactionManager.Transaction.CustomerNo;
                }
                else
                {
                    if (mailConfiguration.CustomerNo != null && mailConfiguration.CustomerNo > 0)
                    {
                        _transactionManager.Transaction.CustomerNo = mailConfiguration.CustomerNo.GetValueOrDefault();
                        await _transactionManager.GetCustomerInfoByCustomerNo();
                    }
                    else
                    {
                        await _transactionManager.GetCustomerInfoByEmail();
                        mailConfiguration.CustomerNo = _transactionManager.CustomerRequestInfo.CustomerNo;
                    }
                }
                
            }
            _transactionManager.MailRequestInfo.MailConfiguration = mailConfiguration;
        }

        private async Task GetCustomerInfo()
        {
            await _transactionManager.GetCustomerInfoByCustomerNo();
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
