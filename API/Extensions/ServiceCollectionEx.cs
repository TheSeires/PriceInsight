using API.Models.Application;
using API.Models.Database;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace API.Extensions;

public static class ServiceCollectionEx
{
    public static IServiceCollection AddJsonLocalization(
        this IServiceCollection services,
        string resourcePath)
    {
        services.AddLocalization(options => options.ResourcesPath = resourcePath);
        services.AddSingleton<IStringLocalizerFactory>(provider =>
        {
            var env = provider.GetRequiredService<IWebHostEnvironment>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var basePath = Path.Combine(env.ContentRootPath, resourcePath);
            return new JsonStringLocalizerFactory(basePath, loggerFactory, httpContextAccessor);
        });
        services.AddSingleton(provider =>
        {
            var factory = provider.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create(null!, null!);
        });
        services.AddSingleton<LocalizationFileProvider>();

        return services;
    }

    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        WebApplicationBuilder builder,
        MongoDbConnectionSettings mongoDbSettings)
    {
        services.AddSingleton(mongoDbSettings);

        var camelCaseConventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreIfNullConvention(true),
        };
        ConventionRegistry.Register("camelCase", camelCaseConventionPack, type => true);
        BaseAggregateOperator.UseCamelCaseFieldNames = true;

        services.AddSingleton<IMongoClient>(s => new MongoClient(mongoDbSettings.ConnectionString));
        services.AddScoped<IMongoDatabase>(s =>
        {
            var client = s.GetRequiredService<IMongoClient>();
            var settings = s.GetRequiredService<MongoDbConnectionSettings>();
            return client.GetDatabase(settings.DatabaseName);
        });

        return services;
    }

    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        MongoDbConnectionSettings mongoDbSettings)
    {
        services.AddTransient<EmailConfirmationTokenProvider<User>>();
        services
            .AddIdentity<User, UserRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 0;

                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.ProviderMap.Add(
                    "EmailConfirmation",
                    new TokenProviderDescriptor(typeof(EmailConfirmationTokenProvider<User>))
                );
                options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
            })
            .AddDefaultTokenProviders()
            .AddMongoDbStores<User, UserRole, Guid>(
                mongoDbSettings.ConnectionString,
                mongoDbSettings.DatabaseName
            );

        return services;
    }

    public static IServiceCollection AddMarketCrawlersConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        var crawlersSettings = builder
            .Configuration.GetSection("MarketCrawlers")
            .Get<Dictionary<string, MarketCrawlerSettings>>();

        ArgumentNullException.ThrowIfNull(crawlersSettings);

        var crawlersConfiguration = new MarketCrawlersConfiguration(crawlersSettings);
        services.AddSingleton(crawlersConfiguration);

        return services;
    }
}
