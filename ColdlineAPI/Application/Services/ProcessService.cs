using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class ProcessService : IProcessService
    {
        private readonly MongoRepository<Process> _processes;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ProcessType> _processTypes;
        private readonly IMongoCollection<Machine> _machines;

      public ProcessService(RepositoryFactory factory)
    {
        _processes = factory.CreateRepository<Process>("Processes");
        _users = factory.CreateRepository<User>("Users").GetCollection();
        _processTypes = factory.CreateRepository<ProcessType>("ProcessTypes").GetCollection();
        _machines = factory.CreateRepository<Machine>("Machines").GetCollection();
    }

        public async Task<Process?> StartProcessAsync(string identificationNumber, string processTypeId, string? machineId, bool preIndustrialization, bool reWork)
        {
            var user = await _users.Find(u => u.IdentificationNumber == identificationNumber).FirstOrDefaultAsync();
            if (user == null) throw new ArgumentException("Usuário não encontrado.");

            var processType = await _processTypes.Find(pt => pt.Id == processTypeId).FirstOrDefaultAsync();
            if (processType == null) throw new ArgumentException("Tipo de Processo não encontrado.");

            ReferenceEntity? machineReference = null;
            if (!string.IsNullOrWhiteSpace(machineId))
            {
                var machine = await _machines.Find(m => m.Id == machineId).FirstOrDefaultAsync();
                if (machine == null) throw new ArgumentException("Máquina não encontrada.");
                machineReference = new ReferenceEntity { Id = machine.Id, Name = machine.MachineType.Name };
            }

            var now = GetCurrentCampoGrandeTime();
            var newProcess = new Process
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IdentificationNumber = GenerateNumericCode(),
                ProcessTime = "00:00:00",
                StartDate = now,
                EndDate = null,
                User = new ReferenceEntity { Id = user.Id, Name = user.Name },
                Department = new ReferenceEntity { Id = user.Department.Id, Name = user.Department.Name },
                ProcessType = new ReferenceEntity { Id = processType.Id, Name = processType.Name },
                Machine = machineReference,
                InOccurrence = false,
                ReWork = reWork,
                PreIndustrialization = preIndustrialization,
                Finished = false
            };

            await _processes.CreateAsync(newProcess);

            await _users.UpdateOneAsync(
                Builders<User>.Filter.Eq(u => u.Id, user.Id),
                Builders<User>.Update.Set(u => u.CurrentProcess, new ReferenceEntity { Id = newProcess.Id, Name = newProcess.IdentificationNumber }));

            if (machineReference != null)
            {
                await _machines.UpdateOneAsync(
                    Builders<Machine>.Filter.Eq(m => m.Id, machineReference.Id),
                    Builders<Machine>.Update
                        .Set(m => m.Status, MachineStatus.InProgress)
                        .Set(m => m.Process, new ReferenceEntity { Id = newProcess.Id, Name = newProcess.IdentificationNumber }));
            }

            return newProcess;
        }

        public async Task<bool> EndProcessAsync(string processId)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == processId);
            if (process == null) throw new ArgumentException("Processo não encontrado.");

            var user = await _users.Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();
            if (user == null) throw new ArgumentException("Usuário do processo não encontrado.");
            if (user.CurrentOccurrence != null)
                throw new InvalidOperationException("Não é possível finalizar o processo enquanto houver uma ocorrência aberta.");

            var now = GetCurrentCampoGrandeTime();

            var updateProcess = Builders<Process>.Update
                .Set(p => p.Finished, true)
                .Set(p => p.EndDate, now);

            var resultProcess = await _processes.GetCollection().UpdateOneAsync(p => p.Id == processId, updateProcess);
            var resultUser = await _users.UpdateOneAsync(u => u.Id == user.Id, Builders<User>.Update.Set(u => u.CurrentProcess, null));

            return resultProcess.IsAcknowledged && resultProcess.ModifiedCount > 0 && resultUser.IsAcknowledged && resultUser.ModifiedCount > 0;
        }

        public async Task<List<Process>> GetAllProcessesAsync()
        {
            var processes = await _processes.GetAllAsync();
            foreach (var process in processes)
            {
                var updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id, updatedTime);
                process.ProcessTime = updatedTime;
            }
            return processes;
        }

        public async Task<Process?> GetProcessByIdAsync(string id)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == id);
            if (process != null)
            {
                var updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id, updatedTime);
                process.ProcessTime = updatedTime;
            }
            return process;
        }

        public async Task<List<Process>> SearchProcessAsync(ProcessFilter filter)
        {
            var builder = Builders<Process>.Filter;
            var filters = new List<FilterDefinition<Process>>();

            if (!string.IsNullOrEmpty(filter.IdentificationNumber)) filters.Add(builder.Eq(p => p.IdentificationNumber, filter.IdentificationNumber));
            if (filter.StartDate.HasValue) filters.Add(builder.Gte(p => p.StartDate, filter.StartDate.Value));
            if (filter.EndDate.HasValue) filters.Add(builder.Lte(p => p.EndDate, filter.EndDate.Value));
            if (!string.IsNullOrEmpty(filter.UserId)) filters.Add(builder.Eq(p => p.User.Id, filter.UserId));
            if (!string.IsNullOrEmpty(filter.DepartmentId)) filters.Add(builder.Eq(p => p.Department.Id, filter.DepartmentId));
            if (!string.IsNullOrEmpty(filter.ProcessTypeId)) filters.Add(builder.Eq(p => p.ProcessType.Id, filter.ProcessTypeId));
            if (!string.IsNullOrEmpty(filter.MachineId)) filters.Add(builder.Eq(p => p.Machine.Id, filter.MachineId));
            if (filter.Finished.HasValue) filters.Add(builder.Eq(p => p.Finished, filter.Finished));
            if (filter.PreIndustrialization.HasValue) filters.Add(builder.Eq(p => p.PreIndustrialization, filter.PreIndustrialization));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var results = await _processes.GetCollection().Find(finalFilter).Skip(skip).Limit(pageSize).ToListAsync();

            foreach (var process in results)
            {
                var updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id!, updatedTime);
                process.ProcessTime = updatedTime;
            }

            return results;
        }

        public async Task<Process> CreateProcessAsync(Process process)
        {
            if (string.IsNullOrEmpty(process.Id))
                process.Id = ObjectId.GenerateNewId().ToString();

            await _processes.CreateAsync(process);
            return process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process process)
        {
            var existing = await _processes.GetByIdAsync(p => p.Id == id);
            if (existing == null) return false;

            process.ProcessTime = CalculateProcessTime(existing.StartDate);

            var update = Builders<Process>.Update
                .Set(p => p.IdentificationNumber, process.IdentificationNumber ?? existing.IdentificationNumber)
                .Set(p => p.ProcessTime, process.ProcessTime)
                .Set(p => p.StartDate, process.StartDate)
                .Set(p => p.EndDate, process.EndDate)
                .Set(p => p.User, process.User ?? existing.User)
                .Set(p => p.Department, process.Department ?? existing.Department)
                .Set(p => p.ProcessType, process.ProcessType ?? existing.ProcessType)
                .Set(p => p.Finished, process.Finished)
                .Set(p => p.ReWork, process.ReWork)
                .Set(p => p.Machine, process.Machine ?? existing.Machine);

            var result = await _processes.GetCollection().UpdateOneAsync(p => p.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            return await _processes.DeleteAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateProcessTimeInDatabase(string processId, string processTime)
        {
            var update = Builders<Process>.Update.Set(p => p.ProcessTime, processTime);
            var result = await _processes.GetCollection().UpdateOneAsync(p => p.Id == processId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<UserProcessDetailsDto> GetUserProcessDataAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) throw new ArgumentException($"Usuário com ID {userId} não encontrado.");

            var dto = new UserProcessDetailsDto
            {
                UserId = user.Id,
                UserName = user.Name,
                UrlPhoto = user.UrlPhoto,
                CurrentOccurrenceName = user.CurrentOccurrence?.Name ?? "Nenhuma"
            };

            if (string.IsNullOrWhiteSpace(user.CurrentProcess?.Id))
            {
                dto.CurrentProcessId = "Nenhum processo atual";
                return dto;
            }

            var process = await _processes.GetByIdAsync(p => p.Id == user.CurrentProcess.Id);
            if (process == null)
            {
                dto.CurrentProcessId = "Processo não encontrado";
                return dto;
            }

            dto.CurrentProcessId = process.Id;
            dto.TypeProcessName = process.ProcessType.Name;

            var stats = await GetProcessStatisticsAsync(process.Id, process.ProcessType.Id);
            dto.ProcessTime = stats.ProcessTime;
            dto.AverageProcessTime = stats.AverageProcessTime;
            dto.StandardDeviation = stats.StandardDeviation;
            dto.UpperLimit = stats.UpperLimit;

            return dto;
        }

        public async Task<ProcessStatisticsDto> GetProcessStatisticsAsync(string processId, string processTypeId)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == processId);
            var filtered = await _processes.GetCollection().Find(p => p.Finished && p.ProcessType.Id == processTypeId && !string.IsNullOrEmpty(p.ProcessTime)).ToListAsync();

            var durations = filtered
                .Where(p => TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out _))
                .Select(p => TimeSpan.Parse(p.ProcessTime))
                .ToList();

            if (!durations.Any()) return new ProcessStatisticsDto();

            var average = TimeSpan.FromTicks((long)durations.Average(d => d.Ticks));
            var stdDev = TimeSpan.FromTicks((long)Math.Sqrt(durations.Average(d => Math.Pow(d.Ticks - durations.Average(x => x.Ticks), 2))));
            var upperLimit = average + (stdDev * 2);

            return new ProcessStatisticsDto
            {
                ProcessTime = process?.ProcessTime,
                AverageProcessTime = average.ToString(@"hh\:mm\:ss"),
                StandardDeviation = stdDev.ToString(@"hh\:mm\:ss"),
                UpperLimit = upperLimit.ToString(@"hh\:mm\:ss")
            };
        }

        private string CalculateProcessTime(DateTime startDate)
        {
            var now = GetCurrentCampoGrandeTime();
            if (now < startDate) return "00:00:00";

            TimeSpan workStart = TimeSpan.FromHours(7.5);
            TimeSpan workEnd = TimeSpan.FromHours(17.5);
            TimeSpan totalTime = TimeSpan.Zero;
            DateTime currentDay = startDate.Date;

            while (currentDay <= now.Date)
            {
                DateTime start = currentDay == startDate.Date ? startDate : currentDay.Add(workStart);
                DateTime end = currentDay == now.Date ? now : currentDay.Add(workEnd);

                if (start.TimeOfDay < workStart) start = currentDay.Add(workStart);
                if (end.TimeOfDay > workEnd) end = currentDay.Add(workEnd);

                if (start < end && start.TimeOfDay >= workStart && start.TimeOfDay <= workEnd)
                    totalTime += end - start;

                currentDay = currentDay.AddDays(1);
            }

            return $"{(int)totalTime.TotalHours:D2}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";
        }

        private DateTime GetCurrentCampoGrandeTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
        }

        private static string GenerateNumericCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
