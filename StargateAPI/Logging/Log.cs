namespace StargateAPI.Logging;

public static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Error,
        Message = "Failed request {method}: {requestUrl} {action}")]
    public static partial void FailedRequest(this ILogger logger, string method, string requestUrl, string action);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "Successful request {method}: {requestUrl} {action}.")]
    public static partial void SuccessfulRequest(this ILogger logger, string method, string requestUrl, string action);
}