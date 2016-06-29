using System;
using System.Web;
using MrCMS.Web.Apps.Stats.Filters;

namespace MrCMS.Web.Apps.Stats.Helpers
{
    public static class AnalyticsHttpContextExtensions
    {
        public static AnalyticsUserChangedResult AnalyticsUserGuidHasChanged(this HttpContextBase context)
        {
            const string key = AnalyticsSessionFilter.UserGuidHasChanged;
            var httpCookie = context.Request.Cookies[key];
            if (httpCookie != null)
            {
                // force expiry
                httpCookie.Expires = new DateTime(1990, 1, 1);
                context.Response.Cookies.Set(httpCookie);
                Guid oldGuid;
                return new AnalyticsUserChangedResult
                {
                    Changed = true,
                    OldGuid = Guid.TryParse(httpCookie.Value, out oldGuid) ? oldGuid : (Guid?)null
                };
            }
            return new AnalyticsUserChangedResult();
        }

        public struct AnalyticsUserChangedResult
        {
            public bool Changed { get; set; }
            public Guid? OldGuid { get; set; }
        }
    }
}