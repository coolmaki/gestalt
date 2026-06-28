using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Supercluster.Lib.Primitives;

namespace Supercluster.Lib.Presentation.Http.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an HTTP response using
    /// <see href="https://datatracker.ietf.org/doc/html/rfc7807">RFC 7807 Problem Details</see>
    /// for errors and <c>200 OK</c> for success values.
    /// </summary>
    public static IResult ToHttpResponse<T>(this Result<T> result, HttpContext? context = null)
    {
        return result.Match(
            onSuccess: value =>
            {
                if (value is Unit)
                {
                    return Results.Ok();
                }

                return Results.Ok(value);
            },
            onFailure: error => error.ToProblemDetails(context));
    }

    // ------------------------------------------------------------
    // Internal
    // ------------------------------------------------------------

    private static IResult ToProblemDetails(this Error error, HttpContext? context)
    {
        var details = new ProblemDetails
        {
            Type = GetProblemType(error.Type),
            Title = GetTitle(error.Type),
            Status = GetStatusCode(error.Type),
            Detail = error.Description,
            Instance = context?.Request.Path,
        };

        details.Extensions["code"] = error.Code;

        return Results.Problem(details);
    }

    private static string GetProblemType(ErrorType type) => type switch
    {
        ErrorType.Validation => "https://errors.supercluster.dev/validation",
        ErrorType.NotFound => "https://errors.supercluster.dev/not-found",
        ErrorType.Conflict => "https://errors.supercluster.dev/conflict",
        ErrorType.Unauthorized => "https://errors.supercluster.dev/unauthorized",
        ErrorType.Unexpected => "https://errors.supercluster.dev/unexpected",
        _ => "about:blank",
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Unexpected => "Internal Server Error",
        _ => "Error",
    };

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError,
    };
}