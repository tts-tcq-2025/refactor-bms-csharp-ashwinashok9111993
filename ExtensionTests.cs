using Xunit;
using System.Linq;
using System.Collections.Generic;

public class ExtensionTests
{
    // Extension 1: Early Warning Tests
    [Fact]
    public void EarlyWarning_ShouldDetectApproachingHypothermia()
    {
        var result = VitalSignValidator.CheckVitals(95.5f, 70, 98, 25); // Just above hypothermia threshold
        var tempVital = result.VitalSigns.First(v => v.Name == "Temperature");
        
        Assert.Equal(VitalStatus.NearHypo, tempVital.Status);
        Assert.Contains("hypothermia", tempVital.StatusMessage.ToLower());
    }

    [Fact]
    public void EarlyWarning_ShouldDetectApproachingHyperthermia()
    {
        var result = VitalSignValidator.CheckVitals(101.5f, 70, 98, 25); // Just below hyperthermia threshold
        var tempVital = result.VitalSigns.First(v => v.Name == "Temperature");
        
        Assert.Equal(VitalStatus.NearHyper, tempVital.Status);
        Assert.Contains("hyperthermia", tempVital.StatusMessage.ToLower());
    }

    [Fact]
    public void EarlyWarning_ShouldDetectNormalRange()
    {
        var result = VitalSignValidator.CheckVitals(98.6f, 70, 98, 25);
        var tempVital = result.VitalSigns.First(v => v.Name == "Temperature");
        
        Assert.Equal(VitalStatus.Normal, tempVital.Status);
    }

    [Fact]
    public void WarningCalculator_ShouldCalculateCorrectTolerances()
    {
        var status1 = WarningCalculator.GetVitalStatus(95.5f, 95f, 102f); // Lower warning zone
        var status2 = WarningCalculator.GetVitalStatus(101.5f, 95f, 102f); // Upper warning zone
        var status3 = WarningCalculator.GetVitalStatus(98f, 95f, 102f); // Normal
        var status4 = WarningCalculator.GetVitalStatus(93f, 95f, 102f); // Critical low
        
        Assert.Equal(VitalStatus.NearHypo, status1);
        Assert.Equal(VitalStatus.NearHyper, status2);
        Assert.Equal(VitalStatus.Normal, status3);
        Assert.Equal(VitalStatus.Critical, status4);
    }

    // Extension 2: Multi-language Tests
    [Fact]
    public void MultiLanguage_ShouldTranslateToGerman()
    {
        LanguageProvider.CurrentLanguage = Language.German;
        
        var (capturedOutput, mockWriter) = TestHelpers.CreateMockOutputWriter();
        VitalSignChecker.VitalsOk(98.6f, 70, 98, 25, mockWriter);

        Assert.Contains(capturedOutput, msg => msg.Contains("Patientenalter"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Jahre"));
        Assert.Contains(capturedOutput, msg => msg.Contains("Erwachsener"));
        
        // Reset to English
        LanguageProvider.CurrentLanguage = Language.English;
    }

    [Fact]
    public void MultiLanguage_ShouldTranslateWarningsToGerman()
    {
        LanguageProvider.CurrentLanguage = Language.German;
        
        var result = VitalSignValidator.CheckVitals(101.5f, 70, 98, 25);
        var tempVital = result.VitalSigns.First(v => v.Name == "Temperature");
        
        Assert.Contains("Warnung", tempVital.LocalizedStatusMessage);
        Assert.Contains("Überhitzung", tempVital.LocalizedStatusMessage);
        
        LanguageProvider.CurrentLanguage = Language.English;
    }

    // Extension 3: Temperature Units Tests
    [Fact]
    public void TemperatureUnits_ShouldConvertCelsiusToFahrenheit()
    {
        var celsiusTemp = 37f; // 37°C = 98.6°F
        var result = VitalSignValidator.CheckVitals(celsiusTemp, 70, 98, 25, TemperatureUnit.Celsius);
        var tempVital = result.VitalSigns.First(v => v.Name == "Temperature");
        
        Assert.Equal(98.6f, tempVital.Value, 1); // Allow 1 degree tolerance for rounding
        Assert.True(result.IsAllNormal);
    }

    [Fact]
    public void TemperatureUnits_CelsiusHypothermiaDetection()
    {
        var celsiusTemp = 34f; // 34°C = 93.2°F (hypothermic)
        var result = VitalSignValidator.CheckVitals(celsiusTemp, 70, 98, 25, TemperatureUnit.Celsius);
        
        Assert.False(result.IsAllNormal);
    }

    // Integration Tests - All Extensions Together
    [Fact]
    public void AllExtensions_IntegrationTest()
    {
        LanguageProvider.CurrentLanguage = Language.German;
        var (capturedOutput, mockWriter) = TestHelpers.CreateMockOutputWriter();
        
        // Test temperature in Celsius that should trigger a warning
        var result = VitalSignChecker.VitalsOk(38.5f, 70, 98, 25, mockWriter, TemperatureUnit.Celsius); // 38.5°C = 101.3°F
        
        Assert.True(result); // Should be ok but with warnings
        Assert.Contains(capturedOutput, msg => msg.Contains("Patientenalter"));
        
        LanguageProvider.CurrentLanguage = Language.English;
    }
}
