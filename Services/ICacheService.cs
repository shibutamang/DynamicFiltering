namespace DistributedCache.Services
{
    public interface ICacheService
    { 
        /// <summary>
        /// Get Data list using key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        IList<T> GetData<T>(string key);

        /// <summary>
        /// Set Data with Value and Expiration Time of Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime"></param>
        /// <returns></returns>
        Task SetData<T>(string key, T value, DateTimeOffset expirationTime);

        /// <summary>
        /// Remove Data
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveData(string key);

        /// <summary>
        /// Flushed cache db
        /// </summary>
        void FlushDb();
    }
}
