using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Enum;
using ColdlineAPI.Infrastructure.Settings;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceService : IOccurrenceService
    {
        private readonly MongoRepository<Occurrence> _occurrences;
        private readonly MongoRepository<Process> _processes;
        private readonly MongoRepository<User> _users;
        private readonly IMongoCollection<Machine> _machines;
        private readonly IProcessService _processService;

        public OccurrenceService(RepositoryFactory factory, IProcessService processService)
        {
            _processService = processService;
            _occurrences = factory.CreateRepository<Occurrence>("Occurrences");
            _processes = factory.CreateRepository<Process>("Processes");
            _users = factory.CreateRepository<User>("Users");
            _machines = factory.CreateRepository<Machine>("Machines").GetCollection();
        }

        public async Task<List<Occurrence>> GetAllOccurrencesAsync() =>
            await _occurrences.GetAllAsync();

        public async Task<List<Occurrence>> SearchOccurrencesAsync(OccurrenceSearchFilter f)
        {
            var col = _occurrences.GetCollection();
            var builder = Builders<Occurrence>.Filter;
            var filters = new List<FilterDefinition<Occurrence>>();

            // UserId => compara por user.id (ReferenceEntity)
            if (!string.IsNullOrWhiteSpace(f.UserId))
                filters.Add(builder.Eq(o => o.User.Id, f.UserId));
            
            if (!string.IsNullOrWhiteSpace(f.MachineID))
                filters.Add(builder.Eq(o => o.Machine.Id, f.MachineID));

            // Finished (bool?)
            if (f.Finished.HasValue)
                filters.Add(builder.Eq(o => o.Finished, f.Finished.Value));

            // OccurrenceTypeId => compara por occurrenceType.id (objeto)
            if (!string.IsNullOrWhiteSpace(f.OccurrenceTypeId))
                filters.Add(builder.Eq(o => o.OccurrenceType!.Id, f.OccurrenceTypeId));

            // Datas:
            // - só StartDate  => StartDate >= f.StartDate
            // - só EndDate    => StartDate <= f.EndDate
            // - ambos         => StartDate entre [StartDate, EndDate]
            if (f.StartDate.HasValue && f.EndDate.HasValue)
            {
                filters.Add(builder.Gte(o => o.StartDate, f.StartDate.Value));
                filters.Add(builder.Lte(o => o.StartDate, f.EndDate.Value));
            }
            else if (f.StartDate.HasValue)
            {
                filters.Add(builder.Gte(o => o.StartDate, f.StartDate.Value));
            }
            else if (f.EndDate.HasValue)
            {
                filters.Add(builder.Lte(o => o.StartDate, f.EndDate.Value));
            }

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            // Ordena por StartDate desc, ajuste se preferir
            var list = await col.Find(finalFilter)
                                .SortByDescending(o => o.StartDate)
                                .ToListAsync();

            return list;
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
                    {"_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            {"format", "%Y-%m-%d"},
                            {"date", "$start date"}
                        })},
                    {"count", new BsonDocument("$sum", 1)}
                })
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();

            return result.Select(doc => new OccurrenceDateChartDto
            {
                Date = doc["_id"].AsString,
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
            await _occurrences.CreateAsync(occurrence);
            return occurrence;
        }

        public async Task<bool> UpdateOccurrenceAsync(string id, Occurrence occurrence)
        {
            var filter = Builders<Occurrence>.Filter.Eq(o => o.Id, id);
            var update = Builders<Occurrence>.Update
                .Set(o => o.CodeOccurrence, occurrence.CodeOccurrence)
                .Set(o => o.ProcessTime, occurrence.ProcessTime)
                .Set(o => o.StartDate, occurrence.StartDate)
                .Set(o => o.EndDate, occurrence.EndDate)
                .Set(o => o.Process, occurrence.Process)
                .Set(o => o.Finished, occurrence.Finished)
                .Set(o => o.User, occurrence.User)
                .Set(o => o.Description, occurrence.Description)
                .Set(o => o.OccurrenceType, occurrence.OccurrenceType)
                .Set(o => o.Part, occurrence.Part)
                .Set(o => o.Machine, occurrence.Machine);

            var collection = _occurrences.GetCollection();
            var result = await collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id) =>
            await _occurrences.DeleteAsync(o => o.Id == id);

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
                    await _machines.UpdateOneAsync(
                        m => m.Id == machineId,
                        Builders<Machine>.Update.Set(m => m.Status, MachineStatus.InProgress)
                    );
                }
            }

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }



        private static string GenerateNumericCode() => new Random().Next(100000, 999999).ToString();
    }
}
