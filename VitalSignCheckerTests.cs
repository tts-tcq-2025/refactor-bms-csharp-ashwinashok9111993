using Xunit;
using System.Linq;
using System.Collections.Generic;

public class VitalSignCheckerTests
{
    [Fact]
    public void VitalsOk_WhenAnyVitalIsOffRange_ShouldReturnFalse()
    {
        Assert.False(VitalSignChecker.VitalsOk(99f, 102, 70)); // Pulse rate and SpO2 out of range for adult
        Assert.True(VitalSignChecker.VitalsOk(98.1f, 70, 98));  // All in range for adult
    }

    [Fact]
    public void VitalsOk_WithAge_ShouldUseAgeSpecificRanges()
    {
        // Test newborn with high pulse rate (normal for newborn)
        Assert.True(VitalSignChecker.VitalsOk(98.6f, 140, 95, 0)); // 140 bpm is normal for newborn
        
        // Test adult with same high pulse rate (abnormal for adult)
        Assert.False(VitalSignChecker.VitalsOk(98.6f, 140, 95, 25)); // 140 bpm is high for adult
    }

    [Fact]
    public void VitalsOk_WithMockOutput_ShouldCaptureOutput()
    {
        var (capturedOutput, mockWriter) = TestHelpers.CreateMockOutputWriter();
        var result = VitalSignChecker.VitalsOk(98.6f, 70, 95, 25, mockWriter);

        Assert.True(result);
        AssertOutputContainsPatientInfo(capturedOutput);
        AssertOutputContainsVitalsInfo(capturedOutput);
    }

    [Fact]
    public void VitalsOk_WithCriticalVitals_ShouldShowAlerts()
    {
        var (capturedOutput, mockWriter) = TestHelpers.CreateMockOutputWriter();
        var result = VitalSignChecker.VitalsOk(103f, 120, 85, 25, mockWriter);

        Assert.False(result);
        AssertCriticalVitalsInOutput(capturedOutput);
    }

    [Fact]
    public void VitalsOk_WithTemperatureReading_ShouldWork()
    {
        var tempReading = UnitConverter.CreateTemperatureReading(37f, TemperatureUnit.Celsius);
        var result = VitalSignChecker.VitalsOk(tempReading, 70, 98, 25);
        
        Assert.True(result);
    }

    private static void AssertOutputContainsPatientInfo(List<string> capturedOutput)
    {
        Assert.Contains(capturedOutput, msg => msg.Contains("Patient Age: 25 years") || msg.Contains("Patientenalter: 25 Jahre"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Adult (15+ years)") || msg.Contains("Erwachsener (15+ Jahre)"));
    }

    private static void AssertOutputContainsVitalsInfo(List<string> capturedOutput)
    {
        Assert.Contains(capturedOutput, msg => msg.Contains("Vitals received within normal range") || msg.Contains("Vitalwerte im normalen Bereich"));
    }

    private static void AssertCriticalVitalsInOutput(List<string> capturedOutput)
    {
        AssertContainsTemperatureMessage(capturedOutput);
        AssertContainsPulseRateMessage(capturedOutput);
        AssertContainsOxygenSaturationMessage(capturedOutput);
    }

    private static void AssertContainsTemperatureMessage(List<string> capturedOutput) =>
        Assert.Contains(capturedOutput, msg => msg.Contains("Temperature") || msg.Contains("Temperatur"));

    private static void AssertContainsPulseRateMessage(List<string> capturedOutput) =>
        Assert.Contains(capturedOutput, msg => msg.Contains("Pulse Rate") || msg.Contains("Pulsfrequenz"));

    private static void AssertContainsOxygenSaturationMessage(List<string> capturedOutput) =>
        Assert.Contains(capturedOutput, msg => msg.Contains("Oxygen Saturation") || msg.Contains("Sauerstoffsättigung"));
}

public class VitalSignValidatorTests
{
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
    public void CheckVitals_BackwardCompatibility_ShouldDefaultToAdult()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 72, 95);
        
        Assert.True(result.IsAllNormal);
        Assert.Equal(25, result.Age); // Should default to adult age
        Assert.Equal("Adult (15+ years)", result.AgeGroup);
    }

    [Fact]
    public void VitalSignDisplay_WithMockOutput_ShouldCaptureAllMessages()
    {
        var (capturedOutput, mockWriter) = TestHelpers.CreateMockOutputWriter();
        
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
