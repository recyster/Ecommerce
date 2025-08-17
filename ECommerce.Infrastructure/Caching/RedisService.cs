using StackExchange.Redis;

namespace ECommerce.Infrastructure.Caching
{
    public interface IRedisService
    {
        void SetString(string key, string value, TimeSpan? expiry = null);
        string? GetString(string key);
        void Remove(string key);
    }

    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
        }

        public void SetString(string key, string value, TimeSpan? expiry = null)
        {
            var db = _redis.GetDatabase();
            db.StringSet(key, value, expiry);
        }

        public string? GetString(string key)
        {
            var db = _redis.GetDatabase();
            return db.StringGet(key);
        }

        public void Remove(string key)
        {
            var db = _redis.GetDatabase();
            db.KeyDelete(key);
        }
    }
}
