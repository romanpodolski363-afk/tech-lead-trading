using Microsoft.AspNetCore.Mvc;
using SupportChat.Api.Domain;
using SupportChat.Api.Services;

namespace SupportChat.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatQueueService _queue;
    private readonly IAssignmentService _assignment;
    private readonly IInactivityMonitor _inactivity;

    public ChatController(IChatQueueService queue, IAssignmentService assignment, IInactivityMonitor inactivity)
    {
        _queue = queue;
        _assignment = assignment;
        _inactivity = inactivity;
    }

    /// <summary>
    /// Creates and queues a chat session. Returns OK with sessionId when accepted, Refused when queue (and overflow) are full.
    /// Client should poll every 1s after receiving OK.
    /// </summary>
    [HttpPost("session")]
    public IActionResult CreateSession()
    {
        var (accepted, sessionId) = _queue.TryEnqueue();
        if (!accepted)
            return Ok(new CreateSessionResponse("Refused", null));

        _assignment.TryAssignNext();
        return Ok(new CreateSessionResponse("OK", sessionId));
    }

    /// <summary>
    /// Poll endpoint. Call every 1s after CreateSession returns OK.
    /// Updates last poll time; monitor marks session inactive after 3 missed polls (3 seconds without poll).
    /// </summary>
    [HttpPost("session/{id:guid}/poll")]
    public IActionResult Poll(Guid id)
    {
        var session = _queue.GetSession(id);
        if (session == null)
            return NotFound();

        if (session.Status == SessionStatus.Inactive)
            return Ok(new PollResponse("Inactive"));

        session.LastPollAtUtc = DateTime.UtcNow;
        session.MissedPolls = 0;

        _inactivity.CheckAndMarkInactive();
        _assignment.TryAssignNext();

        return Ok(new PollResponse(
            session.Status == SessionStatus.Assigned ? "Assigned" : "Queued",
            session.AssignedAgentId));
    }

    [HttpGet("session/{id:guid}")]
    public IActionResult GetSession(Guid id)
    {
        var session = _queue.GetSession(id);
        if (session == null)
            return NotFound();
        return Ok(new SessionDto(session));
    }
}

public record CreateSessionResponse(string Status, Guid? SessionId);

public record PollResponse(string Status, string? AssignedAgentId = null);

public record SessionDto(Guid Id, string Status, string? AssignedAgentId)
{
    public SessionDto(ChatSession s) : this(s.Id, s.Status.ToString(), s.AssignedAgentId) { }
}
