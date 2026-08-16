using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Data;
using OrderAPI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
var tenantId    = builder.Configuration["AzureAd:TenantId"]!;
var clientId    = builder.Configuration["AzureAd:ClientId"]!;
var audience    = builder.Configuration["AzureAd:Audience"] ?? clientId;
var validIssuers = new[]
{
    $"https://login.microsoftonline.com/{tenantId}/v2.0",
    $"https://sts.windows.net/{tenantId}/"
};

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Validate the OrderAPI access token stored in the access_token cookie by AuthService.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Entra ID v2.0 OIDC endpoint — downloads signing keys automatically
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.IncludeErrorDetails = true;
        options.MapInboundClaims = false;
        options.RefreshOnIssuerKeyNotFound = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer    = true,
            ValidIssuers      = validIssuers,
            IssuerValidator   = (issuer, securityToken, validationParameters) =>
            {
                if (validIssuers.Contains(issuer, StringComparer.OrdinalIgnoreCase))
                {
                    return issuer;
                }

                throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
            },
            ValidateAudience  = true,
            ValidAudiences    = new[] { audience, clientId, $"api://{clientId}" },
            ValidateLifetime  = true,
            // Map the standard "roles" claim used by Entra ID App Roles
            RoleClaimType     = "roles",
            NameClaimType     = "name"
        };

        options.Events = new JwtBearerEvents
        {
            // Read the token from the HttpOnly cookie set by AuthService
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            // Log auth failures to help debugging
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[OrderAPI] JWT auth failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


