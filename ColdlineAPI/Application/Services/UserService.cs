using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Infrastructure.Utils;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Application.Factories;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using MongoDB.Bson;
using MongoDB.Driver;

using ClosedXML.Excel;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Elements;
using QuestPDF.Infrastructure;

namespace ColdlineAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly MongoRepository<User> _users;
        private readonly MongoRepository<UserType> _userTypes;
        private readonly MongoRepository<Department> _departments;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(RepositoryFactory factory, IConfiguration configuration, IEmailService emailService)
        {
            _users = factory.CreateRepository<User>("Users");
            _userTypes = factory.CreateRepository<UserType>("UserTypes");
            _departments = factory.CreateRepository<Department>("Departments");
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _users.GetByIdAsync(u => u.Email == email);
            if (user == null) return false;

            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            string subject = "Recuperação de Senha";
            string body = $"Olá {user.Name},<br/><br/> Sua senha é: <b>{decryptedPassword}</b><br/><br/>Caso não tenha solicitado, recomendamos que altere sua senha.";

            await _emailService.SendEmailAsync(email, subject, body);
            return true;
        }

        public async Task<List<User>> GetUsersAsync() => await _users.GetAllAsync();

        public async Task<User?> GetUserByIdAsync(string id) => await _users.GetByIdAsync(u => u.Id == id);

        public async Task<PagedResult<User>> SearchUsersAsync(UserFilter filter)
        {
            var fb = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            // name / email: contains (case-insensitive) com Regex.Escape
            if (!string.IsNullOrWhiteSpace(filter?.Name))
            {
                var pattern = Regex.Escape(filter.Name.Trim());
                filters.Add(fb.Regex(u => u.Name, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter?.Email))
            {
                var pattern = Regex.Escape(filter.Email.Trim());
                filters.Add(fb.Regex(u => u.Email, new BsonRegularExpression(pattern, "i")));
            }

            if (!string.IsNullOrWhiteSpace(filter?.DepartmentId))
                filters.Add(fb.Eq(u => u.Department!.Id, filter.DepartmentId));

            if (!string.IsNullOrWhiteSpace(filter?.UserTypeId))
                filters.Add(fb.Eq(u => u.UserType!.Id, filter.UserTypeId));

            var finalFilter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<User>.Empty;

            // paginação (1-based) com limites
            var Page     = Math.Max(1, filter?.Page ?? 1);
            var PageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 200);
            var skip     = (Page - 1) * PageSize;

            // ordenação
            SortDefinition<User> Sort = Builders<User>.Sort.Ascending(u => u.Name);
            if (!string.IsNullOrWhiteSpace(filter?.SortBy))
            {
                var by   = filter.SortBy.Trim().ToLowerInvariant();
                var desc = filter.SortDesc ?? false;

                Sort = by switch
                {
                    "name"       => desc ? Builders<User>.Sort.Descending(u => u.Name)
                                        : Builders<User>.Sort.Ascending(u => u.Name),
                    "email"      => desc ? Builders<User>.Sort.Descending(u => u.Email)
                                        : Builders<User>.Sort.Ascending(u => u.Email),
                    "department" => desc ? Builders<User>.Sort.Descending(u => u.Department!.Name)
                                        : Builders<User>.Sort.Ascending(u => u.Department!.Name),
                    "usertype"   => desc ? Builders<User>.Sort.Descending(u => u.UserType!.Name)
                                        : Builders<User>.Sort.Ascending(u => u.UserType!.Name),
                    _ => Sort
                };
            }

            // projeção com campos usados no front
            var projection = Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.Name)
                .Include(u => u.Email)
                .Include(u => u.UserType)
                .Include(u => u.Department)
                .Include(u => u.CurrentProcess)
                .Include(u => u.CurrentOccurrence)
                .Include(u => u.IdentificationNumber)
                .Include(u => u.UrlPhoto)
                .Include(u => u.WorkHourCost);

            var collection = _users.GetCollection();

            // total antes da paginação
            var total = await collection.CountDocumentsAsync(finalFilter);

            // ajusta página se passou do fim
            var totalPages = PageSize > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 0;
            if (Page > totalPages && totalPages > 0)
            {
                Page = totalPages;
                skip = (Page - 1) * PageSize;
            }

            // (Opcional) collation pt-BR; se seu driver não suportar, remova findOptions/Collation.
            var collation   = new Collation("pt", strength: CollationStrength.Secondary);
            var findOptions = new FindOptions { Collation = collation };

            var items = await collection
                .Find(finalFilter, findOptions)   // se der erro de compilação, troque para .Find(finalFilter)
                .Sort(Sort)
                .Skip(skip)
                .Limit(PageSize)
                .Project<User>(projection)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items    = items,
                Total    = total,
                Page     = Page,
                PageSize = PageSize
            };
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id) || !ObjectId.TryParse(user.Id, out _))
                user.Id = ObjectId.GenerateNewId().ToString();

            if (!UtilityHelper.IsValidEmail(user.Email))
                throw new ArgumentException("O email fornecido não é válido.");

            var existing = await _users.GetByIdAsync(u => u.Email == user.Email);
            if (existing != null)
                throw new ArgumentException("O email fornecido já está em uso.");

            user.Password = UtilityHelper.Encrypt(user.Password);
            await _users.CreateAsync(user);
            return user;
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(string id, User user)
        {
            if (!ObjectId.TryParse(id, out _)) return (false, "O ID fornecido não é um ObjectId válido.");

            var existingUser = await _users.GetByIdAsync(u => u.Id == id);
            if (existingUser == null) return (false, "Usuário não encontrado.");

            var updatedPassword = string.IsNullOrEmpty(user.Password) ? existingUser.Password : UtilityHelper.Encrypt(user.Password);

            var update = Builders<User>.Update
                .Set(u => u.Name, user.Name ?? existingUser.Name)
                .Set(u => u.Email, user.Email ?? existingUser.Email)
                .Set(u => u.Password, updatedPassword)
                .Set(u => u.UserType, user.UserType ?? existingUser.UserType)
                .Set(u => u.Department, user.Department ?? existingUser.Department)
                .Set(u => u.CurrentProcess, user.CurrentProcess)
                .Set(u => u.CurrentOccurrence, user.CurrentOccurrence)
                .Set(u => u.UrlPhoto, user.UrlPhoto ?? existingUser.UrlPhoto)
                .Set(u => u.IdentificationNumber, user.IdentificationNumber ?? existingUser.IdentificationNumber)
                .Set(u => u.WorkHourCost, user.WorkHourCost);

            var collection = _users.GetCollection();
            await collection.UpdateOneAsync(u => u.Id == id, update);

            return (true, "Usuário atualizado com sucesso.");
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _users.DeleteAsync(u => u.Id == id);
        }

        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            var user = await _users.GetByIdAsync(u => u.Email == email);
            if (user == null) return null;

            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            return password == decryptedPassword ? user : null;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _users.GetByIdAsync(u => u.Id == userId);
            if (user == null) return false;

            string decryptedPassword = UtilityHelper.Decrypt(user.Password);
            if (decryptedPassword != oldPassword) return false;

            string encryptedNewPassword = UtilityHelper.Encrypt(newPassword);
            var update = Builders<User>.Update.Set(u => u.Password, encryptedNewPassword);

            var collection = _users.GetCollection();
            await collection.UpdateOneAsync(u => u.Id == userId, update);

            return true;
        }

        public async Task<User?> GetUserByIdentificationNumberAsync(string identificationNumber)
        {
            return await _users.GetByIdAsync(user => user.IdentificationNumber == identificationNumber);
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

            return Task.FromResult(stream.ToArray());
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
                container.Page(Page =>
                {
                    Page.Size(PageSizes.A4);
                    Page.Margin(40);
                    Page.DefaultTextStyle(x => x.FontSize(12));

                    Page.Header().AlignCenter().Text("idade").Bold().FontSize(18);

                    Page.Content().PaddingTop(30).Row(mainRow =>
                    {
                        mainRow.ConstantColumn(50).Column(yCol =>
                        {
                            for (double i = max; i >= min; i -= step)
                            {
                                yCol.Item().Height(20).AlignRight().AlignMiddle().Text(i.ToString("0.0")).FontSize(10);
                            }
                        });

                        mainRow.RelativeColumn().Border(1).BorderColor(Colors.Red.Medium).PaddingLeft(10).Column(area =>
                        {
                            area.Item().Column(lineCol =>
                            {
                                for (int i = 0; i <= steps; i++)
                                {
                                    lineCol.Item().Height(20).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                }
                            });

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

                                    barRow.RelativeColumn().Column(_ => { });
                                }
                            });

                            area.Item().Row(labelRow =>
                            {
                                foreach (var pessoa in data)
                                {
                                    labelRow.RelativeColumn().AlignCenter().Text(pessoa.Nome).FontSize(10);
                                    labelRow.RelativeColumn();
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
