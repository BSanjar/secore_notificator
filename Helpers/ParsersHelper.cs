namespace NotificationWorker.Helpers
{
    public static class ParsersHelper
    {
        public static DateTime NowForTimestamp()
          => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }
}

