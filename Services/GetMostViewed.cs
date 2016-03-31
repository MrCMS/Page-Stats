using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Web.Apps.AzureStats.Entities;
using MrCMS.Web.Apps.AzureStats.Helpers;
using MrCMS.Web.Apps.Stats.Entities;
using MrCMS.Website;
using MrCMS.Website.Caching;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;

namespace MrCMS.Web.Apps.Stats.Services
{
    public class GetMostViewed : IGetMostViewed
    {
        private readonly IStatelessSession _statelessSession;
        private readonly ISession _session;
        private readonly ICacheManager _cacheManager;
        private readonly Site _site;
        private static readonly object LockObject = new object();


        public GetMostViewed(IStatelessSession statelessSession, ISession session, ICacheManager cacheManager, Site site)
        {
            _statelessSession = statelessSession;
            _session = session;
            _cacheManager = cacheManager;
            _site = site;
        }

        public List<T> GetTopX<T>(Webpage parent, int numberOfPages, int lastXHours = 24) where T : Webpage
        {
            lock (LockObject)
            {
                return _cacheManager.Get(
                    string.Format("get-most-read-articles.{0}.{1}.{2}", parent?.Id ?? -_site.Id, numberOfPages,
                        lastXHours),
                    () =>
                    {
                        var now = CurrentRequestData.Now;

                        var fromDate = now.AddHours(-lastXHours);

                        var pageTypes =
                            TypeHelper.GetAllConcreteTypesAssignableFrom<T>().Select(x => x.FullName).ToList();

                        AnalyticsPageView pageView = null;
                        AnalyticsSession analyticsSession = null;
                        AnalyticsUser analyticsUser = null;
                        Webpage webpageAlias = null;
                        Webpage parentAlias = null;

                        var queryOver = _session.QueryOver(() => pageView)
                            .JoinAlias(() => pageView.Webpage, () => webpageAlias, JoinType.LeftOuterJoin)
                            .JoinAlias(() => pageView.AnalyticsSession, () => analyticsSession)
                            .JoinAlias(() => analyticsSession.AnalyticsUser, () => analyticsUser)
                            .JoinAlias(() => webpageAlias.Parent, () => parentAlias);

                        // Filter by last x days
                        queryOver =
                            queryOver.Where(
                                () =>
                                    pageView.CreatedOn >= fromDate &&
                                    pageView.CreatedOn <= now);

                        queryOver = queryOver.Where(() => webpageAlias.DocumentType.IsIn(pageTypes));

                        // Remove unpublished items
                        queryOver = queryOver.Where(() => webpageAlias.Published);

                        if (parent != null)
                        {
                            queryOver =
                                queryOver.WithSubquery.WhereExists(
                                    QueryOver.Of<AnalyticsHierarchyInfo>()
                                        .Where(x => x.Page.Id == webpageAlias.Id && x.Parent.Id == parent.Id)
                                        .Select(x => x.Id));
                        }

                        // Get uniques - sort

                        var ids = queryOver
                            .Select(Projections.Group<AnalyticsPageView>(view => view.Webpage.Id))
                            .OrderBy(Projections.CountDistinct(() => analyticsUser.Id)).Desc
                            .ThenBy(Projections.CountDistinct(() => analyticsSession.Id)).Desc
                            .ThenBy(Projections.CountDistinct(() => pageView.Id)).Desc
                            .Take(numberOfPages)
                            .Cacheable()
                            .List<int>().ToList();

                        return _session.QueryOver<T>()
                            .Where(x => x.Id.IsIn(ids))
                            .List()
                            .OrderBy(article => ids.IndexOf(article.Id))
                            .ToList();

                        //var queryOver = _statelessSession.QueryOver<PageViewSummary>()
                        //    .Where(x => x.PageType.IsIn(pageTypes))
                        //    .And(x => x.Date >= fromDate && x.Date < startOfThisHour)
                        //    .And(x => x.Site.Id == _site.Id);
                        //queryOver = parent == null
                        //    ? queryOver.Where(x => x.Parent == null)
                        //    : queryOver.Where(x => x.Parent.Id == parent.Id);
                        //TopArticlesInfo info = null;
                        //var topArticlesInfos = queryOver.SelectList(builder =>
                        //{
                        //    builder.SelectGroup(x => x.Webpage.Id).WithAlias(() => info.PageId);
                        //    builder.SelectSum(x => x.Views).WithAlias(() => info.Count);
                        //    return builder;
                        //}).TransformUsing(Transformers.AliasToBean<TopArticlesInfo>())
                        //    .OrderBy(Projections.Sum<PageViewSummary>(x => x.Views)).Desc
                        //    .Take(numberOfPages)
                        //    .List<TopArticlesInfo>();
                        //var articleIds = topArticlesInfos.Select(x => x.PageId).ToList();

                        //var articles = _session.QueryOver<T>().Where(x => x.Id.IsIn(articleIds)).Cacheable().List();

                        //return articles.OrderBy(x => articleIds.IndexOf(x.Id)).ToList();
                    }, TimeSpan.FromMinutes(15), CacheExpiryType.Absolute);
            }
        }

        public class TopArticlesInfo
        {
            public int Count { get; set; }
            public int PageId { get; set; }
        }
    }
}