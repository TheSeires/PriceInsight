namespace API.Extensions;

public static class ResultsEx
{
    public static IResult Unauthorized(params object[] props)
    {
        return Results.Json(props, statusCode: StatusCodes.Status401Unauthorized);
    }

    public static IResult Unauthorized(string error)
    {
        return Results.Json(error, statusCode: StatusCodes.Status401Unauthorized);
    }
}
