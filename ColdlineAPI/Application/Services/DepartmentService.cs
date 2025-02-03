using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IMongoCollection<Department> _departments;
        private readonly IMongoCollection<User> _users; // Referência à coleção de usuários

        public DepartmentService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _departments = database.GetCollection<Department>("Departments");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<Department>> GetAllDepartmentsAsync() =>
            await _departments.Find(department => true).ToListAsync();

        public async Task<Department?> GetDepartmentByIdAsync(string id) =>
            await _departments.Find(department => department.Id == id).FirstOrDefaultAsync();

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            if (string.IsNullOrEmpty(department.Id) || !ObjectId.TryParse(department.Id, out _))
            {
                department.Id = ObjectId.GenerateNewId().ToString();
            }

            await _departments.InsertOneAsync(department);
            return department;
        }

        public async Task<bool> UpdateDepartmentAsync(string id, Department department)
        {
            var result = await _departments.ReplaceOneAsync(d => d.Id == id, department);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteDepartmentAsync(string id)
        {
            // Verifica se existe algum usuário associado a este departamento
            var isDepartmentInUse = await _users.Find(user => user.Department.Id == id).AnyAsync();
            if (isDepartmentInUse)
            {
                throw new InvalidOperationException("O departamento não pode ser excluído pois está vinculado a um ou mais usuários.");
            }

            var result = await _departments.DeleteOneAsync(department => department.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
