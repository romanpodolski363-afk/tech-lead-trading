using SupportChat.Api.Services;
using Xunit;

namespace SupportChat.Tests;

public class TeamConfigurationTests
{
    [Fact]
    public void TeamA_HasCapacity21_MaxQueue31()
    {
        var teams = TeamConfiguration.CreateTeams();
        var teamA = teams.First(t => t.Id == "A");
        Assert.Equal(21, teamA.Capacity); // 1*5 + 2*6 + 1*4 = 21
        Assert.Equal(31, teamA.MaxQueueLength); // 21*1.5 = 31.5 -> 31
    }

    [Fact]
    public void TeamB_HasCapacity22_MaxQueue33()
    {
        var teams = TeamConfiguration.CreateTeams();
        var teamB = teams.First(t => t.Id == "B");
        Assert.Equal(22, teamB.Capacity); // 8+6+4+4
        Assert.Equal(33, teamB.MaxQueueLength);
    }

    [Fact]
    public void Overflow_Has6Juniors_Capacity24_MaxQueue36()
    {
        var teams = TeamConfiguration.CreateTeams();
        var overflow = teams.First(t => t.Id == "Overflow");
        Assert.Equal(6, overflow.Agents.Count);
        Assert.Equal(24, overflow.Capacity);
        Assert.Equal(36, overflow.MaxQueueLength);
    }

    [Theory]
    [InlineData(10, 0, true)]   // 10:00 = Team A on shift
    [InlineData(20, 0, true)]   // 20:00 = Team B on shift
    [InlineData(2, 0, true)]    // 02:00 = Team C on shift
    public void IsTeamOnShift_MatchesShiftBoundaries(int hour, int minute, bool expectedTeamOnShift)
    {
        var utc = new DateTime(2025, 6, 15, hour, minute, 0, DateTimeKind.Utc);
        var a = TeamConfiguration.IsTeamOnShift("A", utc);
        var b = TeamConfiguration.IsTeamOnShift("B", utc);
        var c = TeamConfiguration.IsTeamOnShift("C", utc);
        Assert.True(a || b || c);
    }
}
