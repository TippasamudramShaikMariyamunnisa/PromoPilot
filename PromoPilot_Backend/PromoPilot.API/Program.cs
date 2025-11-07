using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Mapping; 
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;
using PromoPilot.Infrastructure.Formatters;
using PromoPilot.Infrastructure.Identity;
using PromoPilot.Infrastructure.Repositories;
using QuestPDF.Infrastructure;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
//Replace default logging
builder.Host.UseSerilog();

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ------------------ Add Services ------------------


builder.Services.AddDbContext<PromoPilotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICsvExporter, CsvExporter>();
builder.Services.AddScoped<IExcelExporter, ExcelExporter>();
builder.Services.AddScoped<IAuditLoggingService, AuditLoggingService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IEmailService, AzureEmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"];
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero, // Optional: reduce token expiry delay
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

/// Swagger Configuration with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PromoPilot API", Version = "v1" });
    c.SupportNonNullableReferenceTypes();
    c.CustomSchemaIds(type => type.FullName); 
    c.SupportNonNullableReferenceTypes();
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLoggingService,AuditLoggingService>();
builder.Services.AddHttpContextAccessor();

// Register Services
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ICampaignReportService, CampaignReportService>();
builder.Services.AddScoped<IEngagementService, EngagementService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IExecutionStatusService, ExecutionStatusService>();

// Register Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IExecutionStatusRepository, ExecutionStatusRepository>();
builder.Services.AddScoped<IEngagementRepository, EngagementRepository>();
builder.Services.AddScoped<ICampaignReportRepository, CampaignReportRepository>();

// Register UseCases for Campaigns
builder.Services.AddScoped<ICampaignModule, CampaignModule>();
//Register UseCases for Customers
builder.Services.AddScoped<ICustomerModule, CustomerModule>();
// Register UseCases for Sales
builder.Services.AddScoped<ISaleModule, SaleModule>();
// Register UseCases for Budgets
builder.Services.AddScoped<IBudgetModule, BudgetModule>();
// Register UseCases for Engagements
builder.Services.AddScoped<IEngagementModule, EngagementModule>();
//Register UseCases for Products
builder.Services.AddScoped<IProductModule, ProductModule>();
//Register UseCases for ExecutionStatus
builder.Services.AddScoped<IExecutionStatusModule, ExecutionStatusModule>();
//Register UseCases for CampaignReport
builder.Services.AddScoped<ICampaignReportModule, CampaignReportModule>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});


builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true; // Enables content negotiation
    options.OutputFormatters.Add(new CsvOutputFormatter());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
})
.AddXmlSerializerFormatters(); // Adds XML support
builder.Services.AddScoped<CampaignPdfGenerator>();
QuestPDF.Settings.License = LicenseType.Community;
var app = builder.Build();


app.UseSerilogRequestLogging(); // Logs HTTP requests

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PromoPilot API v1");
    c.RoutePrefix = string.Empty; // Optional: serve at root
});

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // For Web API

app.Run();
