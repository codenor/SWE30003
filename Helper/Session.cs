namespace ElectronicsStoreAss3.Helpers
{
    public static class Session
    {
        private const string SessionKey = "CartSessionId";

        public static string GetOrCreate(HttpContext context)
        {
            var sessionId = context.Session.GetString(SessionKey);

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                context.Session.SetString(SessionKey, sessionId);
            }

            return sessionId!;
        }
    }
}

