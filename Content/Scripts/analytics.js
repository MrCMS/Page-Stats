(function ($) {
    var cookiesEnabled = function () {
        var cookieEnabled = (navigator.cookieEnabled) ? true : false;

        if (typeof navigator.cookieEnabled == "undefined" && !cookieEnabled) {
            document.cookie = "testcookie";
            cookieEnabled = (document.cookie.indexOf("testcookie") != -1) ? true : false;
        }
        return (cookieEnabled);
    }
    var analytics = function () {
        var userGuidKey = 'mrcms.analytics.user';
        var userSessionKey = 'mrcms.analytics.session';

        function getParam(p) {
            var match = RegExp('[?&]' + p + '=([^&]*)').exec(window.location.search);
            return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
        }
        
        function gutToCookie() {
            var gutid = getParam('gutid');
            if (gutid) {
                $.cookie('gutid', gutid);
            }
        }
        function logPageView() {
            if (!cookiesEnabled() || !$.cookie)
                return;
            var user = $.cookie(userGuidKey);
            if (!user)
                return;
            var session = $.cookie(userSessionKey);
            if (!session)
                return;
            var url = location.href;
            var data = {
                user: user,
                session: session,
                url: url
            }
            $.post('/analytics/legacy-log-page-view', data);
        }

        return {
            gutToCookie: gutToCookie,
            logPageView: logPageView
        };
    };

    $(function () {
        var a = analytics();
        a.gutToCookie();
        a.logPageView();
    });
})(jQuery);