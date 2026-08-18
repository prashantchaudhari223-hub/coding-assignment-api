using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmacyInventory.Application.Common;
using PharmacyInventory.Application.DTOs;
using PharmacyInventory.Application.Interfaces;
using PharmacyInventory.Domain.Entities;

namespace PharmacyInventory.Application.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _repository;

        public MedicineService(IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<MedicineListItemDto>> GetAllAsync(string? search)
        {
            var medicines = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                medicines = medicines
                    .Where(m =>
                        m.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        m.Brand.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        (m.Notes != null && m.Notes.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return medicines
                .OrderBy(m => m.FullName)
                .Select(ToListItemDto)
                .ToList();
        }

        public async Task<MedicineDetailDto?> GetByIdAsync(Guid id)
        {
            var medicine = await _repository.GetByIdAsync(id);
            return medicine is null ? null : ToDetailDto(medicine);
        }

        public async Task<MedicineDetailDto> CreateAsync(CreateMedicineDto dto)
        {
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Notes = dto.Notes?.Trim(),
                ExpiryDate = dto.ExpiryDate.Date,
                Quantity = dto.Quantity,
                Price = Math.Round(dto.Price, 2),
                Brand = dto.Brand.Trim()
            };

            var created = await _repository.AddAsync(medicine);
            return ToDetailDto(created);
        }

        private static MedicineListItemDto ToListItemDto(Medicine m) => new()
        {
            Id = m.Id,
            FullName = m.FullName,
            ExpiryDate = m.ExpiryDate,
            Quantity = m.Quantity,
            Price = m.Price,
            Brand = m.Brand,
            IsExpiringSoon = (m.ExpiryDate.Date - DateTime.Today).TotalDays < BusinessRules.ExpiryWarningDays,
            IsLowStock = m.Quantity < BusinessRules.LowStockThreshold
        };

        private static MedicineDetailDto ToDetailDto(Medicine m) => new()
        {
            Id = m.Id,
            FullName = m.FullName,
            Notes = m.Notes,
            ExpiryDate = m.ExpiryDate,
            Quantity = m.Quantity,
            Price = m.Price,
            Brand = m.Brand
        };
    }
}
