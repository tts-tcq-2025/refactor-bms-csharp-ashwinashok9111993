using System.Collections.Generic;

// Vital sign status enumeration and warning calculations with centralized constants
public enum VitalStatus
{
    Normal,
    NearHypo,
    NearHyper,
    Critical
}

public static class WarningCalculator
{
    // Dictionary for status messages based on VitalStatus - using constants
    private static readonly Dictionary<VitalStatus, string> StatusMessageTemplates = new()
    {
        { VitalStatus.Critical, "{0} " + VitalSignConstants.Critical },
        { VitalStatus.Normal, "{0} " + VitalSignConstants.Normal }
    };
    
    // Dictionary for hypo warning messages by vital name - using constants
    private static readonly Dictionary<string, string> HypoWarningMessages = new()
    {
        { VitalSignConstants.Temperature.ToLower(), VitalSignConstants.WarningTemplates.ApproachingHypothermia },
        { VitalSignConstants.PulseRate.ToLower(), VitalSignConstants.WarningTemplates.ApproachingBradycardia },
        { VitalSignConstants.OxygenSaturation.ToLower(), VitalSignConstants.WarningTemplates.ApproachingHypoxemia }
    };
    
    // Dictionary for hyper warning messages by vital name - using constants
    private static readonly Dictionary<string, string> HyperWarningMessages = new()
    {
        { VitalSignConstants.Temperature.ToLower(), VitalSignConstants.WarningTemplates.ApproachingHyperthermia },
        { VitalSignConstants.PulseRate.ToLower(), VitalSignConstants.WarningTemplates.ApproachingTachycardia },
        { VitalSignConstants.OxygenSaturation.ToLower(), VitalSignConstants.WarningTemplates.ApproachingHyperoxemia }
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
        var upperTolerance = maxLimit * MedicalThresholds.WarningTolerance;
        var lowerTolerance = maxLimit * MedicalThresholds.WarningTolerance;

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
            : $"{vitalName} {VitalSignConstants.Normal}";
    }

    private static string GetHypoWarningMessage(string vitalName)
    {
        var lowerVitalName = vitalName.ToLower();
        return HypoWarningMessages.TryGetValue(lowerVitalName, out var message)
            ? message
            : string.Format(VitalSignConstants.WarningTemplates.ApproachingLowLimit, vitalName);
    }

    private static string GetHyperWarningMessage(string vitalName)
    {
        var lowerVitalName = vitalName.ToLower();
        return HyperWarningMessages.TryGetValue(lowerVitalName, out var message)
            ? message
            : string.Format(VitalSignConstants.WarningTemplates.ApproachingHighLimit, vitalName);
    }
}
