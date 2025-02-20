using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class ProcessService : IProcessService
    {
        private readonly IMongoCollection<Process> _processes;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ProcessType> _processTypes;

        public ProcessService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _processes = database.GetCollection<Process>("Processes");
            _users = database.GetCollection<User>("Users");
            _processTypes = database.GetCollection<ProcessType>("ProcessTypes");
        }

       public async Task<Process?> StartProcessAsync(string identificationNumber, string processTypeId)
        {
            // Buscar usuário pelo número de identificação
            var user = await _users.Find(u => u.IdentificationNumber == identificationNumber).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ArgumentException("Usuário não encontrado.");
            }

            // Buscar o tipo de processo pelo ID fornecido
            var processType = await _processTypes.Find(pt => pt.Id == processTypeId).FirstOrDefaultAsync();
            if (processType == null)
            {
                throw new ArgumentException("Tipo de Processo não encontrado.");
            }

            // Obter horário atual de Campo Grande (AMT)
            TimeZoneInfo campoGrandeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
            DateTime campoGrandeTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, campoGrandeTimeZone);

            // Criar novo processo
            var newProcess = new Process
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IdentificationNumber = identificationNumber,
                ProcessTime = "00:00:00",
                StartDate = campoGrandeTime,
                EndDate = null,
                User = new ReferenceEntity { Id = user.Id, Name = user.Name },
                Department = new ReferenceEntity { Id = user.Department.Id, Name = user.Department.Name },
                ProcessType = new ReferenceEntity { Id = processType.Id, Name = processType.Name },
                InOccurrence = false
            };

            // Inserir o novo processo no banco
            await _processes.InsertOneAsync(newProcess);

            // Atualizar o usuário para armazenar o processo atual em CurrentProcess
            var updateUser = Builders<User>.Update
                .Set(u => u.CurrentProcess, new ReferenceEntity { Id = newProcess.Id, Name = newProcess.IdentificationNumber });

            await _users.UpdateOneAsync(u => u.Id == user.Id, updateUser);

            return newProcess;
        }



        public async Task<List<Process>> GetAllProcessesAsync()
        {
            return await _processes.Find(process => true).ToListAsync();
        }

        public async Task<Process?> GetProcessByIdAsync(string id)
        {
            return await _processes.Find(process => process.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Process> CreateProcessAsync(Process process)
        {
            if (string.IsNullOrEmpty(process.Id))
            {
                process.Id = ObjectId.GenerateNewId().ToString();
            }

            await _processes.InsertOneAsync(process);
            return process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process process)
        {
            var objectId = ObjectId.Parse(id);
            var existingProcess = await _processes.Find(p => p.Id == objectId.ToString()).FirstOrDefaultAsync();

            if (existingProcess == null) return false;

            var updateDefinition = Builders<Process>.Update
                .Set(p => p.IdentificationNumber, process.IdentificationNumber ?? existingProcess.IdentificationNumber)
                .Set(p => p.ProcessTime, process.ProcessTime ?? existingProcess.ProcessTime) // Agora armazenado como string
                .Set(p => p.StartDate, process.StartDate != default ? process.StartDate : existingProcess.StartDate)
                .Set(p => p.EndDate, process.EndDate != default ? process.EndDate : existingProcess.EndDate)
                .Set(p => p.User, process.User ?? existingProcess.User)
                .Set(p => p.Department, process.Department ?? existingProcess.Department)
                .Set(p => p.ProcessType, process.ProcessType ?? existingProcess.ProcessType)
                .Set(p => p.PauseTypes, process.PauseTypes ?? existingProcess.PauseTypes)
                .Set(p => p.Occurrences, process.Occurrences ?? existingProcess.Occurrences)
                .Set(p => p.InOccurrence, process.InOccurrence)
                .Set(p => p.Finished, process.Finished)
                .Set(p => p.Machine, process.Machine ?? existingProcess.Machine);

            var result = await _processes.UpdateOneAsync(p => p.Id == id, updateDefinition);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var result = await _processes.DeleteOneAsync(p => p.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<List<Process>> SearchProcessAsync(ProcessFilter filter)
        {
            var filters = new List<FilterDefinition<Process>>();
            var builder = Builders<Process>.Filter;

            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq(p => p.IdentificationNumber, filter.IdentificationNumber));

            if (!string.IsNullOrEmpty(filter.ProcessTime))
                filters.Add(builder.Eq(p => p.ProcessTime, filter.ProcessTime));

            if (filter.StartDate.HasValue)
                filters.Add(builder.Gte(p => p.StartDate, filter.StartDate.Value));

            if (filter.EndDate.HasValue)
                filters.Add(builder.Lte(p => p.EndDate, filter.EndDate.Value));

            if (!string.IsNullOrEmpty(filter.UserId))
                filters.Add(builder.Eq(p => p.User!.Id, filter.UserId));

            if (!string.IsNullOrEmpty(filter.DepartmentId))
                filters.Add(builder.Eq(p => p.Department!.Id, filter.DepartmentId));

            if (!string.IsNullOrEmpty(filter.ProcessTypeId))
                filters.Add(builder.Eq(p => p.ProcessType!.Id, filter.ProcessTypeId));

            if (!string.IsNullOrEmpty(filter.PauseTypesId))
                filters.Add(builder.Eq(p => p.PauseTypes!.Id, filter.PauseTypesId));

            if (filter.OccurrencesIds != null && filter.OccurrencesIds.Count > 0)
            {
                var occurrenceIds = filter.OccurrencesIds.ToList();
                filters.Add(builder.In("Occurrences.Id", occurrenceIds));
            }

            if (!string.IsNullOrEmpty(filter.MachineId))
                filters.Add(builder.Eq(p => p.Machine!.Id, filter.MachineId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _processes.Find(finalFilter).ToListAsync();
        }
    }
}
