namespace API.Middleware;

public class RequiredHeaderValidator
{
    private readonly RequestDelegate _next;
    private readonly string _requiredHeaderKey;
    private readonly string _requiredHeaderValue;
    private readonly PathString? _validatePathWithSegments;

    /// <summary>
    ///
    /// </summary>
    /// <param name="headerKey">Key of the required header</param>
    /// <param name="headerValue">Value of the required header</param>
    /// <param name="validatePathWithSegments">Validate only if the relative url of the context starts with this value</param>
    public RequiredHeaderValidator(
        RequestDelegate next,
        string headerKey,
        string headerValue,
        PathString? validatePathWithSegments = null
    )
    {
        _next = next;
        _requiredHeaderKey = headerKey;
        _requiredHeaderValue = headerValue;
        _validatePathWithSegments = validatePathWithSegments;
    }

    public async Task Invoke(HttpContext context)
    {
        if (
            _validatePathWithSegments is not null
            && context.Request.Path.StartsWithSegments(_validatePathWithSegments.Value) == false
        )
        {
            await _next(context);
            return;
        }

        if (
            context.Request.Headers.TryGetValue(_requiredHeaderKey, out var headerValue) == false
            || string.Equals(headerValue, _requiredHeaderValue, StringComparison.OrdinalIgnoreCase)
                == false
        )
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync($"Forbidden: Invalid {_requiredHeaderKey} header");
            return;
        }

        await _next(context);
    }
}
