using MrCMS.Entities;
using MrCMS.Entities.Documents.Web;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Stats.Entities
{
    public class AnalyticsHierarchyInfo : SystemEntity
    {
        public AnalyticsHierarchyInfo()
        {
            CreatedOn = CurrentRequestData.Now;
            UpdatedOn = CurrentRequestData.Now;
        }
        public virtual Webpage Page { get; set; }
        public virtual Webpage Parent { get; set; }
    }
}