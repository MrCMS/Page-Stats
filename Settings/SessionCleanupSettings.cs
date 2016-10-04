using System.ComponentModel.DataAnnotations;
using MrCMS.Settings;

namespace MrCMS.Web.Apps.Stats.Settings
{
    public class SessionCleanupSettings : SiteSettingsBase
    {
        public SessionCleanupSettings()
        {
            DaysToKeep = 90;
            SessionsToClear = 500;
        }
        public int DaysToKeep { get; set; }

        [Range(1, 2000)]
        public int SessionsToClear { get; set; }

        public override bool RenderInSettings
        {
            get { return true; }
        }
    }
}