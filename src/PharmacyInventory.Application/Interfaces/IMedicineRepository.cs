using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyInventory.Domain.Entities;

namespace PharmacyInventory.Application.Interfaces
{
    /// <summary>
    /// Abstraction owned by the Application layer. The concrete implementation
    /// (JSON file storage) lives in Infrastructure and is wired up via DI in the API layer.
    /// This is what makes the architecture "clean" / dependency-inverted.
    /// </summary>
    public interface IMedicineRepository
    {
        Task<IReadOnlyList<Medicine>> GetAllAsync();
        Task<Medicine?> GetByIdAsync(Guid id);
        Task<Medicine> AddAsync(Medicine medicine);
    }
}
