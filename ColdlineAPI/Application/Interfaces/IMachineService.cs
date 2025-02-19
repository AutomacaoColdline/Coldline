using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMachineService
    {
        Task<List<Machine>> GetAllMachinesAsync();
        Task<Machine?> GetMachineByIdAsync(string id);
        Task<Machine> CreateMachineAsync(Machine machine);
       Task<bool> UpdateMachineAsync(string id, Machine machine);
        Task<bool> DeleteMachineAsync(string id);
        Task<List<Machine>> SearchMachinesAsync(MachineFilter filter);
    }
}
