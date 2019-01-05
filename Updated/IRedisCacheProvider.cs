namespace Laserbeam.DataManager.Interfaces.Common
{
    public interface IRedisCacheProvider
    {
        T GetCache<T>(string cacheName);
        void SetCache<T>(string cacheName, T data);
        void RemoveCache(string cacheName);
        void RemoveCachesEndingWith(string endingLike);
        bool CacheExists(string cacheName);
    }
}
