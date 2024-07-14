using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateApiTests.Specifications;

namespace StargateApiTests.Controllers;

public class PersonControllerTests : UnitTest
{
    [Fact]
    public void PersonController_ControllerActions_ShouldHaveExpectedAttributes()
    {
        var personControllerMembers = typeof(PersonController).GetMembers();
        var httpGetMember = personControllerMembers.Where(m => m.IsDefined(typeof(HttpGetAttribute), false));
        var httpGetAttributes = httpGetMember.SelectMany(m => m.CustomAttributes.Where(c => c.AttributeType == typeof(HttpGetAttribute)));
        var httpGetConstructorAttribute = httpGetAttributes.Single(a => a.ConstructorArguments.Count == 0);
        var httpGetPersonByNameAttribute = httpGetAttributes.Single(a => a.ConstructorArguments.Any(c => c.Value!.Equals("{name}")));
        var httpGetPersonByNameConstructorArgument = httpGetPersonByNameAttribute.ConstructorArguments.Single();

        var httpPostMember = personControllerMembers.Where(m => m.IsDefined(typeof(HttpPostAttribute), false));
        var httpPostAttribute = httpPostMember.Single().CustomAttributes.Single(a => a.AttributeType == typeof(HttpPostAttribute));

        Assert.Empty(httpGetConstructorAttribute.ConstructorArguments);

        Assert.Equal(2, httpGetAttributes.Count());
        Assert.True(typeof(string) == httpGetPersonByNameConstructorArgument.ArgumentType);
        Assert.Equal("{name}", httpGetPersonByNameConstructorArgument.Value);

        Assert.Empty(httpPostAttribute.ConstructorArguments);
    }
}