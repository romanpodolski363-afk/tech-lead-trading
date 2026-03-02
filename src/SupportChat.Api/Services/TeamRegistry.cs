using SupportChat.Api.Domain;

namespace SupportChat.Api.Services;

/// <summary>
/// Holds teams and agents. Provides agents on shift for assignment (junior first).
/// </summary>
public interface ITeamRegistry
{
    IReadOnlyList<Agent> GetAgentsOnShift(DateTime utcNow);
    Agent? GetAgent(string agentId);
    int GetMainPoolMaxQueueLength(DateTime utcNow);
    int GetOverflowPoolMaxQueueLength(DateTime utcNow);
}

public class TeamRegistry : ITeamRegistry
{
    private readonly ITimeProvider _time;
    private readonly List<Team> _teams;

    public TeamRegistry(ITimeProvider time)
    {
        _time = time;
        _teams = TeamConfiguration.CreateTeams();
    }

    public IReadOnlyList<Agent> GetAgentsOnShift(DateTime utcNow)
    {
        var main = _teams
            .Where(t => !t.IsOverflow && TeamConfiguration.IsTeamOnShift(t.Id, utcNow))
            .SelectMany(t => t.Agents)
            .ToList();

        if (_time.IsOfficeHours(utcNow))
        {
            var overflow = _teams.FirstOrDefault(t => t.IsOverflow);
            if (overflow != null)
                main.AddRange(overflow.Agents);
        }

        return main
            .OrderBy(a => (int)a.Seniority)
            .ToList();
    }

    public Agent? GetAgent(string agentId)
    {
        return _teams.SelectMany(t => t.Agents).FirstOrDefault(a => a.Id == agentId);
    }

    public int GetMainPoolMaxQueueLength(DateTime utcNow)
    {
        var main = _teams
            .FirstOrDefault(t => !t.IsOverflow && TeamConfiguration.IsTeamOnShift(t.Id, utcNow));
        return main?.MaxQueueLength ?? 0;
    }

    public int GetOverflowPoolMaxQueueLength(DateTime utcNow)
    {
        if (!_time.IsOfficeHours(utcNow)) return 0;
        var overflow = _teams.FirstOrDefault(t => t.IsOverflow);
        return overflow?.MaxQueueLength ?? 0;
    }
}
