using System.Reflection;
using StargateAPI.Business.Responses;
using StargateApiTests.Specifications;

namespace StargateApiTests.Controllers;

public class BaseResponseTests : UnitTest
{
    [Fact]
    public void BaseResponse_DerivedResponses_ShouldBeConstructedWithDefaultPropertyValues()
    {
        var assembly = Assembly.GetAssembly(typeof(Program));
        var assemblyTypes = AssemblyExtensions.GetTypes(assembly!);
        var responseTypes = assemblyTypes.Where(t => t.BaseType == typeof(IBaseResponse));
        var defaultProperties = new List<string> {
                nameof(IBaseResponse.Success),
                nameof(IBaseResponse.Message),
                nameof(IBaseResponse.StatusCode)
            };

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
                object defaultValue = new();
                typeof(IBaseResponse).GetProperty(propertyName)?.GetGetMethod()?.Invoke(defaultValue, null);

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