using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IProcessService
    {
        Task<List<Process>> GetAllProcessesAsync();
        Task<Process?> GetProcessByIdAsync(string id);
        Task<Process> CreateProcessAsync(Process process);
        Task<bool> UpdateProcessAsync(string id, Process process);
        Task<bool> DeleteProcessAsync(string id);
        Task<List<Process>> SearchProcessAsync(ProcessFilter filter);
        Task<Process?> StartProcessAsync(string identificationNumber, string processTypeId, string? machineId, bool preIndustrialization, bool reWork);
        Task<bool> UpdateProcessTimeInDatabase(string processId, string processTime);
        Task<bool> EndProcessAsync(string processId);
        Task<ProcessStatisticsDto> GetProcessStatisticsAsync(string processId, string processTypeId);
        Task<UserProcessDetailsDto> GetUserProcessDataAsync(string userId);
    }
}
