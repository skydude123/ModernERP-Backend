using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using twbERRANDSapi.AUTH;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twbERRANDSapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }
        // GET: api/<AuthController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<AuthController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        [AllowAnonymous]
        [HttpPost("latest/login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "User ID and password are required."
                });
            }

            var result = await _repo.LoginAsync(request, cancellationToken);

            if (!result.Success || result.User is null)
            {
                return Unauthorized(result);
            }

            var user = result.User;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId),
                new(ClaimTypes.Name, user.FullName),
                new("user_id", user.UserId),
                new("branch", user.Branch),
                new("department", user.Dept),
                new("user_rights", user.UserRights),
                new("administrator", user.Administrator)
            };

            if (!string.IsNullOrWhiteSpace(user.UserRights))
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    user.UserRights.Trim().ToUpperInvariant()));
            }

            if (string.Equals(
                    user.Administrator,
                    "Y",
                    StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));
            }

            claims.AddRange(
                user.Permissions
                    .Where(permission => permission.CanOpen)
                    .Select(permission =>
                        permission.PermissionKey.Trim().ToUpperInvariant())
                    .Where(permissionKey => permissionKey.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(permissionKey =>
                        new Claim("erp_permission", permissionKey)));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                    IsPersistent = true
                });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return NoContent();
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        //{
        //    if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
        //    {
        //        return BadRequest(new LoginResponse
        //        {
        //            Success = false,
        //            Message = "User ID and password are required."
        //        });
        //    }

        //    var user = await _repo.LoginAsync(request.UserId.Trim(), request.Password, cancellationToken);

        //    if (user is null)
        //    {
        //        return Unauthorized(new LoginResponse
        //        {
        //            Success = false,
        //            Message = "Invalid credentials."
        //        });
        //    }

        //    return Ok(new LoginResponse
        //    {
        //        Success = true,
        //        Message = "Login successful.",
        //        User = user
        //    });
        //}

        // PUT api/<AuthController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AuthController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
