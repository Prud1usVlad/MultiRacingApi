using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MultiRacingApi.Models;
using MultiRacingApi.Settings;

namespace MultiRacingApi.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _users;

        public UsersService(IOptions<DbSettings> dbSettings)
        {
            var c = new MongoClient(dbSettings.Value.ConnectionString);

            _users = c.GetDatabase(dbSettings.Value.DbName)
                .GetCollection<User>(dbSettings.Value.UsersCollection);
        }

        public async Task<List<User>> GetAll()
            => await _users.Find(_ => true).ToListAsync();

        public async Task<User> GetById(string id)
            => await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetByName(string name)
            => await _users.Find(u => u.Name == name).FirstOrDefaultAsync();

        public async Task Add(User user)
            => await _users.InsertOneAsync(user);

        public async Task Update(string id, User user)
            => await _users.ReplaceOneAsync(u => u.Id == id, user);

        public async Task AddResult(float res, string id)
        {
            var user = await GetById(id);
            
            if (user.Results.Count == 10)
            {
                if (user.Results.Last() > res)
                    user.Results[9] = res;
            }
            else
            {
                user.Results.Add(res);
            }

            user.Results.OrderByDescending(i => i);
            await Update(id, user);
        }

        public async Task Delete(string id)
            => await _users.DeleteOneAsync(u => u.Id == id);
    }
}
