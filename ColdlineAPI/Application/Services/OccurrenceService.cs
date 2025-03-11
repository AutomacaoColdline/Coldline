using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Services
{
    public class OccurrenceService : IOccurrenceService
    {
        private readonly IMongoCollection<Occurrence> _occurrences;
        private readonly IMongoCollection<Process> _processes;
        private readonly IMongoCollection<User> _users;

        public OccurrenceService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _occurrences = database.GetCollection<Occurrence>("Occurrences");
            _processes = database.GetCollection<Process>("Processes");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<Occurrence>> GetAllOccurrencesAsync() =>
            await _occurrences.Find(Occurrence => true).ToListAsync();

        public async Task<Occurrence?> GetOccurrenceByIdAsync(string id)
        {
            var occurrence = await _occurrences.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (occurrence != null)
            {
                // 🔹 Calcula o tempo decorrido para a ocorrência
                string updatedTime = CalculateOccurrenceTime(occurrence.StartDate);

                // 🔹 Atualiza no banco de dados
                await UpdateOccurrenceTimeInDatabase(occurrence.Id, updatedTime);

                // 🔹 Atualiza o objeto retornado
                occurrence.ProcessTime = updatedTime;
            }
            return occurrence;
        }



        public async Task<Occurrence> CreateOccurrenceAsync(Occurrence Occurrence)
        {
            if (string.IsNullOrEmpty(Occurrence.Id) || !ObjectId.TryParse(Occurrence.Id, out _))
            {
                Occurrence.Id = ObjectId.GenerateNewId().ToString();
            }

            await _occurrences.InsertOneAsync(Occurrence);
            return Occurrence;
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

            var result = await _occurrences.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOccurrenceAsync(string id)
        {
            var result = await _occurrences.DeleteOneAsync(Occurrence => Occurrence.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<Occurrence> StartOccurrenceAsync(StartOccurrenceRequest request)
        {
            if ( request.PauseType == null)
            {
                throw new ArgumentException(" PauseType são obrigatórios.");
            }

            if (request.User == null || string.IsNullOrWhiteSpace(request.User.Id))
            {
                throw new ArgumentException($"ID do Usuário inválido: {request.User?.Id}");
            }

            // 🔹 Busca o usuário diretamente sem conversão para ObjectId
            var user = await _users.Find(u => u.Id == request.User.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ArgumentException("Usuário não encontrado.");
            }

            // 🔹 Validação do processo
            if (request.Process == null || string.IsNullOrWhiteSpace(request.Process.Id))
            {
                throw new ArgumentException("ID do Processo inválido.");
            }

            // 🔹 Busca o processo diretamente sem conversão para ObjectId
            var process = await _processes.Find(p => p.Id == request.Process.Id).FirstOrDefaultAsync();
            if (process == null)
            {
                throw new ArgumentException("Processo não encontrado.");
            }
            TimeZoneInfo campoGrandeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
            DateTime campoGrandeTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, campoGrandeTimeZone);

            // 🔹 Criando a nova ocorrência com os dados validados
            var newOccurrence = new Occurrence
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CodeOccurrence = GenerateNumericCode(),
                ProcessTime = "00:00:00",
                StartDate = campoGrandeTime,
                EndDate = null,
                Process = new ReferenceEntity { Id = process.Id, Name = process.IdentificationNumber },
                PauseType = request.PauseType,
                Defect = request.Defect,
                Finished = false,
                User = new ReferenceEntity { Id = user.Id, Name = user.Name }
            };

            await _occurrences.InsertOneAsync(newOccurrence);

            // 🔹 Atualiza o processo para adicionar a nova ocorrência
            var updateProcess = Builders<Process>.Update.Push(p => p.Occurrences, new ReferenceEntity { Id = newOccurrence.Id, Name = newOccurrence.CodeOccurrence });
            await _processes.UpdateOneAsync(p => p.Id == process.Id, updateProcess);

            // 🔹 Atualiza o usuário para associar a nova ocorrência
            var updateUser = Builders<User>.Update.Set(u => u.CurrentOccurrence, new ReferenceEntity { Id = newOccurrence.Id, Name = newOccurrence.CodeOccurrence });
            await _users.UpdateOneAsync(u => u.Id == user.Id, updateUser);

            return newOccurrence;
        }

        public async Task<bool> EndOccurrenceAsync(string occurrenceId)
        {
            var occurrence = await _occurrences.Find(o => o.Id == occurrenceId).FirstOrDefaultAsync();
            if (occurrence == null)
            {
                throw new ArgumentException("Ocorrência não encontrada.");
            }

            // 🔹 Busca o usuário associado à ocorrência
            var user = await _users.Find(u => u.Id == occurrence.User.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new ArgumentException("Usuário da ocorrência não encontrado.");
            }

            // 🔹 Obtém a data e hora no fuso correto
            TimeZoneInfo campoGrandeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
            DateTime campoGrandeTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, campoGrandeTimeZone);

            // 🔹 Atualiza a ocorrência para finalizada
            var updateOccurrence = Builders<Occurrence>.Update
                .Set(o => o.Finished, true)
                .Set(o => o.EndDate, campoGrandeTime); // Define a data de finalização

            var resultOccurrence = await _occurrences.UpdateOneAsync(o => o.Id == occurrenceId, updateOccurrence);

            // 🔹 Remove a ocorrência atual do usuário
            var updateUser = Builders<User>.Update.Set(u => u.CurrentOccurrence, null);
            var resultUser = await _users.UpdateOneAsync(u => u.Id == user.Id, updateUser);

            return resultOccurrence.IsAcknowledged && resultOccurrence.ModifiedCount > 0 &&
                resultUser.IsAcknowledged && resultUser.ModifiedCount > 0;
        }


        public async Task<bool> UpdateOccurrenceTimeInDatabase(string occurrenceId, string processTime)
        {
            var update = Builders<Occurrence>.Update.Set(o => o.ProcessTime, processTime);
            var result = await _occurrences.UpdateOneAsync(o => o.Id == occurrenceId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        private string CalculateOccurrenceTime(DateTime startDate)
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande"));

            // Se a ocorrência ainda não começou, retorna zero
            if (now < startDate)
            {
                return "00:00:00";
            }

            TimeSpan workStart = TimeSpan.FromHours(7.5);  // 07:30
            TimeSpan workEnd = TimeSpan.FromHours(17.5);   // 17:30

            TimeSpan totalTime = TimeSpan.Zero;
            DateTime currentDay = startDate.Date;

            while (currentDay <= now.Date)
            {
                DateTime start = currentDay == startDate.Date ? startDate : currentDay.Add(workStart);
                DateTime end = currentDay == now.Date ? now : currentDay.Add(workEnd);

                // Ajuste correto para respeitar o horário comercial
                if (start.TimeOfDay < workStart)
                {
                    start = currentDay.Add(workStart);
                }
                if (end.TimeOfDay > workEnd)
                {
                    end = currentDay.Add(workEnd);
                }

                // Somar tempo apenas dentro do horário comercial e se o início for antes do fim
                if (start < end && start.TimeOfDay >= workStart && start.TimeOfDay <= workEnd)
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
