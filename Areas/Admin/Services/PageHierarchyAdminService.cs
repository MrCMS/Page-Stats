using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Stats.Entities;
using MrCMS.Web.Apps.Stats.Services;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Stats.Areas.Admin.Services
{
    public class PageHierarchyAdminService : IPageHierarchyAdminService
    {
        private readonly IStatelessSession _statelessSession;
        private readonly IGetStatsWebpageInfo _getStatsWebpageInfo;

        public PageHierarchyAdminService(IStatelessSession statelessSession, IGetStatsWebpageInfo getStatsWebpageInfo)
        {
            _statelessSession = statelessSession;
            _getStatsWebpageInfo = getStatsWebpageInfo;
        }

        private class DummyWebpage : Webpage
        {

        }

        public void ResetHierarchies()
        {
            var allHierarchyInfo = _getStatsWebpageInfo.GetAllHierarchyInfo();

            // delete all
            _statelessSession.Transact(session =>
            {
                session.CreateQuery("delete AnalyticsHierarchyInfo ahi").ExecuteUpdate();

                foreach (var key in allHierarchyInfo.Keys)
                {
                    foreach (var parent in allHierarchyInfo[key])
                    {
                        session.Insert(new AnalyticsHierarchyInfo
                        {
                            Page = new DummyWebpage { Id = key },
                            Parent = new DummyWebpage { Id = parent }
                        });
                    }
                }
            });
        }

        public void UpdateHierarchy(Webpage webpage)
        {
            var hierarchyInfo = _getStatsWebpageInfo.GetHierarchyInfo(webpage).ToList();

            var existingHierarchyInfo = _statelessSession.QueryOver<AnalyticsHierarchyInfo>().Where(x => x.Page.Id == webpage.Id).List();
            var parentsToUpdate = existingHierarchyInfo.Select(x => x.Parent.Id).ToList();

            var existingDependencies = _statelessSession.QueryOver<AnalyticsHierarchyInfo>().Where(x => x.Parent.Id == webpage.Id).List();
            var childrenToUpdate = existingDependencies.Select(x => x.Page.Id);

            AnalyticsHierarchyInfo info = null;
            var toRemove = _statelessSession.QueryOver(() => info)
                .Where(x => x.Parent.Id.IsIn(parentsToUpdate))
                .WithSubquery.WhereExists(
                    QueryOver.Of<AnalyticsHierarchyInfo>().Where(x => x.Page.Id == info.Page.Id && x.Parent.Id == webpage.Id).Select(x => x.Id))
                .List();

            _statelessSession.Transact(session =>
            {
                // update associated
                foreach (var entity in toRemove)
                    session.Delete(entity);
                foreach (var child in childrenToUpdate)
                    foreach (var parent in hierarchyInfo)
                    {
                        session.Insert(new AnalyticsHierarchyInfo
                        {
                            Page = new DummyWebpage { Id = child },
                            Parent = new DummyWebpage { Id = parent }
                        });
                    }
                // update current page
                foreach (var entity in existingHierarchyInfo)
                    session.Delete(entity);
                foreach (var parent in hierarchyInfo)
                    session.Insert(new AnalyticsHierarchyInfo
                    {
                        Page = new DummyWebpage { Id = webpage.Id },
                        Parent = new DummyWebpage { Id = parent }
                    });
            });
        }

        public void DeleteHierarchy(Webpage webpage)
        {
            var existingHierarchyInfo = _statelessSession.QueryOver<AnalyticsHierarchyInfo>().Where(x => x.Page.Id == webpage.Id).List();
            _statelessSession.Transact(session =>
            {
                foreach (var entity in existingHierarchyInfo)
                    session.Delete(entity);
            });
        }
    }
}