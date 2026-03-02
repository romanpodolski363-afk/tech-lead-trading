namespace SupportChat.Api.Domain;

/// <summary>
/// Agent seniority. Assignment order: Junior first, then Mid, then Senior, then Team Lead.
/// </summary>
public enum Seniority
{
    Junior = 0,
    MidLevel = 1,
    Senior = 2,
    TeamLead = 3
}
