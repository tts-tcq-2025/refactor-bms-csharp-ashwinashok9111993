using Xunit;
using System.Linq;
using System.Collections.Generic;

public class CheckerTests
{
    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.False(Checker.VitalsOk(99f, 102, 70)); // Pulse rate and SpO2 out of range for adult
        Assert.True(Checker.VitalsOk(98.1f, 70, 98));  // All in range for adult
    }

    [Fact]
    public void CheckerWithAge_ShouldUseAgeSpecificRanges()
    {
        // Test newborn with high pulse rate (normal for newborn)
        Assert.True(Checker.VitalsOk(98.6f, 140, 95, 0)); // 140 bpm is normal for newborn
        
        // Test adult with same high pulse rate (abnormal for adult)
        Assert.False(Checker.VitalsOk(98.6f, 140, 95, 25)); // 140 bpm is high for adult
    }
}

public class VitalSignValidatorTests
{
    [Theory]
    [InlineData(95f, true)]   // Lower boundary
    [InlineData(102f, true)]  // Upper boundary
    [InlineData(98.6f, true)] // Normal value
    [InlineData(94.9f, false)] // Below range
    [InlineData(102.1f, false)] // Above range
    public void IsTemperatureOk_ShouldValidateCorrectly(float temperature, bool expected)
    {
        Assert.Equal(expected, VitalSignValidator.IsTemperatureOk(temperature));
    }

    [Theory]
    [InlineData(60, 25, true)]   // Adult: Lower boundary
    [InlineData(100, 25, true)]  // Adult: Upper boundary
    [InlineData(80, 25, true)]   // Adult: Normal value
    [InlineData(59, 25, false)]  // Adult: Below range
    [InlineData(101, 25, false)] // Adult: Above range
    [InlineData(140, 0, true)]   // Newborn: Normal high rate
    [InlineData(140, 25, false)] // Adult: Same rate is abnormal
    [InlineData(50, 0, false)]   // Newborn: Too low
    [InlineData(90, 7, true)]    // Child (6-10): Normal
    [InlineData(115, 25, false)] // Adult: High rate (>100 is abnormal for adults)
    public void IsPulseRateOk_ShouldValidateCorrectlyByAge(int pulseRate, int age, bool expected)
    {
        Assert.Equal(expected, VitalSignValidator.IsPulseRateOk(pulseRate, age));
    }

    [Theory]
    [InlineData(90, true)]   // Lower boundary
    [InlineData(100, true)]  // Upper boundary
    [InlineData(95, true)]   // Normal value
    [InlineData(89, false)]  // Below range
    public void IsSpo2Ok_ShouldValidateCorrectly(int spo2, bool expected)
    {
        Assert.Equal(expected, VitalSignValidator.IsSpo2Ok(spo2));
    }

    [Fact]
    public void CheckVitals_AllNormal_Adult_ShouldReturnAllNormal()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 72, 95, 25);
        
        Assert.True(result.IsAllNormal);
        Assert.Equal(25, result.Age);
        Assert.Equal("Adult (15+ years)", result.AgeGroup);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.All(result.VitalSigns, vital => Assert.True(vital.IsInRange));
        Assert.Empty(result.CriticalVitals);
    }

    [Fact]
    public void CheckVitals_AllNormal_Newborn_ShouldReturnAllNormal()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 130, 95, 0);
        
        Assert.True(result.IsAllNormal);
        Assert.Equal(0, result.Age);
        Assert.Equal("Newborn (0-12 months)", result.AgeGroup);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.All(result.VitalSigns, vital => Assert.True(vital.IsInRange));
        Assert.Empty(result.CriticalVitals);
    }

    [Fact]
    public void CheckVitals_HighPulseRate_DifferentByAge()
    {
        // 120 bpm for different ages
        var adultResult = VitalSignValidator.CheckVitals(98.6f, 120, 95, 25);
        var childResult = VitalSignValidator.CheckVitals(98.6f, 120, 95, 4);
        
        Assert.False(adultResult.IsAllNormal); // 120 is high for adult (max 100)
        Assert.True(childResult.IsAllNormal);  // 120 is normal for child 3-5 years (max 120)
    }

    [Fact]
    public void CheckVitals_SomeOutOfRange_ShouldReturnCritical()
    {
        var result = VitalSignValidator.CheckVitals(103f, 55, 85, 25);
        
        Assert.False(result.IsAllNormal);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.Equal(3, result.CriticalVitals.Count); // All three are out of range for adult
    }

    [Fact]
    public void CheckVitals_BoundaryValues_ShouldValidateCorrectly()
    {
        // Test boundary conditions for adult
        var result1 = VitalSignValidator.CheckVitals(95f, 60, 90, 25);
        Assert.True(result1.IsAllNormal);

        var result2 = VitalSignValidator.CheckVitals(102f, 100, 100, 25);
        Assert.True(result2.IsAllNormal);
        
        // Test boundary conditions for newborn
        var result3 = VitalSignValidator.CheckVitals(95f, 100, 90, 0);
        Assert.True(result3.IsAllNormal);

        var result4 = VitalSignValidator.CheckVitals(102f, 160, 100, 0);
        Assert.True(result4.IsAllNormal);
    }

    [Fact]
    public void BackwardCompatibility_CheckVitals_ShouldDefaultToAdult()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 72, 95);
        
        Assert.True(result.IsAllNormal);
        Assert.Equal(25, result.Age); // Should default to adult age
        Assert.Equal("Adult (15+ years)", result.AgeGroup);
    }
}

