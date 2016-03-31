using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Web.Apps.Stats.Services
{
    public interface IGetStatsWebpageInfo
    {
        Dictionary<int, HashSet<int>> GetAllHierarchyInfo();
        HashSet<int> GetHierarchyInfo(Webpage webpage);
    }
}