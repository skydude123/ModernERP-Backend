
using HRIStwinbeeAPI.Models.Security;
using HRIStwinbeeAPI.Repositories.Security;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using System.Security.Claims;

namespace HRIStwinbeeAPI.Controllers.Security
{
    [ApiController]
    [Route("api/security")]
    [Authorize(
        AuthenticationSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme
            + ",QuotationCookie"
    )]
    public class SecurityUsersController : ControllerBase
    {
        private readonly SecurityUserRepository _repository;

        public SecurityUsersController(
            SecurityUserRepository repository)
        {
            _repository = repository;
        }

        // ====================================================
        // AUTHENTICATED ADMIN VALIDATION
        // ====================================================

        private bool IsAdministrator()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var adminClaim =
                User.FindAll("administrator")
                    .Any(claim =>
                        string.Equals(
                            claim.Value.Trim(),
                            "Y",
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

            return adminClaim || User.IsInRole("ADMIN");
        }

        private IActionResult? RequireAdministrator()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new
                {
                    message = "Authentication required."
                });
            }

            if (!IsAdministrator())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Administrator permission is required."
                    }
                );
            }

            return null;
        }

        private string? GetCurrentUserId()
        {
            return
                User.FindFirstValue("user_id")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name);
        }

        // ====================================================
        // GET /api/security/users
        // ====================================================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search = null)
        {
            var denied = RequireAdministrator();

            if (denied is not null)
            {
                return denied;
            }

            try
            {
                var users =
                    await _repository.GetUsersAsync(search);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to load users.",
                    detail: ex.Message,
                    statusCode:
                        StatusCodes.Status500InternalServerError
                );
            }
        }

        // ====================================================
        // GET /api/security/users/{userId}
        // ====================================================

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(
            string userId)
        {
            var denied = RequireAdministrator();

            if (denied is not null)
            {
                return denied;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new
                {
                    message = "User ID is required."
                });
            }

            try
            {
                var result =
                    await _repository.GetUserAsync(
                        userId.Trim()
                    );

                if (result is null)
                {
                    return NotFound(new
                    {
                        message =
                            $"User '{userId}' was not found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to load user.",
                    detail: ex.Message,
                    statusCode:
                        StatusCodes.Status500InternalServerError
                );
            }
        }

        // ====================================================
        // GET /api/security/modules
        // ====================================================

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            var denied = RequireAdministrator();

            if (denied is not null)
            {
                return denied;
            }

            try
            {
                var modules =
                    await _repository.GetModulesAsync();

                return Ok(modules);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to load security modules.",
                    detail: ex.Message,
                    statusCode:
                        StatusCodes.Status500InternalServerError
                );
            }
        }

        // ====================================================
        // POST /api/security/users
        // CREATE USER
        // ====================================================

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser(
            [FromBody] SaveSecurityUserRequest request)
        {
            var denied = RequireAdministrator();

            if (denied is not null)
            {
                return denied;
            }

            if (request is null)
            {
                return BadRequest(new
                {
                    message = "User information is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new
                {
                    message = "User ID is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new
                {
                    message = "First name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message =
                        "Password is required for a new user."
                });
            }

            var actorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                return Unauthorized(new
                {
                    message =
                        "Unable to determine the logged-in user."
                });
            }

            try
            {
                // SQL procedure must also validate actor admin.
                await _repository.SaveAsync(
                    actorUserId,
                    request
                );

                return Ok(new
                {
                    success = true,
                    userId = request.UserId.Trim(),
                    message = "User created successfully."
                });
            }
            catch (SqlException ex)
                when (ex.Number >= 50000)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to create user.",
                    detail: ex.Message,
                    statusCode:
                        StatusCodes.Status500InternalServerError
                );
            }
        }

        // ====================================================
        // PUT /api/security/users/{userId}
        // UPDATE USER
        // ====================================================

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(
            string userId,
            [FromBody] SaveSecurityUserRequest request)
        {
            var denied = RequireAdministrator();

            if (denied is not null)
            {
                return denied;
            }

            if (request is null)
            {
                return BadRequest(new
                {
                    message = "User information is required."
                });
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new
                {
                    message = "User ID is required."
                });
            }

            if (!string.Equals(
                    userId.Trim(),
                    request.UserId?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "User ID in URL does not match request."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new
                {
                    message = "First name is required."
                });
            }

            var actorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                return Unauthorized(new
                {
                    message =
                        "Unable to determine the logged-in user."
                });
            }

            try
            {
                await _repository.SaveAsync(
                    actorUserId,
                    request
                );

                return Ok(new
                {
                    success = true,
                    userId = request.UserId.Trim(),
                    message = "User updated successfully."
                });
            }
            catch (SqlException ex)
                when (ex.Number >= 50000)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to update user.",
                    detail: ex.Message,
                    statusCode:
                        StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}
