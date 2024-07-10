using System.Reflection;
using StargateAPI.Controllers;
using StargateApiTests.Specifications;

namespace StargateApiTests.Controllers;

public class BaseResponseTests : UnitTest
{
    [Fact]
    public void BaseResponse_DerivedResponses_ShouldBeConstructedWithDefaultPropertyValues()
    {
        var assembly = Assembly.GetAssembly(typeof(Program));
        var assemblyTypes = AssemblyExtensions.GetTypes(assembly!);
        var responseTypes = assemblyTypes.Where(t => t.BaseType == typeof(BaseResponse));
        var defaultProperties = new List<string> {
                nameof(BaseResponse.Success),
                nameof(BaseResponse.Message),
                nameof(BaseResponse.ResponseCode)
            };
        var defaultBaseResponse = new BaseResponse();

        Assert.NotNull(assembly);
        foreach (Type derivedType in responseTypes)
        {
            foreach (string propertyName in defaultProperties)
            {
                var constructorInfo = derivedType.GetConstructor([]);
                var derivedObject = constructorInfo?.Invoke(null);
                var propertyInfo = derivedType.GetProperty(propertyName);
                var propertyGetMethod = propertyInfo?.GetGetMethod();
                var propertyValue = propertyGetMethod?.Invoke(derivedObject, null);
                var defaultValue = defaultBaseResponse.GetType().GetProperty(propertyName)?.GetGetMethod()?.Invoke(defaultBaseResponse, null);

                Assert.NotNull(propertyInfo);
                Assert.NotNull(propertyGetMethod);
                Assert.NotNull(defaultValue);
                Assert.True(defaultValue.Equals(propertyValue),
                    $"Expected property {propertyInfo.ReflectedType}.{propertyInfo.Name} to have value {defaultValue}, but was {propertyValue}."
                );
            }
        }
    }
}