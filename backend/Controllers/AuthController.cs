using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TriviaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
            return Unauthorized("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized("Invalid username or password.");

        var token = GenerateJwt(user);
        return Ok(new LoginResponse
        {
            Token = token,
            UserName = user.UserName ?? request.UserName,
            UserId = user.Id
        });
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        return Ok(new CurrentUserResponse
        {
            UserId = userId,
            UserName = userName ?? string.Empty
        });
    }

    private string GenerateJwt(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetJwtSecret()
    {
        var secret = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("Jwt:Key is not set. Use user secrets or appsettings.");
        return secret;
    }
}

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class CurrentUserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
