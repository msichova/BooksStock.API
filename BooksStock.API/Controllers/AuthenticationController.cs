using Asp.Versioning;
using BooksStock.API.Models.Authentication;
using BooksStock.API.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BooksStock.API.Controllers
{
    [ApiVersion("1")]
    [ApiController]
    [Route("authorization/")]
    public class AuthenticationController(UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthenticationController> logger) : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthenticationController> _logger = logger;

        #region of Hellp Mehtods
        private void LogingError(Exception error) => _logger.LogError(message: error.Message, args: error.StackTrace);
        private void LogingInformation(string message) => _logger.LogInformation(message: message);
        private void LogingWarning(string message) => _logger.LogWarning(message: message);
        #endregion

        [HttpPost, Route("loging")]
        public async Task<ActionResult> LoginAdmin([FromForm]LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserLogin);

                if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    if (!userRoles.Contains(ApiRolesConstants.Admin))
                    {
                        LogingWarning((@"Unauthorized access rejected for: {@user}, at {@DateTime}", user, DateTime.Now).ToString());
                        return Unauthorized("Access denied for user: '" + user.UserName + "', current user does not have authority");
                    }

                    List<Claim> authorizationClaims =
                    [
                        new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    ];

                    foreach (var userRole in userRoles)
                    {
                        authorizationClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authorizationSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(1),
                        claims: authorizationClaims,
                        signingCredentials: new SigningCredentials(authorizationSignInKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        exparation = token.ValidTo
                    });
                }

                LogingWarning((@"Unauthorized access rejected for: {@user}, at {@DateTime}", user, DateTime.Now).ToString());
                return Unauthorized(user is null ? "Not registrated user:'" + model.UserLogin + "'." : "Entered incorrect password");
            }
            catch (Exception error)
            {
                LogingError(error);
                return Problem(error.Message);
            }
        }

        [HttpPost, Route("register")]
        public async Task<ActionResult> Register([FromForm]RegisterModel model)
        {
            try
            {
                var emailExists = await _userManager.FindByEmailAsync(model.Email);
                var userNameExists = await _userManager.FindByNameAsync(model.Login);

                if(emailExists is not null || userNameExists is not null)
                {
                    string message = emailExists is not null && userNameExists is not null ?
                        "User with this email and login already registrated" :
                        emailExists is not null ?
                        "User with this email already registrated" :
                        "User with this login already registrated";
                    return Problem(message);
                }

                ApiUser apiUser = new()
                {
                    Email = model.Email,
                    UserName = model.Login,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(apiUser, model.Password);

                if(!result.Succeeded)
                {
                    return Problem("Unable registrate user with entered details. Please check details.");
                }

                if(!await _roleManager.RoleExistsAsync(ApiRolesConstants.Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(ApiRolesConstants.Admin));
                }
                if(!await _roleManager.RoleExistsAsync(ApiRolesConstants.User))
                {
                    await _roleManager.CreateAsync(new IdentityRole(ApiRolesConstants.User));
                }

                if(await _roleManager.RoleExistsAsync(ApiRolesConstants.Admin))
                {
                    await _userManager.AddToRoleAsync(apiUser, ApiRolesConstants.Admin);
                }
                string roles = "";
                (await _userManager.GetRolesAsync(apiUser)).ToList().ForEach(role => roles += role);
                return Ok(new
                {
                    login = apiUser.UserName,
                    email = apiUser.Email,
                    role = roles,
                    message = "Registrated successully"
                });
            }
            catch(Exception error)
            {
                LogingError(error);
                return Problem(error.Message);
            }
        }
    }
}
