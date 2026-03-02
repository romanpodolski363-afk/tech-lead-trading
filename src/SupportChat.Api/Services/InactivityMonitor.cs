using SupportChat.Api.Domain;

namespace SupportChat.Api.Services;

/// <summary>
/// Marks sessions inactive when they have not received 3 poll requests (e.g. no poll in 3 seconds).
/// </summary>
public interface IInactivityMonitor
{
    void CheckAndMarkInactive();
}

public class InactivityMonitor : IInactivityMonitor
{
    private const int InactivityThresholdSeconds = 3;

    private readonly IChatQueueService _queue;
    private readonly ITeamRegistry _registry;
    private readonly ITimeProvider _time;

    public InactivityMonitor(IChatQueueService queue, ITeamRegistry registry, ITimeProvider time)
    {
        _queue = queue;
        _registry = registry;
        _time = time;
    }

    public void CheckAndMarkInactive()
    {
        var now = _time.UtcNow;
        var cutoff = now.AddSeconds(-InactivityThresholdSeconds);

        foreach (var session in GetActiveSessions())
        {
            var lastPoll = session.LastPollAtUtc ?? session.CreatedAtUtc;
            if (lastPoll < cutoff && session.Status == SessionStatus.Assigned)
            {
                session.Status = SessionStatus.Inactive;
                if (session.AssignedAgentId != null)
                {
                    var agent = _registry.GetAgent(session.AssignedAgentId);
                    if (agent != null && agent.CurrentChats > 0)
                        agent.CurrentChats--;
                }
            }
        }
    }

    private IEnumerable<ChatSession> GetActiveSessions()
    {
        return _queue.GetAllSessions()
            .Where(s => s.Status == SessionStatus.Queued || s.Status == SessionStatus.Assigned);
    }
}
