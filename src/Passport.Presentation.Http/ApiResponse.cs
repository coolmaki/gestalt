using Supercluster.Lib.Primitives;

namespace Passport.Presentation.Http;

internal static class ApiResponse
{
    public static IResult Ok<T>(T data)
    {
        return Results.Ok(new { Success = true, Data = data, Errors = Array.Empty<object>() });
    }

    public static IResult Ok()
    {
        return Results.Ok(new { Success = true, Data = (object?)null, Errors = Array.Empty<object>() });
    }

    public static IResult Error(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(Envelope(error)),
            ErrorType.NotFound => Results.NotFound(Envelope(error)),
            ErrorType.Conflict => Results.Conflict(Envelope(error)),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Unexpected => Results.Problem(Envelope(error).ToString()),
            _ => Results.Problem(),
        };
    }

    public static IResult FromResult<T>(Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        return result.Match(
            onSuccess: onSuccess ?? (value => value is Unit ? Ok() : Ok(value!)),
            onFailure: Error);
    }

    private static object Envelope(Error error)
    {
        return new
        {
            Success = false,
            Data = (object?)null,
            Errors = new[] { new { error.Type, error.Code, error.Description } },
        };
    }
}