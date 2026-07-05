using Context;
using Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Middleware;
using Messaging;
using RabbitMQ.Client;
using Repository;
using Services;
using System.Net;
using System.Text;
using Workers;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMq:HostName"] ?? "localhost";
var rabbitPort = int.TryParse(builder.Configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;
var rabbitUser = builder.Configuration["RabbitMq:UserName"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = rabbitHost,
    Port = rabbitPort,
    UserName = rabbitUser,
    Password = rabbitPass,
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var connectionString = builder.Configuration.GetConnectionString("FIAPGamesConnection");

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Configuration key Jwt:Key is required.");
}

var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em prod, mude para true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
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

app.UseMiddleware<ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    var logger = app.Logger;
    const int maxAttempts = 20;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
            await DbInitializer.SeedAsync(context);
            logger.LogInformation("Database initialized successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex,
                "Database initialization failed (attempt {Attempt}/{MaxAttempts}). Retrying in 5 seconds...",
                attempt,
                maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
