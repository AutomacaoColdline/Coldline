using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IQualityService
    {
        Task<List<Quality>> GetAllQualitysAsync();
        Task<Quality?> GetQualityByIdAsync(string id);
        Task<Quality> CreateQualityAsync(Quality Quality);
        Task<bool> UpdateQualityAsync(string id, Quality Quality);
        Task<bool> DeleteQualityAsync(string id);
    }
}
