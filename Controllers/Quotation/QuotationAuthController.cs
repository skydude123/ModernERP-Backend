
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using HRIStwinbeeAPI.Repo.Interface.Quotation;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using twbERRANDSapi.AUTH;

namespace HRIStwinbeeAPI.Controllers.Quotation
{
    [ApiController]
    [Route("api/quotation/auth")]
    public sealed class QuotationAuthController : ControllerBase
    {
        private const string AuthScheme = "QuotationCookie";

        private readonly IQuotationAuthRepository _authRepository;
        private readonly ILogger<QuotationAuthController> _authLogger;

        public QuotationAuthController(
            IQuotationAuthRepository authRepository,
            ILogger<QuotationAuthController> authLogger)
        {
            _authRepository = authRepository;
            _authLogger = authLogger;
        }

        // =========================================================
        // LOGIN
        // POST /api/quotation/auth/login
        // =========================================================

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "User ID and password are required."
                });
            }

            if (request.UserId.Trim().Length > 10 ||
                request.Password.Length > 20)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials format."
                });
            }

            LoginResponse result;

            try
            {
                result = await _authRepository.LoginAsync(
                    request,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _authLogger.LogError(
                    ex,
                    "TPAQUOTATION authentication failed.");

                return Problem(
                    title: "Authentication service unavailable.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (!result.Success || result.User == null)
            {
                return Unauthorized(result);
            }

            var user = result.User;

            var administrator =
                (user.Administrator ?? "N")
                    .Trim()
                    .ToUpperInvariant();

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.UserId
                ),
                new(
                    ClaimTypes.Name,
                    user.FullName ?? user.UserId
                ),
                new(
                    "user_id",
                    user.UserId
                ),
                new(
                    "branch",
                    user.Branch ?? string.Empty
                ),
                new(
                    "department",
                    user.Dept ?? string.Empty
                ),
                new(
                    "user_rights",
                    user.UserRights ?? string.Empty
                ),
                new(
                    "administrator",
                    administrator
                ),
                new(
                    "application",
                    "TPAQUOTATION"
                )
            };

            if (!string.IsNullOrWhiteSpace(user.UserRights))
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    user.UserRights.Trim().ToUpperInvariant()));
            }

            if (administrator == "Y")
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    "ADMIN"));
            }

            // Effective permissions are calculated by SQL.
            // No automatic permission grant inside the controller.

            var permissions =
                user.Permissions ?? new List<UserPermissionDto>();

            foreach (var permission in permissions)
            {
                if (string.IsNullOrWhiteSpace(
                    permission.PermissionKey))
                {
                    continue;
                }

                var key = permission.PermissionKey
                    .Trim()
                    .ToUpperInvariant();

                if (permission.CanView)
                {
                    claims.Add(new Claim(
                        "erp_permission_view",
                        key));
                }

                if (permission.CanView && permission.CanOpen)
                {
                    claims.Add(new Claim(
                        "erp_permission",
                        key));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                AuthScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                AuthScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                });

            return Ok(result);
        }

        // =========================================================
        // CURRENT SESSION
        // GET /api/quotation/auth/me
        // =========================================================

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = AuthScheme)]
        public IActionResult Me()
        {
            if (!User.HasClaim(
                    "application",
                    "TPAQUOTATION"))
            {
                return Forbid(AuthScheme);
            }

            var viewPermissions = User.Claims
                .Where(x => x.Type == "erp_permission_view")
                .Select(x => x.Value.Trim().ToUpperInvariant())
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var openPermissions = User.Claims
                .Where(x => x.Type == "erp_permission")
                .Select(x => x.Value.Trim().ToUpperInvariant())
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var permissionKeys = viewPermissions
                .Union(
                    openPermissions,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var effectivePermissions = permissionKeys
                .Select(key => new
                {
                    permissionKey = key,
                    canView = viewPermissions.Contains(key),
                    canOpen =
                        viewPermissions.Contains(key) &&
                        openPermissions.Contains(key)
                })
                .ToList();

            return Ok(new
            {
                userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier),

                fullName = User.Identity?.Name,

                branch = User.FindFirstValue("branch"),

                department = User.FindFirstValue("department"),

                userRights = User.FindFirstValue("user_rights"),

                administrator = User.FindFirstValue("administrator"),

                application = User.FindFirstValue("application"),

                permissions = effectivePermissions
            });
        }

        // =========================================================
        // LOGOUT
        // POST /api/quotation/auth/logout
        // =========================================================

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = AuthScheme)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthScheme);

            return NoContent();
        }
    }
}
