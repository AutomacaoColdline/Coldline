using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineAPI.Application.DTOs;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMachineService
    {
        Task<List<Machine>> GetAllMachinesAsync();
        Task<Machine?> GetMachineByIdAsync(string id);
        Task<List<MachineByDateDto>> GetMachineCountPerDayAsync(DateTime startDate, DateTime endDate);
        Task<List<MachineByTypeDto>> GetMachineCountByTypeAsync(DateTime startDate, DateTime endDate);
        Task<List<MachineTotalProcessTimeDto>> GetTotalProcessTimeByMachineAsync(DateTime start, DateTime end);
        Task<List<MachineTypeAverageTimeDto>> GetAverageProcessTimeByMachineTypeAsync(DateTime start, DateTime end);
        Task<MachineDashboardDto> GetMachineDashboardAsync(DateTime startDate, DateTime endDate);
        Task<Machine> CreateMachineAsync(Machine machine);
        Task<bool> UpdateMachineAsync(string id, Machine machine);
        Task<bool> DeleteMachineAsync(string id);
        Task<List<Machine>> SearchMachinesAsync(MachineFilter filter);
        Task<int> UpdateMachinesCreatedAtAsync();
        Task<bool> FinalizeMachineAsync(string id);
    }
}
