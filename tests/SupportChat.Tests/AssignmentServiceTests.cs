using SupportChat.Api.Domain;
using SupportChat.Api.Services;
using Xunit;

namespace SupportChat.Tests;

public class AssignmentServiceTests
{
    [Fact]
    public void AssignsToJuniorFirst_ThenMid_ThenSenior()
    {
        var time = new FakeTimeProvider { UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc) };
        var registry = new TeamRegistry(time);
        var queue = new ChatQueueService(time, registry);
        var assignment = new AssignmentService(queue, registry, time);

        queue.TryEnqueue();
        queue.TryEnqueue();
        queue.TryEnqueue();
        queue.TryEnqueue();
        queue.TryEnqueue();
        assignment.TryAssignNext();

        var agents = registry.GetAgentsOnShift(time.UtcNow);
        var junior = agents.Where(a => a.Seniority == Seniority.Junior).ToList();
        var assigned = queue.GetAllSessions().Where(s => s.Status == SessionStatus.Assigned).ToList();
        Assert.True(assigned.Count > 0);
        var firstAssignedAgent = registry.GetAgent(assigned[0].AssignedAgentId!);
        Assert.NotNull(firstAssignedAgent);
        Assert.Equal(Seniority.Junior, firstAssignedAgent.Seniority);
    }

    [Fact]
    public void AfterAssignment_SessionHasAssignedAgent()
    {
        var time = new FakeTimeProvider { UtcNow = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc) };
        var registry = new TeamRegistry(time);
        var queue = new ChatQueueService(time, registry);
        var assignment = new AssignmentService(queue, registry, time);

        var (_, sessionId) = queue.TryEnqueue();
        Assert.NotNull(sessionId);
        assignment.TryAssignNext();

        var session = queue.GetSession(sessionId!.Value);
        Assert.NotNull(session);
        Assert.Equal(SessionStatus.Assigned, session.Status);
        Assert.NotNull(session.AssignedAgentId);
    }
}
