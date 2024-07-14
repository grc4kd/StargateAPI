using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StargateAPI.Business.Responses;
using StargateAPI.Logging;

namespace StargateAPI.Exceptions;

public class HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger) : IActionFilter, IOrderedFilter
{
    private readonly ILogger _logger = logger;

    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            _logger.FailedRequest(context.HttpContext.Request.Method, context.HttpContext.Request.GetEncodedUrl(), context.ActionDescriptor.DisplayName!);

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
            _logger.SuccessfulRequest(context.HttpContext.Request.Method, context.HttpContext.Request.GetEncodedUrl(), context.ActionDescriptor.DisplayName!);
        }
    }
}