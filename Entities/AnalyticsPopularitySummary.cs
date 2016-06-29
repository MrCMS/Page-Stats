using MrCMS.Entities;
using MrCMS.Entities.Documents.Web;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Stats.Entities
{
    public class AnalyticsPopularitySummary : SystemEntity
    {
        public AnalyticsPopularitySummary()
        {
            CreatedOn = CurrentRequestData.Now;
            UpdatedOn = CurrentRequestData.Now;
        }

        public virtual string Type { get; set; }
        public virtual Webpage Parent { get; set; }
        public virtual Webpage Page { get; set; }
        public virtual int Order { get; set; }
    }
}