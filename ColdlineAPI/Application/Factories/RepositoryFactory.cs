using ColdlineAPI.Application.Repositories;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ColdlineAPI.Application.Factories
{
    public class RepositoryFactory
    {
        private readonly IMongoClient _client;
        private readonly IOptions<MongoDBSettings> _settings;

        public RepositoryFactory(IMongoClient client, IOptions<MongoDBSettings> settings)
        {
            _client = client;
            _settings = settings;
        }

        public MongoRepository<T> CreateRepository<T>(string collectionName)
        {
            return new MongoRepository<T>(_client, _settings, collectionName);
        }
    }
}
