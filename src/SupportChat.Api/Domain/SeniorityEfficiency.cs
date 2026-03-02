namespace SupportChat.Api.Domain;

/// <summary>
/// Max concurrency per agent is 10; efficiency multiplier by seniority.
/// Capacity per agent = 10 * multiplier (rounded down).
/// </summary>
public static class SeniorityEfficiency
{
    private const int MaxConcurrencyPerAgent = 10;

    public static double Multiplier(Seniority seniority) => seniority switch
    {
        Seniority.Junior => 0.4,
        Seniority.MidLevel => 0.6,
        Seniority.Senior => 0.8,
        Seniority.TeamLead => 0.5,
        _ => 0.4
    };

    public static int CapacityPerAgent(Seniority seniority)
    {
        return (int)Math.Floor(MaxConcurrencyPerAgent * Multiplier(seniority));
    }
}
