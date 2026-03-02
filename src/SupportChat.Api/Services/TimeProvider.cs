namespace SupportChat.Api.Services;

public class TimeProvider : ITimeProvider
{
    /// <summary>
    /// Office hours: 08:00 - 24:00 UTC (overflow available).
    /// Night 00:00-08:00 UTC has no overflow.
    /// </summary>
    public bool IsOfficeHours(DateTime utcNow)
    {
        var hour = utcNow.TimeOfDay.TotalHours;
        return hour >= 8 && hour < 24;
    }

    public DateTime UtcNow => DateTime.UtcNow;
}
