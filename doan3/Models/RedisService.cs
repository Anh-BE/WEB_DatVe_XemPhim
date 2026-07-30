using System;
using System.Configuration;
using StackExchange.Redis;

namespace doan3.Models
{
    public class RedisService
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisService()
        {
            string connectionString = ConfigurationManager.AppSettings["RedisConnectionString"] ?? "localhost:6379,abortConnect=false";
            LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        /// <summary>
        /// Lấy đối tượng kết nối tới Database Redis (mặc định dbId = 0)
        /// </summary>
        public static IDatabase GetDatabase(int dbId = 0)
        {
            return Connection.GetDatabase(dbId);
        }
    }
}
