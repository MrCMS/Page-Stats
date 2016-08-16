using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Stats.Entities;
using MrCMS.Website;
using MrCMS.Website.Caching;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using StackExchange.Profiling;

namespace MrCMS.Web.Apps.Stats.Services
{
    public class GetMostViewed : IGetMostViewed
    {
        private static readonly object LockObject = new object();
        private readonly ICacheManager _cacheManager;
        private readonly Site _site;
        private readonly IStatelessSession _statelessSession;


        public GetMostViewed(IStatelessSession statelessSession, ICacheManager cacheManager, Site site)
        {
            _statelessSession = statelessSession;
            _cacheManager = cacheManager;
            _site = site;
        }

        public List<T> GetTopX<T>(Webpage parent, int numberOfPages, int lastXHours = 24) where T : Webpage
        {
            var cacheKey = string.Format("get-most-read-articles.{0}.{1}.{2}", parent?.Id ?? -_site.Id, numberOfPages,
                lastXHours);
            using (MiniProfiler.Current.Step("Getting most read articles for key - " + cacheKey))
                lock (LockObject)
                {
                    return _cacheManager.Get(
                        cacheKey,
                        () =>
                        {
                            var now = CurrentRequestData.Now;

                            var fromDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(-lastXHours);

                            var pageTypes =
                                TypeHelper.GetAllConcreteTypesAssignableFrom<T>().Select(x => x.FullName).ToList();

                            AnalyticsPageView pageView = null;
                            Webpage webpageAlias = null;

                            var queryOver = _statelessSession.QueryOver(() => pageView)
                                .JoinAlias(() => pageView.Webpage, () => webpageAlias, JoinType.LeftOuterJoin);

                            // Filter by last x hours
                            queryOver = queryOver.Where(() => pageView.CreatedOn >= fromDate);

                            queryOver = queryOver.Where(() => webpageAlias.DocumentType.IsIn(pageTypes));

                            // Remove unpublished items
                            queryOver = queryOver.Where(() => webpageAlias.Published);

                            if (parent != null)
                            {
                                queryOver =
                                    queryOver.WithSubquery.WhereProperty(x => x.Webpage.Id)
                                        .In(QueryOver.Of<AnalyticsHierarchyInfo>()
                                            .Where(x => x.Parent.Id == parent.Id)
                                            .Select(x => x.Page.Id));
                            }
                            else
                            {
                                queryOver.Where(() => webpageAlias.Site.Id == _site.Id);
                            }

                            // Get uniques - sort

                            var ids = queryOver
                                .Select(Projections.Group<AnalyticsPageView>(view => view.Webpage.Id))
                                .OrderBy(Projections.Count(() => pageView.Webpage.Id)).Desc
                                .Take(numberOfPages)
                                .Cacheable()
                                .List<int>().ToList();

                            return _statelessSession.QueryOver<T>()
                                .Where(x => x.Id.IsIn(ids))
                                .Fetch(x => x.Parent).Eager
                                .Fetch(x => x.Site).Eager
                                .Cacheable()
                                .List()
                                .OrderBy(article => ids.IndexOf(article.Id))
                                .ToList();
                        }, TimeSpan.FromMinutes(1800), CacheExpiryType.Absolute);
                }
        }

        public class TopArticlesInfo
        {
            public int Count { get; set; }
            public int PageId { get; set; }
        }
    }
}