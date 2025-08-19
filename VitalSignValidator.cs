using System.Collections.Generic;
using System.Linq;

// Pure functions for vital sign validation with extensible design
public static class VitalSignValidator
{
    // Updated method to include age parameter with injectable delegates and temperature unit support
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2, int age,
        TemperatureUnit temperatureUnit = TemperatureUnit.Fahrenheit,
        AgeClassifier? ageClassifier = null, PulseRateLimitProvider? pulseRateProvider = null)
    {
        var providers = GetProviders(ageClassifier, pulseRateProvider);
        var temperatureInFahrenheit = TemperatureConverter.ToFahrenheit(temperature, temperatureUnit);
        var vitals = CreateVitalSigns(temperatureInFahrenheit, pulseRate, spo2, age, providers.pulseRateProvider);
        
        return CreateVitalSignResult(vitals, age, providers.ageClassifier);
    }

    private static (AgeClassifier ageClassifier, PulseRateLimitProvider pulseRateProvider) GetProviders(
        AgeClassifier? ageClassifier, PulseRateLimitProvider? pulseRateProvider) =>
        (ageClassifier ?? AgeClassification.DefaultAgeClassifier,
         pulseRateProvider ?? AgeClassification.DefaultPulseRateProvider);

    private static List<VitalSign> CreateVitalSigns(float temperature, int pulseRate, int spo2, int age, 
        PulseRateLimitProvider pulseRateProvider)
    {
        var pulseRateLimits = pulseRateProvider(age);
        return new List<VitalSign>
        {
            new(VitalSignConstants.Temperature, temperature, AgeLimits.Temperature.Min, AgeLimits.Temperature.Max),
            new(VitalSignConstants.PulseRate, pulseRate, pulseRateLimits.Min, pulseRateLimits.Max),
            new(VitalSignConstants.OxygenSaturation, spo2, AgeLimits.Spo2.Min, AgeLimits.Spo2.Max)
        };
    }

    private static VitalSignResult CreateVitalSignResult(List<VitalSign> vitals, int age, AgeClassifier ageClassifier) =>
        new(vitals.All(v => v.IsInRange), vitals, age) { AgeGroup = ageClassifier(age) };

    // Backward compatibility method without age (assumes adult)
    public static VitalSignResult CheckVitals(float temperature, int pulseRate, int spo2) =>
        CheckVitals(temperature, pulseRate, spo2, MedicalThresholds.DefaultAdultAge);

    // Overload with temperature unit support
    public static VitalSignResult CheckVitals(TemperatureReading temperature, int pulseRate, int spo2, int age,
        AgeClassifier? ageClassifier = null, PulseRateLimitProvider? pulseRateProvider = null) =>
        CheckVitals(temperature.Value, pulseRate, spo2, age, temperature.Unit, ageClassifier, pulseRateProvider);

    // Age-aware individual validation methods
    public static bool IsTemperatureOk(float temperature) =>
        temperature >= AgeLimits.Temperature.Min && temperature <= AgeLimits.Temperature.Max;

    public static bool IsPulseRateOk(int pulseRate, int age, PulseRateLimitProvider? pulseRateProvider = null)
    {
        pulseRateProvider ??= AgeClassification.DefaultPulseRateProvider;
        var limits = pulseRateProvider(age);
        return pulseRate >= limits.Min && pulseRate <= limits.Max;
    }
    
    // Backward compatibility method without age (assumes adult)
    public static bool IsPulseRateOk(int pulseRate) => IsPulseRateOk(pulseRate, MedicalThresholds.DefaultAdultAge);

    public static bool IsSpo2Ok(int spo2) =>
        spo2 >= AgeLimits.Spo2.Min && spo2 <= AgeLimits.Spo2.Max;
}
