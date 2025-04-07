using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Utils;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Infrastructure.Settings;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MongoDB.Bson;
using MongoDB.Driver;

using ClosedXML.Excel;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Elements;
using QuestPDF.Infrastructure;

using SkiaSharp;

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

        public async Task<(List<User> Items, long TotalCount)> SearchUsersAsync(
            string? name,
            string? email,
            string? departmentId,
            string? userTypeId,
            int pageNumber,
            int pageSize)
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

            var finalFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Consulta sem paginação para contar o total de documentos.
            // Você pode também usar CountDocumentsAsync, dependendo da versão do driver.
            var totalCount = await _users.CountDocumentsAsync(finalFilter);

            // Aplicando paginação (Skip e Limit):
            // Se pageNumber for 1 e pageSize for 10, por exemplo, skip = 0, limit = 10
            // Se pageNumber for 2, skip = 10, limit = 10, e assim por diante.
            var items = await _users
                .Find(finalFilter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, totalCount);
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

        public Task<byte[]> GenerateExcelWithSampleDataAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pessoas");

            worksheet.Cell(1, 1).Value = "Nome";
            worksheet.Cell(1, 2).Value = "Idade";
            worksheet.Cell(1, 3).Value = "Altura (m)";

            var data = new[]
            {
                new { Nome = "Ana", Idade = 28, Altura = 1.65 },
                new { Nome = "Bruno", Idade = 34, Altura = 1.80 },
                new { Nome = "Carlos", Idade = 22, Altura = 1.75 },
                new { Nome = "Daniela", Idade = 30, Altura = 1.70 }
            };

            int row = 2;
            foreach (var pessoa in data)
            {
                worksheet.Cell(row, 1).Value = pessoa.Nome;
                worksheet.Cell(row, 2).Value = pessoa.Idade;
                worksheet.Cell(row, 3).Value = pessoa.Altura;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return Task.FromResult(stream.ToArray()); // ✅ CORRETO
        }

        public Task<byte[]> GeneratePdfWithAgeChartAsync()
        {
            var data = new[]
            {
                new { Nome = "eduardo", Idade = 18.0 },
                new { Nome = "amanda", Idade = 19.0 },
                new { Nome = "caio", Idade = 20.0 }
            };

            double max = 20.5;
            double min = 17.0;
            double step = 0.5;
            int steps = (int)((max - min) / step);

            QuestPDF.Settings.License = LicenseType.Community;

            using var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().AlignCenter().Text("idade").Bold().FontSize(18);

                    page.Content().PaddingTop(30).Row(mainRow =>
                    {
                        // 🔹 Números da lateral (eixo Y)
                        mainRow.ConstantColumn(50).Column(yCol =>
                        {
                            for (double i = max; i >= min; i -= step)
                            {
                                yCol.Item().Height(20).AlignRight().AlignMiddle().Text(i.ToString("0.0")).FontSize(10);
                            }
                        });

                        // 🔹 Área do gráfico com borda
                        mainRow.RelativeColumn().Border(1).BorderColor(Colors.Red.Medium).PaddingLeft(10).Column(area =>
                        {
                            // 🔸 Linhas horizontais
                            area.Item().Column(lineCol =>
                            {
                                for (int i = 0; i <= steps; i++)
                                {
                                    lineCol.Item().Height(20).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                }
                            });

                            // 🔸 Barras verticais (com espaçamento entre elas)
                            area.Item().PaddingTop(-20 * steps).Row(barRow =>
                            {
                                foreach (var pessoa in data)
                                {
                                    int barSteps = (int)((pessoa.Idade - min) / step);
                                    int barHeight = barSteps * 20;

                                    barRow.RelativeColumn().Column(col =>
                                    {
                                        col.Item().Height(20 * (steps - barSteps));
                                        col.Item().Height(barHeight).Background(Colors.Blue.Medium);
                                    });

                                    // Adiciona uma coluna vazia entre barras (espaço)
                                    barRow.RelativeColumn().Column(_ => { });
                                }
                            });

                            // 🔸 Nomes abaixo do gráfico
                            area.Item().Row(labelRow =>
                            {
                                foreach (var pessoa in data)
                                {
                                    labelRow.RelativeColumn().AlignCenter().Text(pessoa.Nome).FontSize(10);
                                    labelRow.RelativeColumn(); // Espaço entre nomes
                                }
                            });
                        });
                    });
                });
            }).GeneratePdf(stream);

            return Task.FromResult(stream.ToArray());
        }
    }

}
