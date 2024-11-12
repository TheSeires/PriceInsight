using API.Models.Application.Dto.Responses;
using Microsoft.AspNetCore.Identity;

namespace API.Extensions;

public static class IdentityResultEx
{
    public static ErrorResponse GetErrorFromDescriptions(this IdentityResult identityResult)
    {
        return new ErrorResponse(string.Join(" ", identityResult.Errors.Select(e => e.Description)));
    }
}
