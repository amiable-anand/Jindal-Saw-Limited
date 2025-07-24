namespace Jindal.Models
{
    public class ActivityItem
    {
        // Properties used by DashboardPage
        public string Icon { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; }

        // Properties used by EnterpriseAnalyticsPage
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;

        // Optional: Helper property to format time for display
        public string FormattedTime => Time.ToString("HH:mm");

        // Constructor for DashboardPage usage
        public ActivityItem(string icon, string message, DateTime time)
        {
            Icon = icon;
            Message = message;
            Time = time;
            Title = message; // Map message to title for consistency
            Description = string.Empty;
            TimeAgo = GetTimeAgo(time);
        }

        // Constructor for EnterpriseAnalyticsPage usage
        public ActivityItem(string icon, string title, string description, DateTime time)
        {
            Icon = icon;
            Title = title;
            Description = description;
            Time = time;
            Message = title; // Map title to message for consistency
            TimeAgo = GetTimeAgo(time);
        }

        // Default constructor
        public ActivityItem()
        {
            Time = DateTime.Now;
            TimeAgo = GetTimeAgo(Time);
        }

        private string GetTimeAgo(DateTime time)
        {
            var timeSpan = DateTime.Now - time;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hr ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day ago";
            else
                return time.ToString("MMM dd");
        }
    }
}
