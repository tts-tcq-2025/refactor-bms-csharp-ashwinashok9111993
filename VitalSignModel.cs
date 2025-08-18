using System.Collections.Generic;
using System.Linq;

// Delegates for age classification and pulse rate limits
public delegate string AgeClassifier(int age);
public delegate (int Min, int Max) PulseRateLimitProvider(int age);
public delegate void OutputWriter(string message);

// Pure data structures for vital signs with status support
public record VitalSign(string Name, float Value, float MinLimit, float MaxLimit)
{
    public bool IsInRange => Value >= MinLimit && Value <= MaxLimit;
    public VitalStatus Status => WarningCalculator.GetVitalStatus(Value, MinLimit, MaxLimit);
    public string StatusMessage => WarningCalculator.GetStatusMessage(Name, Status);
    public string LocalizedName => LanguageProvider.Translate(Name);
    public string LocalizedStatusMessage => LanguageProvider.Translate(StatusMessage);
}

public record VitalSignResult(bool IsAllNormal, List<VitalSign> VitalSigns, int Age)
{
    public List<VitalSign> CriticalVitals => VitalSigns.Where(v => !v.IsInRange).ToList();
    public List<VitalSign> WarningVitals => VitalSigns.Where(v => v.Status == VitalStatus.NearHypo || v.Status == VitalStatus.NearHyper).ToList();
    public string AgeGroup { get; init; } = "";
    public string LocalizedAgeGroup => LanguageProvider.Translate(AgeGroup);
}
