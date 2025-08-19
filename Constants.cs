// Centralized constants to eliminate hardcoded strings and improve maintainability
public static class VitalSignConstants
{
    // Vital sign names - centralized to avoid duplication
    public const string Temperature = nameof(Temperature);
    public const string PulseRate = "Pulse Rate";
    public const string OxygenSaturation = "Oxygen Saturation";
    
    // Status messages
    public const string Critical = "critical!";
    public const string Normal = "normal";
    public const string VitalsNormalRange = "Vitals received within normal range";
    public const string PatientAge = "Patient Age";
    public const string Years = "years";
    public const string NormalRange = "Normal range";
    public const string Value = "Value";
    
    // Age group names
    public static class AgeGroups
    {
        public const string Newborn = "Newborn (0-12 months)";
        public const string Child1To3 = "Child (1-3 years)";
        public const string Child3To5 = "Child (3-5 years)";
        public const string Child6To10 = "Child (6-10 years)";
        public const string Adolescent = "Adolescent (11-14 years)";
        public const string Adult = "Adult (15+ years)";
    }
    
    // Warning message templates
    public static class WarningTemplates
    {
        public const string ApproachingHypothermia = "Warning: Approaching hypothermia";
        public const string ApproachingHyperthermia = "Warning: Approaching hyperthermia";
        public const string ApproachingBradycardia = "Warning: Approaching bradycardia";
        public const string ApproachingTachycardia = "Warning: Approaching tachycardia";
        public const string ApproachingHypoxemia = "Warning: Approaching hypoxemia";
        public const string ApproachingHyperoxemia = "Warning: Approaching hyperoxemia";
        public const string ApproachingLowLimit = "Warning: {0} approaching low limit";
        public const string ApproachingHighLimit = "Warning: {0} approaching high limit";
    }
}

// Medical thresholds and limits
public static class MedicalThresholds
{
    public const float WarningTolerance = 0.015f; // 1.5%
    
    // Temperature conversion constants
    public const float CelsiusToFahrenheitMultiplier = 9f / 5f;
    public const float CelsiusToFahrenheitOffset = 32f;
    
    // Default adult age for backward compatibility
    public const int DefaultAdultAge = 25;
}
