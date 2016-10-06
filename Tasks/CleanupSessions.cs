using System.Linq;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Tasks;
using MrCMS.Web.Apps.Stats.Entities;
using MrCMS.Web.Apps.Stats.Settings;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace MrCMS.Web.Apps.Stats.Tasks
{
    public class CleanupSessions : SchedulableTask
    {
        private readonly IStatelessSession _session;
        private readonly SessionCleanupSettings _settings;

        public CleanupSessions(IStatelessSession session, SessionCleanupSettings settings)
        {
            _session = session;
            _settings = settings;
        }
        public override int Priority { get { return 1; } }

        protected override void OnExecute()
        {
            AnalyticsUser analyticsUser = null;
            var date = CurrentRequestData.Now.AddDays(-_settings.DaysToKeep);
            var sessions = _session.QueryOver<AnalyticsSession>()
                .JoinAlias(x => x.AnalyticsUser, () => analyticsUser)
                .Where(() => analyticsUser.Email == null && analyticsUser.User == null)
                .And(x => x.CreatedOn < date)
                //.OrderBy(x => x.CreatedOn).Desc no need to order by + timing out over large dataset
                .Take(_settings.SessionsToClear)
                .List();

            var ids = sessions.Select(x => x.Id).ToList();
            var views = _session.QueryOver<AnalyticsPageView>()
                .Where(x => x.AnalyticsSession.Id.IsIn(ids))
                .List();

            _session.Transact(session =>
            {
                views.ForEach(session.Delete);
                sessions.ForEach(session.Delete);
            });
        }
    }
}