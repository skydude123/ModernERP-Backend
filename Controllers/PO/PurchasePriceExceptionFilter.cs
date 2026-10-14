using HRIStwinbeeAPI.DTO.Suppliers;
using HRIStwinbeeAPI.Models.PO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRIStwinbeeAPI.Controllers.PO;

public sealed class PurchasePriceExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var mapped = context.Exception switch
        {
            PurchasePriceException exception =>
                (exception.StatusCode, exception.Title, exception.Message),
            SupplierUnauthorizedException exception =>
                (exception.StatusCode, exception.Title, exception.Message),
            _ => ((int Status, string Title, string Detail)?)null
        };

        if (mapped is null)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = mapped.Value.Status,
            Title = mapped.Value.Title,
            Detail = mapped.Value.Detail,
            Type = $"https://httpstatuses.com/{mapped.Value.Status}",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = mapped.Value.Status
        };
        context.ExceptionHandled = true;
    }
}
