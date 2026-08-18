using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Authentication;
using Context;
using Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Middleware;
using Messaging;
using Prometheus;
using RabbitMQ.Client;
using Repository;
using Services;
using System.Net;
using Workers;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var keyVaultUriValue = Environment.GetEnvironmentVariable("KeyVaultUri");
    if (!Uri.TryCreate(keyVaultUriValue, UriKind.Absolute, out var keyVaultUri) ||
        keyVaultUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "Environment variable KeyVaultUri must contain a valid HTTPS URI.");
    }

    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}

var rabbitHost = builder.Configuration["RabbitMq:HostName"] ?? "localhost";
var rabbitPort = int.TryParse(builder.Configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;
var rabbitUser = builder.Configuration["RabbitMq:UserName"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

var rabbitConnectionFactory = new ConnectionFactory
{
    HostName = rabbitHost,
    Port = rabbitPort,
    UserName = rabbitUser,
    Password = rabbitPass,
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};
var rabbitHealthConnection = new Lazy<Task<IConnection>>(
    () => rabbitConnectionFactory.CreateConnectionAsync());

builder.Services.AddSingleton<IConnectionFactory>(rabbitConnectionFactory);

// Add services to the container.

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
}
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "FiapGamesCatalog:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();

builder.Services.AddScoped<IBibliotecaRepository, BibliotecaRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IJogoRepository, JogoRepository>();
builder.Services.AddScoped<IPromocaoRepository, PromocaoRepository>();

builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IJogoService, JogoService>();
builder.Services.AddScoped<IPromocaoService, PromocaoService>();

builder.Services.AddSingleton<ICompraEventPublisher, RabbitMqCompraEventPublisher>();
builder.Services.AddHostedService<BaixarEstoqueWorker>();

var connectionString = builder.Configuration.GetConnectionString("CatalogConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string CatalogConnection is required.");
}

var jwksUri = ResolveJwksUri(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.ConfigurationManager =
        new ConfigurationManager<OpenIdConnectConfiguration>(
            jwksUri.AbsoluteUri,
            new JwksConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = false });
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RequireSignedTokens = true,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
        ClockSkew = TimeSpan.Zero // Remove o tempo de tolerância padrão do .NET
    };
});

builder.Services.AddAuthorization();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erro = new
        {
            erro = "Requisição inválida",
            status = 400,
            detalhes = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage)
                )
        };

        return new BadRequestObjectResult(erro);
    };
});

builder.Services.AddDbContext<CatalogContext>(opts =>
    opts
        .UseLazyLoadingProxies()
        .UseSqlServer(
            connectionString,
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"])
    .AddRabbitMQ(
        _ => rabbitHealthConnection.Value,
        name: "rabbitmq",
        tags: ["ready"]);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var erros = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors
                .Select(e => $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            return new BadRequestObjectResult(new
            {
                erro = "Requisição inválida",
                status = (int)HttpStatusCode.BadRequest,
                detalhes = erros
            });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();

    await context.Database.MigrateAsync();
    await DbInitializer.SeedAsync(context);

    Console.WriteLine("Catalog database migrations and seeds applied successfully.");
    return;
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpMetrics();


app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/health"),
    branch => branch.UseHttpsRedirection());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics("/metrics");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

static Uri ResolveJwksUri(IConfiguration configuration)
{
    var configuredJwksUri = configuration["Jwt:JwksUri"];
    if (!string.IsNullOrWhiteSpace(configuredJwksUri) &&
        Uri.TryCreate(configuredJwksUri, UriKind.Absolute, out var jwksUri) &&
        IsHttpUri(jwksUri))
    {
        return jwksUri;
    }

    var authority = configuration["Jwt:Authority"]?.TrimEnd('/');
    if (!string.IsNullOrWhiteSpace(authority) &&
        Uri.TryCreate($"{authority}/.well-known/jwks", UriKind.Absolute, out jwksUri) &&
        IsHttpUri(jwksUri))
    {
        return jwksUri;
    }

    throw new InvalidOperationException(
        "Configure Jwt:JwksUri or a valid absolute Jwt:Authority.");
}

static bool IsHttpUri(Uri uri) =>
    uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
    uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
