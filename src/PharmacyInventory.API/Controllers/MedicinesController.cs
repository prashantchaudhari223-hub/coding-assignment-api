using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyInventory.Application.DTOs;
using PharmacyInventory.Application.Interfaces;

namespace PharmacyInventory.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;
        private readonly IValidator<CreateMedicineDto> _createValidator;
        private const int MaxPageSize = 100;

        public MedicinesController(
            IMedicineService medicineService,
            IValidator<CreateMedicineDto> createValidator)
        {
            _medicineService = medicineService;
            _createValidator = createValidator;
        }

        /// <summary>
        /// GET api/medicines?search=paracetamol&page=1&pageSize=20
        /// Returns the grid-ready list (Notes excluded, IsExpiringSoon / IsLowStock flags included).
        /// Response body includes the HTTP status code.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var medicines = await _medicineService.GetAllAsync(search);
            var total = medicines.Count;

            var items = medicines
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        }

        /// <summary>GET api/medicines/{id} - full detail including Notes.</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var medicine = await _medicineService.GetByIdAsync(id);
            if (medicine is null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Medicine with id '{id}' not found."
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = medicine
            });
        }

        /// <summary>POST api/medicines - add a new medicine.</summary>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateMedicineDto dto)
        {
            await _createValidator.ValidateAndThrowAsync(dto);

            var created = await _medicineService.CreateAsync(dto);

            // Return 201 with Location header (CreatedAtAction) and include HTTP code in body
            var payload = new
            {
                StatusCode = StatusCodes.Status201Created,
                Data = created
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, payload);
        }
    }
}
