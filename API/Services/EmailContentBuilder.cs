using API.Services.Interfaces;
using Microsoft.Extensions.Localization;

namespace API.Services;

public class EmailContentBuilder : IEmailContentBuilder
{
    private readonly IStringLocalizer _localizer;

    public EmailContentBuilder(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public string GenerateEmailConfirmation(string confirmationLink, string userName)
    {
        var bodyContent =
            $@"<div class=""container"">
                <h2>{_localizer["emailService.common.greeting", userName]}</h2>
                <p>{_localizer["emailService.registration.requestToConfirmEmail"]}:</p>
                <a href=""{confirmationLink}"" class=""button"">{_localizer["emailService.registration.confirmEmail"]}</a>
                <p>{_localizer["emailService.registration.ignoreIfHaveNotRegistered"]}</p>
            </div>";

        return GenerateHtmlDocument(
            _localizer["emailService.registration.emailTitle"],
            bodyContent,
            _commonCssStyles
        );
    }

    private const string _commonCssStyles =
        $@"
            body {{
                    font-family: Arial, sans-serif;
                    color: #333333;
            }}
            .container {{
                max-width: 600px;
                margin: 0 auto;
                padding: 20px;
                border: 1px solid #dddddd;
                border-radius: 10px;
                background-color: #f9f9f9;
            }}
            .button {{
                display: inline-block;
                padding: 10px 20px;
                margin-top: 20px;
                font-size: 16px;
                color: #101719;
                background-color: #00E096;
                text-decoration: none;
                border-radius: 5px;
            }}
            .button:hover {{
                background-color: #0056b3;
            }}
        ";

    private string GenerateHtmlDocument(string title, string bodyContent, string styles)
    {
        return $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>{title}</title>
                <style>
                    {styles}
                </style>
            </head>
            <body>
                {bodyContent}
            </body>
            </html>";
    }
}
