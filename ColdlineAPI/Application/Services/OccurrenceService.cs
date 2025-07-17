using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
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

        public OccurrenceService(RepositoryFactory factory)
        {
            _occurrences = factory.CreateRepository<Occurrence>("Occurrences");
            _processes = factory.CreateRepository<Process>("Processes");
            _users = factory.CreateRepository<User>("Users");
        }

        public async Task<List<Occurrence>> GetAllOccurrencesAsync() =>
            await _occurrences.GetAllAsync();

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
                .Set(o => o.PauseType, occurrence.PauseType)
                .Set(o => o.Defect, occurrence.Defect)
                .Set(o => o.Finished, occurrence.Finished)
                .Set(o => o.User, occurrence.User);

            var collection = _occurrences.GetCollection();
            var result = await collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id) =>
            await _occurrences.DeleteAsync(o => o.Id == id);

        public async Task<Occurrence> StartOccurrenceAsync(StartOccurrenceRequest request)
        {
            if (request.PauseType == null)
                throw new ArgumentException("PauseType é obrigatório.");
            if (request.User?.Id == null)
                throw new ArgumentException("Usuário inválido.");
            if (request.Process?.Id == null)
                throw new ArgumentException("Processo inválido.");

            var user = await _users.GetByIdAsync(u => u.Id == request.User.Id)
                ?? throw new ArgumentException("Usuário não encontrado.");
            var process = await _processes.GetByIdAsync(p => p.Id == request.Process.Id)
                ?? throw new ArgumentException("Processo não encontrado.");

            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
            var newOccurrence = new Occurrence
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CodeOccurrence = GenerateNumericCode(),
                ProcessTime = "00:00:00",
                StartDate = now,
                EndDate = null,
                Process = new ReferenceEntity(process.Id, process.IdentificationNumber),
                PauseType = request.PauseType,
                Defect = request.Defect,
                Finished = false,
                User = new ReferenceEntity(user.Id, user.Name)
            };

            await _occurrences.CreateAsync(newOccurrence);

            var processUpdate = Builders<Process>.Update.Push(p => p.Occurrences, new ReferenceEntity(newOccurrence.Id, newOccurrence.CodeOccurrence));
            await _processes.GetCollection().UpdateOneAsync(p => p.Id == process.Id, processUpdate);

            var userUpdate = Builders<User>.Update.Set(u => u.CurrentOccurrence, new ReferenceEntity(newOccurrence.Id, newOccurrence.CodeOccurrence));
            await _users.GetCollection().UpdateOneAsync(u => u.Id == user.Id, userUpdate);

            return newOccurrence;
        }

        public async Task<bool> EndOccurrenceAsync(string occurrenceId)
        {
            var occurrence = await _occurrences.GetByIdAsync(o => o.Id == occurrenceId)
                ?? throw new ArgumentException("Ocorrência não encontrada.");

            var user = await _users.GetByIdAsync(u => u.Id == occurrence.User.Id)
                ?? throw new ArgumentException("Usuário da ocorrência não encontrado.");

            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
            var updateOccurrence = Builders<Occurrence>.Update
                .Set(o => o.Finished, true)
                .Set(o => o.EndDate, now);

            var resultOccurrence = await _occurrences.GetCollection().UpdateOneAsync(o => o.Id == occurrenceId, updateOccurrence);
            var resultUser = await _users.GetCollection().UpdateOneAsync(u => u.Id == user.Id, Builders<User>.Update.Set(u => u.CurrentOccurrence, null));

            return resultOccurrence.IsAcknowledged && resultOccurrence.ModifiedCount > 0 &&
                   resultUser.IsAcknowledged && resultUser.ModifiedCount > 0;
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

        private static string GenerateNumericCode() => new Random().Next(100000, 999999).ToString();
    }
}
