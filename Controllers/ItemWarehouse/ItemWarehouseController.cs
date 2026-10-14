using HRIStwinbeeAPI.DTO.Inventory.ItemWarehouse;
using HRIStwinbeeAPI.Repo.Interface.ItemWarehouse;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HRIStwinbeeAPI.Controllers.ItemWarehouse
{
    [Route("api/item-warehouse")]
    [ApiController]
    public class ItemWarehouseController : ControllerBase
    {
        private readonly IItemWarehouseRepository _repository;

        public ItemWarehouseController(IItemWarehouseRepository repository)
        {
            _repository = repository;
        }

        // GET: api/item-warehouse/by-item-code/CYNO
        // GET: api/item-warehouse/by-item-code/CYNO?active=Y
        [HttpGet("by-item-code/{itemCode}")]
        public async Task<IActionResult> GetByItemCode(
            [FromRoute] string itemCode,
            [FromQuery] string active = "Y"
        )
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return BadRequest(new ApiResponseDto<IEnumerable<ItemWarehouseBalanceDto>>
                {
                    Success = false,
                    Message = "Item code is required.",
                    Data = []
                });
            }

            try
            {
                var result = await _repository.GetByItemCodeAsync(
                    itemCode.Trim(),
                    active
                );

                return Ok(new ApiResponseDto<IEnumerable<ItemWarehouseBalanceDto>>
                {
                    Success = true,
                    Message = "Item warehouse balance loaded successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<IEnumerable<ItemWarehouseBalanceDto>>
                {
                    Success = false,
                    Message = "Failed to load item warehouse balance.",
                    Error = ex.Message,
                    Data = []
                });
            }
        }
    }
}
