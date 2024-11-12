using System.Security.Claims;
using API.Extensions;
using API.Models.Application;
using API.Models.Application.Dto.Requests;
using API.Models.Application.Dto.Responses;
using API.Models.Database;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace API.Endpoints.V1;

public static partial class AuthEndpoints
{
    public static void MapAuthEndpointsV1(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/api/{ApiVersion.V1}/auth");
        group.MapPost("/register", Register);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/login", LogIn);
        group.MapPost("/logout", LogOut);
        group.MapGet("/ping", Ping);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterUserRequest registerUserRequest,
        IUserService userService,
        UserManager<User> userManager,
        IEmailSender emailSender,
        IEmailContentBuilder emailContentGenerator,
        HttpContext httpContext,
        IStringLocalizer localizer)
    {
        var existingUser = await userService.FindOneByAsync(user =>
            user.Email == registerUserRequest.Email);

        if (existingUser is not null)
        {
            return Results.Conflict(new ErrorResponse(localizer["auth.registration.validation.email.taken"]));
        }

        var user = new User
        {
            UserName = registerUserRequest.Username,
            Email = registerUserRequest.Email,
        };

        var createUserIdentity = await userManager.CreateAsync(user, registerUserRequest.Password);
        if (createUserIdentity.Succeeded == false)
        {
            var createUserError = createUserIdentity.GetErrorFromDescriptions();
            return Results.Conflict(createUserError);
        }

        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = confirmationToken.ToBase64();

        var confirmationLink =
            $"{ClientConfiguration.BaseUrl}/confirm-email?token={encodedToken}&email={user.Email}";
        var htmlMessage = emailContentGenerator.GenerateEmailConfirmation(confirmationLink,
            user.UserName);

        var wasEmailSent = await emailSender.SendEmailAsync(user.Email, "Email Confirmation",
            htmlMessage);

        if (wasEmailSent is false)
        {
            return Results.Conflict(localizer["auth.registration.apiResponse.failedToSendEmail"]);
        }

        return Results.Ok();
    }

    private static async Task<IResult> ConfirmEmail(
        string email,
        string token,
        UserManager<User> userManager,
        IStringLocalizer localizer)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Results.BadRequest(
                new ErrorResponse(localizer["auth.common.validation.email.invalidEmail"]));
        }

        if (user.EmailConfirmed)
        {
            return Results.BadRequest(new ErrorResponse(
                localizer["auth.emailConfirmation.validation.alreadyConfirmed"]));
        }

        var decodedToken = token.FromBase64();
        var confirmTokenResult = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (confirmTokenResult.Succeeded)
            return Results.Ok();

        var confirmTokenError = confirmTokenResult.GetErrorFromDescriptions();
        return Results.BadRequest(confirmTokenError);
    }

    private static async Task<IResult> LogIn(
        [FromBody] LoginUserRequest? loginRequest,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<UserRole> roleManager,
        IStringLocalizer localizer)
    {
        if (loginRequest is null)
            return Results.BadRequest(new ErrorResponse(localizer["common.missingOrInvalidBodyParams"]));

        var user = await userManager.FindByEmailAsync(loginRequest.Email);

        if (user is null)
        {
            return ResultsEx.Unauthorized(
                new ErrorResponse(localizer["auth.common.validation.email.invalidEmail"]));
        }

        var isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
        if (isEmailConfirmed is false)
        {
            return ResultsEx.Unauthorized(
                new ErrorResponse(localizer["auth.login.validation.unconfirmedEmail"]));
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user, loginRequest.Password, false, false);

        if (signInResult.Succeeded)
        {
            var roleNames = new List<string>();
            foreach (var roleId in user.Roles)
            {
                var role = await roleManager.FindByIdAsync(roleId.ToString());
                if (role != null && role.Name.IsNullOrWhiteSpace() == false)
                {
                    roleNames.Add(role.Name);
                }
            }

            var response = new LoginUserResponse
            {
                Email = user.Email,
                Username = user.UserName,
                Roles = roleNames,
            };

            return Results.Ok(response);
        }

        string error = localizer["auth.login.validation.invalidPassword"];
        if (signInResult.IsLockedOut)
        {
            error = localizer["auth.login.validation.isLockedOut"];
        }
        else if (signInResult.IsNotAllowed)
        {
            error = localizer["auth.login.validation.isNotAllowed"];
        }
        else if (signInResult.RequiresTwoFactor)
        {
            error = localizer["auth.login.validation.requiresTwoFactor"];
        }

        return ResultsEx.Unauthorized(new ErrorResponse(error));
    }

    private static async Task<IResult> LogOut(SignInManager<User> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    private static IResult Ping(ClaimsPrincipal userClaims, IStringLocalizer localizer)
    {
        var id = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = userClaims.FindFirstValue(ClaimTypes.Email);
        var username = userClaims.FindFirstValue(ClaimTypes.Name);
        var roles = userClaims.Claims.Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList()
                    ?? [];

        if (id.IsNullOrWhiteSpace() || email.IsNullOrWhiteSpace() || username.IsNullOrWhiteSpace())
            return Results.Unauthorized();

        var response = new AuthPingResponse(id, email, username, roles);
        return Results.Json(response);
    }
}