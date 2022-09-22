using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DistributedCache.Services
{
    public class CacheService : ICacheService
    { 
        private IDistributedCache _distributedCache;
        private IConfiguration _config;
        static ConnectionMultiplexer redis;
        public CacheService(IDistributedCache distributedCache, IConfiguration config)
        {
            _distributedCache = distributedCache;
            _config = config;
        } 

        public T GetData<T>(string key)
        {
            var value = _distributedCache.GetString(key);

            //var value = _db.StringGet(key);

            if (!string.IsNullOrEmpty(value))
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default;
        }
        public async Task SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);

            var option = new DistributedCacheEntryOptions 
            {
                AbsoluteExpiration = expirationTime
            }; 

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(value), option); 
        }
        public async Task RemoveData(string key)
        {
            var _isKeyExist = _distributedCache.GetString(key);
            
            if (_isKeyExist == null)
            {
                await _distributedCache.RemoveAsync(key);
            } 
        }
         
        public void FlushDb()
        {
            var options = ConfigurationOptions.Parse($"{_config.GetValue<string>("Redis:Server")}:{_config.GetValue<int>("Redis:Port")}");
            options.ConnectRetry = 5;
            options.AllowAdmin = true;

            var _redis = ConnectionMultiplexer.Connect(options);
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());
             
            server.FlushDatabase();
        }
    }
}   
