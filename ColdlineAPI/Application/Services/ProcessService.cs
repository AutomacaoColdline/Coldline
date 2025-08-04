using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Infrastructure.Utilities; 
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
        private readonly IMongoCollection<Occurrence> _occurrences;
        private readonly IMongoCollection<ProcessType> _processTypes;
        private readonly IMongoCollection<Machine> _machines;
        private readonly MongoRepository<Machine> _machinesRepo;
        private const string SpecialProcessTypeId = "67f6b00f101359f06e303492";

        public ProcessService(RepositoryFactory factory)
        {
            _processes = factory.CreateRepository<Process>("Processes");
            _users = factory.CreateRepository<User>("Users").GetCollection();
            _processTypes = factory.CreateRepository<ProcessType>("ProcessTypes").GetCollection();
            _machines = factory.CreateRepository<Machine>("Machines").GetCollection();
            _machinesRepo = factory.CreateRepository<Machine>("Machines");
            _occurrences = factory.CreateRepository<Occurrence>("Occurrences").GetCollection();
        }

        public async Task<Process?> StartProcessAsync(string identificationNumber, string processTypeId, string? machineId, bool preIndustrialization, bool reWork,bool prototype )
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
                machineReference = new ReferenceEntity { Id = machine.Id, Name = machine.IdentificationNumber };
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
                Prototype = prototype,
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
                var duration = await CalculateProcessTime(process.StartDate, process.EndDate);

                process.ProcessTime = duration.ToString(@"hh\:mm\:ss");

                await _processes.UpdateAsync(p => p.Id == process.Id, process); // Atualiza o processo com a nova duração
            }
            
            return processes;
        }

        public async Task<Process?> GetProcessByIdAsync(string id)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == id);
            var user = await _users.Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();

            if (user.CurrentOccurrence == null || string.IsNullOrEmpty(user.CurrentOccurrence.Id))
            {
                var duration = await CalculateProcessTime(process.StartDate, process.EndDate);
                process.ProcessTime = duration.ToString(@"hh\:mm\:ss");
                await _processes.UpdateAsync(p => p.Id == id, process);
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
            if (filter.Prototype.HasValue) filters.Add(builder.Eq(p => p.Prototype, filter.Prototype));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var results = await _processes.GetCollection().Find(finalFilter).Skip(skip).Limit(pageSize).ToListAsync();
            foreach (var process in results)
            {
                var duration = await CalculateProcessTime(process.StartDate, process.EndDate);

                process.ProcessTime = duration.ToString(@"hh\:mm\:ss");

                await _processes.UpdateAsync(p => p.Id == process.Id, process);
            }
            return results;
        }
        
       public async Task<List<ProcessByDateDto>> GetProcessCountByStartDateAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte("start date", start) &
                        Builders<Process>.Filter.Lte("start date", end);

            var result = await _processes.GetCollection()
                .Aggregate()
                .Match(filter)
                .Group(new BsonDocument {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument {
                        { "format", "%Y-%m-%d" },
                        { "date", "$start date" }  // importante: nome real do campo no Mongo
                    })},
                    { "count", new BsonDocument("$sum", 1) }
                })
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();

            return result
                .Where(doc => doc.Contains("_id") && doc["_id"].IsString)
                .Select(doc => new ProcessByDateDto
                {
                    Date = doc["_id"].AsString,
                    Quantity = doc["count"].AsInt32
                }).ToList();
        }

        public async Task<List<ProcessTypeChartDto>> GetProcessCountByTypeAndDateAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte("start date", start) &
                        Builders<Process>.Filter.Lte("start date", end);

            var result = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var agrupado = result
                .Where(p => p.ProcessType != null && !string.IsNullOrEmpty(p.ProcessType.Name))
                .GroupBy(p => p.ProcessType!.Name)
                .Select(g => new ProcessTypeChartDto
                {
                    ProcessTypeName = g.Key,
                    Quantity = g.Count()
                })
                .ToList();

            return agrupado;
        }

        public async Task<List<ProcessUserChartDto>> GetProcessCountByUserAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte("start date", start) &
                        Builders<Process>.Filter.Lte("start date", end);

            var result = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var agrupado = result
                .Where(p => p.User != null && !string.IsNullOrEmpty(p.User.Name))
                .GroupBy(p => p.User!.Name)
                .Select(g => new ProcessUserChartDto
                {
                    UserName = g.Key,
                    Quantity = g.Count()
                })
                .ToList();

            return agrupado;
        }

        public async Task<List<UserTotalProcessTimeDto>> GetTotalProcessTimeByUserAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                        Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var groupedByUser = allProcesses
                .Where(p => p.User != null && !string.IsNullOrWhiteSpace(p.User.Id))
                .GroupBy(p => p.User.Id)
                .Select(g =>
                {
                    string userId = g.Key;
                    string userName = g.First().User.Name ?? "Desconhecido";

                    TimeSpan total = TimeSpan.Zero;
                    int processCount = 0;

                    foreach (var p in g)
                    {
                        if (!string.IsNullOrWhiteSpace(p.ProcessTime) &&
                            TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out var duration))
                        {
                            total += duration;
                        }

                        processCount++;
                    }

                    // ✅ Corrige o formato de hora total para não resetar a cada 24h
                    string formattedTotalTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        (int)total.TotalHours, total.Minutes, total.Seconds);

                    return new UserTotalProcessTimeDto
                    {
                        UserId = userId,
                        UserName = userName,
                        TotalProcessTime = formattedTotalTime,
                        ProcessCount = processCount
                    };
                })
                .ToList();

            return groupedByUser;
        }

        public async Task<List<ProcessTypeTotalTimeDto>> GetTotalProcessTimeByProcessTypeAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                        Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var groupedByType = allProcesses
                .Where(p => p.ProcessType != null && !string.IsNullOrWhiteSpace(p.ProcessType.Id))
                .GroupBy(p => p.ProcessType.Id)
                .Select(g =>
                {
                    string typeId = g.Key;
                    string typeName = g.First().ProcessType.Name ?? "Desconhecido";

                    TimeSpan total = TimeSpan.Zero;
                    int count = 0;

                    foreach (var p in g)
                    {
                        if (!string.IsNullOrWhiteSpace(p.ProcessTime) &&
                            TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out var duration))
                        {
                            total += duration;
                        }
                        count++;
                    }

                    string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        (int)total.TotalHours, total.Minutes, total.Seconds);

                    return new ProcessTypeTotalTimeDto
                    {
                        ProcessTypeId = typeId,
                        ProcessTypeName = typeName,
                        TotalProcessTime = formattedTime,
                        ProcessCount = count
                    };
                })
                .ToList();

            return groupedByType;
        }

        public async Task<List<IndividualUserProcessDto>> GetIndividualProcessTimesByUserAsync(string userId, DateTime startDate, DateTime endDate, bool? preIndustrialization = null)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new ArgumentException("Usuário não encontrado.");

            var builder = Builders<Process>.Filter; 
            var filter = Builders<Process>.Filter.Eq(p => p.User.Id, userId) &
                        Builders<Process>.Filter.Gte(p => p.StartDate, startDate) &
                        Builders<Process>.Filter.Lte(p => p.StartDate, endDate);
            
            if (preIndustrialization.HasValue)
            {
                filter &= builder.Eq(p => p.PreIndustrialization, preIndustrialization.Value);
            }

            var processos = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var resultado = new List<IndividualUserProcessDto>();

            foreach (var processo in processos)
            {
                // Recalcula tempo se necessário
                var duration = await CalculateProcessTime(processo.StartDate, processo.EndDate);
                processo.ProcessTime = duration.ToString(@"hh\:mm\:ss");

                resultado.Add(new IndividualUserProcessDto
                {
                    ProcessId = processo.Id!,
                    IdentificationNumber = processo.IdentificationNumber,
                    ProcessTypeName = processo.ProcessType?.Name ?? "Desconhecido",
                    StartDate = processo.StartDate,
                    EndDate = processo.EndDate,
                    ProcessTime = processo.ProcessTime
                });
            }

            return resultado;
        }



       private string SumProcessTimes(IEnumerable<string> times)
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (var t in times)
            {
                if (TimeSpan.TryParseExact(t, @"hh\:mm\:ss", null, out TimeSpan parsed))
                {
                    total += parsed;
                }
            }
            return total.ToString(@"hh\:mm\:ss");
        }


        public async Task<Process> CreateProcessAsync(Process process)
        {
            if (string.IsNullOrEmpty(process.Id))
                process.Id = ObjectId.GenerateNewId().ToString();

            var duration = await CalculateProcessTime(process.StartDate, process.EndDate);
            process.ProcessTime = duration.ToString(@"hh\:mm\:ss");
            process.IdentificationNumber = GenerateNumericCode();

            if (process.ProcessType.Id == SpecialProcessTypeId)
            {
                var machine = await _machinesRepo.GetByIdAsync(p => p.Id == process.Machine.Id);
                var processFilter = new ProcessFilter { MachineId = process.Machine.Id };
                var processList = await SearchProcessAsync(processFilter);
                var startdate = await CalculateMachineTime(processList);
                var timeMachine = await CalculateProcessTime(startdate, process.EndDate);

                await _machines.UpdateOneAsync(
                    Builders<Machine>.Filter.Eq(m => m.Id, process.Machine.Id),
                    Builders<Machine>.Update
                        .Set(m => m.Status, MachineStatus.Finished)
                        .Set(m => m.Process, null)
                        .Set(m => m.Time, timeMachine.ToString(@"hh\:mm\:ss")));
            }

            await _processes.CreateAsync(process);
            return process;
        }

        public async Task<bool> UpdateProcessAsync(string id, Process process)
        {
            var existing = await _processes.GetByIdAsync(p => p.Id == id);
            if (existing == null) return false;


            var duration = await CalculateProcessTime(process.StartDate, process.EndDate);
            process.ProcessTime = duration.ToString(@"hh\:mm\:ss");
                
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
            dto.TypeProcessName = stats.ProcessTypeName;
            dto.ProcessTime = stats.ProcessTime;
            dto.AverageProcessTime = stats.AverageProcessTime;
            dto.StandardDeviation = stats.StandardDeviation;
            dto.UpperLimit = stats.UpperLimit;

            return dto;
        }

        public async Task<ProcessStatisticsDto> GetProcessStatisticsAsync(string processId, string processTypeId)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == processId);
            var user = await _users.Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();

            var filtered = await _processes.GetCollection()
                .Find(p => p.Finished && p.ProcessType.Id == processTypeId && !string.IsNullOrEmpty(p.ProcessTime))
                .ToListAsync();

            var durations = filtered
                .Where(p => TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out _))
                .Select(p => TimeSpan.Parse(p.ProcessTime))
                .ToList();

            var dto = new ProcessStatisticsDto
            {
                ProcessTime = process?.ProcessTime ?? "00:00:00"
            };

            if (user.CurrentOccurrence != null)
            {
                var ocurrence = await _occurrences.Find(u => u.Id == user.CurrentOccurrence.Id).FirstOrDefaultAsync();
                dto.OcorrenceTypeName = ocurrence?.PauseType?.Name ?? "Nenhuma";
            }
            else
            {
                dto.OcorrenceTypeName = "Nenhuma";
            }

            if (durations.Count == 0)
            {
                // Sem dados suficientes para estatísticas
                dto.AverageProcessTime = "00:00:00";
                dto.StandardDeviation = "00:00:00";
                dto.UpperLimit = "00:00:00";
                dto.ProcessTypeName = process?.ProcessType?.Name ?? "Não identificado";
                return dto;
            }

            var averageTicks = durations.Average(d => d.Ticks);
            var stdDevTicks = Math.Sqrt(durations.Average(d => Math.Pow(d.Ticks - averageTicks, 2)));
            var upperLimitTicks = averageTicks + stdDevTicks * 2;

            dto.AverageProcessTime = TimeSpan.FromTicks((long)averageTicks).ToString(@"hh\:mm\:ss");
            dto.StandardDeviation = TimeSpan.FromTicks((long)stdDevTicks).ToString(@"hh\:mm\:ss");
            dto.UpperLimit = TimeSpan.FromTicks((long)upperLimitTicks).ToString(@"hh\:mm\:ss");
            dto.ProcessTypeName = process?.ProcessType?.Name ?? "Não identificado";

            return dto;
        }


        private async Task<DateTime> CalculateMachineTime(List<Process> processes)
        {
            if (processes == null || !processes.Any())
                throw new ArgumentException("A lista de processos está vazia ou nula.");

            DateTime menorData = processes.Min(p => p.StartDate);

            return menorData;
        }


        private async Task<TimeSpan> CalculateProcessTime(DateTime start, DateTime? end)
        {
            DateTime endTime = end ?? GetCurrentCampoGrandeTime();

            TimeSpan total = TimeSpan.Zero;

            // Períodos úteis por dia
            var workPeriods = new List<(TimeSpan start, TimeSpan end)>
            {
                (TimeSpan.FromHours(7.5), TimeSpan.FromHours(11.5)),   // 07:30 – 11:30
                (TimeSpan.FromHours(13.25), TimeSpan.FromHours(15.0)), // 13:15 – 15:00
                (TimeSpan.FromHours(15.25), TimeSpan.FromHours(17.5))  // 15:15 – 17:30
            };

            DateTime currentDate = start.Date;

            while (currentDate <= endTime.Date)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    foreach (var period in workPeriods)
                    {
                        DateTime periodStart = currentDate.Add(period.start);
                        DateTime periodEnd = currentDate.Add(period.end);

                        // Ajuste para o intervalo real
                        DateTime effectiveStart = periodStart < start ? start : periodStart;
                        DateTime effectiveEnd = periodEnd > endTime ? endTime : periodEnd;

                        if (effectiveEnd > effectiveStart)
                        {
                            total += effectiveEnd - effectiveStart;
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return total;
        }

        public async Task<byte[]> GenerateExcelReportAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, startDate) &
                        Builders<Process>.Filter.Lte(p => p.StartDate, endDate);

            var processos = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var linhas = new List<List<string>>
            {
                new() { "Usuário", "Tipo de Processo", "Tempo de Processo" }
            };

            foreach (var processo in processos)
            {
                var duration = await CalculateProcessTime(processo.StartDate, processo.EndDate);
                var tempoFormatado = duration.ToString(@"hh\:mm\:ss");

                linhas.Add(new List<string>
                {
                    processo.User?.Name ?? "Desconhecido",
                    processo.ProcessType?.Name ?? "Desconhecido",
                    tempoFormatado
                });
            }

            return Utils.GenerateExcel(linhas); // Aqui usa a utility já existente
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
