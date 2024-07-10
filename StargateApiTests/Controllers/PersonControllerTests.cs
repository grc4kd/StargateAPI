using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;

namespace StargateApiTests.Controllers;

public class PersonControllerTests
{
    [Fact]
    public async Task PersonControllerGetPeople_RequestWithMediatorTimeout_ReturnsServerError()
    {
        var request = new GetPeople();
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(request, CancellationToken.None))
            .Throws<TimeoutException>();
        var controller = new PersonController(mockMediator.Object);

        var response = await controller.GetPeople();
        
        Assert.IsType<ObjectResult>(response);
        var objectResult = response as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task PersonControllerGetPersonByName_RequestWithMediatorTimeout_ReturnsServerError()
    {
        var request = new GetPersonByName { Name = "any" };
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(request, CancellationToken.None))
            .Throws<TimeoutException>();
        var controller = new PersonController(mockMediator.Object);

        var response = await controller.GetPersonByName(request.Name);
        
        Assert.IsType<ObjectResult>(response);
        var objectResult = response as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }
}