// Temperature unit support with conversion
public enum TemperatureUnit
{
    Fahrenheit,
    Celsius
}

public record TemperatureReading(float Value, TemperatureUnit Unit)
{
    public float ToFahrenheit() => Unit == TemperatureUnit.Fahrenheit 
        ? Value 
        : (Value * 9f / 5f) + 32f;
}

public static class UnitConverter
{
    public static float ConvertTemperatureToFahrenheit(float value, TemperatureUnit unit) =>
        unit == TemperatureUnit.Fahrenheit ? value : (value * 9f / 5f) + 32f;
    
    public static TemperatureReading CreateTemperatureReading(float value, TemperatureUnit unit = TemperatureUnit.Fahrenheit) =>
        new(value, unit);
}
