namespace SupportChat.Api.Domain;

/// <summary>
/// Team capacity = sum of (10 * efficiency) per agent, rounded down.
/// Max queue length = capacity * 1.5 (rounded down).
/// </summary>
public class Team
{
    public string Id { get; }
    public List<Agent> Agents { get; }
    public bool IsOverflow { get; }
    public int Capacity { get; }
    public int MaxQueueLength { get; }

    public Team(string id, List<Agent> agents, bool isOverflow = false)
    {
        Id = id;
        Agents = agents;
        IsOverflow = isOverflow;
        Capacity = (int)Math.Floor(Agents.Sum(a => 10 * SeniorityEfficiency.Multiplier(a.Seniority)));
        MaxQueueLength = (int)Math.Floor(Capacity * 1.5);
    }
}
