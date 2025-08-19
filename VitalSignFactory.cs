using System.Collections.Generic;
using System.Linq;

// Extensible vital sign factory for creating different types of vital signs
public interface IVitalSignFactory
{
    VitalSign CreateVitalSign(string name, float value, int age);
}

// Default factory implementation using existing medical limits
public class StandardVitalSignFactory : IVitalSignFactory
{
    private readonly Dictionary<string, System.Func<int, (float Min, float Max)>> _limitProviders;

    public StandardVitalSignFactory()
    {
        _limitProviders = new Dictionary<string, System.Func<int, (float Min, float Max)>>
        {
            { VitalSignConstants.Temperature, _ => (AgeLimits.Temperature.Min, AgeLimits.Temperature.Max) },
            { VitalSignConstants.OxygenSaturation, _ => (AgeLimits.Spo2.Min, AgeLimits.Spo2.Max) },
            { VitalSignConstants.PulseRate, age => 
                {
                    var limits = AgeClassification.DefaultPulseRateProvider(age);
                    return (limits.Min, limits.Max);
                }
            }
        };
    }

    public VitalSign CreateVitalSign(string name, float value, int age)
    {
        if (!_limitProviders.TryGetValue(name, out var provider))
            throw new System.ArgumentException($"Unknown vital sign: {name}");

        var limits = provider(age);
        return new VitalSign(name, value, limits.Min, limits.Max);
    }

    // Extension point for adding new vital signs
    public void RegisterVitalSign(string name, System.Func<int, (float Min, float Max)> limitProvider)
    {
        _limitProviders[name] = limitProvider;
    }
}

// Enhanced vital sign validator with factory pattern for better extensibility
public static class EnhancedVitalSignValidator
{
    private static readonly IVitalSignFactory DefaultFactory = new StandardVitalSignFactory();

    public static VitalSignResult CreateVitalSignResult(
        Dictionary<string, float> vitalValues, 
        int age,
        IVitalSignFactory? factory = null,
        AgeClassifier? ageClassifier = null)
    {
        factory ??= DefaultFactory;
        ageClassifier ??= AgeClassification.DefaultAgeClassifier;

        var vitals = vitalValues.Select(kvp => factory.CreateVitalSign(kvp.Key, kvp.Value, age)).ToList();
        
        return new VitalSignResult(vitals.All(v => v.IsInRange), vitals, age) 
        { 
            AgeGroup = ageClassifier(age) 
        };
    }
}

// Configuration-driven vital sign limits for future extensibility
public static class VitalSignConfiguration
{
    private static readonly Dictionary<string, Dictionary<string, (float Min, float Max)>> AgeBasedLimits = new()
    {
        [VitalSignConstants.AgeGroups.Newborn] = new()
        {
            [VitalSignConstants.Temperature] = (95f, 102f),
            [VitalSignConstants.PulseRate] = (100f, 160f),
            [VitalSignConstants.OxygenSaturation] = (90f, 100f)
        },
        [VitalSignConstants.AgeGroups.Adult] = new()
        {
            [VitalSignConstants.Temperature] = (95f, 102f),
            [VitalSignConstants.PulseRate] = (60f, 100f),
            [VitalSignConstants.OxygenSaturation] = (90f, 100f)
        }
        // Additional age groups can be added here
    };

    public static (float Min, float Max) GetLimitsForVitalSign(string vitalSignName, string ageGroup)
    {
        if (AgeBasedLimits.TryGetValue(ageGroup, out var vitalLimits) &&
            vitalLimits.TryGetValue(vitalSignName, out var limits))
        {
            return limits;
        }

        // Fallback to adult limits
        return AgeBasedLimits[VitalSignConstants.AgeGroups.Adult][vitalSignName];
    }

    // Extension point for adding new age groups or vital signs
    public static void AddAgeGroupLimits(string ageGroup, Dictionary<string, (float Min, float Max)> limits)
    {
        AgeBasedLimits[ageGroup] = limits;
    }
}
