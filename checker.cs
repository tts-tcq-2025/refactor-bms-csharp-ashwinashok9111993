using System;
using System.Collections.Generic;
using System.Linq;

// Pure data structures for vital signs
public record VitalSign(string Name, float Value, float MinLimit, float MaxLimit)
{
    public bool IsInRange => Value >= MinLimit && Value <= MaxLimit;
    public string Status => IsInRange ? "Normal" : "Critical";
}

public record VitalSignResult(bool IsAllNormal, List<VitalSign> VitalSigns, int Age)
{
    public List<VitalSign> CriticalVitals => VitalSigns.Where(v => !v.IsInRange).ToList();
    public string AgeGroup => GetAgeGroup(Age);
    
    private static string GetAgeGroup(int age) => age switch
    {
        < 1 when age >= 0 => "Newborn (0-12 months)",
        >= 1 and <= 3 => "Child (1-3 years)",
        >= 4 and <= 5 => "Child (3-5 years)", 
        >= 6 and <= 10 => "Child (6-10 years)",
        >= 11 and <= 14 => "Adolescent (11-14 years)",
        >= 15 => "Adult (15+ years)",
        _ => "Unknown"
    };
}

// Pure functions for vital sign validation
public static class VitalSignValidator
{
    // Age-based vital sign limits based on medical standards
    public static class AgeLimits
    {
        // Temperature limits remain consistent across all ages
        public static readonly (float Min, float Max) Temperature = (95f, 102f);
        
        // SpO2 limits remain consistent across all ages  
        public static readonly (int Min, int Max) Spo2 = (90, 100);
        
        // Age-based pulse rate limits based on medical literature
        public static (int Min, int Max) GetPulseRateLimits(int age) => age switch
        {
            < 1 when age >= 0 => (100, 160),        // Newborn (0-12 months)
            >= 1 and <= 3 => (80, 130),             // Child (1-3 years)
            >= 4 and <= 5 => (80, 120),             // Child (3-5 years)
            >= 6 and <= 10 => (70, 110),            // Child (6-10 years)
            >= 11 and <= 14 => (60, 105),           // Adolescent (11-14 years)
            >= 15 => (60, 100),                     // Adult (15+ years)
            _ => (60, 100)                          // Default to adult ranges
        };
    }

    // Updated method to include age parameter
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2, int age)
    {
        var pulseRateLimits = AgeLimits.GetPulseRateLimits(age);
        
        var vitals = new List<VitalSign>
        {
            new("Temperature", temperature, AgeLimits.Temperature.Min, AgeLimits.Temperature.Max),
            new("Pulse Rate", pulseRate, pulseRateLimits.Min, pulseRateLimits.Max),
            new("Oxygen Saturation", spo2, AgeLimits.Spo2.Min, AgeLimits.Spo2.Max)
        };

        return new VitalSignResult(vitals.All(v => v.IsInRange), vitals, age);
    }

    // Backward compatibility method without age (assumes adult)
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2)
    {
        return CheckVitals(temperature, pulseRate, spo2, 25); // Default to adult age
    }

    // Age-aware individual validation methods
    public static bool IsTemperatureOk(float temperature) =>
        temperature >= AgeLimits.Temperature.Min && temperature <= AgeLimits.Temperature.Max;

    public static bool IsPulseRateOk(int pulseRate, int age)
    {
        var limits = AgeLimits.GetPulseRateLimits(age);
        return pulseRate >= limits.Min && pulseRate <= limits.Max;
    }
    
    // Backward compatibility method without age (assumes adult)
    public static bool IsPulseRateOk(int pulseRate) => IsPulseRateOk(pulseRate, 25);

    public static bool IsSpo2Ok(int spo2) =>
        spo2 >= AgeLimits.Spo2.Min && spo2 <= AgeLimits.Spo2.Max;
}

// I/O operations separated from pure functions
public static class VitalSignDisplay
{
    public static void DisplayResult(VitalSignResult result)
    {
        Console.WriteLine($"Patient Age: {result.Age} years ({result.AgeGroup})");
        
        if (result.IsAllNormal)
        {
            DisplayNormalVitals(result.VitalSigns);
        }
        else
        {
            DisplayCriticalVitals(result.CriticalVitals);
        }
    }

    private static void DisplayNormalVitals(List<VitalSign> vitals)
    {
        Console.WriteLine("Vitals received within normal range");
        foreach (var vital in vitals)
        {
            Console.WriteLine($"{vital.Name}: {vital.Value} (Normal range: {vital.MinLimit}-{vital.MaxLimit})");
        }
    }

    private static void DisplayCriticalVitals(List<VitalSign> criticalVitals)
    {
        foreach (var vital in criticalVitals)
        {
            DisplayCriticalAlert($"{vital.Name} critical! Value: {vital.Value} (Normal range: {vital.MinLimit}-{vital.MaxLimit})");
        }
    }

    private static void DisplayCriticalAlert(string message)
    {
        Console.WriteLine(message);
        for (int i = 0; i < 6; i++)
        {
            Console.Write("\r* ");
            System.Threading.Thread.Sleep(1000);
            Console.Write("\r *");
            System.Threading.Thread.Sleep(1000);
        }
    }
}

// Main checker class with reduced complexity and age support
class Checker
{
    // New age-aware method
    public static bool VitalsOk(float temperature, int pulseRate, int spo2, int age)
    {
        var result = VitalSignValidator.CheckVitals(temperature, pulseRate, spo2, age);
        VitalSignDisplay.DisplayResult(result);
        return result.IsAllNormal;
    }
    
    // Backward compatibility method (assumes adult age)
    public static bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        return VitalsOk(temperature, pulseRate, spo2, 25); // Default to adult age
    }
}
