using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IPauseTypeService
    {
        Task<List<PauseType>> GetAllPauseTypesAsync();
        Task<PauseType?> GetPauseTypeByIdAsync(string id);
        Task<PauseType> CreatePauseTypeAsync(PauseType PauseType);
        Task<bool> UpdatePauseTypeAsync(string id, PauseType PauseType);
        Task<bool> DeletePauseTypeAsync(string id);
    }
}
