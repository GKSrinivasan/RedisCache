using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;
using Laserbeam.DataManager.Interfaces.Common;
using System.Configuration;
using Newtonsoft.Json;

namespace Laserbeam.DataManager.Common
{
    public class RedisCacheProvider : IRedisCacheProvider
    {
        #region Fields
        public string TenantName;
        public static int DatabaseNumber;    

        //private MemoryCache Cache;

        private static readonly Lazy<ConnectionMultiplexer> LazyRedisConnection;
        private static ConnectionMultiplexer RedisConnection;
        private static IDatabase RedisCache;
        #endregion

        #region Constructor
        static RedisCacheProvider()
        {
            if (LazyRedisConnection == null || !LazyRedisConnection.Value.IsConnected)
            {
                string cacheConnection = ConfigurationManager.AppSettings["CacheConnection"].ToString();
                LazyRedisConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(cacheConnection));
                RedisConnection = LazyRedisConnection.Value;
                DatabaseNumber =Convert.ToInt32(ConfigurationManager.AppSettings["DatabaseNumber"]);
                RedisCache = RedisConnection.GetDatabase(DatabaseNumber);
            }
        }

        public RedisCacheProvider(string tenantName)
        {
            TenantName = ("_" + (tenantName).Replace(" ", "__"));
        }
        #endregion

        #region Public Methods

        // Author         :   Boobalan Ranganathan		
        // Creation Date  :   07-Apr-2017
        // Ticket ID      :   
        /// <summary>
        /// Gets data from cache
        /// </summary>
        /// <typeparam name="T">The type of data object</typeparam>
        /// <param name="cacheName">Name of the cache</param>
        /// <returns>Return T type object from cache</returns>
        public T GetCache<T>(string cacheName)
        {
            cacheName += TenantName;
            var jsonValue = RedisCache.StringGet(cacheName);
            var cacheData = JsonConvert.DeserializeObject<T>(jsonValue);
            //var hashEntryList = RedisCache.HashGetAll(cacheName);
            //var instance = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
            //var cacheData = (T)ConvertFromHashEntryList(hashEntryList, instance);
            return cacheData;
        }

        // Author         :   Boobalan Ranganathan		
        // Creation Date  :   07-Apr-2017
        // Ticket ID      :   
        /// <summary>
        /// Sets data into cache
        /// </summary>
        /// <typeparam name="T">The type of data object</typeparam>
        /// <param name="cacheName">Name of the cache</param>
        /// <param name="data">Object of type T</param>
        public void SetCache<T>(string cacheName, T data)
        {
            cacheName += TenantName;
            //var hashEntry = ConvertToHashEntryList(data).ToArray();
            var jsonValue = JsonConvert.SerializeObject(data);
            RedisCache.StringSet(cacheName, jsonValue);
        }

        // Author         :   Boobalan Ranganathan		
        // Creation Date  :   07-Apr-2017
        // Ticket ID      :   
        /// <summary>
        /// Removes cached item from cache
        /// </summary>
        /// <param name="cacheName">Name of the cache</param>
        public void RemoveCache(string cacheName)
        {
            cacheName += TenantName;
            if (RedisCache.KeyExists(cacheName))
                RedisCache.KeyDelete(cacheName);
        }

        // Author         :   Boobalan Ranganathan		
        // Creation Date  :   07-Apr-2017
        // Ticket ID      :   
        /// <summary>
        /// Removes all cached item with keys ending with the match
        /// </summary>
        /// <param name="match">Ending part of the cache name</param>
        public void RemoveCachesEndingWith(string match)
        {
            match += TenantName;
            string server = ConfigurationManager.AppSettings["CacheServer"].ToString();
            var keys = RedisConnection.GetServer(server).Keys(DatabaseNumber);
            foreach (var key in keys)
            {
                if (key.ToString().EndsWith(match))
                    RedisCache.KeyDelete(key);
            }
        }

        // Author         :   Boobalan Ranganathan		
        // Creation Date  :   07-Apr-2017
        // Ticket ID      :   
        /// <summary>
        /// Checks existence of a cache item
        /// </summary>
        /// <param name="cacheName">Name of the cache</param>
        /// <returns>Return true if cache exists and false if cache doesn't exists</returns>
        public bool CacheExists(string cacheName)
        {
            cacheName += TenantName;
            var isCacheExists = RedisCache.KeyExists(cacheName);
            return isCacheExists;
        }

        #endregion

        #region Private Methods
        private static List<HashEntry> ConvertToHashEntryList(object instance)
        {
            var propertiesInHashEntityList = new List<HashEntry>();
            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType.Namespace.Contains("Laserbeam"))
                {
                    var subClassProperties = ConvertToHashEntryList(property.GetValue(instance));
                    propertiesInHashEntityList.AddRange(subClassProperties);
                }
                else
                {
                    var hashKey = instance.GetType().Name + "_" + property.Name;
                    var value = Convert.ToString(property.GetValue(instance));
                    propertiesInHashEntityList.Add(new HashEntry(hashKey, value));
                }
            }
            return propertiesInHashEntityList;
        }

        private static object ConvertFromHashEntryList(HashEntry[] hashEntryList, object instance)
        {
            var properties = instance.GetType().GetProperties();
            string hashKeyName = ConfigurationManager.AppSettings["HashKey"].ToString();
            foreach (var property in properties)
            {
                if (property.PropertyType.Namespace.Contains(hashKeyName))
                {
                    var valueObject = ConvertFromHashEntryList(hashEntryList, property.PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { }));
                    property.SetValue(instance, valueObject);
                }
                else
                {

                    var hashKey = instance.GetType().Name + "_" + property.Name;
                    var hashValue = hashEntryList.Where(m => m.Name == hashKey).First().Value;
                    var value = (hashValue.IsNullOrEmpty)
                        ? (property.PropertyType.Name.Contains("String") ? "" : null)
                        : (
                            property.PropertyType.IsGenericType
                            ? Convert.ChangeType(hashValue.ToString(), property.PropertyType.GenericTypeArguments[0])
                            : Convert.ChangeType(hashValue.ToString(), property.PropertyType)
                        );
                    property.SetValue(instance, value);
                }
            }
            return instance;
        }

        #endregion
    }    
}
