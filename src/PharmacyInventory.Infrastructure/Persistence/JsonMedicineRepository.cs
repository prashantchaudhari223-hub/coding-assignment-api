using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PharmacyInventory.Application.Interfaces;
using PharmacyInventory.Domain.Entities;

namespace PharmacyInventory.Infrastructure.Persistence
{
    /// <summary>
    /// Stores medicines as a JSON file on disk, per the assessment's technical requirement
    /// ("Data to be stored in Json on server side"). A SemaphoreSlim guards read/modify/write
    /// so concurrent requests don't corrupt the file.
    /// </summary>
    public class JsonMedicineRepository : IMedicineRepository
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public JsonMedicineRepository(IConfiguration configuration)
        {
            var configuredPath = configuration["Storage:MedicinesFilePath"] ?? "Data/medicines.json";
            _filePath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppContext.BaseDirectory, configuredPath);

            EnsureFileExists();
        }

        public async Task<IReadOnlyList<Medicine>> GetAllAsync()
        {
            var medicines = await ReadAllAsync();
            return medicines;
        }

        public async Task<Medicine?> GetByIdAsync(Guid id)
        {
            var medicines = await ReadAllAsync();
            return medicines.FirstOrDefault(m => m.Id == id);
        }

        public async Task<Medicine> AddAsync(Medicine medicine)
        {
            await _lock.WaitAsync();
            try
            {
                var medicines = (await ReadAllInternalAsync()).ToList();
                medicines.Add(medicine);
                await WriteAllInternalAsync(medicines);
                return medicine;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Medicine>> ReadAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return await ReadAllInternalAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Medicine>> ReadAllInternalAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Medicine>();
            }

            await using var stream = File.OpenRead(_filePath);
            if (stream.Length == 0)
            {
                return new List<Medicine>();
            }

            var medicines = await JsonSerializer.DeserializeAsync<List<Medicine>>(stream, _jsonOptions);
            return medicines ?? new List<Medicine>();
        }

        private async Task WriteAllInternalAsync(List<Medicine> medicines)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, medicines, _jsonOptions);
        }

        private void EnsureFileExists()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }
    }
}
