using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Transform;

namespace MrCMS.Web.Apps.Stats.Services
{
    public class GetStatsWebpageInfo : IGetStatsWebpageInfo
    {
        private readonly IStatelessSession _statelessSession;
        //private readonly Site _site;

        public GetStatsWebpageInfo(IStatelessSession statelessSession)
        {
            _statelessSession = statelessSession;
            //_site = site;
        }

        public Dictionary<int, HashSet<int>> GetAllHierarchyInfo()
        {
            HierarchyInfoMap info = null;
            Dictionary<int, int?> parentMappings =
                _statelessSession.QueryOver<Webpage>()
                    //.Where(x => x.Site.Id == _site.Id)
                    .SelectList(
                        builder =>
                            builder.Select(webpage => webpage.Id)
                                .WithAlias(() => info.PageId)
                                .Select(webpage => webpage.Parent.Id)
                                .WithAlias(() => info.ParentId))
                    .TransformUsing(Transformers.AliasToBean<HierarchyInfoMap>())
                    .List<HierarchyInfoMap>()
                    .ToDictionary(x => x.PageId, x => x.ParentId);

            return parentMappings.ToDictionary(x => x.Key, x => GetHierarchy(x.Key, parentMappings));
        }

        public HashSet<int> GetHierarchyInfo(Webpage webpage)
        {
            // .Skip(1) to remove self
            return webpage.ActivePages.Skip(1).Select(x => x.Id).ToHashSet();
        }

        //public Dictionary<int, string> GetPageTypes()
        //{
        //    PageTypeMap map = null;
        //    return _statelessSession.QueryOver<Webpage>()
        //        //.Where(x => x.Site.Id == _site.Id)
        //        .SelectList(
        //            builder =>
        //                builder.Select(webpage => webpage.Id)
        //                    .WithAlias(() => map.PageId)
        //                    .Select(webpage => webpage.DocumentType)
        //                    .WithAlias(() => map.DocumentType))
        //        .TransformUsing(Transformers.AliasToBean<PageTypeMap>())
        //        .List<PageTypeMap>()
        //        .ToDictionary(x => x.PageId, x => x.DocumentType);
        //}

        private HashSet<int> GetHierarchy(int page, Dictionary<int, int?> parentMappings)
        {
            var hashSet = new HashSet<int>();
            var pageId = page;
            while (parentMappings.ContainsKey(pageId) && parentMappings[pageId].HasValue)
            {
                var value = parentMappings[pageId].Value;
                hashSet.Add(value);
                pageId = value;
            }
            return hashSet;
        }

        public class HierarchyInfoMap
        {
            public int PageId { get; set; }
            public int? ParentId { get; set; }
        }
        public class PageTypeMap
        {
            public int PageId { get; set; }
            public string DocumentType { get; set; }
        }
    }
}