namespace SupportChat.Api.Services;

public interface ITimeProvider
{
    DateTime UtcNow { get; }

    /// <summary>
    /// Overflow team is only available during office hours.
    /// </summary>
    bool IsOfficeHours(DateTime utcNow);
}
