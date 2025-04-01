using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
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
        private readonly IMongoCollection<Process> _processes;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ProcessType> _processTypes;
        private readonly IMongoCollection<Machine> _machines;

        public ProcessService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _processes = database.GetCollection<Process>("Processes");
            _users = database.GetCollection<User>("Users");
            _processTypes = database.GetCollection<ProcessType>("ProcessTypes");
            _machines = database.GetCollection<Machine>("Machines");
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

            TimeZoneInfo campoGrandeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
            DateTime campoGrandeTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, campoGrandeTimeZone);

            var newProcess = new Process
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IdentificationNumber = GenerateNumericCode(),
                ProcessTime = "00:00:00",
                StartDate = campoGrandeTime,
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

            await _processes.InsertOneAsync(newProcess);

            var updateUser = Builders<User>.Update
                .Set(u => u.CurrentProcess, new ReferenceEntity { Id = newProcess.Id, Name = newProcess.IdentificationNumber });
            await _users.UpdateOneAsync(u => u.Id == user.Id, updateUser);

            if (machineReference != null)
            {
                var updateMachine = Builders<Machine>.Update
                    .Set(m => m.Status, MachineStatus.InProgress)
                    .Set(m => m.Process, new ReferenceEntity { Id = newProcess.Id, Name = newProcess.IdentificationNumber });
                await _machines.UpdateOneAsync(m => m.Id == machineReference.Id, updateMachine);
            }

            return newProcess;
        }

        public async Task<bool> EndProcessAsync(string processId)
        {
            var process = await _processes.Find(p => p.Id == processId).FirstOrDefaultAsync();
            if (process == null)
            {
                throw new ArgumentException("Processo não encontrado.");
            }

            // 🔹 Verifica se o usuário ainda tem uma ocorrência aberta
            var user = await _users.Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ArgumentException("Usuário do processo não encontrado.");
            }

            if (user.CurrentOccurrence != null)
            {
                throw new InvalidOperationException("Não é possível finalizar o processo enquanto houver uma ocorrência aberta.");
            }

            // 🔹 Obtém a data e hora no fuso correto
            TimeZoneInfo campoGrandeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
            DateTime campoGrandeTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, campoGrandeTimeZone);

            // 🔹 Atualiza o processo para finalizado
            var updateProcess = Builders<Process>.Update
                .Set(p => p.Finished, true)
                .Set(p => p.EndDate, campoGrandeTime); // Define a data de finalização

            var resultProcess = await _processes.UpdateOneAsync(p => p.Id == processId, updateProcess);

            // 🔹 Remove o processo atual do usuário
            var updateUser = Builders<User>.Update.Set(u => u.CurrentProcess, null);
            var resultUser = await _users.UpdateOneAsync(u => u.Id == user.Id, updateUser);

            return resultProcess.IsAcknowledged && resultProcess.ModifiedCount > 0 &&
                resultUser.IsAcknowledged && resultUser.ModifiedCount > 0;
        }



        public async Task<List<Process>> GetAllProcessesAsync()
        {
            var processes = await _processes.Find(process => true).ToListAsync();
            foreach (var process in processes)
            {
                string updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id, updatedTime);
                process.ProcessTime = updatedTime;
            }
            return processes;
        }

        public async Task<Process?> GetProcessByIdAsync(string id)
        {
            var process = await _processes.Find(process => process.Id == id).FirstOrDefaultAsync();
            if (process != null)
            {
                string updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id, updatedTime);
                process.ProcessTime = updatedTime;
            }
            return process;
        }

        public async Task<List<Process>> SearchProcessAsync(ProcessFilter filter)
        {
            var filters = new List<FilterDefinition<Process>>();
            var builder = Builders<Process>.Filter;

            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
                filters.Add(builder.Eq(p => p.IdentificationNumber, filter.IdentificationNumber));

            if (filter.StartDate.HasValue)
                filters.Add(builder.Gte(p => p.StartDate, filter.StartDate.Value));

            if (filter.EndDate.HasValue)
                filters.Add(builder.Lte(p => p.EndDate, filter.EndDate.Value));

            if (!string.IsNullOrEmpty(filter.UserId))
                filters.Add(builder.Eq(p => p.User.Id, filter.UserId));

            if (!string.IsNullOrEmpty(filter.DepartmentId))
                filters.Add(builder.Eq(p => p.Department.Id, filter.DepartmentId));

            if (!string.IsNullOrEmpty(filter.ProcessTypeId))
                filters.Add(builder.Eq(p => p.ProcessType.Id, filter.ProcessTypeId));

            if (!string.IsNullOrEmpty(filter.MachineId))
                filters.Add(builder.Eq(p => p.Machine.Id, filter.MachineId));

            if (filter.Finished.HasValue)
                filters.Add(builder.Eq(p => p.Finished, filter.Finished));

            if (filter.PreIndustrialization.HasValue)
                filters.Add(builder.Eq(p => p.PreIndustrialization, filter.PreIndustrialization));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            // Paginação
            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var processes = await _processes.Find(finalFilter)
                                            .Skip(skip)
                                            .Limit(pageSize)
                                            .ToListAsync();

            foreach (var process in processes)
            {
                string updatedTime = CalculateProcessTime(process.StartDate);
                await UpdateProcessTimeInDatabase(process.Id!, updatedTime);
                process.ProcessTime = updatedTime;
            }

            return processes;
        }


        public async Task<Process> CreateProcessAsync(Process process)
        {
            if (string.IsNullOrEmpty(process.Id))
                process.Id = ObjectId.GenerateNewId().ToString();

            await _processes.InsertOneAsync(process);
            return process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process process)
        {
            var existingProcess = await _processes.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingProcess == null) return false;

            process.ProcessTime = CalculateProcessTime(existingProcess.StartDate);

            var updateDefinition = Builders<Process>.Update
                .Set(p => p.IdentificationNumber, process.IdentificationNumber ?? existingProcess.IdentificationNumber)
                .Set(p => p.ProcessTime, process.ProcessTime)
                .Set(p => p.StartDate, process.StartDate)
                .Set(p => p.EndDate, process.EndDate)
                .Set(p => p.User, process.User ?? existingProcess.User)
                .Set(p => p.Department, process.Department ?? existingProcess.Department)
                .Set(p => p.ProcessType, process.ProcessType ?? existingProcess.ProcessType)
                .Set(p => p.Finished, process.Finished)
                .Set(p => p.ReWork, process.ReWork)
                .Set(p => p.Machine, process.Machine ?? existingProcess.Machine);

            var result = await _processes.UpdateOneAsync(p => p.Id == id, updateDefinition);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var result = await _processes.DeleteOneAsync(p => p.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> UpdateProcessTimeInDatabase(string processId, string processTime)
        {
            var update = Builders<Process>.Update.Set(p => p.ProcessTime, processTime);
            var result = await _processes.UpdateOneAsync(p => p.Id == processId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        public async Task<UserProcessDetailsDto> GetUserProcessDataAsync(string userId)
        {
            // 1) Acha o usuário
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ArgumentException($"Usuário com ID {userId} não encontrado.");
            }

            // Cria um DTO base com infos do usuário
            var dto = new UserProcessDetailsDto
            {
                UserId = user.Id!,
                UserName = user.Name,
                UrlPhoto = user.UrlPhoto,
                CurrentOccurrenceName = user.CurrentOccurrence?.Name ?? "Nenhuma"
            };

            // 2) Verifica se há CurrentProcess
            if (string.IsNullOrWhiteSpace(user.CurrentProcess?.Id))
            {
                // Retorna com ProcessTime = "00:00:00" e etc. (pode customizar se quiser)
                dto.CurrentProcessId = "Nenhum processo atual";
                return dto;
            }

            // 3) Carrega o processo atual
            var process = await _processes.Find(p => p.Id == user.CurrentProcess.Id).FirstOrDefaultAsync();
            if (process == null)
            {
                // Se não achar o processo, devolve sem stats
                dto.CurrentProcessId = "Processo não encontrado";
                return dto;
            }

            // Atribui o ID (caso precise exibir no front)
            dto.CurrentProcessId = process.Id!;
            dto.TypeProcessName = process.ProcessType.Name;

            // 4) Carrega as estatísticas (se quiser, pode chamar seu método existente de estatísticas)
            // Ex: var statsDto = await GetProcessStatisticsAsync(process.Id, process.ProcessType.Id);
            // Supondo que esse método retorne as 4 infos que você quer (ProcessTime, Avg, Std, Upper).
            var stats = await GetProcessStatisticsAsync(process.Id!, process.ProcessType!.Id!);
            
            // 5) Ajusta os valores no DTO
            dto.ProcessTime = stats.ProcessTime; 
            dto.AverageProcessTime = stats.AverageProcessTime;
            dto.StandardDeviation = stats.StandardDeviation;
            dto.UpperLimit = stats.UpperLimit;

            // 6) Retorna DTO completo
            return dto;
        }


        public async Task<ProcessStatisticsDto> GetProcessStatisticsAsync(string processId, string processTypeId)
        {
            var process = await _processes.Find(process => process.Id == processId).FirstOrDefaultAsync();
            var filteredProcesses = await _processes
                .Find(p => p.Finished == true && p.ProcessType.Id == processTypeId && !string.IsNullOrEmpty(p.ProcessTime))
                .ToListAsync();

            if (!filteredProcesses.Any())
            {
                return new ProcessStatisticsDto();
            }

            var durations = new List<TimeSpan>();
            foreach (var p in filteredProcesses)
            {
                if (TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out var duration))
                {
                    durations.Add(duration);
                }
            }

            if (!durations.Any())
            {
                return new ProcessStatisticsDto();
            }

            // 🔹 Calcula a Média
            TimeSpan total = durations.Aggregate(TimeSpan.Zero, (sum, next) => sum + next);
            TimeSpan averageTime = TimeSpan.FromTicks(total.Ticks / durations.Count);

            // 🔹 Calcula o Desvio Padrão Populacional
            double meanTicks = durations.Average(d => d.Ticks);
            double variance = durations.Average(d => Math.Pow(d.Ticks - meanTicks, 2));
            TimeSpan standardDeviation = TimeSpan.FromTicks((long)Math.Sqrt(variance));

            // 🔹 Define o Limite Superior com 2 desvios padrão
            TimeSpan upperLimit = averageTime + (standardDeviation * 2);

            return new ProcessStatisticsDto
            {
                ProcessTime = process.ProcessTime,
                AverageProcessTime = averageTime.ToString(@"hh\:mm\:ss"),
                StandardDeviation = standardDeviation.ToString(@"hh\:mm\:ss"),
                UpperLimit = upperLimit.ToString(@"hh\:mm\:ss") // Novo cálculo do limite superior
            };
        }


        private string CalculateProcessTime(DateTime startDate)
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));

            // 🔥 Garante que não haverá erro de tempo negativo
            if (now < startDate)
            {
                return "00:00:00";
            }

            // Definir horário comercial
            TimeSpan workStart = TimeSpan.FromHours(7.5);  // 07:30
            TimeSpan workEnd = TimeSpan.FromHours(17.5);   // 17:30

            TimeSpan totalTime = TimeSpan.Zero;
            DateTime currentDay = startDate.Date;

            while (currentDay <= now.Date)
            {
                DateTime start = currentDay == startDate.Date ? startDate : currentDay.Add(workStart);
                DateTime end = currentDay == now.Date ? now : currentDay.Add(workEnd);

                // 🔥 Ajuste correto para respeitar o horário comercial
                if (start.TimeOfDay < workStart) start = currentDay.Add(workStart);
                if (end.TimeOfDay > workEnd) end = currentDay.Add(workEnd);

                // 🔥 Só soma tempo SE estamos dentro do horário comercial
                if (now.TimeOfDay >= workStart && now.TimeOfDay <= workEnd)
                {
                    totalTime += end - start;
                }

                currentDay = currentDay.AddDays(1);
            }

            return $"{(int)totalTime.TotalHours:D2}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";
        }
        private static string GenerateNumericCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Gera um número entre 100000 e 999999
        }

    }
}
