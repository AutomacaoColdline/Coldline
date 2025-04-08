using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IDefectService
    {
        Task<List<Defect>> GetAllDefectsAsync();
        Task<Defect?> GetDefectByIdAsync(string id);
        Task<Defect> CreateDefectAsync(Defect defect);
        Task<bool> UpdateDefectAsync(string id, Defect defect);
        Task<bool> DeleteDefectAsync(string id);
    }
}
