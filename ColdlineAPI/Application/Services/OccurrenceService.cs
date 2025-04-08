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
                string updatedTime = CalculateOccurrenceTime(occurrence.StartDate);
                await UpdateOccurrenceTimeInDatabase(occurrence.Id, updatedTime);
                occurrence.ProcessTime = updatedTime;
            }
            return occurrence;
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

        public async Task<bool> UpdateOccurrenceTimeInDatabase(string occurrenceId, string processTime)
        {
            var update = Builders<Occurrence>.Update.Set(o => o.ProcessTime, processTime);
            var result = await _occurrences.GetCollection().UpdateOneAsync(o => o.Id == occurrenceId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        private string CalculateOccurrenceTime(DateTime startDate)
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));
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

        private static string GenerateNumericCode() => new Random().Next(100000, 999999).ToString();
    }
}
