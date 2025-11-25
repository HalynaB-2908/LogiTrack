using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogiTrack.WebApi.Contracts.Customers;
using LogiTrack.WebApi.Models;
using LogiTrack.WebApi.Services.Abstractions;

namespace LogiTrack.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for managing customers.
    /// Provides endpoints for CRUD operations on customer entities.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,User")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersRepository _customers;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CustomersController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        /// <param name="customers">Customer repository for database operations.</param>
        /// <param name="uow">Unit of Work for transaction management.</param>
        /// <param name="logger">Logger for tracking controller actions.</param>
        public CustomersController(
            ICustomersRepository customers,
            IUnitOfWork uow,
            ILogger<CustomersController> logger)
        {
            _customers = customers;
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all customers.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of customers.</returns>
        /// <response code="200">Customers successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("Getting all customers");

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

            _logger.LogInformation("Returning {Count} customers", result.Count);

            return Ok(result);
        }

        /// <summary>
        /// Returns a customer by its id.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Customer data.</returns>
        /// <response code="200">Customer found.</response>
        /// <response code="400">Invalid id provided.</response>
        /// <response code="404">Customer not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetById called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            _logger.LogInformation("Getting customer by id {Id}", id);

            var c = await _customers.GetByIdAsync(id, ct);
            if (c == null)
            {
                _logger.LogWarning("Customer with id {Id} not found", id);
                return NotFound($"Customer with id {id} not found.");
            }

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

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="dto">Customer creation data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Created customer.</returns>
        /// <response code="201">Customer successfully created.</response>
        /// <response code="400">Validation error or email already exists.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CustomerCreateUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state on customer creation");
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim();

            _logger.LogInformation("Attempting to create new customer with email {Email}", email);

            if (await _customers.ExistsByEmailAsync(email, ct))
            {
                _logger.LogWarning("Customer creation failed: email {Email} already exists", email);
                return BadRequest("Customer with the same email already exists.");
            }

            var entity = new Customer
            {
                Name = dto.Name.Trim(),
                Email = email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            await _customers.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Customer created successfully with id {Id}", entity.Id);

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

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <param name="dto">Updated customer data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Customer successfully updated.</response>
        /// <response code="400">Invalid request.</response>
        /// <response code="404">Customer not found.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerCreateUpdateDto dto, CancellationToken ct)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Update called with invalid id {Id}", id);
                return BadRequest("Id must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating customer {Id}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating customer with id {Id}", id);

            var entity = await _customers.GetByIdAsync(id, ct);
            if (entity == null)
            {
                return NotFound($"Customer with id {id} not found.");
            }

            entity.Name = dto.Name.Trim();
            entity.Email = dto.Email.Trim();
            entity.Phone = dto.Phone;
            entity.Address = dto.Address;

            _customers.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }

        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content if deleted.</returns>
        /// <response code="204">Customer successfully deleted.</response>
        /// <response code="400">Invalid id.</response>
        /// <response code="404">Customer not found.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0.");
            }

            var entity = await _customers.GetByIdAsync(id, ct);
            if (entity == null)
            {
                return NotFound($"Customer with id {id} not found.");
            }

            _customers.Remove(entity);
            await _uow.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
