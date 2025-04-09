using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly MongoRepository<Department> _departments;
        private readonly MongoRepository<User> _users;

        public DepartmentService(RepositoryFactory factory)
        {
            _departments = factory.CreateRepository<Department>("Departments");
            _users = factory.CreateRepository<User>("Users");
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            var projection = Builders<Department>.Projection
                .Include(d => d.Id)
                .Include(d => d.Name)
                .Include(d => d.Description);

            return await _departments.GetCollection()
                .Find(Builders<Department>.Filter.Empty)
                .Project<Department>(projection)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(string id)
        {
            return await _departments.GetByIdAsync(d => d.Id == id);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            department.Id ??= ObjectId.GenerateNewId().ToString();
            return await _departments.CreateAsync(department);
        }

        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(string id, Department department)
        {
            var existing = await _departments.GetByIdAsync(d => d.Id == id);
            if (existing == null)
                return (false, "Departamento não encontrado.");

            var update = Builders<Department>.Update
                .Set(d => d.Name, department.Name ?? existing.Name);

            var result = await _departments.GetCollection().UpdateOneAsync(d => d.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0
                ? (true, "Departamento atualizado com sucesso.")
                : (false, "Nenhuma modificação realizada.");
        }

        public async Task<(bool Success, string Message)> DeleteDepartmentAsync(string id)
        {
            var inUse = await _users.GetCollection().Find(u => u.Department.Id == id).AnyAsync();
            if (inUse)
                return (false, "O departamento está vinculado a usuários e não pode ser excluído.");

            var result = await _departments.DeleteAsync(d => d.Id == id);
            return result
                ? (true, "Departamento excluído com sucesso.")
                : (false, "Departamento não encontrado para exclusão.");
        }
    }
}
