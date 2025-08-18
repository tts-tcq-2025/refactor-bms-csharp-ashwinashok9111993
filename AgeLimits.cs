// Age-based vital sign limits based on medical standards
internal static class AgeLimits
{
    // Temperature limits remain consistent across all ages
    internal static readonly (float Min, float Max) Temperature = (95f, 102f);
    
    // SpO2 limits remain consistent across all ages  
    internal static readonly (int Min, int Max) Spo2 = (90, 100);
}
