
using System.Security.Claims;
using HRIStwinbeeAPI.Models.Security;
using HRIStwinbeeAPI.Repo.Quotation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HRIStwinbeeAPI.Controllers.Quotation
{
    [ApiController]
    [Route("api/quotation/security")]
    [Authorize(
        AuthenticationSchemes = "QuotationCookie"
    )]
    public sealed class QuotationSecurityUsersController
        : ControllerBase
    {
        private readonly QuotationSecurityUserRepository
            _repository;

        private readonly ILogger<QuotationSecurityUsersController>
            _logger;

        public QuotationSecurityUsersController(
            QuotationSecurityUserRepository repository,
            ILogger<QuotationSecurityUsersController> logger
        )
        {
            _repository = repository;
            _logger = logger;
        }

        // ====================================================
        // CURRENT AUTHENTICATED USER
        // ====================================================

        private string? ActorUserId =>
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        private bool IsAdministrator =>
            string.Equals(
                User.FindFirstValue("administrator")?.Trim(),
                "Y",
                StringComparison.OrdinalIgnoreCase
            );

        private IActionResult? CheckAccess()
        {
            if (string.IsNullOrWhiteSpace(ActorUserId))
            {
                return Unauthorized(new
                {
                    message = "Authentication required."
                });
            }

            if (!IsAdministrator)
            {
                return Forbid();
            }

            return null;
        }

        // ====================================================
        // GET /api/quotation/security/users
        // ====================================================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            CancellationToken cancellationToken
        )
        {
            var denied = CheckAccess();

            if (denied is not null)
            {
                return denied;
            }

            try
            {
                var users = await _repository.GetUsersAsync(
                    search,
                    cancellationToken
                );

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load TWINBEE users."
                );

                return Problem(
                    title: "Unable to load users.",
                    statusCode: 500
                );
            }
        }

        // ====================================================
        // GET /api/quotation/security/modules
        // ====================================================

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules(
            CancellationToken cancellationToken
        )
        {
            var denied = CheckAccess();

            if (denied is not null)
            {
                return denied;
            }

            try
            {
                var modules =
                    await _repository.GetModulesAsync(
                        cancellationToken
                    );

                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load TWINBEE security modules."
                );

                return Problem(
                    title: "Unable to load modules.",
                    statusCode: 500
                );
            }
        }

        // ====================================================
        // GET /api/quotation/security/users/{userId}
        // ====================================================

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(
            string userId,
            CancellationToken cancellationToken
        )
        {
            var denied = CheckAccess();

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
                var user = await _repository.GetUserAsync(
                    userId.Trim(),
                    cancellationToken
                );

                if (user is null)
                {
                    return NotFound(new
                    {
                        message = "User not found."
                    });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to load TWINBEE user."
                );

                return Problem(
                    title: "Unable to load user.",
                    statusCode: 500
                );
            }
        }

        // ====================================================
        // POST /api/quotation/security/users
        // CREATE USER
        // ====================================================

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser(
            [FromBody] SaveSecurityUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var denied = CheckAccess();

            if (denied is not null)
            {
                return denied;
            }

            if (
                request is null ||
                string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return BadRequest(new
                {
                    message =
                        "User ID, first name and password are required."
                });
            }

            if (
                request.UserId.Trim().Length > 10 ||
                request.FirstName.Trim().Length > 25 ||
                request.Password.Length > 20
            )
            {
                return BadRequest(new
                {
                    message = "One or more fields exceed their maximum length."
                });
            }

            try
            {
                await _repository.SaveAsync(
                    ActorUserId!,
                    request,
                    cancellationToken
                );

                return Ok(new
                {
                    success = true,
                    userId = request.UserId.Trim(),
                    message = "User saved successfully."
                });
            }
            catch (SqlException ex) when (ex.Number >= 50000)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to create TWINBEE user."
                );

                return Problem(
                    title: "Unable to create user.",
                    statusCode: 500
                );
            }
        }

        // ====================================================
        // PUT /api/quotation/security/users/{userId}
        // UPDATE USER
        // ====================================================

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(
            string userId,
            [FromBody] SaveSecurityUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var denied = CheckAccess();

            if (denied is not null)
            {
                return denied;
            }

            if (
                request is null ||
                string.IsNullOrWhiteSpace(userId) ||
                !string.Equals(
                    userId.Trim(),
                    request.UserId?.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return BadRequest(new
                {
                    message =
                        "User ID does not match the request."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return BadRequest(new
                {
                    message = "First name is required."
                });
            }

            if (
                request.FirstName.Trim().Length > 25 ||
                (request.Password?.Length ?? 0) > 20
            )
            {
                return BadRequest(new
                {
                    message =
                        "One or more fields exceed their maximum length."
                });
            }

            try
            {
                await _repository.SaveAsync(
                    ActorUserId!,
                    request,
                    cancellationToken
                );

                return Ok(new
                {
                    success = true,
                    userId = request.UserId.Trim(),
                    message = "User updated successfully."
                });
            }
            catch (SqlException ex) when (ex.Number >= 50000)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to update TWINBEE user."
                );

                return Problem(
                    title: "Unable to update user.",
                    statusCode: 500
                );
            }
        }
    }
}
