using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StargateAPI.Controllers;
using StargateAPI.Logging;

namespace StargateAPI.Exceptions;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    private readonly ILogger _logger;

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
    {
        _logger = logger;
    }

    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            _logger.FailedRequest(context.HttpContext.Request.Method, context.HttpContext.Request.GetEncodedUrl(), context.ActionDescriptor.DisplayName!);
        }

        if (context.Exception is HttpResponseException httpResponseException)
        {
            if (httpResponseException.Value is BaseResponse response)
            {
                context.HttpContext.Response.StatusCode = response.ResponseCode;
            }

            context.Result = new ObjectResult(httpResponseException.Value);
            context.ExceptionHandled = true;
        }

        if (context.Result is BaseResponse baseResponse)
        {
            context.Result = new ObjectResult(baseResponse)
            {
                StatusCode = baseResponse.ResponseCode
            };

            if (!baseResponse.Success)
            {
                _logger.FailedRequest(context.HttpContext.Request.Method, context.HttpContext.Request.GetEncodedUrl(), context.ActionDescriptor.DisplayName!);
            }
        }

        if (context.Exception == null && context.ModelState.IsValid)
        {
            _logger.SuccessfulRequest(context.HttpContext.Request.Method, context.HttpContext.Request.GetEncodedUrl(), context.ActionDescriptor.DisplayName!);
        }
    }
}