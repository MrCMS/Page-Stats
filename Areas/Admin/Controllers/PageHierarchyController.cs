using MrCMS.Website.Controllers;
using System.Web.Mvc;
using MrCMS.Web.Apps.Stats.Areas.Admin.Services;

namespace MrCMS.Web.Apps.Stats.Areas.Admin.Controllers
{
    public class PageHierarchyController : MrCMSAppAdminController<StatsApp>
    {
        private readonly IPageHierarchyAdminService _pageHierarchyAdminService;

        public PageHierarchyController(IPageHierarchyAdminService pageHierarchyAdminService)
        {
            _pageHierarchyAdminService = pageHierarchyAdminService;
        }

        public ViewResult Index()
        {
            return View();
        }

        public RedirectToRouteResult ResetHierarchies()
        {
            _pageHierarchyAdminService.ResetHierarchies();
            TempData["reset"] = true;
            return RedirectToAction("Index");
        }
    }
}