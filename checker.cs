using System;
using System.Collections.Generic;
using System.Linq;

// Pure data structures for vital signs
public record VitalSign(string Name, float Value, float MinLimit, float MaxLimit)
{
    public bool IsInRange => Value >= MinLimit && Value <= MaxLimit;
    public string Status => IsInRange ? "Normal" : "Critical";
}

public record VitalSignResult(bool IsAllNormal, List<VitalSign> VitalSigns)
{
    public List<VitalSign> CriticalVitals => VitalSigns.Where(v => !v.IsInRange).ToList();
}

// Pure functions for vital sign validation
public static class VitalSignValidator
{
    // Vital sign limits as constants for easy modification
    public static class Limits
    {
        public static readonly (float Min, float Max) Temperature = (95f, 102f);
        public static readonly (int Min, int Max) PulseRate = (60, 100);
        public static readonly (int Min, int Max) Spo2 = (90, 100);
    }

    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2)
    {
        var vitals = new List<VitalSign>
        {
            new("Temperature", temperature, Limits.Temperature.Min, Limits.Temperature.Max),
            new("Pulse Rate", pulseRate, Limits.PulseRate.Min, Limits.PulseRate.Max),
            new("Oxygen Saturation", spo2, Limits.Spo2.Min, Limits.Spo2.Max)
        };

        return new VitalSignResult(vitals.All(v => v.IsInRange), vitals);
    }

    public static bool IsTemperatureOk(float temperature) =>
        temperature >= Limits.Temperature.Min && temperature <= Limits.Temperature.Max;

    public static bool IsPulseRateOk(int pulseRate) =>
        pulseRate >= Limits.PulseRate.Min && pulseRate <= Limits.PulseRate.Max;

    public static bool IsSpo2Ok(int spo2) =>
        spo2 >= Limits.Spo2.Min && spo2 <= Limits.Spo2.Max;
}

// I/O operations separated from pure functions
public static class VitalSignDisplay
{
    public static void DisplayResult(VitalSignResult result)
    {
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
            Console.WriteLine($"{vital.Name}: {vital.Value}");
        }
    }

    private static void DisplayCriticalVitals(List<VitalSign> criticalVitals)
    {
        foreach (var vital in criticalVitals)
        {
            DisplayCriticalAlert($"{vital.Name} critical! Value: {vital.Value}");
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

// Main checker class with reduced complexity
class Checker
{
    public static bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        var result = VitalSignValidator.CheckVitals(temperature, pulseRate, spo2);
        VitalSignDisplay.DisplayResult(result);
        return result.IsAllNormal;
    }
}
