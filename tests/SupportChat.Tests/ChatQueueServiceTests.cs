using SupportChat.Api.Services;
using Xunit;

namespace SupportChat.Tests;

public class ChatQueueServiceTests
{
    private static (IChatQueueService queue, FakeTimeProvider time) CreateService()
    {
        var time = new FakeTimeProvider { UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc), OfficeHours = true };
        var registry = new TeamRegistry(time);
        var queue = new ChatQueueService(time, registry);
        return (queue, time);
    }

    [Fact]
    public void TryEnqueue_WhenUnderCapacity_ReturnsAccepted()
    {
        var (queue, _) = CreateService();
        var (accepted, sessionId) = queue.TryEnqueue();
        Assert.True(accepted);
        Assert.NotNull(sessionId);
        Assert.NotNull(queue.GetSession(sessionId!.Value));
    }

    [Fact]
    public void TryEnqueue_WhenMainQueueFull_AndOfficeHours_UsesOverflow()
    {
        var (queue, time) = CreateService();
        var mainMax = 31; // Team A max queue at 12:00
        for (var i = 0; i < mainMax; i++)
            queue.TryEnqueue();
        var (accepted, sessionId) = queue.TryEnqueue();
        Assert.True(accepted);
        Assert.NotNull(sessionId);
    }

    [Fact]
    public void TryEnqueue_WhenOverflowAlsoFull_ReturnsRefused()
    {
        var (queue, time) = CreateService();
        time.OfficeHours = true;
        var mainMax = 31;
        var overflowMax = 36;
        for (var i = 0; i < mainMax + overflowMax; i++)
            queue.TryEnqueue();
        var (accepted, sessionId) = queue.TryEnqueue();
        Assert.False(accepted);
        Assert.Null(sessionId);
    }

    [Fact]
    public void TryEnqueue_WhenMainFull_AndNotOfficeHours_ReturnsRefused()
    {
        var (queue, time) = CreateService();
        time.UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        time.OfficeHours = false;
        var mainMax = 31;
        for (var i = 0; i < mainMax; i++)
            queue.TryEnqueue();
        var (accepted, _) = queue.TryEnqueue();
        Assert.False(accepted);
    }
}
