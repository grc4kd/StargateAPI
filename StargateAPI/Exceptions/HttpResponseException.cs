using System.Net;

namespace StargateAPI.Exceptions;

public class HttpResponseException(object? value = null) : Exception
{
    public object? Value { get; } = value;
}