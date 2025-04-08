using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMonitoringTypeService
    {
        Task<List<MonitoringType>> GetAllMonitoringTypesAsync();
        Task<MonitoringType?> GetMonitoringTypeByIdAsync(string id);
        Task<MonitoringType> CreateMonitoringTypeAsync(MonitoringType monitoringType);
        Task<bool> UpdateMonitoringTypeAsync(string id, MonitoringType monitoringType);
        Task<bool> DeleteMonitoringTypeAsync(string id);
    }
}
