using SupportChat.Api.Domain;
using Xunit;

namespace SupportChat.Tests;

public class SeniorityEfficiencyTests
{
    [Theory]
    [InlineData(Seniority.Junior, 4)]   // 10 * 0.4
    [InlineData(Seniority.MidLevel, 6)] // 10 * 0.6
    [InlineData(Seniority.Senior, 8)]   // 10 * 0.8
    [InlineData(Seniority.TeamLead, 5)] // 10 * 0.5
    public void CapacityPerAgent_MatchesSpec(Seniority seniority, int expectedCapacity)
    {
        var cap = SeniorityEfficiency.CapacityPerAgent(seniority);
        Assert.Equal(expectedCapacity, cap);
    }

    [Fact]
    public void TeamOf2Mid1Junior_HasCapacity16_Queue24()
    {
        var capacity = 2 * SeniorityEfficiency.CapacityPerAgent(Seniority.MidLevel)
                       + 1 * SeniorityEfficiency.CapacityPerAgent(Seniority.Junior);
        Assert.Equal(16, capacity);
        var maxQueue = (int)Math.Floor(capacity * 1.5);
        Assert.Equal(24, maxQueue);
    }
}
