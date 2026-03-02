using SupportChat.Api.Domain;
using SupportChat.Api.Services;
using Xunit;

namespace SupportChat.Tests;

public class InactivityMonitorTests
{
    [Fact]
    public void MarksSessionInactive_WhenNoPollFor3Seconds()
    {
        var time = new FakeTimeProvider { UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc) };
        var registry = new TeamRegistry(time);
        var queue = new ChatQueueService(time, registry);
        var assignment = new AssignmentService(queue, registry, time);
        var monitor = new InactivityMonitor(queue, registry, time);

        var (_, sessionId) = queue.TryEnqueue();
        Assert.NotNull(sessionId);
        assignment.TryAssignNext();
        var session = queue.GetSession(sessionId!.Value);
        Assert.NotNull(session);
        session!.LastPollAtUtc = time.UtcNow.AddSeconds(-4);

        monitor.CheckAndMarkInactive();

        session = queue.GetSession(sessionId.Value);
        Assert.Equal(SessionStatus.Inactive, session!.Status);
    }

    [Fact]
    public void DoesNotMarkInactive_WhenPolledWithin3Seconds()
    {
        var time = new FakeTimeProvider { UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc) };
        var registry = new TeamRegistry(time);
        var queue = new ChatQueueService(time, registry);
        var assignment = new AssignmentService(queue, registry, time);
        var monitor = new InactivityMonitor(queue, registry, time);

        var (_, sessionId) = queue.TryEnqueue();
        Assert.NotNull(sessionId);
        assignment.TryAssignNext();
        var session = queue.GetSession(sessionId!.Value);
        Assert.NotNull(session);
        session!.LastPollAtUtc = time.UtcNow.AddSeconds(-1);

        monitor.CheckAndMarkInactive();

        session = queue.GetSession(sessionId.Value);
        Assert.Equal(SessionStatus.Assigned, session!.Status);
    }
}
