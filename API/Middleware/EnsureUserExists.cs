using System.Security.Claims;
using API.Models.Database;
using Microsoft.AspNetCore.Identity;

namespace API.Middleware;

public class EnsureUserExists
{
    private readonly RequestDelegate _next;

    public EnsureUserExists(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        var identity = context.User.Identity;

        if (identity is null || identity.IsAuthenticated == false)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            await _next(context);
            return;
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/login", true, true);
            return;
        }

        await _next(context);
    }
}