using MrCMS.Entities.Documents.Web;
using MrCMS.Events;
using MrCMS.Web.Apps.Stats.Areas.Admin.Services;

namespace MrCMS.Web.Apps.Stats.EventHandlers
{
    public class UpdatePageHierarchy : IOnAdded<Webpage>, IOnUpdated<Webpage>, IOnDeleted<Webpage>
    {
        private readonly IPageHierarchyAdminService _pageHierarchyAdminService;

        public UpdatePageHierarchy(IPageHierarchyAdminService pageHierarchyAdminService)
        {
            _pageHierarchyAdminService = pageHierarchyAdminService;
        }

        public void Execute(OnAddedArgs<Webpage> args)
        {
            _pageHierarchyAdminService.UpdateHierarchy(args.Item);
        }

        public void Execute(OnDeletedArgs<Webpage> args)
        {
            _pageHierarchyAdminService.DeleteHierarchy(args.Item);
        }

        public void Execute(OnUpdatedArgs<Webpage> args)
        {
            if (args.Original == null || args.Item == null)
                return;

            if (args.Original.Parent?.Id != args.Item.Parent?.Id)
                _pageHierarchyAdminService.UpdateHierarchy(args.Item);
        }
    }
}