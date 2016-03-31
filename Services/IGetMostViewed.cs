using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Web.Apps.Stats.Services
{
    public interface IGetMostViewed
    {
        List<T> GetTopX<T>(Webpage parent, int numberOfPages, int lastXHours = 24) where T : Webpage;
    }
}