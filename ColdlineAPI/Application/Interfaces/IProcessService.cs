using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IProcessService
    {
        Task<List<Process>> GetAllProcesssAsync();
        Task<Process?> GetProcessByIdAsync(string id);
        Task<Process> CreateProcessAsync(Process Process);
        Task<bool> UpdateProcessAsync(string id, Process Process);
        Task<bool> DeleteProcessAsync(string id);
        Task<List<Process>> SearchProcessAsync(ProcessFilter filter);
    }
}
