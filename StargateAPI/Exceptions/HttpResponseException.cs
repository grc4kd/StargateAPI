using StargateAPI.Business.Responses;

namespace StargateAPI.Exceptions;

public class HttpResponseException(IBaseResponse Value) : Exception
{
    public IBaseResponse Value { get; } = Value;
}
