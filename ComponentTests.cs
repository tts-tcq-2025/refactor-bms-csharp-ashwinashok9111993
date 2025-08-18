using Xunit;
using System.Linq;

public class ComponentTests
{
    // VitalSign component tests
    [Fact]
    public void VitalSign_InRange_ShouldHaveCorrectStatus()
    {
        var vital = new VitalSign("Temperature", 98.6f, 95f, 102f);
        
        Assert.True(vital.IsInRange);
        Assert.Equal(VitalStatus.Normal, vital.Status);
    }

    [Fact]
    public void VitalSign_OutOfRange_ShouldHaveCorrectStatus()
    {
        var vital = new VitalSign("Temperature", 103f, 95f, 102f);
        
        Assert.False(vital.IsInRange);
        Assert.Equal(VitalStatus.Critical, vital.Status);
    }

    // VitalSignResult component tests
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

    // Age limits and dependency injection tests
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

    [Fact]
    public void IsPulseRateOk_WithCustomProvider_ShouldUseProvidedLimits()
    {
        // Custom provider that allows wider range for testing
        PulseRateLimitProvider customProvider = age => (50, 200);
        
        // This would normally be out of range for adults, but should pass with custom provider
        Assert.True(VitalSignValidator.IsPulseRateOk(150, 25, customProvider));
        Assert.False(VitalSignValidator.IsPulseRateOk(40, 25, customProvider)); // Below custom range
    }
}
