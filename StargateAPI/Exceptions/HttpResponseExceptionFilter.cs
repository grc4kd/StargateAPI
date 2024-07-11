using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StargateAPI.Business.Commands;
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
            _logger.FailedRequest(context.Controller, context.HttpContext.Request.GetDisplayUrl());

            if (context.Exception is not HttpResponseException)
            {
                _logger.LogError(context.Exception, "Exception was thrown in action: {action}", context.ActionDescriptor.DisplayName);
                context.ExceptionHandled = true;
            }
        }

        if (context.Result is ObjectResult objectResult
            && objectResult.Value is BaseResponse baseResponse)
        {
            if (!baseResponse.Success)
            {
                _logger.FailedRequest(context.Controller, context.HttpContext.Request.GetDisplayUrl());
            }
            if (baseResponse.Success)
            {
                _logger.SuccessfulRequest(context.Controller, context.HttpContext.Request.GetDisplayUrl());
            }
        }

        if (context.Exception is HttpResponseException httpResponseException)
        {
            context.ModelState.TryAddModelException(nameof(HttpResponseException), httpResponseException);    
            context.Result = new ObjectResult(httpResponseException.Value)
            {
                StatusCode = (int)httpResponseException.StatusCode
            };

            context.ExceptionHandled = true;
        }

        context.Result ??= new ObjectResult(context.HttpContext.Response)
        {
            StatusCode = context.HttpContext.Response.StatusCode
        };
    }
}