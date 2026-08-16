using OrderAPI.Application.Common.Interfaces;

namespace OrderAPI.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirst("oid")?.Value
        ?? User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

    public string? UserName => User?.Identity?.Name;

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
