namespace StargateAPI.Logging;

public static class Log
{
    public static void FailedRequest(this Serilog.ILogger logger, string method, string requestUrl, string action)
    {
        logger.Error("Failed request {method}: {requestUrl} {action}", method, requestUrl, action);
    }

    public static void SuccessfulRequest(this Serilog.ILogger logger, string method, string requestUrl, string action)
    {
        logger.Verbose("Successful request {method}: {requestUrl} {action}.", method, requestUrl, action);
    }
}