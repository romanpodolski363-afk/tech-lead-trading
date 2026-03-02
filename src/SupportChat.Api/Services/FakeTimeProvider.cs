namespace SupportChat.Api.Services;

/// <summary>
/// Test double for ITimeProvider. Set UtcNow and OfficeHours to control behaviour.
/// </summary>
public class FakeTimeProvider : ITimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc); // 12:00 = shift 1 (Team A)

    public bool OfficeHours { get; set; } = true;

    public bool IsOfficeHours(DateTime utcNow) => OfficeHours;
}
