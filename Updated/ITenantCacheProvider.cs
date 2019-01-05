using Laserbeam.BusinessObject.Common;
using System.Collections.Generic;

namespace Laserbeam.DataManager.Interfaces.Common
{
    public interface ITenantCacheProvider
    {
        List<MessageResourceModel> GetMessageCache();
        void RemoveCache(string cacheName);
    }
}
