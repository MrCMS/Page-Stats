using System.Web;
using MrCMS.Web.Apps.Stats.Models;

namespace MrCMS.Web.Apps.Stats.Services
{
    /// <summary>
    /// This allows us to use a third party system to gather the email of the user, 
    /// even if they may not have registered with the website. Default implementation
    /// is a dummy implementation to allow IoC to resolve an instance, which is to be
    /// overriden in implementation
    /// </summary>
    public interface IGetEmailFromRequest
    {
        bool CanCheck { get; }
        GetEmailResult GetEmail(HttpContextBase context);
    }
}