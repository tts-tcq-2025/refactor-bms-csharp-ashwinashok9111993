// Main vital sign checker class with reduced complexity and extension support
class VitalSignChecker
{
    // New age-aware method with injectable output writer and temperature unit support
    public static bool VitalsOk(float temperature, int pulseRate, int spo2, int age, 
        OutputWriter? outputWriter = null, TemperatureUnit temperatureUnit = TemperatureUnit.Fahrenheit)
    {
        var result = VitalSignValidator.CheckVitals(temperature, pulseRate, spo2, age, temperatureUnit);
        VitalSignDisplay.DisplayResult(result, outputWriter);
        return result.IsAllNormal;
    }
    
    // Temperature reading overload
    public static bool VitalsOk(TemperatureReading temperature, int pulseRate, int spo2, int age, OutputWriter? outputWriter = null)
    {
        var result = VitalSignValidator.CheckVitals(temperature, pulseRate, spo2, age);
        VitalSignDisplay.DisplayResult(result, outputWriter);
        return result.IsAllNormal;
    }
    
    // Backward compatibility method (assumes adult age)
    public static bool VitalsOk(float temperature, int pulseRate, int spo2) =>
        VitalsOk(temperature, pulseRate, spo2, 25); // Default to adult age
}
