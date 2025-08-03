using Xunit;
using System.Linq;
using System.Collections.Generic;

public class CheckerTests
{
    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.False(Checker.VitalsOk(99f, 102, 70)); // Pulse rate and SpO2 out of range
        Assert.True(Checker.VitalsOk(98.1f, 70, 98));  // All in range
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
    [InlineData(60, true)]   // Lower boundary
    [InlineData(100, true)]  // Upper boundary
    [InlineData(80, true)]   // Normal value
    [InlineData(59, false)]  // Below range
    [InlineData(101, false)] // Above range
    public void IsPulseRateOk_ShouldValidateCorrectly(int pulseRate, bool expected)
    {
        Assert.Equal(expected, VitalSignValidator.IsPulseRateOk(pulseRate));
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
    public void CheckVitals_AllNormal_ShouldReturnAllNormal()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 72, 95);
        
        Assert.True(result.IsAllNormal);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.All(result.VitalSigns, vital => Assert.True(vital.IsInRange));
        Assert.Empty(result.CriticalVitals);
    }

    [Fact]
    public void CheckVitals_SomeOutOfRange_ShouldReturnCritical()
    {
        var result = VitalSignValidator.CheckVitals(103f, 55, 85);
        
        Assert.False(result.IsAllNormal);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.Equal(3, result.CriticalVitals.Count); // All three are out of range
    }

    [Fact]
    public void CheckVitals_OnlyTemperatureCritical_ShouldReturnOneCritical()
    {
        var result = VitalSignValidator.CheckVitals(94f, 70, 95);
        
        Assert.False(result.IsAllNormal);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.Single(result.CriticalVitals);
        Assert.Equal("Temperature", result.CriticalVitals.First().Name);
    }

    [Fact]
    public void CheckVitals_BoundaryValues_ShouldValidateCorrectly()
    {
        // Test all boundary conditions
        var result1 = VitalSignValidator.CheckVitals(95f, 60, 90);
        Assert.True(result1.IsAllNormal);

        var result2 = VitalSignValidator.CheckVitals(102f, 100, 100);
        Assert.True(result2.IsAllNormal);
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
            new("Pulse Rate", 55, 60, 100),          // Critical
            new("SpO2", 95, 90, 100)                 // Normal
        };

        var result = new VitalSignResult(false, vitals);
        
        Assert.False(result.IsAllNormal);
        Assert.Single(result.CriticalVitals);
        Assert.Equal("Pulse Rate", result.CriticalVitals.First().Name);
    }
}