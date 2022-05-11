using bbt.gateway.messaging.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace bbt.gateway.messaging.Filters
{
    public class LogActionFilter : ActionFilterAttribute
    {
        private ITransactionManager _transactionManager;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ipAdress = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            _transactionManager = context.HttpContext.RequestServices.GetService<ITransactionManager>();
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var response = context.Result;
            if (response is ObjectResult json)
            {
                var res = JsonConvert.SerializeObject(json.Value);
            }
        }
    }
}
