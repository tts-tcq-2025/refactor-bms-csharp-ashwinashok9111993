using Xunit;
using System.Linq;
using System.Collections.Generic;

public class CheckerTests
{
    // Helper method to create mock output writer and capture output
    private static (List<string> capturedOutput, OutputWriter mockWriter) CreateMockOutputWriter()
    {
        var capturedOutput = new List<string>();
        OutputWriter mockWriter = message => capturedOutput.Add(message);
        return (capturedOutput, mockWriter);
    }

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

    [Fact]
    public void CheckerWithMockOutput_ShouldCaptureOutput()
    {
        var (capturedOutput, mockWriter) = CreateMockOutputWriter();

        var result = Checker.VitalsOk(98.6f, 70, 95, 25, mockWriter);

        Assert.True(result);
        Assert.Contains(capturedOutput, msg => msg.Contains("Patient Age: 25 years"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Adult (15+ years)"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Vitals received within normal range"));
    }

    [Fact]
    public void CheckerWithCriticalVitals_ShouldShowAlerts()
    {
        var (capturedOutput, mockWriter) = CreateMockOutputWriter();

        var result = Checker.VitalsOk(103f, 120, 85, 25, mockWriter);

        Assert.False(result);
        Assert.Contains(capturedOutput, msg => msg.Contains("Temperature critical!"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Pulse Rate critical!"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Oxygen Saturation critical!"));
    }
}

public class VitalSignValidatorTests
{
    // Helper method to create mock output writer and capture output
    private static (List<string> capturedOutput, OutputWriter mockWriter) CreateMockOutputWriter()
    {
        var capturedOutput = new List<string>();
        OutputWriter mockWriter = message => capturedOutput.Add(message);
        return (capturedOutput, mockWriter);
    }

    // Helper method to reduce test duplication
    private static void AssertAllVitalsNormal(VitalSignResult result, int expectedAge, string expectedAgeGroup)
    {
        Assert.True(result.IsAllNormal);
        Assert.Equal(expectedAge, result.Age);
        Assert.Equal(expectedAgeGroup, result.AgeGroup);
        Assert.Equal(3, result.VitalSigns.Count);
        Assert.All(result.VitalSigns, vital => Assert.True(vital.IsInRange));
        Assert.Empty(result.CriticalVitals);
    }

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
        AssertAllVitalsNormal(result, 25, "Adult (15+ years)");
    }

    [Fact]
    public void CheckVitals_AllNormal_Newborn_ShouldReturnAllNormal()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 130, 95, 0);
        AssertAllVitalsNormal(result, 0, "Newborn (0-12 months)");
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

    [Fact]
    public void IsPulseRateOk_WithCustomProvider_ShouldUseProvidedLimits()
    {
        // Custom provider that allows wider range for testing
        PulseRateLimitProvider customProvider = age => (50, 200);
        
        // This would normally be out of range for adults, but should pass with custom provider
        Assert.True(VitalSignValidator.IsPulseRateOk(150, 25, customProvider));
        Assert.False(VitalSignValidator.IsPulseRateOk(40, 25, customProvider)); // Below custom range
    }

    [Fact]
    public void VitalSignDisplay_WithMockOutput_ShouldCaptureAllMessages()
    {
        var (capturedOutput, mockWriter) = CreateMockOutputWriter();
        
        var result = VitalSignValidator.CheckVitals(98.6f, 90, 95, 5); // Normal values for child
        VitalSignDisplay.DisplayResult(result, mockWriter);
        
        Assert.Contains(capturedOutput, msg => msg.Contains("Patient Age: 5 years"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Child (3-5 years)"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Vitals received within normal range"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Temperature: 98.6"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Pulse Rate: 90"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Oxygen Saturation: 95"));
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
    public void CustomPulseRateProvider_ShouldWorkWithCheckVitals(int age, int expectedMin, int expectedMax)
    {
        // Custom pulse rate provider for testing
        PulseRateLimitProvider customProvider = testAge => (expectedMin, expectedMax);
        
        var result = VitalSignValidator.CheckVitals(98.6f, expectedMin + 5, 95, age, pulseRateProvider: customProvider);
        
        Assert.True(result.VitalSigns.First(v => v.Name == "Pulse Rate").IsInRange);
    }

    [Fact]
    public void CustomAgeClassifier_ShouldWorkWithCheckVitals()
    {
        AgeClassifier customClassifier = age => "Custom Age Group";
        
        var result = VitalSignValidator.CheckVitals(98.6f, 70, 95, 25, ageClassifier: customClassifier);
        
        Assert.Equal("Custom Age Group", result.AgeGroup);
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
        // Use VitalSignValidator.CheckVitals to properly create VitalSignResult with AgeGroup
        var result = VitalSignValidator.CheckVitals(98.6f, 55, 95, 25);
        
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
        // Use VitalSignValidator.CheckVitals to properly create VitalSignResult with AgeGroup
        var result = VitalSignValidator.CheckVitals(98.6f, 70, 95, age);
        
        Assert.Equal(expectedAgeGroup, result.AgeGroup);
    }
}