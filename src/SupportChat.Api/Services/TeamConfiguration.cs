using SupportChat.Api.Domain;

namespace SupportChat.Api.Services;

/// <summary>
/// Teams as per spec:
/// Team A: 1 TL, 2 Mid, 1 Junior
/// Team B: 1 Senior, 1 Mid, 2 Junior
/// Team C: 2 Mid (night shift)
/// Overflow: 6 Junior
/// Shifts: 3 x 8h — 00-08, 08-16, 16-24 UTC.
/// </summary>
public static class TeamConfiguration
{
    public const string TeamA = "A";
    public const string TeamB = "B";
    public const string TeamC = "C";
    public const string OverflowTeam = "Overflow";

    /// <summary>
    /// Returns which shift (0, 1, 2) the given UTC time falls into.
    /// Shift 0: 00:00-08:00, Shift 1: 08:00-16:00, Shift 2: 16:00-24:00.
    /// </summary>
    public static int GetShiftIndex(DateTime utcNow)
    {
        var hour = utcNow.Hour;
        if (hour < 8) return 0;
        if (hour < 16) return 1;
        return 2;
    }

    /// <summary>
    /// End of the current shift for the given time (exclusive).
    /// </summary>
    public static DateTime GetCurrentShiftEndUtc(DateTime utcNow)
    {
        var date = utcNow.Date;
        var shift = GetShiftIndex(utcNow);
        return shift switch
        {
            0 => date.AddHours(8),
            1 => date.AddHours(16),
            _ => date.AddDays(1)
        };
    }

    public static List<Team> CreateTeams()
    {
        var teamA = new Team(TeamA, new List<Agent>
        {
            new("A-TL", Seniority.TeamLead, TeamA),
            new("A-M1", Seniority.MidLevel, TeamA),
            new("A-M2", Seniority.MidLevel, TeamA),
            new("A-J", Seniority.Junior, TeamA)
        });

        var teamB = new Team(TeamB, new List<Agent>
        {
            new("B-S", Seniority.Senior, TeamB),
            new("B-M", Seniority.MidLevel, TeamB),
            new("B-J1", Seniority.Junior, TeamB),
            new("B-J2", Seniority.Junior, TeamB)
        });

        var teamC = new Team(TeamC, new List<Agent>
        {
            new("C-M1", Seniority.MidLevel, TeamC),
            new("C-M2", Seniority.MidLevel, TeamC)
        }, isOverflow: false);

        var overflow = new Team(OverflowTeam, Enumerable.Range(1, 6)
            .Select(i => new Agent($"OF-J{i}", Seniority.Junior, OverflowTeam, isOverflow: true))
            .ToList(), isOverflow: true);

        return new List<Team> { teamA, teamB, teamC, overflow };
    }

    /// <summary>
    /// Team A: 08-16, Team B: 16-24, Team C: 00-08 (night).
    /// </summary>
    public static bool IsTeamOnShift(string teamId, DateTime utcNow)
    {
        var shift = GetShiftIndex(utcNow);
        return teamId switch
        {
            TeamA => shift == 1,
            TeamB => shift == 2,
            TeamC => shift == 0,
            OverflowTeam => new TimeProvider().IsOfficeHours(utcNow),
            _ => false
        };
    }
}
