using API.Endpoints;
using API.Endpoints.V1;
using API.Extensions;
using API.Middleware;
using API.Models.Application;
using API.Services;
using API.Services.Background;
using API.Services.Crawlers;
using API.Services.Interfaces;
using API.Services.Repositories;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddLazyCache();

builder.Services.AddHttpContextAccessor(); // required for localization
builder.Services.AddTransient<SimpleHttpLoggingHandler>();
builder.Services.AddHttpClient(HttpClients.Simple) // required for services
    .RemoveAllLoggers()
    .AddHttpMessageHandler<SimpleHttpLoggingHandler>();

builder.Services.AddJsonLocalization("Resources/Locales");

var mongoDbSettings = builder
    .Configuration.GetSection("MongoDatabaseConnection")
    .Get<MongoDbConnectionSettings>();
ArgumentNullException.ThrowIfNull(mongoDbSettings);

builder.Services.AddDatabaseServices(builder, mongoDbSettings);
builder.Services.AddSingleton<IServiceStateTracker, ServiceStateTracker>();

// Raw repository services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPriceEntryService, PriceEntryService>();
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<ICrawlerHistoryService, CrawlerHistoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryMappingService, CategoryMappingService>();
builder.Services.AddScoped<ICategorizationIssueService, CategorizationIssueService>();
builder.Services.AddScoped<IProductFollowingService, ProductFollowingService>();

// Cached repository services
builder.Services.AddScoped<ICachedProductService, CachedProductService>();
builder.Services.AddScoped<ICachedPriceEntryService, CachedPriceEntryService>();
builder.Services.AddScoped<ICachedCategoryService, CachedCategoryService>();
builder.Services.AddScoped<ICachedCategoryMappingService, CachedCategoryMappingService>();
builder.Services.AddScoped<ICachedCategorizationIssueService, CachedCategorizationIssueService>();
builder.Services.AddScoped<ICachedMarketService, CachedMarketService>();
builder.Services.AddScoped<ICachedProductFollowingService, CachedProductFollowingService>();

// Crawlers configuration
builder.Services.AddMarketCrawlersConfiguration(builder);

// Crawler services
builder.Services.AddSingleton<IMarketCrawler, AtbCrawler>();
builder.Services.AddSingleton<IMarketCrawler, RukavychkaCrawler>();

// Background services
builder.Services.AddSingleton<MarketCrawlersStarter>();
builder.Services.AddHostedService<MarketCrawlersStarter>(provider =>
    provider.GetRequiredService<MarketCrawlersStarter>());
builder.Services.AddSingleton<CategoryUpdaterService>();
builder.Services.AddHostedService<CategoryUpdaterService>(provider =>
    provider.GetRequiredService<CategoryUpdaterService>());

// Email services
builder.Services.AddSingleton<IEmailContentBuilder, EmailContentBuilder>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// SPA integration

// Other
builder.Services.AddSingleton<IProductMatcher, ProductMatcher>();
builder.Services.AddSingleton<IProductMatchingStrategy, FuzzyProductMatchingStrategy>();
builder.Services.AddScoped<ICategoryUpdateManager, CategoryUpdateManager>();
builder.Services.AddScoped<IProductManager, ProductManager>();

builder.Services.AddIdentityServices(mongoDbSettings);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Localization
var supportedCultures = new[] { "en", "uk" };
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(supportedCultures[0]);
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// Control SPA routes
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarUi();
    app.UseCors("Development");
}
else
{
    app.UseSpaMiddleware("/index.html");
    app.MapFallbackToFile("index.html");
    app.UseStaticFiles();
}

app.UseHttpsRedirection();
app.UseMiddleware<RequiredHeaderValidator>("Host-Name", "PriceInsight", new PathString("/api/"));

// Map minimal API endpoints
app.MapLocalizationEndpoints();
app.MapAuthEndpointsV1();
app.MapUserEndpointsV1();
app.MapCategorizationIssueEndpointsV1();
app.MapCategoryEndpointsV1();
app.MapCategoryMappingEndpointsV1();
app.MapProductEndpointsV1();
app.MapServiceEndpointsV1();
app.MapMarketEndpointsV1();
app.MapPriceEntryEndpointsV1();

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EnsureUserExists>();

app.Run();