using HRIStwinbeeAPI.DTO.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRIStwinbeeAPI.Controllers.Suppliers;

public sealed class SupplierExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not SupplierException exception)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = exception.Title,
            Detail = exception.Message,
            Type = $"https://httpstatuses.com/{exception.StatusCode}",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = exception.StatusCode
        };
        context.ExceptionHandled = true;
    }
}
