using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IOccurrenceService
    {
        Task<List<Occurrence>> GetAllOccurrencesAsync();
        Task<Occurrence?> GetOccurrenceByIdAsync(string id);
        Task<List<OccurrenceDateChartDto>> GetOccurrenceCountByDateAsync(DateTime start, DateTime end);
        Task<List<OccurrenceUserChartDto>> GetOccurrenceCountByUserAsync(DateTime start, DateTime end);
        Task<Occurrence> CreateOccurrenceAsync(Occurrence Occurrence);
        Task<bool> UpdateOccurrenceAsync(string id, Occurrence Occurrence);
        Task<bool> DeleteOccurrenceAsync(string id);
        Task<List<Occurrence>> SearchOccurrencesAsync(OccurrenceSearchFilter filter);
        Task<bool> FinalizeOccurrenceAsync(string id);
    }
}
