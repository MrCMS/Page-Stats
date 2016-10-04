namespace MrCMS.Web.Apps.Stats.Models
{
    public struct GetEmailResult
    {
        private GetEmailResult(bool couldLookup, string email)
        {
            CouldLookup = couldLookup;
            Email = email;
        }

        public static GetEmailResult LookupResult(string email)
        {
            return new GetEmailResult(true, email);
        }

        public static GetEmailResult CouldNotLookup()
        {
            return new GetEmailResult(false, null);
        }
        public bool CouldLookup { get; }
        public string Email { get; }
    }
}