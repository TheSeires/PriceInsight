namespace API.Models.Application.Dto.Responses;

public struct ErrorResponse
{
    public string Error { get; init; } = "";

    public ErrorResponse(string error)
    {
        Error = error;
    }
}