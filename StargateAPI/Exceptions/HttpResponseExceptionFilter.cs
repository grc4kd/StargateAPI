using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StargateAPI.Business.Responses;
using StargateAPI.Logging;

using ILogger = Serilog.ILogger;

namespace StargateAPI.Exceptions;

public class HttpResponseExceptionFilter(ILogger logger) : IActionFilter, IOrderedFilter
{
    private readonly ILogger _logger = logger;

    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        string method = context.HttpContext.Request.Method;
        string requestUrl = context.HttpContext.Request.GetEncodedUrl();
        string action = context.ActionDescriptor.DisplayName ?? string.Empty;

        if (context.Exception != null)
        {
            _logger.FailedRequest(method, requestUrl, action);

            if (context.Exception is HttpResponseException httpResponseException)
            {
                IBaseResponse baseResponse = httpResponseException.Value;
                context.Result = new ObjectResult(baseResponse)
                {
                    StatusCode = (int)baseResponse.StatusCode
                };
                context.HttpContext.Response.StatusCode = (int)baseResponse.StatusCode;
                context.ExceptionHandled = true;
            }
        }

        if (context.Exception == null)
        {
            _logger.SuccessfulRequest(method, requestUrl, action);
        }
    }
}