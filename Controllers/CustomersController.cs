using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogiTrack.WebApi.Contracts.Customers;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersRepository _customers;
        private readonly IUnitOfWork _uow;

        public CustomersController(ICustomersRepository customers, IUnitOfWork uow)
        {
            _customers = customers;
            _uow = uow;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _customers.GetAllAsync(ct);

            var result = list
                .OrderBy(c => c.Id)
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var c = await _customers.GetByIdAsync(id, ct);
            if (c == null) return NotFound($"Customer with id {id} not found.");

            var dto = new CustomerResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CustomerCreateUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = dto.Email.Trim();
            if (await _customers.ExistsByEmailAsync(email, ct))
                return BadRequest("Customer with the same email already exists.");

            var entity = new Customer
            {
                Name = dto.Name.Trim(),
                Email = email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            await _customers.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var result = new CustomerResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Address = entity.Address
            };

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerCreateUpdateDto dto, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _customers.GetByIdAsync(id, ct);
            if (entity == null) return NotFound($"Customer with id {id} not found.");

            var newEmail = dto.Email.Trim();
            if (!string.Equals(entity.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                if (await _customers.ExistsByEmailAsync(newEmail, ct))
                    return BadRequest("Customer with the same email already exists.");
            }

            entity.Name = dto.Name.Trim();
            entity.Email = newEmail;
            entity.Phone = dto.Phone;
            entity.Address = dto.Address;

            _customers.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("Id must be greater than 0.");

            var entity = await _customers.GetByIdAsync(id, ct);
            if (entity == null) return NotFound($"Customer with id {id} not found.");

            _customers.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
