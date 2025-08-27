using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.DTOs;
using System.Text.RegularExpressions;


using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMongoCollection<Process> _processes;
        private readonly IProcessService _processService;
        private readonly IOccurrenceService _occurrenceService;
        private readonly MongoRepository<Machine> _machines;

        public MachineService(IProcessService processService, IOccurrenceService occurrenceService, RepositoryFactory factory)
        {
            _machines = factory.CreateRepository<Machine>("Machines");
            _processes = factory.CreateRepository<Process>("Processes").GetCollection();
            _processService = processService;
            _occurrenceService = occurrenceService;
        }

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            return await _machines.GetAllAsync();
        }

        public async Task<Machine?> GetMachineByIdAsync(string id)
        {
            return await _machines.GetByIdAsync(m => m.Id == id);
        }
       public async Task<List<MachineByDateDto>> GetMachineCountPerDayAsync(DateTime startDate, DateTime endDate)
        {
            var collection = _machines.GetCollection();

            var filter = Builders<Machine>.Filter.And(
                Builders<Machine>.Filter.Gte(m => m.CreatedAt, startDate),
                Builders<Machine>.Filter.Lte(m => m.CreatedAt, endDate)
            );

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "createdAt", new BsonDocument {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    }}
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument {
                        { "$dateToString", new BsonDocument {
                            { "format", "%Y-%m-%d" },
                            { "date", "$createdAt" }
                        }}
                    }},
                    { "count", new BsonDocument { { "$sum", 1 } } }
                }),
                new BsonDocument("$sort", new BsonDocument { { "_id", 1 } })
            };

            var rawResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return rawResult.Select(doc => new MachineByDateDto
            {
                Date = doc["_id"].AsString,
                Count = doc["count"].AsInt32
            }).ToList();
        }
        public async Task<List<MachineByTypeDto>> GetMachineCountByTypeAsync(DateTime startDate, DateTime endDate)
        {
            var collection = _machines.GetCollection();

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "createdAt", new BsonDocument {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    }}
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$machineType.Name" }, // ⚠️ CORREÇÃO AQUI
                    { "count", new BsonDocument { { "$sum", 1 } } }
                }),
                new BsonDocument("$sort", new BsonDocument { { "count", -1 } })
            };

            var rawResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return rawResult.Select(doc => new MachineByTypeDto
            {
                Type = doc["_id"].IsBsonNull ? "Desconhecido" : doc["_id"].AsString,
                Count = doc["count"].AsInt32
            }).ToList();
        }


        public async Task<List<MachineTotalProcessTimeDto>> GetTotalProcessTimeByMachineAsync(DateTime start, DateTime end)
        {
            var machineCollection = _machines.GetCollection();

            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                        Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.Find(filter).ToListAsync();
            var machines = await machineCollection.Find(Builders<Machine>.Filter.Empty).ToListAsync();

            var groupedByMachine = allProcesses
                .Where(p => p.Machine != null && !string.IsNullOrWhiteSpace(p.Machine.Id))
                .GroupBy(p => p.Machine.Id)
                .Select(g =>
                {
                    string machineId = g.Key;
                    string machineName = g.First().Machine.Name ?? "Desconhecida";

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

                    var machineData = machines.FirstOrDefault(m => m.Id == machineId);

                    return new MachineTotalProcessTimeDto
                    {
                        MachineId = machineId,
                        IdentificationNumber = machineName,
                        CustomerName = machineData?.CustomerName ?? "Desconhecido",
                        TotalProcessTime = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)total.TotalHours, total.Minutes, total.Seconds),
                        ProcessCount = processCount,
                        MachineTypeName = machineData?.MachineType?.Name ?? "Desconhecido"
                    };
                })
                .ToList();

            return groupedByMachine;
        }
        public async Task<MachineDashboardDto> GetMachineDashboardAsync(DateTime startDate, DateTime endDate)
        {
            var machineCollection = _machines.GetCollection();

            // 1. Buscar máquinas criadas no intervalo
            var machineFilter = Builders<Machine>.Filter.Gte(m => m.CreatedAt, startDate) &
                                Builders<Machine>.Filter.Lte(m => m.CreatedAt, endDate);
            var machines = await machineCollection.Find(machineFilter).ToListAsync();
            var machineIds = machines.Select(m => m.Id).ToHashSet();

            // ➕ NOVO: contar máquinas finalizadas (status == 5)
            var finishedCount = machines.Count(m => m.Status == MachineStatus.Finished); // MachineStatus.Finished == 5

            // 2. Buscar todos os processos do período
            var totalProcessFilter = Builders<Process>.Filter.Gte(p => p.StartDate, startDate) &
                                    Builders<Process>.Filter.Lte(p => p.StartDate, endDate);
            var allProcesses = await _processes.Find(totalProcessFilter).ToListAsync();

            // 3. Filtrar processos ligados às máquinas encontradas
            var relatedProcesses = allProcesses
                .Where(p => p.Machine != null && machineIds.Contains(p.Machine.Id))
                .ToList();

            // 4. Tempo e contagem por máquina
            var machineTimes = relatedProcesses
                .Where(p => p.Machine != null && !string.IsNullOrWhiteSpace(p.Machine.Id))
                .GroupBy(p => p.Machine.Id)
                .Select(g =>
                {
                    var machine = machines.FirstOrDefault(m => m.Id == g.Key);
                    var total = TimeSpan.Zero;
                    int processCount = 0;

                    foreach (var p in g)
                    {
                        if (!string.IsNullOrWhiteSpace(p.ProcessTime) &&
                            TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out var duration))
                        {
                            total += duration;
                            processCount++;
                        }
                    }

                    return new MachineTotalProcessTimeDto
                    {
                        MachineId = g.Key,
                        IdentificationNumber = machine?.IdentificationNumber ?? "Desconhecida",
                        CustomerName = machine?.CustomerName ?? "Desconhecido",
                        TotalProcessTime = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)total.TotalHours, total.Minutes, total.Seconds),
                        ProcessCount = processCount,
                        MachineTypeName = machine?.MachineType?.Name ?? "Desconhecido"
                    };
                })
                .ToList();

            // 5. Agrupamento por tipo
            var machineTypeCounts = machines
                .GroupBy(m => m.MachineType?.Name ?? "Desconhecido")
                .Select(g => new MachineByTypeDto
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // 6. Processos por usuário e tipo
            var userProcessCounts = await _processService.GetTotalProcessTimeByUserAsync(startDate, endDate);
            var processCountByType = await _processService.GetProcessCountByTypeAndDateAsync(startDate, endDate);

            // 7. Tempo médio por tipo de máquina
            var machineTypeAverages = await GetAverageProcessTimeByMachineTypeAsync(startDate, endDate);

            // ➕ NOVO: buscar quantidade de ocorrências no período
            var occurrences = await _occurrenceService.GetAllOccurrencesAsync();
            var totalOccurrences = occurrences.Count(o =>
                o.StartDate >= startDate &&
                o.StartDate <= endDate
            );

            // ✅ retorno
            return new MachineDashboardDto
            {
                TotalMachines = machines.Count,
                TotalProcesses = allProcesses.Count,
                FinishedMachines = finishedCount,           // NOVO
                TotalOccurrences = totalOccurrences,        // NOVO
                Machines = machineTimes,
                MachineTypeCounts = machineTypeCounts,
                ProcessCountByUser = userProcessCounts,
                ProcessCountByType = processCountByType,
                MachineTypeAverageTimes = machineTypeAverages
            };
        }

        public async Task<int> UpdateMachinesCreatedAtAsync()
        {
            var machineCollection = _machines.GetCollection();
            var processCollection = _processes;

            var todasMaquinas = await machineCollection.Find(Builders<Machine>.Filter.Empty).ToListAsync();
            int atualizadas = 0;

            foreach (var maquina in todasMaquinas)
            {
                if (string.IsNullOrWhiteSpace(maquina.Id))
                    continue;

                var filtro = Builders<Process>.Filter.Eq(p => p.Machine.Id, maquina.Id);
                var processos = await processCollection.Find(filtro)
                    .SortBy(p => p.StartDate)
                    .Limit(1)
                    .ToListAsync();

                if (processos.Count == 0)
                    continue;

                var menorData = processos[0].StartDate;

                if (maquina.CreatedAt != menorData)
                {
                    var update = Builders<Machine>.Update.Set(m => m.CreatedAt, menorData);
                    await machineCollection.UpdateOneAsync(m => m.Id == maquina.Id, update);
                    atualizadas++;
                }
            }

            return atualizadas;
        }


        public async Task<List<MachineTypeAverageTimeDto>> GetAverageProcessTimeByMachineTypeAsync(DateTime start, DateTime end)
        {
            var machineCollection = _machines.GetCollection();

            // 1. Buscar todos os processos no intervalo desejado
            var processFilter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                                Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.Find(processFilter).ToListAsync();

            // 2. Buscar todas as máquinas citadas nesses processos
            var machineIds = allProcesses
                .Where(p => p.Machine != null && !string.IsNullOrWhiteSpace(p.Machine.Id))
                .Select(p => p.Machine.Id)
                .Distinct()
                .ToList();

            var machines = await machineCollection.Find(m => machineIds.Contains(m.Id)).ToListAsync();

            // 3. Somar tempos por máquina
            var machineTimes = allProcesses
                .Where(p => p.Machine != null && !string.IsNullOrWhiteSpace(p.Machine.Id))
                .GroupBy(p => p.Machine.Id)
                .Select(g => new
                {
                    MachineId = g.Key,
                    TotalTime = g.Sum(p =>
                    {
                        return TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out var ts) ? ts.TotalSeconds : 0;

                    })
                })
                .ToDictionary(x => x.MachineId, x => TimeSpan.FromSeconds(x.TotalTime));

            // 4. Agrupar por tipo de máquina e calcular médias
            var groupedByType = machines
                .Where(m => m.MachineType != null && !string.IsNullOrWhiteSpace(m.MachineType.Id))
                .GroupBy(m => m.MachineType.Id)
                .Select(g =>
                {
                    var totalTime = TimeSpan.Zero;
                    int count = 0;

                    foreach (var machine in g)
                    {
                        if (machineTimes.TryGetValue(machine.Id, out var time))
                        {
                            totalTime += time;
                            count++;
                        }
                    }

                    var avgTicks = count > 0 ? totalTime.Ticks / count : 0;
                    var average = TimeSpan.FromTicks(avgTicks);

                    return new MachineTypeAverageTimeDto
                    {
                        MachineTypeId = g.Key,
                        MachineTypeName = g.First().MachineType.Name ?? "Desconhecido",
                        AverageProcessTime = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)average.TotalHours, average.Minutes, average.Seconds),
                        MachineCount = count
                    };
                })
                .ToList();

            return groupedByType;
        }

        public async Task<Machine> CreateMachineAsync(Machine machine)
        {
            machine.Status = MachineStatus.WaitingProduction;

            if (string.IsNullOrEmpty(machine.Id))
                machine.Id = ObjectId.GenerateNewId().ToString();

            machine.CreatedAt = DateTime.UtcNow;
            await _machines.CreateAsync(machine);
            return machine;
        }

        public async Task<bool> UpdateMachineAsync(string id, Machine machine)
        {
            if (!ObjectId.TryParse(id, out _))
                return false;

            var existing = await _machines.GetByIdAsync(m => m.Id == id);
            if (existing == null)
                return false;

            var update = Builders<Machine>.Update
                .Set(m => m.CustomerName, machine.CustomerName ?? existing.CustomerName)
                .Set(m => m.IdentificationNumber, machine.IdentificationNumber ?? existing.IdentificationNumber)
                .Set(m => m.Phase, machine.Phase ?? existing.Phase)
                .Set(m => m.Voltage, machine.Voltage ?? existing.Voltage)
                .Set(m => m.Process, machine.Process ?? existing.Process)
                .Set(m => m.Quality, machine.Quality ?? existing.Quality)
                .Set(m => m.Monitoring, machine.Monitoring ?? existing.Monitoring)
                .Set(m => m.MachineType, machine.MachineType ?? existing.MachineType)
                .Set(m => m.Status, existing.Status);

            var collection = _machines.GetCollection();
            var result = await collection.UpdateOneAsync(m => m.Id == id, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            return await _machines.DeleteAsync(m => m.Id == id);
        }

        public async Task<List<Machine>> SearchMachinesAsync(MachineFilter filter)
        {
            var builder = Builders<Machine>.Filter;
            var filters = new List<FilterDefinition<Machine>>();

            if (!string.IsNullOrWhiteSpace(filter.IdentificationNumber))
            {
                var pattern = Regex.Escape(filter.IdentificationNumber.Trim()); 
                filters.Add(builder.Regex(m => m.IdentificationNumber, new BsonRegularExpression(pattern, "i")));
            }
            if (filter.Status.HasValue)
                filters.Add(builder.Eq(m => m.Status, filter.Status.Value));


            if (!string.IsNullOrWhiteSpace(filter.MachineTypeId))
                filters.Add(builder.Eq(m => m.MachineType.Id, filter.MachineTypeId.Trim()));

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                filters.Add(builder.ElemMatch(m => m.Users, u => u.Id == filter.UserId.Trim()));
                

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            int page = filter.Page <= 0 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var collection = _machines.GetCollection();
            var projection = Builders<Machine>.Projection
                .Include(m => m.Id)
                .Include(m => m.CustomerName)
                .Include(m => m.IdentificationNumber)
                .Include(m => m.MachineType)
                .Include(m => m.Process)
                .Include(m => m.Status)
                .Include(m => m.Voltage)
                .Include(m => m.Phase)
                .Include(m => m.Quality)
                .Include(m => m.Monitoring)
                .Include(m => m.CreatedAt)
                .Include(m => m.Users);

            var findOptions = new FindOptions<Machine>
            {
                Projection = projection,
                Sort = Builders<Machine>.Sort.Ascending(m => m.CustomerName),
                Skip = skip,
                Limit = pageSize
            };

            var cursor = await collection.FindAsync(finalFilter, findOptions);
            var machines = await cursor.ToListAsync();

            return machines;
        }
    }
}
