using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyInventory.Application.DTOs;

namespace PharmacyInventory.Application.Interfaces
{
    public interface IMedicineService
    {
        /// <param name="search">Optional free-text term matched against FullName, Brand or Notes.</param>
        Task<IReadOnlyList<MedicineListItemDto>> GetAllAsync(string? search);
        Task<MedicineDetailDto?> GetByIdAsync(Guid id);
        Task<MedicineDetailDto> CreateAsync(CreateMedicineDto dto);
    }
}
