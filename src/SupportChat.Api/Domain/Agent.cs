namespace SupportChat.Api.Domain;

public class Agent
{
    public string Id { get; }
    public Seniority Seniority { get; }
    public int Capacity { get; }
    public string TeamId { get; }
    public bool IsOverflow { get; }

    /// <summary>
    /// Shift end (exclusive). Agent accepts new chats only when now &lt; ShiftEndUtc.
    /// </summary>
    public DateTime? ShiftEndUtc { get; set; }

    /// <summary>
    /// Number of chats currently assigned to this agent.
    /// </summary>
    public int CurrentChats { get; set; }

    public Agent(string id, Seniority seniority, string teamId, bool isOverflow = false)
    {
        Id = id;
        Seniority = seniority;
        TeamId = teamId;
        IsOverflow = isOverflow;
        Capacity = SeniorityEfficiency.CapacityPerAgent(seniority);
    }

    public bool HasCapacity => CurrentChats < Capacity;

    public bool IsOnShift(DateTime utcNow)
    {
        if (ShiftEndUtc == null) return true;
        return utcNow < ShiftEndUtc.Value;
    }
}
