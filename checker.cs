using System;
using System.Collections.Generic;
using System.Linq;

// Delegates for age classification and pulse rate limits
public delegate string AgeClassifier(int age);
public delegate (int Min, int Max) PulseRateLimitProvider(int age);
public delegate void OutputWriter(string message);

// Pure data structures for vital signs
public record VitalSign(string Name, float Value, float MinLimit, float MaxLimit)
{
    public bool IsInRange => Value >= MinLimit && Value <= MaxLimit;
    internal string Status => IsInRange ? "Normal" : "Critical";
}

public record VitalSignResult(bool IsAllNormal, List<VitalSign> VitalSigns, int Age)
{
    public List<VitalSign> CriticalVitals => VitalSigns.Where(v => !v.IsInRange).ToList();
    public string AgeGroup { get; init; } = "";
}

// Pure functions for vital sign validation
public static class VitalSignValidator
{
    // Age-based vital sign limits based on medical standards
    internal static class AgeLimits
    {
        // Temperature limits remain consistent across all ages
        internal static readonly (float Min, float Max) Temperature = (95f, 102f);
        
        // SpO2 limits remain consistent across all ages  
        internal static readonly (int Min, int Max) Spo2 = (90, 100);
    }

    // Age group definitions with ranges for better performance and reduced complexity
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

    // Default implementations using simple array lookups with reduced complexity
    private static readonly AgeClassifier DefaultAgeClassifier = age =>
    {
        foreach (var (minAge, maxAge, group) in AgeGroupRanges)
        {
            if (age >= minAge && age <= maxAge)
                return group;
        }
        return "Unknown";
    };

    private static readonly PulseRateLimitProvider DefaultPulseRateProvider = age =>
    {
        foreach (var (minAge, maxAge, minPulse, maxPulse) in PulseRateRanges)
        {
            if (age >= minAge && age <= maxAge)
                return (minPulse, maxPulse);
        }
        return (60, 100); // Default to adult ranges
    };

    // Updated method to include age parameter with injectable delegates
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2, int age,
        AgeClassifier? ageClassifier = null, PulseRateLimitProvider? pulseRateProvider = null)
    {
        ageClassifier ??= DefaultAgeClassifier;
        pulseRateProvider ??= DefaultPulseRateProvider;
        
        var pulseRateLimits = pulseRateProvider(age);
        
        var vitals = new List<VitalSign>
        {
            new("Temperature", temperature, AgeLimits.Temperature.Min, AgeLimits.Temperature.Max),
            new("Pulse Rate", pulseRate, pulseRateLimits.Min, pulseRateLimits.Max),
            new("Oxygen Saturation", spo2, AgeLimits.Spo2.Min, AgeLimits.Spo2.Max)
        };

        return new VitalSignResult(vitals.All(v => v.IsInRange), vitals, age) { AgeGroup = ageClassifier(age) };
    }

    // Backward compatibility method without age (assumes adult)
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2)
    {
        return CheckVitals(temperature, pulseRate, spo2, 25); // Default to adult age
    }

    // Age-aware individual validation methods
    public static bool IsTemperatureOk(float temperature) =>
        temperature >= AgeLimits.Temperature.Min && temperature <= AgeLimits.Temperature.Max;

    public static bool IsPulseRateOk(int pulseRate, int age, PulseRateLimitProvider? pulseRateProvider = null)
    {
        pulseRateProvider ??= DefaultPulseRateProvider;
        var limits = pulseRateProvider(age);
        return pulseRate >= limits.Min && pulseRate <= limits.Max;
    }
    
    // Backward compatibility method without age (assumes adult)
    public static bool IsPulseRateOk(int pulseRate) => IsPulseRateOk(pulseRate, 25);

    public static bool IsSpo2Ok(int spo2) =>
        spo2 >= AgeLimits.Spo2.Min && spo2 <= AgeLimits.Spo2.Max;
}

// I/O operations separated from pure functions with injectable output writer
public static class VitalSignDisplay
{
    private static readonly OutputWriter DefaultOutputWriter = Console.WriteLine;

    public static void DisplayResult(VitalSignResult result, OutputWriter? outputWriter = null)
    {
        outputWriter ??= DefaultOutputWriter;
        outputWriter($"Patient Age: {result.Age} years ({result.AgeGroup})");
        
        if (result.IsAllNormal)
        {
            DisplayNormalVitals(result.VitalSigns, outputWriter);
        }
        else
        {
            DisplayCriticalVitals(result.CriticalVitals, outputWriter);
        }
    }

    private static void DisplayNormalVitals(List<VitalSign> vitals, OutputWriter outputWriter)
    {
        outputWriter("Vitals received within normal range");
        foreach (var vital in vitals)
        {
            outputWriter($"{vital.Name}: {vital.Value} (Normal range: {vital.MinLimit}-{vital.MaxLimit})");
        }
    }

    private static void DisplayCriticalVitals(List<VitalSign> criticalVitals, OutputWriter outputWriter)
    {
        foreach (var vital in criticalVitals)
        {
            DisplayCriticalAlert($"{vital.Name} critical! Value: {vital.Value} (Normal range: {vital.MinLimit}-{vital.MaxLimit})", outputWriter);
        }
    }

    private static void DisplayCriticalAlert(string message, OutputWriter outputWriter)
    {
        outputWriter(message);
        for (int i = 0; i < 6; i++)
        {
            outputWriter("\r* ");
            System.Threading.Thread.Sleep(1000);
            outputWriter("\r *");
            System.Threading.Thread.Sleep(1000);
        }
    }
}

// Main checker class with reduced complexity and age support
class Checker
{
    // New age-aware method with injectable output writer
    public static bool VitalsOk(float temperature, int pulseRate, int spo2, int age, OutputWriter? outputWriter = null)
    {
        var result = VitalSignValidator.CheckVitals(temperature, pulseRate, spo2, age);
        VitalSignDisplay.DisplayResult(result, outputWriter);
        return result.IsAllNormal;
    }
    
    // Backward compatibility method (assumes adult age)
    public static bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        return VitalsOk(temperature, pulseRate, spo2, 25); // Default to adult age
    }
}
