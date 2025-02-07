using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMachineService
    {
        Task<List<Machine>> GetAllMachinesAsync();
        Task<Machine?> GetMachineByIdAsync(string id);
        Task<Machine> CreateMachineAsync(Machine Machine);
        Task<bool> UpdateMachineAsync(string id, Machine Machine);
        Task<bool> DeleteMachineAsync(string id);
    }
}
