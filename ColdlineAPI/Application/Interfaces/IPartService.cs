using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IPartService
    {
        Task<List<Part>> GetAllPartsAsync();
        Task<Part?> GetPartByIdAsync(string id);
        Task<Part> CreatePartAsync(Part part);
        Task<bool> UpdatePartAsync(string id, Part part);
        Task<bool> DeletePartAsync(string id);
    }
}
