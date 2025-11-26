using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceManager.Api.Misc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken ct)
    {
        var problemDetails = CreateProblemDetails(exception);

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogCritical(exception, "Exception occurred: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        var status = StatusCodes.Status500InternalServerError;
        var title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError);
        var details = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError);

        switch (exception)
        {
            case MaxLengthExceededException:
            case NumericOverflowException:
            case CannotInsertNullException:
            case ReferenceConstraintException:
            case BadHttpRequestException:
                status = StatusCodes.Status400BadRequest;
                title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest);
                details = exception.Message;
                break;

            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized);
                details = "You do not have permission to perform this action";
                break;

            case UniqueConstraintException err:
                status = StatusCodes.Status409Conflict;
                title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict);
                details = $"{exception.Message}({err.ConstraintName})";
                break;

            case NullReferenceException:
                status = StatusCodes.Status500InternalServerError;
                title = "Unexpected Error";
                details = "A required resource was missing. Please contact support if the issue persists";
                break;


            case DbUpdateException dbUpdateEx:
                if (dbUpdateEx.InnerException is PostgresException { SqlState: "23514" } pgEx)
                {
                    status = StatusCodes.Status400BadRequest;
                    title = "Check Constraint Violation";
                    details = pgEx.ConstraintName;
                }

                break;
        }

        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = details
        };
    }
}