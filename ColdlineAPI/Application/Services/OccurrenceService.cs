using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ColdlineAPI.Application.Common;       // PagedResult<T>
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories; // MongoRepository<T>

using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;

using MongoDB.Bson;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceService : IOccurrenceService
    {
        private readonly MongoRepository<Occurrence> _occurrences;
        private readonly MongoRepository<Process> _processes;
        private readonly MongoRepository<User> _users;
        private readonly MongoRepository<Machine> _machines;
        private readonly IProcessService _processService;

        public OccurrenceService(RepositoryFactory factory, IProcessService processService)
        {
            _processService = processService;
            _occurrences    = factory.CreateRepository<Occurrence>("Occurrences");
            _processes      = factory.CreateRepository<Process>("Processes");
            _users          = factory.CreateRepository<User>("Users");
            _machines       = factory.CreateRepository<Machine>("Machines");
        }

        public async Task<List<Occurrence>> GetAllOccurrencesAsync() =>
            await _occurrences.GetAllAsync();

        // Agora com PagedResult<Occurrence>
        public async Task<PagedResult<Occurrence>> SearchOccurrencesAsync(OccurrenceSearchFilter f)
        {
            var col     = _occurrences.GetCollection();
            var fb      = Builders<Occurrence>.Filter;
            var filters = new List<FilterDefinition<Occurrence>>();

            if (!string.IsNullOrWhiteSpace(f.UserId))
                filters.Add(fb.Eq(o => o.User.Id, f.UserId));

            if (!string.IsNullOrWhiteSpace(f.MachineID))
                filters.Add(fb.Eq(o => o.Machine.Id, f.MachineID));

            if (!string.IsNullOrWhiteSpace(f.DepartmentId))
                filters.Add(fb.Eq(o => o.Department.Id, f.DepartmentId));

            if (f.Finished.HasValue)
                filters.Add(fb.Eq(o => o.Finished, f.Finished.Value));

            if (!string.IsNullOrWhiteSpace(f.OccurrenceTypeId))
                filters.Add(fb.Eq(o => o.OccurrenceType!.Id, f.OccurrenceTypeId));

            if (f.StartDate.HasValue)
                filters.Add(fb.Gte(o => o.StartDate, f.StartDate.Value));

            if (f.EndDate.HasValue)
                filters.Add(fb.Lte(o => o.StartDate, f.EndDate.Value));

            var finalFilter = filters.Count > 0 ? fb.And(filters) : fb.Empty;

            // Paginação (1-based) com saneamento
            var Page     = (f.Page.Value <= 0) ? 1 : f.Page.Value;
            var PageSize = (f.PageSize.Value <= 0) ? 20 : Math.Min(f.PageSize.Value, 200);
            var skip     = (Page - 1) * PageSize;

            // Ordenação (default: StartDate desc)
            SortDefinition<Occurrence> Sort = Builders<Occurrence>.Sort.Descending(o => o.StartDate);
            if (!string.IsNullOrWhiteSpace(f.SortBy))
            {
                var by   = f.SortBy.Trim().ToLowerInvariant();
                var desc = f.SortDesc ?? false;

                Sort = by switch
                {
                    "startdate" => desc ? Builders<Occurrence>.Sort.Descending(o => o.StartDate)
                                        : Builders<Occurrence>.Sort.Ascending(o => o.StartDate),
                    "enddate"   => desc ? Builders<Occurrence>.Sort.Descending(o => o.EndDate)
                                        : Builders<Occurrence>.Sort.Ascending(o => o.EndDate),
                    "finished"  => desc ? Builders<Occurrence>.Sort.Descending(o => o.Finished)
                                        : Builders<Occurrence>.Sort.Ascending(o => o.Finished),
                    _ => Sort
                };
            }

            // Total antes de paginar
            var total = await col.CountDocumentsAsync(finalFilter);

            // Ajusta página se passou do fim
            var totalPages = PageSize > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 0;
            if (Page > totalPages && totalPages > 0)
            {
                Page = totalPages;
                skip = (Page - 1) * PageSize;
            }

            var items = await col.Find(finalFilter)
                                 .Sort(Sort)
                                 .Skip(skip)
                                 .Limit(PageSize)
                                 .ToListAsync();

            // Recalcula/atualiza ProcessTime conforme regra atual
            foreach (var o in items)
            {
                var duration  = await CalculateOcurrenceTime(o.StartDate, o.EndDate);
                var formatted = duration.ToString(@"hh\:mm\:ss");

                if (!string.Equals(o.ProcessTime, formatted, StringComparison.Ordinal))
                {
                    o.ProcessTime = formatted;
                    await _occurrences.UpdateAsync(x => x.Id == o.Id, o);
                }
            }

            return new PagedResult<Occurrence>
            {
                Items    = items,
                Total    = total,
                Page     = Page,
                PageSize = PageSize
            };
        }

        public async Task<Occurrence?> GetOccurrenceByIdAsync(string id)
        {
            var occurrence = await _occurrences.GetByIdAsync(o => o.Id == id);
            if (occurrence != null)
            {
                var updatedTime = await CalculateOcurrenceTime(occurrence.StartDate, occurrence.EndDate);
                occurrence.ProcessTime = updatedTime.ToString(@"hh\:mm\:ss");
                await _occurrences.UpdateAsync(p => p.Id == id, occurrence);
            }
            return occurrence;
        }

        public async Task<List<OccurrenceDateChartDto>> GetOccurrenceCountByDateAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Occurrence>.Filter.Gte("start date", start) &
                         Builders<Occurrence>.Filter.Lte("start date", end);

            var result = await _occurrences.GetCollection()
                .Aggregate()
                .Match(filter)
                .Group(new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", "%Y-%m-%d" },
                            { "date", "$start date" }
                        })
                    },
                    { "count", new BsonDocument("$sum", 1) }
                })
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();

            return result.Select(doc => new OccurrenceDateChartDto
            {
                Date     = doc["_id"].AsString,
                Quantity = doc["count"].AsInt32
            }).ToList();
        }

        public async Task<List<OccurrenceUserChartDto>> GetOccurrenceCountByUserAsync(DateTime start, DateTime end)
        {
            var filter = Builders<Occurrence>.Filter.Gte("start date", start) &
                         Builders<Occurrence>.Filter.Lte("start date", end);

            var result = await _occurrences.GetCollection()
                .Find(filter)
                .ToListAsync();

            var agrupado = result
                .Where(o => o.User != null && !string.IsNullOrEmpty(o.User.Name))
                .GroupBy(o => o.User!.Name)
                .Select(g => new OccurrenceUserChartDto
                {
                    UserName = g.Key,
                    Quantity = g.Count()
                })
                .ToList();

            return agrupado;
        }

        public async Task<Occurrence> CreateOccurrenceAsync(Occurrence occurrence)
        {
            occurrence.Id ??= ObjectId.GenerateNewId().ToString();
            occurrence.CodeOccurrence = GenerateNumericCode();
            await _occurrences.CreateAsync(occurrence);
            return occurrence;
        }

        public async Task<bool> UpdateOccurrenceAsync(string id, Occurrence occurrence)
        {
            var filter = Builders<Occurrence>.Filter.Eq(o => o.Id, id);
            var update = Builders<Occurrence>.Update
                .Set(o => o.CodeOccurrence,  occurrence.CodeOccurrence)
                .Set(o => o.ProcessTime,     occurrence.ProcessTime)
                .Set(o => o.StartDate,       occurrence.StartDate)
                .Set(o => o.EndDate,         occurrence.EndDate)
                .Set(o => o.Process,         occurrence.Process)
                .Set(o => o.Finished,        occurrence.Finished)
                .Set(o => o.User,            occurrence.User)
                .Set(o => o.Description,     occurrence.Description)
                .Set(o => o.Department,     occurrence.Department)
                .Set(o => o.OccurrenceType,  occurrence.OccurrenceType)
                .Set(o => o.Part,            occurrence.Part)
                .Set(o => o.Machine,         occurrence.Machine);

            var result = await _occurrences.GetCollection()
                .UpdateOneAsync(filter, update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id) =>
            await _occurrences.DeleteAsync(o => o.Id == id);

        public async Task<bool> FinalizeOccurrenceAsync(string id)
        {
            var occurrence = await _occurrences.GetByIdAsync(o => o.Id == id);
            if (occurrence is null)
                return false;

            var now = GetCurrentCampoGrandeTime();

            var updates = new List<UpdateDefinition<Occurrence>>
            {
                Builders<Occurrence>.Update.Set(o => o.EndDate, now),
                Builders<Occurrence>.Update.Set(o => o.Finished, true)
            };

            if (occurrence.OccurrenceType != null)
            {
                updates.Add(Builders<Occurrence>.Update.Set(o => o.OccurrenceType!.PendingEvent, false));
            }

            var update = Builders<Occurrence>.Update.Combine(updates);
            var result = await _occurrences.GetCollection()
                .UpdateOneAsync(o => o.Id == id, update);

            if (!string.IsNullOrWhiteSpace(occurrence.Process?.Id))
            {
                var proc = await _processes.GetByIdAsync(p => p.Id == occurrence.Process.Id);
                var machineId = proc?.Machine?.Id;

                if (!string.IsNullOrWhiteSpace(machineId))
                {
                    await _machines.GetCollection().UpdateOneAsync(
                        m => m.Id == machineId,
                        Builders<Machine>.Update.Set(m => m.Status, MachineStatus.InProgress)
                    );
                }
            }

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        private DateTime GetCurrentCampoGrandeTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
        }

        private async Task<TimeSpan> CalculateOcurrenceTime(DateTime start, DateTime? end)
        {
            DateTime endTime = end ?? GetCurrentCampoGrandeTime();

            TimeSpan total = TimeSpan.Zero;

            // Períodos úteis por dia
            var workPeriods = new List<(TimeSpan start, TimeSpan end)>
            {
                (TimeSpan.FromHours(7.5),  TimeSpan.FromHours(11.5)), // 07:30–11:30
                (TimeSpan.FromHours(13.25),TimeSpan.FromHours(15.0)), // 13:15–15:00
                (TimeSpan.FromHours(15.25),TimeSpan.FromHours(17.5))  // 15:15–17:30
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

        private static string GenerateNumericCode() =>
            new Random().Next(100000, 999999).ToString();
    }
}
