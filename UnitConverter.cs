// Temperature unit support with centralized conversion logic
public enum TemperatureUnit
{
    Fahrenheit,
    Celsius
}

public record TemperatureReading(float Value, TemperatureUnit Unit)
{
    public float ToFahrenheit() => TemperatureConverter.ToFahrenheit(Value, Unit);
}

// Centralized temperature conversion to eliminate duplication
public static class TemperatureConverter
{
    public static float ToFahrenheit(float value, TemperatureUnit unit) =>
        unit == TemperatureUnit.Fahrenheit 
            ? value 
            : (value * MedicalThresholds.CelsiusToFahrenheitMultiplier) + MedicalThresholds.CelsiusToFahrenheitOffset;
    
    public static float ToCelsius(float value, TemperatureUnit unit) =>
        unit == TemperatureUnit.Celsius 
            ? value 
            : (value - MedicalThresholds.CelsiusToFahrenheitOffset) / MedicalThresholds.CelsiusToFahrenheitMultiplier;
}

public static class UnitConverter
{
    public static float ConvertTemperatureToFahrenheit(float value, TemperatureUnit unit) =>
        TemperatureConverter.ToFahrenheit(value, unit);
    
    public static TemperatureReading CreateTemperatureReading(float value, TemperatureUnit unit = TemperatureUnit.Fahrenheit) =>
        new(value, unit);
}
