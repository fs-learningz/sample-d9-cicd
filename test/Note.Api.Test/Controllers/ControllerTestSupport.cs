using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using Note.Application.Wrapper;

namespace Note.Api.Test.Controllers;

internal static class ControllerTestSupport
{
    /// <summary>Lets <c>ControllerBase.Problem(...)</c> work without a running host.</summary>
    public static void UseProblemDetailsFactory(ControllerBase controller)
    {
        var factory = Substitute.For<ProblemDetailsFactory>();
        factory.CreateProblemDetails(Arg.Any<HttpContext>(), Arg.Any<int?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(call => new ProblemDetails
            {
                Status = call.ArgAt<int?>(1),
                Title = call.ArgAt<string?>(2),
                Detail = call.ArgAt<string?>(4),
            });
        controller.ProblemDetailsFactory = factory;
    }

    public static void AssertProblem(ActionResult result, int statusCode, string title, string detail)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(statusCode, objectResult.StatusCode);
        Assert.Equal(statusCode, problem.Status);
        Assert.Equal(title, problem.Title);
        Assert.Equal(detail, problem.Detail);
    }

    public static void AssertUnexpectedOutputProblem(ActionResult result)
        => AssertProblem(result, StatusCodes.Status500InternalServerError, "Internal Server Error",
            "An unexpected error occurred while processing your request.");

    /// <summary>An <see cref="Output"/> type the controller does not know how to handle.</summary>
    public sealed class UnknownOutput() : Output(200);
}
