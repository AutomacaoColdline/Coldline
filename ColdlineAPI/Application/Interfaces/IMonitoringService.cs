using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using ColdlineAPI.Application.Filters;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMonitoringService
    {
        Task<List<Monitoring>> GetAllMonitoringsAsync();
        Task<Monitoring?> GetMonitoringByIdAsync(string id);
        Task<List<Monitoring>> GetFilteredMonitoringsAsync(MonitoringFilter filter);

        Task<Monitoring> CreateMonitoringAsync(Monitoring monitoring);
        Task<List<Monitoring>> CreateAllMonitoringsAsync(List<Monitoring> monitorings);
        Task<bool> UpdateMonitoringAsync(string id, Monitoring monitoring);
        Task<bool> DeleteMonitoringAsync(string id);
    }
}
