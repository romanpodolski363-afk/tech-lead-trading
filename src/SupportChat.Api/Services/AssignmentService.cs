using SupportChat.Api.Domain;

namespace SupportChat.Api.Services;

/// <summary>
/// Picks sessions from the queue and assigns to next available agent.
/// Round robin preferring junior first (agents sorted by seniority).
/// When shift is over, agent is not in GetAgentsOnShift so won't get new chats.
/// </summary>
public interface IAssignmentService
{
    void TryAssignNext();
}

public class AssignmentService : IAssignmentService
{
    private readonly IChatQueueService _queue;
    private readonly ITeamRegistry _registry;
    private readonly ITimeProvider _time;

    public AssignmentService(IChatQueueService queue, ITeamRegistry registry, ITimeProvider time)
    {
        _queue = queue;
        _registry = registry;
        _time = time;
    }

    public void TryAssignNext()
    {
        var now = _time.UtcNow;
        var agents = _registry.GetAgentsOnShift(now);
        if (agents.Count == 0) return;

        while (_queue.TryPeek(out var session) && session != null)
        {
            var agent = agents.FirstOrDefault(a => a.HasCapacity);
            if (agent == null) break;

            _queue.TryDequeue(out _);
            session.Status = SessionStatus.Assigned;
            session.AssignedAgentId = agent.Id;
            agent.CurrentChats++;
        }
    }
}