public class AgeLimitsTests
{
    [Theory]
    [InlineData(0, 100, 160)]   // Newborn
    [InlineData(2, 80, 130)]    // Child 1-3 years
    [InlineData(4, 80, 120)]    // Child 3-5 years
    [InlineData(8, 70, 110)]    // Child 6-10 years
    [InlineData(13, 60, 105)]   // Adolescent 11-14 years
    [InlineData(25, 60, 100)]   // Adult 15+ years
    [InlineData(65, 60, 100)]   // Adult 15+ years
    public void GetPulseRateLimits_ShouldReturnCorrectRangesForAge(int age, int expectedMin, int expectedMax)
    {
        var limits = VitalSignValidator.AgeLimits.GetPulseRateLimits(age);
        
        Assert.Equal(expectedMin, limits.Min);
        Assert.Equal(expectedMax, limits.Max);
    }

    [Fact]
    public void GetPulseRateLimits_InvalidAge_ShouldDefaultToAdult()
    {
        var limits = VitalSignValidator.AgeLimits.GetPulseRateLimits(-1);
        
        Assert.Equal(60, limits.Min);
        Assert.Equal(100, limits.Max);
    }
}

public class VitalSignTests
{
    [Fact]
    public void VitalSign_InRange_ShouldHaveCorrectStatus()
    {
        var vital = new VitalSign("Temperature", 98.6f, 95f, 102f);
        
        Assert.True(vital.IsInRange);
        Assert.Equal("Normal", vital.Status);
    }

    [Fact]
    public void VitalSign_OutOfRange_ShouldHaveCorrectStatus()
    {
        var vital = new VitalSign("Temperature", 103f, 95f, 102f);
        
        Assert.False(vital.IsInRange);
        Assert.Equal("Critical", vital.Status);
    }
}

public class VitalSignResultTests
{
    [Fact]
    public void VitalSignResult_ShouldCorrectlyIdentifyCriticalVitals()
    {
        var vitals = new List<VitalSign>
        {
            new("Temperature", 98.6f, 95f, 102f),    // Normal
            new("Pulse Rate", 55, 60, 100),          // Critical for adult
            new("SpO2", 95, 90, 100)                 // Normal
        };

        var result = new VitalSignResult(false, vitals, 25);
        
        Assert.False(result.IsAllNormal);
        Assert.Single(result.CriticalVitals);
        Assert.Equal("Pulse Rate", result.CriticalVitals.First().Name);
        Assert.Equal("Adult (15+ years)", result.AgeGroup);
    }

    [Theory]
    [InlineData(0, "Newborn (0-12 months)")]
    [InlineData(2, "Child (1-3 years)")]
    [InlineData(4, "Child (3-5 years)")]
    [InlineData(8, "Child (6-10 years)")]
    [InlineData(13, "Adolescent (11-14 years)")]
    [InlineData(25, "Adult (15+ years)")]
    [InlineData(65, "Adult (15+ years)")]
    public void VitalSignResult_ShouldCorrectlyIdentifyAgeGroup(int age, string expectedAgeGroup)
    {
        var vitals = new List<VitalSign>();
        var result = new VitalSignResult(true, vitals, age);
        
        Assert.Equal(expectedAgeGroup, result.AgeGroup);
    }
}