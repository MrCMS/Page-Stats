using MrCMS.Entities.Documents.Web;

namespace MrCMS.Web.Apps.Stats.Areas.Admin.Services
{
    public interface IPageHierarchyAdminService
    {
        void ResetHierarchies();
        void UpdateHierarchy(Webpage webpage);
        void DeleteHierarchy(Webpage webpage);
    }
}