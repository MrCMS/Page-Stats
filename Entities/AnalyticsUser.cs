using System;
using System.Collections.Generic;
using MrCMS.Entities;
using MrCMS.Entities.People;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Stats.Entities
{
    public class AnalyticsUser : SystemEntity
    {
        //public virtual Guid Guid { get; set; }
        public virtual User User { get; set; }
        public virtual IList<AnalyticsSession> AnalyticsSessions { get; set; }

        public virtual string Email { get; set; }
        public virtual DateTime? DateLastChecked { get; set; }

        public virtual bool RequiresEmailCheck
        {
            get
            {
                if (User != null || !string.IsNullOrWhiteSpace(Email))
                    return false;
                if (!DateLastChecked.HasValue)
                    return true;
                return DateLastChecked < CurrentRequestData.Now.AddHours(-2);
            }
        }

    }
}