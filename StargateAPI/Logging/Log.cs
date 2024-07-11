namespace StargateAPI.Logging;

public static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Error,
        Message = "Controller {controller} had a failed request: {requestUrl}.")]
    public static partial void FailedRequest(this ILogger logger, object controller, string requestUrl);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "Controller {controller} responding to successful request: {requestUrl}.")]
    public static partial void SuccessfulRequest(this ILogger logger, object controller, string requestUrl);
}