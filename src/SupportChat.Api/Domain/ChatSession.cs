namespace SupportChat.Api.Domain;

public enum SessionStatus
{
    Queued,
    Assigned,
    Inactive
}

public class ChatSession
{
    public Guid Id { get; }
    public SessionStatus Status { get; set; }
    public string? AssignedAgentId { get; set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime? LastPollAtUtc { get; set; }
    public int MissedPolls { get; set; }

    public ChatSession(Guid id)
    {
        Id = id;
        Status = SessionStatus.Queued;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
