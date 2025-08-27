using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IOccurrenceTypeService
    {
        Task<List<OccurrenceType>> GetAllOccurrenceTypesAsync();
        Task<OccurrenceType?> GetOccurrenceTypeByIdAsync(string id);
        Task<OccurrenceType> CreateOccurrenceTypeAsync(OccurrenceType occurrenceType);
        Task<bool> UpdateOccurrenceTypeAsync(string id, OccurrenceType occurrenceType);
        Task<bool> DeleteOccurrenceTypeAsync(string id);
    }
}
