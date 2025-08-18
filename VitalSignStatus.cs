using System.Collections.Generic;

// Vital sign status enumeration and warning calculations
public enum VitalStatus
{
    Normal,
    NearHypo,
    NearHyper,
    Critical
}

public static class WarningCalculator
{
    private const float WarningTolerance = 0.015f; // 1.5%
    
    // Dictionary for status messages based on VitalStatus
    private static readonly Dictionary<VitalStatus, string> StatusMessageTemplates = new()
    {
        { VitalStatus.Critical, "{0} critical!" },
        { VitalStatus.Normal, "{0} normal" }
    };
    
    // Dictionary for hypo warning messages by vital name
    private static readonly Dictionary<string, string> HypoWarningMessages = new()
    {
        { "temperature", "Warning: Approaching hypothermia" },
        { "pulse rate", "Warning: Approaching bradycardia" },
        { "oxygen saturation", "Warning: Approaching hypoxemia" }
    };
    
    // Dictionary for hyper warning messages by vital name
    private static readonly Dictionary<string, string> HyperWarningMessages = new()
    {
        { "temperature", "Warning: Approaching hyperthermia" },
        { "pulse rate", "Warning: Approaching tachycardia" },
        { "oxygen saturation", "Warning: Approaching hyperoxemia" }
    };
    
    // Dictionary for status message handlers
    private static readonly Dictionary<VitalStatus, System.Func<string, string>> StatusMessageHandlers = new()
    {
        { VitalStatus.NearHypo, GetHypoWarningMessage },
        { VitalStatus.NearHyper, GetHyperWarningMessage },
        { VitalStatus.Critical, vitalName => string.Format(StatusMessageTemplates[VitalStatus.Critical], vitalName) },
        { VitalStatus.Normal, vitalName => string.Format(StatusMessageTemplates[VitalStatus.Normal], vitalName) }
    };

    public static VitalStatus GetVitalStatus(float value, float minLimit, float maxLimit)
    {
        if (IsInCriticalRange(value, minLimit, maxLimit))
            return VitalStatus.Critical;

        return GetWarningStatus(value, minLimit, maxLimit);
    }

    private static bool IsInCriticalRange(float value, float minLimit, float maxLimit) =>
        value < minLimit || value > maxLimit;

    private static VitalStatus GetWarningStatus(float value, float minLimit, float maxLimit)
    {
        var upperTolerance = maxLimit * WarningTolerance;
        var lowerTolerance = maxLimit * WarningTolerance;

        if (IsInLowerWarningZone(value, minLimit, lowerTolerance))
            return VitalStatus.NearHypo;

        if (IsInUpperWarningZone(value, maxLimit, upperTolerance))
            return VitalStatus.NearHyper;

        return VitalStatus.Normal;
    }

    private static bool IsInLowerWarningZone(float value, float minLimit, float tolerance) =>
        value >= minLimit && value <= minLimit + tolerance;

    private static bool IsInUpperWarningZone(float value, float maxLimit, float tolerance) =>
        value >= maxLimit - tolerance && value <= maxLimit;

    public static string GetStatusMessage(string vitalName, VitalStatus status)
    {
        return StatusMessageHandlers.TryGetValue(status, out var handler)
            ? handler(vitalName)
            : $"{vitalName} normal";
    }

    private static string GetHypoWarningMessage(string vitalName)
    {
        var lowerVitalName = vitalName.ToLower();
        return HypoWarningMessages.TryGetValue(lowerVitalName, out var message)
            ? message
            : $"Warning: {vitalName} approaching low limit";
    }

    private static string GetHyperWarningMessage(string vitalName)
    {
        var lowerVitalName = vitalName.ToLower();
        return HyperWarningMessages.TryGetValue(lowerVitalName, out var message)
            ? message
            : $"Warning: {vitalName} approaching high limit";
    }
}
