using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.BisleriumBlog;
using Infrastructure.BisleriumBlog; // Assuming NotificationServices is here

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly NotificationServices _notificationServices; 

    public AuthenticationController(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        NotificationServices notificationServices) 
    {
        _userManager = userManager;
        _configuration = configuration;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _notificationServices = notificationServices;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginUser)

    {
        Console.WriteLine(loginUser);
        Console.WriteLine("AA");
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse(false, null, "Invalid model state"));
        }

        Console.WriteLine(loginUser);

        var user = await _userManager.FindByEmailAsync(loginUser.Email!);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginUser.Password!))
        {
            return Unauthorized(new LoginResponse(false, null, "Invalid login attempt"));
        }

        if (!string.IsNullOrWhiteSpace(loginUser.DeviceToken))
        {
            await _notificationServices.SaveToken(new MobileTokens { UserId = user.Id, Token = loginUser.DeviceToken });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userSession = new UserSession(user.Id.ToString(), user.UserName, user.Email, roles.FirstOrDefault());
        string token = GenerateJSONWebToken(userSession);

        return Ok(new LoginResponse(true, token, "Login completed", roles.FirstOrDefault(), user.UserName, user.Image, user.Id, user.Email));
    }

    private string GenerateJSONWebToken(UserSession userInfo)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, userInfo.Email),
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
            new Claim(ClaimTypes.Role, userInfo.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginResponse(bool Flag, string Token, string Message, string? Role = "", string? Name = "", string? Image = "", string? UserID = "", string? Email = "");
    public record UserSession(string? Id, string? Name, string? Email, string? Role);
}
