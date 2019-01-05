using Laserbeam.BusinessObject.Common;
using Laserbeam.DataManager.Interfaces.Common;
using System.Collections.Generic;
using System.Linq;

namespace Laserbeam.DataManager.Common
{
    public class TenantCacheProvider: ITenantCacheProvider
    {
        private IBaseRepository m_baseRepository;
        private IRedisCacheProvider m_redisCacheProvider;

        public TenantCacheProvider(IBaseRepository baseRepository, IRedisCacheProvider redisCacheProvider)
        {
            m_baseRepository = baseRepository;
            m_redisCacheProvider = redisCacheProvider;
        }

        public List<MessageResourceModel> GetMessageCache()
        {
            if (m_redisCacheProvider.CacheExists("MessageResource"))
                return m_redisCacheProvider.GetCache<List<MessageResourceModel>>("MessageResource");
            var data = (from r in m_baseRepository.GetQuery<ResourceData>()
                        select new MessageResourceModel
                        {
                            ResourceNum = r.ResourceNum,
                            Action = r.Action,
                            ObjectKey = r.ObjectKey,
                            ModuleName = r.ModuleName,
                            MessageTitle = r.MessageTitle,
                            Message = r.Message
                        }).ToList();
            m_redisCacheProvider.SetCache<List<MessageResourceModel>>("MessageResource", data);
            return data;
        }

        public void RemoveCache(string cacheName)
        {
            m_redisCacheProvider.RemoveCache(cacheName);
        }
    }
}
