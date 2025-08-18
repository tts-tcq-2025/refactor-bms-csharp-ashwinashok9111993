using System.Linq;

// Age group classification with range-based lookup
public static class AgeClassification
{
    // Age group definitions with ranges for better performance
    private static readonly (int MinAge, int MaxAge, string Group)[] AgeGroupRanges = 
    {
        (0, 0, "Newborn (0-12 months)"),
        (1, 3, "Child (1-3 years)"),
        (4, 5, "Child (3-5 years)"),
        (6, 10, "Child (6-10 years)"),
        (11, 14, "Adolescent (11-14 years)"),
        (15, int.MaxValue, "Adult (15+ years)")
    };

    private static readonly (int MinAge, int MaxAge, int MinPulse, int MaxPulse)[] PulseRateRanges = 
    {
        (0, 0, 100, 160),        // Newborn (0-12 months)
        (1, 3, 80, 130),         // Child (1-3 years)
        (4, 5, 80, 120),         // Child (3-5 years)
        (6, 10, 70, 110),        // Child (6-10 years)
        (11, 14, 60, 105),       // Adolescent (11-14 years)
        (15, int.MaxValue, 60, 100)  // Adult (15+ years)
    };

    // Default implementations using simple array lookups
    public static readonly AgeClassifier DefaultAgeClassifier = age =>
    {
        var ageGroup = AgeGroupRanges.FirstOrDefault(r => age >= r.MinAge && age <= r.MaxAge);
        return ageGroup.Group ?? "Unknown";
    };

    public static readonly PulseRateLimitProvider DefaultPulseRateProvider = age =>
    {
        var pulseRange = PulseRateRanges.FirstOrDefault(r => age >= r.MinAge && age <= r.MaxAge);
        return pulseRange.MinPulse == 0 ? (60, 100) : (pulseRange.MinPulse, pulseRange.MaxPulse);
    };
}
