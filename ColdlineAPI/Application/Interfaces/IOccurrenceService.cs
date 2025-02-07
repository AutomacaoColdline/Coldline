using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IOccurrenceService
    {
        Task<List<Occurrence>> GetAllOccurrencesAsync();
        Task<Occurrence?> GetOccurrenceByIdAsync(string id);
        Task<Occurrence> CreateOccurrenceAsync(Occurrence Occurrence);
        Task<bool> UpdateOccurrenceAsync(string id, Occurrence Occurrence);
        Task<bool> DeleteOccurrenceAsync(string id);
    }
}
