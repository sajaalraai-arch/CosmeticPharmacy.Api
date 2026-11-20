using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Helpers;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrdersIndexDto>> GetAll(
            [FromQuery] string? searchString, [FromQuery] int? statusId,
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [FromQuery] string? sortOrder, [FromQuery] int? customerId)
        {
            var result = await _orderService.GetAllAsync(searchString, statusId, fromDate, toDate, sortOrder, customerId);
            return Ok(result);
        }


       

        [HttpGet("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDetailsDto>> GetById([FromRoute] int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPut("{id:int:min(1)}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var error = await _orderService.UpdateStatusAsync(id, dto.StatusId);

            if (error == "NotFound") return NotFound();
            if (error != null) return BadRequest(new MessageDto { Message = error });

            return NoContent();
        }


        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var error = await _orderService.DeleteOrderAsync(id);

            if (error == "NotFound") return NotFound();
            if (error != null) return Conflict(new MessageDto { Message = error });

            return NoContent();
        }
    }
}