using System.Web;
using MrCMS.Web.Apps.Stats.Models;

namespace MrCMS.Web.Apps.Stats.Services
{
    public class DefaultGetEmailFromRequest : IGetEmailFromRequest
    {
        public bool CanCheck
        {
            get { return false; }
        }

        public GetEmailResult GetEmail(HttpContextBase context)
        {
            return GetEmailResult.CouldNotLookup();
        }
    }
}