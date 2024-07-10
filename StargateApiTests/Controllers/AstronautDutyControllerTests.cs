using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;

namespace StargateApiTests.Controllers;

public class AstronautDutyControllerTests
{
    [Fact]
    public async Task AstronautDutyControllerGetAstronautDutiesByName_RequestWithMediatorTimeout_ReturnsServerError()
    {
        var request = new GetAstronautDutiesByName { Name = "shouldn't matter for this test" };
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(request, CancellationToken.None))
            .Throws<TimeoutException>();
        var controller = new AstronautDutyController(mockMediator.Object);

        var response = await controller.GetAstronautDutiesByName(request.Name);
        
        Assert.IsType<ObjectResult>(response);
        var objectResult = response as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }
}