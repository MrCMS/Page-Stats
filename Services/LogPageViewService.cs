using System;
using System.Linq;
using System.Web;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Apps.Stats.Entities;
using MrCMS.Web.Apps.Stats.Helpers;
using MrCMS.Web.Apps.Stats.Models;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Stats.Services
{
    public class LogPageViewService : ILogPageViewService
    {
        private readonly IGetCurrentUser _getCurrentUser;
        private readonly HttpContextBase _context;
        private readonly IStatelessSession _session;
        private readonly Site _site;
        private readonly IGetEmailFromRequest _getEmailFromRequest;

        public LogPageViewService(IStatelessSession session, IGetCurrentUser getCurrentUser, HttpContextBase context,
            Site site, IGetEmailFromRequest getEmailFromRequest)
        {
            _session = session;
            _getCurrentUser = getCurrentUser;
            _context = context;
            _site = site;
            _getEmailFromRequest = getEmailFromRequest;
        }

        public void LogPageView(PageViewInfo info)
        {
            User user = _getCurrentUser.Get();
            var site = _session.Get<Site>(_site.Id);
            DateTime now = CurrentRequestData.Now;
            AnalyticsUser analyticsUser = GetUser(user == null ? info.User : user.Guid);
            bool userIsNew = analyticsUser == null;
            if (userIsNew)
            {
                analyticsUser = new AnalyticsUser
                {
                    User = user,
                    CreatedOn = now,
                    UpdatedOn = now,
                };
                analyticsUser.SetGuid(info.User);
                _session.Insert(analyticsUser);
            }

            if (analyticsUser.RequiresEmailCheck && _getEmailFromRequest.CanCheck)
                CheckEmail(analyticsUser, now);

            AnalyticsSession analyticsSession = GetCurrentSession(info.Session);
            bool sessionIsNew = analyticsSession == null;
            var changedResult = _context.AnalyticsUserGuidHasChanged();
            if (sessionIsNew)
            {
                analyticsSession = new AnalyticsSession
                {
                    AnalyticsUser = analyticsUser,
                    IP = _context.GetCurrentIP(),
                    UserAgent = _context.Request.UserAgent,
                    Site = site,
                    CreatedOn = now,
                    UpdatedOn = now,
                };
                analyticsSession.SetGuid(info.Session);
                _session.Insert(analyticsSession);
            }
            // only move it if it's going to a live user
            else if (changedResult.Changed && analyticsUser.User != null)
            {
                UpdateOldUsersSessions(changedResult, analyticsSession, analyticsUser);
            }

            var pageView = new AnalyticsPageView
            {
                Webpage = GetWebpage(info.Url),
                Url = info.Url,
                AnalyticsSession = analyticsSession,
                Site = site,
                CreatedOn = now,
                UpdatedOn = now,
            };

            _session.Insert(pageView);
        }

        private void CheckEmail(AnalyticsUser analyticsUser, DateTime now)
        {
            var result = _getEmailFromRequest.GetEmail(_context);
            if (!result.CouldLookup)
                return;

            if (!string.IsNullOrWhiteSpace(result.Email))
                analyticsUser.Email = result.Email;
            else
                analyticsUser.DateLastChecked = now;
            _session.Transact(session => session.Update(analyticsUser));
        }

        private void UpdateOldUsersSessions(AnalyticsHttpContextExtensions.AnalyticsUserChangedResult changedResult, AnalyticsSession analyticsSession, AnalyticsUser analyticsUser)
        {
            if (!changedResult.OldGuid.HasValue || analyticsUser.Guid == changedResult.OldGuid.Value)
                return;

            _session.Transact(session =>
            {
                analyticsSession.AnalyticsUser = analyticsUser;
                _session.Update(analyticsSession);

                var oldUser = GetUser(changedResult.OldGuid.Value);
                if (oldUser != null && oldUser.Id != analyticsUser.Id)
                {
                    // this must have been the current user, so move over their sessions
                    var analyticsSessions =
                        _session.QueryOver<AnalyticsSession>().Where(x => x.AnalyticsUser.Id == oldUser.Id).List();
                    foreach (var entity in analyticsSessions)
                    {
                        entity.AnalyticsUser = analyticsUser;
                        _session.Update(entity);
                    }
                    _session.Delete(oldUser);
                }
            });
        }

        private Webpage GetWebpage(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                return null;

            string path = uri.AbsolutePath.TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                return CurrentRequestData.HomePage;
            Webpage page = _session.QueryOver<Webpage>()
                .Where(webpage => webpage.UrlSegment == path)
                .Cacheable()
                .List()
                .FirstOrDefault();
            return page;
        }

        private AnalyticsSession GetCurrentSession(Guid session)
        {
            return _session.QueryOver<AnalyticsSession>().Where(user => user.Guid == session)
                .Cacheable()
                .List().FirstOrDefault();
        }

        private AnalyticsUser GetUser(Guid guid)
        {
            return _session.QueryOver<AnalyticsUser>().Where(user => user.Guid == guid)
                .Cacheable()
                .List().FirstOrDefault();
        }
    }
}