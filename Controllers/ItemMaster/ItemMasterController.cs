using HRIStwinbeeAPI.DTO.Inventory.ItemMaster;
using HRIStwinbeeAPI.Repo.Interface.ItemMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace HRIStwinbeeAPI.Controllers.ItemMaster
{
    [Route("api/item-master")]
    [ApiController]
    public class ItemMasterController : ControllerBase
    {
        private readonly IItemMasterRepository _repository;


        public ItemMasterController(
            IItemMasterRepository repository
        )
        {
            _repository = repository;
        }


        /*
         * ============================================================
         * ITEM MASTER LIST
         * ============================================================
         *
         * Examples:
         *
         * /api/item-master/list
         *
         * /api/item-master/list
         *      ?active=Y
         *      &skip=0
         *      &take=20
         *
         * /api/item-master/list
         *      ?search=INK
         *      &skip=0
         *      &take=20
         *
         * /api/item-master/list
         *      ?sortField=itemDesc
         *      &sortDir=desc
         */
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search,
            [FromQuery] string? groupCode,
            [FromQuery] string? itemType,
            [FromQuery] string active = "Y",
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string sortField = "itemCode",
            [FromQuery] string sortDir = "asc"
        )
        {
            try
            {
                /*
                 * Server-side safety.
                 */
                skip = Math.Max(
                    0,
                    skip
                );

                take = Math.Clamp(
                    take,
                    1,
                    100
                );


                var result =
                    await _repository.GetListAsync(
                        search,
                        groupCode,
                        itemType,
                        active,
                        skip,
                        take,
                        sortField,
                        sortDir
                    );


                return Ok(
                    new
                    {
                        success = true,

                        message =
                            "Item Master list loaded successfully.",

                        data =
                            result.Data,

                        total =
                            result.Total,

                        skip =
                            result.Skip,

                        take =
                            result.Take
                    }
                );
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        success = false,

                        message =
                            "Failed to load Item Master list.",

                        error =
                            exception.Message
                    }
                );
            }
        }


        /*
         * ============================================================
         * ITEM GROUP LOOKUP
         * ============================================================
         */
        [HttpGet("group-lookup")]
        public async Task<IActionResult>
            GetItemGroupLookup(
                [FromQuery]
                bool includeAll = true
            )
        {
            try
            {
                var result =
                    await _repository
                        .GetItemGroupLookupAsync(
                            includeAll
                        );


                return Ok(result);
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        success = false,

                        message =
                            "Failed to load Item Group lookup.",

                        error =
                            exception.Message
                    }
                );
            }
        }


        /*
         * ============================================================
         * UNIT OF MEASURE LOOKUP
         * ============================================================
         */
        [HttpGet("um-lookup")]
        public async Task<IActionResult>
            GetItemUmLookup()
        {
            try
            {
                var result =
                    await _repository
                        .GetItemUmLookupAsync();


                return Ok(
                    new
                    {
                        success = true,

                        message =
                            "Unit of Measure lookup loaded successfully.",

                        data =
                            result
                    }
                );
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        success = false,

                        message =
                            "Failed to load Unit of Measure lookup.",

                        error =
                            exception.Message
                    }
                );
            }
        }


        /*
         * ============================================================
         * ITEM MASTER SAVE / UPDATE
         * ============================================================
         */
        [Authorize]
        [HttpPost("save")]
        public async Task<IActionResult> SaveItem(
            [FromBody]
            ItemMasterUpsertRequestDto request
        )
        {
            if (request is null)
            {
                return BadRequest(
                    new
                    {
                        success = false,
                        message = "Invalid request."
                    }
                );
            }


            if (!ModelState.IsValid)
            {
                return ValidationProblem(
                    ModelState
                );
            }


            /*
             * Normalize Item Code.
             */
            request.ItemCode =
                (request.ItemCode ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();


            /*
             * Normalize Item Description.
             */
            request.ItemDesc =
                (request.ItemDesc ?? string.Empty)
                    .Trim();


            /*
             * Normalize Group Code.
             */
            request.GroupCode =
                (request.GroupCode ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();


            /*
             * Normalize Unit of Measure.
             */
            request.Um =
                (request.Um ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();


            /*
             * Normalize Item Type.
             */
            request.ItemType =
                string.IsNullOrWhiteSpace(
                    request.ItemType
                )
                    ? null
                    : request.ItemType
                        .Trim()
                        .ToUpperInvariant();


            /*
             * Normalize active flag.
             */
            request.Active =
                string.IsNullOrWhiteSpace(
                    request.Active
                )
                    ? "Y"
                    : request.Active
                        .Trim()
                        .ToUpperInvariant();


            /*
             * Item Code rule:
             * New/edited Item Codes cannot exceed 10 characters.
             */
            if (request.ItemCode.Length > 10)
            {
                return BadRequest(
                    new
                    {
                        success = false,

                        message =
                            "Item Code cannot exceed 10 characters."
                    }
                );
            }


            /*
             * Get the logged-in user.
             */
            var currentUserId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value

                ?? User.FindFirst(
                    "userId"
                )?.Value

                ?? User.FindFirst(
                    "UserId"
                )?.Value

                ?? User.FindFirst(
                    "userid"
                )?.Value

                ?? User.FindFirst(
                    "sub"
                )?.Value

                ?? User.Identity?.Name;


            if (
                string.IsNullOrWhiteSpace(
                    currentUserId
                )
            )
            {
                return Unauthorized(
                    new
                    {
                        success = false,

                        message =
                            "Unable to determine the currently logged-in user."
                    }
                );
            }


            currentUserId =
                currentUserId
                    .Trim()
                    .ToUpperInvariant();


            if (
                currentUserId.Length > 10
            )
            {
                return BadRequest(
                    new
                    {
                        success = false,

                        message =
                            "Logged-in User ID cannot exceed 10 characters."
                    }
                );
            }


            /*
             * User ID must always come from authentication.
             */
            request.UserId =
                currentUserId;


            try
            {
                var result =
                    await _repository
                        .UpsertAsync(
                            request
                        );


                if (
                    !result.Success ||
                    result.Item is null
                )
                {
                    return Conflict(
                        new
                        {
                            success = false,

                            message =
                                result.Message ??
                                "The database did not confirm the Item Master save.",

                            item =
                                result.Item
                        }
                    );
                }


                return Ok(
                    new
                    {
                        success = true,

                        message =
                            result.Message ??
                            "Inventory item saved successfully.",

                        item =
                            result.Item
                    }
                );
            }
            catch (
                ArgumentException exception
            )
            {
                return BadRequest(
                    new
                    {
                        success = false,
                        message = exception.Message
                    }
                );
            }
            catch (
                InvalidOperationException exception
            )
            {
                return Conflict(
                    new
                    {
                        success = false,
                        message = exception.Message
                    }
                );
            }
            catch (
                SqlException exception
            )
            when (
                exception.Number >= 50000
            )
            {
                return Conflict(
                    new
                    {
                        success = false,
                        message = exception.Message
                    }
                );
            }
            catch (
                SqlException exception
            )
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        success = false,

                        message =
                            $"A database error occurred while saving the inventory item. " +
                            $"SQL Error {exception.Number}: {exception.Message}"
                    }
                );
            }
            catch (
                Exception exception
            )
            {
                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        success = false,

                        message =
                            "An unexpected error occurred while saving " +
                            $"the inventory item: {exception.Message}"
                    }
                );
            }
        }
    }
}