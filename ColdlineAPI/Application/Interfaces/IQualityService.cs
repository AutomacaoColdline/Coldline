using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IQualityService
    {
        Task<List<Quality>> GetAllQualitysAsync();
        Task<Quality?> GetQualityByIdAsync(string id);
        Task<Quality> CreateQualityAsync(Quality quality);
        Task<bool> UpdateQualityAsync(string id, Quality quality);
        Task<bool> DeleteQualityAsync(string id);
        Task<List<Quality>> SearchQualityAsync(QualityFilter filter);
    }
}