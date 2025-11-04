using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Common;
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
        private readonly MongoRepository<User> _users;
        private readonly MongoRepository<Occurrence> _occurrences;
        private readonly MongoRepository<ProcessType> _processTypes;
        private readonly MongoRepository<Machine> _machines;

        private const string SpecialProcessTypeId = "67f6b00f101359f06e303492";

        public ProcessService(RepositoryFactory factory)
        {
            _processes    = factory.CreateRepository<Process>("Processes");
            _users        = factory.CreateRepository<User>("Users");
            _processTypes = factory.CreateRepository<ProcessType>("ProcessTypes");
            _machines     = factory.CreateRepository<Machine>("Machines");
            _occurrences  = factory.CreateRepository<Occurrence>("Occurrences");
        }

        public async Task<Process?> StartProcessAsync(
            string identificationNumber,
            string processTypeId,
            string? machineId,
            bool preIndustrialization,
            bool reWork,
            bool prototype)
        {
            // Busca o usuário pelo número de identificação
            var user = await _users.GetCollection()
                .Find(u => u.IdentificationNumber == identificationNumber)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("Usuário não encontrado.");

            // Busca o tipo de processo
            var processType = await _processTypes.GetCollection()
                .Find(pt => pt.Id == processTypeId)
                .FirstOrDefaultAsync();

            if (processType == null)
                throw new ArgumentException("Tipo de Processo não encontrado.");

            // Referência de máquina (se informada)
            ReferenceEntity? machineReference = null;
            if (!string.IsNullOrWhiteSpace(machineId))
            {
                var machine = await _machines.GetCollection()
                    .Find(m => m.Id == machineId)
                    .FirstOrDefaultAsync();

                if (machine == null)
                    throw new ArgumentException("Máquina não encontrada.");

                machineReference = new ReferenceEntity
                {
                    Id = machine.Id,
                    Name = machine.IdentificationNumber
                };
            }

            // Hora atual ajustada para fuso horário de Campo Grande
            var now = GetCurrentCampoGrandeTime();

            // Verifica se o usuário possui departamento
            ReferenceEntity? departmentReference = null;
            if (user.Department != null)
            {
                departmentReference = new ReferenceEntity
                {
                    Id = user.Department.Id,
                    Name = user.Department.Name
                };
            }

            // Criação do novo processo
            var newProcess = new Process
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IdentificationNumber = GenerateNumericCode(), // ou use "identificationNumber" se for o correto
                ProcessTime = "00:00:00",
                StartDate = now,
                EndDate = null,
                User = new ReferenceEntity
                {
                    Id = user.Id,
                    Name = user.Name
                },
                Department = departmentReference,
                ProcessType = new ReferenceEntity
                {
                    Id = processType.Id,
                    Name = processType.Name
                },
                Machine = machineReference,
                InOccurrence = false,
                ReWork = reWork,
                PreIndustrialization = preIndustrialization,
                Prototype = prototype,
                Finished = false
            };

            // Salva o processo no banco
            await _processes.CreateAsync(newProcess);

            // Atualiza o usuário com referência ao processo atual
            await _users.GetCollection().UpdateOneAsync(
                Builders<User>.Filter.Eq(u => u.Id, user.Id),
                Builders<User>.Update.Set(
                    u => u.CurrentProcess,
                    new ReferenceEntity
                    {
                        Id = newProcess.Id,
                        Name = newProcess.IdentificationNumber
                    }
                )
            );

            // Atualiza a máquina, caso vinculada
            if (machineReference != null)
            {
                await _machines.GetCollection().UpdateOneAsync(
                    Builders<Machine>.Filter.Eq(m => m.Id, machineReference.Id),
                    Builders<Machine>.Update
                        .Set(m => m.Status, MachineStatus.InProgress)
                        .Set(
                            m => m.Process,
                            new ReferenceEntity
                            {
                                Id = newProcess.Id,
                                Name = newProcess.IdentificationNumber
                            }
                        )
                );
            }

            return newProcess;
        }


        public async Task<bool> EndProcessAsync(string processId, bool Finished, StartOccurrenceRequest requestOccurrence)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == processId);
            if (process == null) throw new ArgumentException("Processo não encontrado.");

            var user = await _users.GetCollection().Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();
            if (user == null) throw new ArgumentException("Usuário do processo não encontrado.");

            var now = GetCurrentCampoGrandeTime();

            if (!Finished)
            {
                DateTime? enddateOcurrence = now;
                var finishedOccurrence = true;

                if (requestOccurrence.OccurrenceType.PendingEvent)
                {
                    enddateOcurrence = null;
                    finishedOccurrence = false;

                    if (process.Machine != null)
                    {
                        await _machines.GetCollection().UpdateOneAsync(
                            m => m.Id == process.Machine.Id,
                            Builders<Machine>.Update.Set(m => m.Status, MachineStatus.Stop)
                        );
                    }
                }

                var objDepartment = new ReferenceEntity
                {
                    Id = "67f41c323a596bf4e95bfe6d",
                    Name = "Industria"
                };

                var newOccurrence = new Occurrence
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    CodeOccurrence = GenerateNumericCode(),
                    ProcessTime = "00:00:00",
                    StartDate = now,
                    EndDate = enddateOcurrence,
                    Process = new ReferenceEntity(process.Id, process.IdentificationNumber),
                    OccurrenceType = requestOccurrence.OccurrenceType,
                    Department = objDepartment,
                    Finished = finishedOccurrence,
                    User = new ReferenceEntity(user.Id, user.Name),
                    Description = requestOccurrence.Description,
                    Part = requestOccurrence.Part,
                    Machine = process.Machine != null
                        ? new ReferenceEntity(process.Machine.Id, process.Machine.Name)
                        : null
                };

                await _occurrences.CreateAsync(newOccurrence);
            }

            var updateProcess = Builders<Process>.Update
                .Set(p => p.Finished, Finished)
                .Set(p => p.EndDate, now);

            var resultProcess = await _processes.GetCollection().UpdateOneAsync(p => p.Id == processId, updateProcess);
            var resultUser = await _users.GetCollection().UpdateOneAsync(u => u.Id == user.Id, Builders<User>.Update.Set(u => u.CurrentProcess, null));

            return resultProcess.IsAcknowledged && resultProcess.ModifiedCount > 0
                && resultUser.IsAcknowledged && resultUser.ModifiedCount > 0;
        }

        public async Task<List<Process>> GetAllProcessesAsync()
        {
            var processes = await _processes.GetAllAsync();
            foreach (var process in processes)
            {
                var duration = await CalculateProcessTime(process.StartDate, process.EndDate);
                process.ProcessTime = duration.ToString(@"hh\:mm\:ss");
                await _processes.UpdateAsync(p => p.Id == process.Id, process);
            }
            return processes;
        }

        public async Task<Process?> GetProcessByIdAsync(string id)
        {
            var process = await _processes.GetByIdAsync(p => p.Id == id);
            if (process == null) return null;

            var user = await _users.GetCollection().Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();

            if (user?.CurrentOccurrence == null || string.IsNullOrEmpty(user.CurrentOccurrence.Id))
            {
                var duration = await CalculateProcessTime(process.StartDate, process.EndDate);
                process.ProcessTime = duration.ToString(@"hh\:mm\:ss");
                await _processes.UpdateAsync(p => p.Id == id, process);
            }

            return process;
        }

        public async Task<PagedResult<Process>> SearchProcessAsync(ProcessFilter filter)
        {
            var filterBuilder = Builders<Process>.Filter;
            var filtersList = new List<FilterDefinition<Process>>();

            // Filtros baseados no ProcessFilter
            if (!string.IsNullOrEmpty(filter.IdentificationNumber))
            {
                filtersList.Add(filterBuilder.Eq(p => p.IdentificationNumber, filter.IdentificationNumber));
            }

            if (filter.StartDate.HasValue)
            {
                filtersList.Add(filterBuilder.Gte(p => p.StartDate, filter.StartDate.Value.Date));
            }

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                filtersList.Add(filterBuilder.Lte(p => p.StartDate, endOfDay));
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                filtersList.Add(filterBuilder.Eq(p => p.User.Id, filter.UserId));
            }

            if (!string.IsNullOrEmpty(filter.DepartmentId))
            {
                filtersList.Add(filterBuilder.Eq(p => p.Department.Id, filter.DepartmentId));
            }

            if (!string.IsNullOrEmpty(filter.ProcessTypeId))
            {
                filtersList.Add(filterBuilder.Eq(p => p.ProcessType.Id, filter.ProcessTypeId));
            }

            if (!string.IsNullOrEmpty(filter.MachineId))
            {
                filtersList.Add(filterBuilder.Eq(p => p.Machine.Id, filter.MachineId));
            }

            if (filter.Finished.HasValue)
            {
                filtersList.Add(filterBuilder.Eq(p => p.Finished, filter.Finished.Value));
            }

            if (filter.PreIndustrialization.HasValue)
            {
                filtersList.Add(filterBuilder.Eq(p => p.PreIndustrialization, filter.PreIndustrialization.Value));
            }

            if (filter.Prototype.HasValue)
            {
                filtersList.Add(filterBuilder.Eq(p => p.Prototype, filter.Prototype.Value));
            }

            // Filtro final
            var finalFilter = filtersList.Count > 0 ? filterBuilder.And(filtersList) : filterBuilder.Empty;

            // Paginação
            int currentPage = Math.Max(1, filter.Page ?? 1);
            int pageSize = Math.Clamp(filter.PageSize ?? 10, 1, 200);
            int skipCount = (currentPage - 1) * pageSize;


            var processCollection = _processes.GetCollection();

            // Contagem total
            long totalRecords = await processCollection.CountDocumentsAsync(finalFilter);
            int totalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0;
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
                skipCount = (currentPage - 1) * pageSize;
            }

            // Ordenação por data de início decrescente
            var sortDefinition = Builders<Process>.Sort.Descending(p => p.StartDate);

            // Recuperar registros
            var processItems = await processCollection
                .Find(finalFilter)
                .Sort(sortDefinition)
                .Skip(skipCount)
                .Limit(pageSize)
                .ToListAsync();

            // Atualizar ProcessTime para cada processo, se necessário
            foreach (var process in processItems)
            {
                TimeSpan calculatedDuration = await CalculateProcessTime(process.StartDate, process.EndDate);
                string formattedDuration = calculatedDuration.ToString(@"hh\:mm\:ss");

                if (!string.Equals(process.ProcessTime, formattedDuration, StringComparison.Ordinal))
                {
                    process.ProcessTime = formattedDuration;
                    await _processes.UpdateAsync(p => p.Id == process.Id, process);
                }
            }

            // Retornar resultado paginado
            return new PagedResult<Process>
            {
                Items = processItems,
                Total = totalRecords,
                Page = currentPage,
                PageSize = pageSize
            };
        }


        public async Task<List<ProcessByDateDto>> GetProcessCountByStartDateAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                         Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var result = await _processes.GetCollection()
                .Aggregate()
                .Match(filter)
                .Group(new BsonDocument {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument {
                        { "format", "%Y-%m-%d" },
                        { "date", "$StartDate" }
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
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                         Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var result = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            return result
                .Where(p => p.ProcessType != null && !string.IsNullOrEmpty(p.ProcessType.Name))
                .GroupBy(p => p.ProcessType!.Name)
                .Select(g => new ProcessTypeChartDto
                {
                    ProcessTypeName = g.Key,
                    Quantity = g.Count()
                })
                .ToList();
        }

        public async Task<List<ProcessUserChartDto>> GetProcessCountByUserAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                         Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var result = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            return result
                .Where(p => p.User != null && !string.IsNullOrEmpty(p.User.Name))
                .GroupBy(p => p.User!.Name)
                .Select(g => new ProcessUserChartDto
                {
                    UserName = g.Key,
                    Quantity = g.Count()
                })
                .ToList();
        }

        public async Task<List<UserTotalProcessTimeDto>> GetTotalProcessTimeByUserAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                         Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            return allProcesses
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
        }

        public async Task<List<ProcessTypeTotalTimeDto>> GetTotalProcessTimeByProcessTypeAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Process>.Filter.Gte(p => p.StartDate, start) &
                         Builders<Process>.Filter.Lte(p => p.StartDate, end);

            var allProcesses = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            return allProcesses
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
        }

        public async Task<List<IndividualUserProcessDto>> GetIndividualProcessTimesByUserAsync(
            string userId, DateTime startDate, DateTime endDate, bool? preIndustrialization = null)
        {
            var user = await _users.GetCollection().Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                throw new ArgumentException("Usuário não encontrado.");

            var builder = Builders<Process>.Filter;
            var filter = builder.Eq(p => p.User.Id, userId) &
                         builder.Gte(p => p.StartDate, startDate) &
                         builder.Lte(p => p.StartDate, endDate);

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

            if (process.ProcessType?.Id == SpecialProcessTypeId && process.Machine != null)
            {
                var machine = await _machines.GetByIdAsync(p => p.Id == process.Machine.Id);
                var processFilter = new ProcessFilter { MachineId = process.Machine.Id };
                var processListPaged = await SearchProcessAsync(processFilter);
                var startDate = await CalculateMachineTime(processListPaged.Items.ToList());
                var timeMachine = await CalculateProcessTime(startDate, process.EndDate);

                await _machines.GetCollection().UpdateOneAsync(
                    Builders<Machine>.Filter.Eq(m => m.Id, process.Machine.Id),
                    Builders<Machine>.Update
                        .Set(m => m.Status, MachineStatus.Finished)
                        .Set(m => m.Process, null)
                        .Set(m => m.Time, timeMachine.ToString(@"hh\:mm\:ss"))
                );
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
                .Set(p => p.Occurrences, process.Occurrences ?? existing.Occurrences)
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
            var user = await _users.GetCollection().Find(u => u.Id == userId).FirstOrDefaultAsync();
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
            var user = await _users.GetCollection().Find(u => u.Id == process.User.Id).FirstOrDefaultAsync();

            var filtered = await _processes.GetCollection()
                .Find(p => p.Finished == true
                           && p.ProcessType.Id == processTypeId
                           && !string.IsNullOrEmpty(p.ProcessTime))
                .ToListAsync();

            var durations = filtered
                .Where(p => TimeSpan.TryParseExact(p.ProcessTime, @"hh\:mm\:ss", null, out _))
                .Select(p => TimeSpan.Parse(p.ProcessTime))
                .ToList();

            var dto = new ProcessStatisticsDto
            {
                ProcessTime = process?.ProcessTime ?? "00:00:00"
            };

            if (user?.CurrentOccurrence != null)
            {
                var ocurrence = await _occurrences.GetCollection()
                    .Find(u => u.Id == user.CurrentOccurrence.Id)
                    .FirstOrDefaultAsync();
            }
            else
            {
                dto.OcorrenceTypeName = "Nenhuma";
            }

            if (durations.Count == 0)
            {
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
            dto.StandardDeviation  = TimeSpan.FromTicks((long)stdDevTicks).ToString(@"hh\:mm\:ss");
            dto.UpperLimit         = TimeSpan.FromTicks((long)upperLimitTicks).ToString(@"hh\:mm\:ss");
            dto.ProcessTypeName    = process?.ProcessType?.Name ?? "Não identificado";

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

            var workPeriods = new List<(TimeSpan start, TimeSpan end)>
            {
                (TimeSpan.FromHours(7.5),  TimeSpan.FromHours(11.5)),
                (TimeSpan.FromHours(13.25),TimeSpan.FromHours(15.0)),
                (TimeSpan.FromHours(15.25),TimeSpan.FromHours(17.5))
            };

            DateTime currentDate = start.Date;

            while (currentDate <= endTime.Date)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    foreach (var period in workPeriods)
                    {
                        DateTime periodStart = currentDate.Add(period.start);
                        DateTime periodEnd   = currentDate.Add(period.end);

                        DateTime effectiveStart = periodStart < start ? start : periodStart;
                        DateTime effectiveEnd   = periodEnd > endTime ? endTime : periodEnd;

                        if (effectiveEnd > effectiveStart)
                            total += (effectiveEnd - effectiveStart);
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

            return Utils.GenerateExcel(linhas);
        }

        private DateTime GetCurrentCampoGrandeTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
        }

        private static string GenerateNumericCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        public async Task<List<MonthlyWorkSummaryDto>> GetUserMonthlyWorkSummaryAsync(string userId, int year, int month)
        {
            var builder = Builders<Process>.Filter;
            var startMonth = new DateTime(year, month, 1);
            var endMonth = startMonth.AddMonths(1).AddDays(-1);

            var filter = builder.Eq(p => p.User.Id, userId) &
                        builder.Gte(p => p.StartDate, startMonth) &
                        builder.Lte(p => p.StartDate, endMonth);

            var processes = await _processes.GetCollection()
                .Find(filter)
                .ToListAsync();

            var result = new List<MonthlyWorkSummaryDto>();

            for (var day = startMonth; day <= endMonth; day = day.AddDays(1))
            {
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                TimeSpan total = TimeSpan.Zero;
                int processCount = 0;

                var workPeriods = new List<(TimeSpan start, TimeSpan end)>
                {
                    (TimeSpan.FromHours(7.5),  TimeSpan.FromHours(11.5)),   // 07:30–11:30
                    (TimeSpan.FromHours(13),   TimeSpan.FromHours(15)),     // 13:00–15:00
                    (TimeSpan.FromHours(15.25),TimeSpan.FromHours(17.5))    // 15:15–17:30
                };

                var dayProcesses = processes
                    .Where(p => p.StartDate.Date <= day && (p.EndDate?.Date ?? day) >= day)
                    .ToList();

                foreach (var proc in dayProcesses)
                {
                    DateTime processStart = proc.StartDate < day ? day : proc.StartDate;
                    DateTime processEnd = (proc.EndDate ?? day.AddDays(1).AddSeconds(-1));
                    if (processEnd.Date > day) processEnd = day.AddDays(1).AddSeconds(-1);

                    foreach (var (periodStart, periodEnd) in workPeriods)
                    {
                        var start = day.Add(periodStart);
                        var end = day.Add(periodEnd);

                        var effectiveStart = processStart > start ? processStart : start;
                        var effectiveEnd = processEnd < end ? processEnd : end;

                        if (effectiveEnd > effectiveStart)
                            total += (effectiveEnd - effectiveStart);
                    }

                    processCount++;
                }

                if (processCount > 0)
                {
                    result.Add(new MonthlyWorkSummaryDto
                    {
                        Day = day,
                        TotalHours = total.ToString(@"hh\:mm\:ss"),
                        ProcessCount = processCount
                    });
                }
            }

            return result;
        }


    }
}
