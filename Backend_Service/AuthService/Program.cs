using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";

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

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.SameSite = SameSiteMode.None;

        // Avoid requesting a token for the same app registration.
        // AADSTS90009 is raised when the configured scope points back to this app.
        // For a downstream API, configure a separate API scope such as api://<OrderApi-AppId>/access_as_user.
        var scope = builder.Configuration["AzureAd:Scope"];
        var clientId = builder.Configuration["AzureAd:ClientId"];

        if (!string.IsNullOrWhiteSpace(scope) &&
            !string.IsNullOrWhiteSpace(clientId) &&
            scope.Contains(clientId, StringComparison.OrdinalIgnoreCase))
        {
            // Skip the self-scope to prevent Entra from rejecting the token request.
        }
        else if (!string.IsNullOrWhiteSpace(scope))
        {
            options.Scope.Add(scope);
        }

        options.Events.OnTokenValidated = context =>
        {
            var accessToken = context.TokenEndpointResponse?.AccessToken;
            var idToken = context.TokenEndpointResponse?.IdToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.HttpContext.Response.Cookies.Append("access_token", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Requires HTTPS
                    SameSite = SameSiteMode.None, // Needed for cross-port/cross-origin local dev
                    Expires = DateTime.UtcNow.AddHours(1)
                });
            }
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            var redirectUri = builder.Configuration["AzureAd:RedirectUri"];
            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                redirectUri = $"{context.Request.Scheme}://{context.Request.Host}{options.CallbackPath}";
            }

            context.ProtocolMessage.RedirectUri = redirectUri;

            // If it's an API call that is unauthorized, return 401 instead of redirecting to login
            if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == 401)
            {
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
