using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Bson;
using ColdlineAPI.Infrastructure.Utils;

namespace ColdlineAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<UserType> _userTypes;
        private readonly IMongoCollection<Department> _departments;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(IOptions<MongoDBSettings> mongoDBSettings, IConfiguration configuration, IEmailService emailService)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);

            _users = database.GetCollection<User>("Users");
            _userTypes = database.GetCollection<UserType>("UserTypes");
            _departments = database.GetCollection<Department>("Departments");
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                return false; // Usuário não encontrado
            }

            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            string subject = "Recuperação de Senha";
            string body = $"Olá {user.Name},<br/><br/> Sua senha é: <b>{decryptedPassword}</b><br/><br/>" +
                          "Caso não tenha solicitado, recomendamos que altere sua senha.";

            await _emailService.SendEmailAsync(email, subject, body);
            return true;
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _users.Find(user => true).ToListAsync();

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<List<User>> SearchUsersAsync(string? name, string? email, string? departmentId, string? userTypeId)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            if (!string.IsNullOrEmpty(name))
                filters.Add(filterBuilder.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"))); 

            if (!string.IsNullOrEmpty(email))
                filters.Add(filterBuilder.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(email, "i")));

            if (!string.IsNullOrEmpty(departmentId))
                filters.Add(filterBuilder.Eq(u => u.Department.Id, departmentId));

            if (!string.IsNullOrEmpty(userTypeId))
                filters.Add(filterBuilder.Eq(u => u.UserType.Id, userTypeId));

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;
            return await _users.Find(finalFilter).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id) || !ObjectId.TryParse(user.Id, out _))
            {
                user.Id = ObjectId.GenerateNewId().ToString();
            }

            if (!UtilityHelper.IsValidEmail(user.Email))
            {
                throw new ArgumentException("O email fornecido não é válido.");
            }

            var emailExists = await _users.Find(u => u.Email == user.Email).AnyAsync();
            if (emailExists)
            {
                throw new ArgumentException("O email fornecido já está em uso.");
            }

            user.Password = UtilityHelper.Encrypt(user.Password);
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(string id, User user)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return (false, "O ID fornecido não é um ObjectId válido.");
                }

                var existingUser = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    return (false, "Usuário não encontrado.");
                }

                // 🔹 Se a senha estiver preenchida, criptografá-la antes de salvar.
                var updatedPassword = string.IsNullOrEmpty(user.Password) ? existingUser.Password : UtilityHelper.Encrypt(user.Password);

                var updateDefinition = Builders<User>.Update
                    .Set(u => u.Name, user.Name ?? existingUser.Name)
                    .Set(u => u.Email, user.Email ?? existingUser.Email)
                    .Set(u => u.Password, updatedPassword)
                    .Set(u => u.UserType, user.UserType ?? existingUser.UserType)
                    .Set(u => u.Department, user.Department ?? existingUser.Department)
                    .Set(u => u.CurrentProcess, user.CurrentProcess)
                    .Set(u => u.CurrentOccurrence, user.CurrentOccurrence)
                    .Set(u => u.UrlPhoto, user.UrlPhoto ?? existingUser.UrlPhoto)
                    .Set(u => u.IdentificationNumber, user.IdentificationNumber ?? existingUser.IdentificationNumber);

                var result = await _users.UpdateOneAsync(u => u.Id == id, updateDefinition);

                return (true, "Usuário atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar usuário: {ex.Message}");
            }
        }


        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _users.DeleteOneAsync(user => user.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            if (password != decryptedPassword)
            {
                return null;
            }

            return user;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return false; // Usuário não encontrado
            }

            // 🔹 Descriptografar e verificar se a senha antiga corresponde
            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            if (decryptedPassword != oldPassword)
            {
                return false; // Senha antiga não confere
            }

            // 🔹 Criptografar a nova senha e atualizar no banco
            string encryptedNewPassword = UtilityHelper.Encrypt(newPassword);
            var update = Builders<User>.Update.Set(u => u.Password, encryptedNewPassword);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            return true; // Senha alterada com sucesso
        }

        public async Task<User?> GetUserByIdentificationNumberAsync(string identificationNumber)
        {
            return await _users.Find(user => user.IdentificationNumber == identificationNumber).FirstOrDefaultAsync();
        }



        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserType", user.UserType.Name),
                new Claim("Department", user.Department.Name)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
