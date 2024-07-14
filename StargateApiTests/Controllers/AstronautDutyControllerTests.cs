using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateApiTests.Specifications;

namespace StargateApiTests.Controllers;

public class AstronautDutyControllerTests : UnitTest
{
    [Fact]
    public void AstronautDutyController_ControllerActions_ShouldHaveExpectedAttributes()
    {
        var astronautDutyControllerMembers = typeof(AstronautDutyController).GetMembers();
        var httpGetMember = astronautDutyControllerMembers.Where(m => m.IsDefined(typeof(HttpGetAttribute), false));
        var httpGetAttribute = httpGetMember.Single().CustomAttributes.Single(a => a.AttributeType == typeof(HttpGetAttribute));
        var httpGetConstructorArgument = httpGetAttribute.ConstructorArguments.Single();

        var httpPostMember = astronautDutyControllerMembers.Where(m => m.IsDefined(typeof(HttpPostAttribute), false));
        var httpPostAttribute = httpPostMember.Single().CustomAttributes.Single(a => a.AttributeType == typeof(HttpPostAttribute));

        Assert.Single(httpGetAttribute.ConstructorArguments);
        Assert.True(typeof(string) == httpGetConstructorArgument.ArgumentType);
        Assert.Equal("{name}", httpGetConstructorArgument.Value);

        Assert.Empty(httpPostAttribute.ConstructorArguments);
    }
}