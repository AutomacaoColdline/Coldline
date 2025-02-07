using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMonitoringService
    {
        Task<List<Monitoring>> GetAllMonitoringsAsync();
        Task<Monitoring?> GetMonitoringByIdAsync(string id);
        Task<Monitoring> CreateMonitoringAsync(Monitoring Monitoring);
        Task<bool> UpdateMonitoringAsync(string id, Monitoring Monitoring);
        Task<bool> DeleteMonitoringAsync(string id);
    }
}
