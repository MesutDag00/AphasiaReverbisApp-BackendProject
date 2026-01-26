namespace AphaisaReverbes.Services;

public sealed class InvitationCleanupOptions
{
    public const string SectionName = "InvitationCleanup";

    public int IntervalMinutes { get; set; } = 60;
    public int MaxAgeDays { get; set; } = 7;
    public int StartupDelaySeconds { get; set; } = 10;
}

