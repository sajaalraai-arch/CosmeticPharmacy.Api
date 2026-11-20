using CosmeticPharmacy.Api.Dtos;
using CosmeticPharmacy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticPharmacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<CustomersIndexDto>> GetAll(
            [FromQuery] string? searchTerm, [FromQuery] string? role, [FromQuery] string? status)
        {
            var result = await _customerService.GetAllAsync(searchTerm, role, status);
            return Ok(result);
        }

        

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<CustomerDto>> GetById([FromRoute] int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            var (error, customer) = await _customerService.CreateAsync(dto);

            if (error == "Invalid role or status.")
                return BadRequest(new MessageDto { Message = error });
            if (error == "This email address is already in use.")
                return Conflict(new MessageDto { Message = error });

            return CreatedAtAction(nameof(GetById), new { id = customer!.CustomerId }, customer);
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerUpdateDto dto)
        {
            var (error, customer) = await _customerService.UpdateAsync(id, dto);

            if (error == "NotFound") return NotFound();
            if (error == "Invalid role or status.")
                return BadRequest(new MessageDto { Message = error });
            if (error == "This email address is already in use.")
                return Conflict(new MessageDto { Message = error });

            return Ok(customer);
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var error = await _customerService.DeleteAsync(id);

            if (error == "NotFound") return NotFound();
            if (error != null)
                return Conflict(new MessageDto { Message = error });

            return NoContent();
        }
    }
}