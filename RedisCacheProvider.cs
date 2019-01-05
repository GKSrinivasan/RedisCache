using Laserbeam.Libraries.Interfaces.Core;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;
using Laserbeam.BusinessObject;

namespace Laserbeam.Libraries.Core
{
    public class RedisCacheProvider: ITenantCacheProvider
    {
        #region Fields
        public string TenantName;

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
                LazyRedisConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("LaserRedisCache.redis.cache.windows.net:6380,password=9wq6lk7rtoA3WSxaRVUawCUabZ7kv2i6yOO9oAUGNWs=,ssl=True,abortConnect=False"));
                RedisConnection = LazyRedisConnection.Value;
                RedisCache = RedisConnection.GetDatabase(1);
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
            var hashEntryList = RedisCache.HashGetAll(cacheName);
            var instance = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
            var cacheData = (T)ConvertFromHashEntryList(hashEntryList, instance);
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
            var hashEntry = ConvertToHashEntryList(data).ToArray();
            RedisCache.HashSet(cacheName, hashEntry);
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
            var keys = RedisConnection.GetServer("LaserRedisCache.redis.cache.windows.net:6380").Keys(1);
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
            foreach (var property in properties)
            {
                if (property.PropertyType.Namespace.Contains("Laserbeam"))
                {
                    var valueObject =  ConvertFromHashEntryList(hashEntryList, property.PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { }));
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
