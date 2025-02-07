using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface ITypeDefectService
    {
        Task<List<TypeDefect>> GetAllTypeDefectsAsync();
        Task<TypeDefect?> GetTypeDefectByIdAsync(string id);
        Task<TypeDefect> CreateTypeDefectAsync(TypeDefect TypeDefect);
        Task<bool> UpdateTypeDefectAsync(string id, TypeDefect TypeDefect);
        Task<bool> DeleteTypeDefectAsync(string id);
    }
}
