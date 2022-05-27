﻿using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using bbt.gateway.messaging.Exceptions;
using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class WhitelistMiddleware
    {
        private readonly RequestDelegate _next;
        private ITransactionManager _transactionManager;
        private IRepositoryManager _repositoryManager;
        public WhitelistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context,ITransactionManager transactionManager,IRepositoryManager repositoryManager)
        {
            _transactionManager = transactionManager;
            _repositoryManager = repositoryManager;
            _transactionManager.UseFakeSmtp = false;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment != "Prod")
            {
                if (environment == "Mock")
                {
                    _transactionManager.UseFakeSmtp = true;
                }
                else
                {
                    CheckWhitelist();
                }
            }
            return _next(context);
        }

        private void CheckWhitelist()
        {
            if (_transactionManager.Transaction.TransactionType == TransactionType.Otp ||
                _transactionManager.Transaction.TransactionType == TransactionType.TransactionalSms ||
                _transactionManager.Transaction.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                Phone phone = _transactionManager.Transaction.Phone;

                if (phone == null)
                    throw new WorkflowException("Phone number couldn't be resolved",System.Net.HttpStatusCode.NotFound);

                if (_repositoryManager.Whitelist.Find(w =>
                (w.Phone.CountryCode == phone.CountryCode
                && w.Phone.Prefix == phone.Prefix
                && w.Phone.Number == phone.Number
                )).FirstOrDefault() == null)
                {
                    _transactionManager.UseFakeSmtp = true;
                }
            }

            if (_transactionManager.Transaction.TransactionType == TransactionType.TransactionalMail ||
                _transactionManager.Transaction.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                string email = _transactionManager.Transaction.Mail;
                if (string.IsNullOrEmpty(email))
                    throw new WorkflowException("Mail address couldn't be resolved", System.Net.HttpStatusCode.NotFound);

                if (_repositoryManager.Whitelist.Find(w => w.Mail == email).FirstOrDefault()
                    == null)
                {
                    _transactionManager.UseFakeSmtp = true;
                }
            }

            if (_transactionManager.Transaction.TransactionType == TransactionType.TransactionalPush ||
                _transactionManager.Transaction.TransactionType == TransactionType.TransactionalTemplatedPush)
            {
                if (string.IsNullOrEmpty(_transactionManager.Transaction.CitizenshipNo))
                    throw new WorkflowException("CitizenshipNumber couldn't be resolved", System.Net.HttpStatusCode.NotFound);
                if (_repositoryManager.Whitelist.Find(w => w.ContactId == _transactionManager.Transaction.CitizenshipNo).FirstOrDefault()
                    == null)
                {
                    _transactionManager.UseFakeSmtp = true;
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class WhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseWhitelistMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseWhen(context => (context.Request.Path.Value.IndexOf("/Messaging") != -1
            && context.Request.Path.Value.IndexOf("/sms/check") == -1
            ), builder =>
            {
                builder.UseMiddleware<WhitelistMiddleware>();
            });
        }
    }
}
