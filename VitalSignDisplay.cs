using System;
using System.Collections.Generic;
using System.Linq;

// I/O operations separated from pure functions with injectable output writer
public static class VitalSignDisplay
{
    private static readonly OutputWriter DefaultOutputWriter = Console.WriteLine;

    public static void DisplayResult(VitalSignResult result, OutputWriter? outputWriter = null)
    {
        outputWriter ??= DefaultOutputWriter;
        outputWriter($"{LanguageProvider.Translate("Patient Age")}: {result.Age} {LanguageProvider.Translate("years")} ({result.LocalizedAgeGroup})");
        
        if (result.IsAllNormal)
        {
            DisplayNormalVitals(result.VitalSigns, outputWriter);
        }
        else
        {
            DisplayCriticalVitals(result.CriticalVitals, outputWriter);
        }
        
        // Display warnings for vitals that are approaching limits
        DisplayWarnings(result.WarningVitals, outputWriter);
    }

    private static void DisplayNormalVitals(List<VitalSign> vitals, OutputWriter outputWriter)
    {
        outputWriter(LanguageProvider.Translate("Vitals received within normal range"));
        vitals.ForEach(vital => 
            outputWriter($"{vital.LocalizedName}: {vital.Value} ({LanguageProvider.Translate("Normal range")}: {vital.MinLimit}-{vital.MaxLimit})"));
    }

    private static void DisplayCriticalVitals(List<VitalSign> criticalVitals, OutputWriter outputWriter) =>
        criticalVitals.ForEach(vital => 
            DisplayCriticalAlert($"{vital.LocalizedName} {LanguageProvider.Translate("critical!")} {LanguageProvider.Translate("Value")}: {vital.Value} ({LanguageProvider.Translate("Normal range")}: {vital.MinLimit}-{vital.MaxLimit})", outputWriter));

    private static void DisplayWarnings(List<VitalSign> warningVitals, OutputWriter outputWriter) =>
        warningVitals.ForEach(vital => outputWriter(vital.LocalizedStatusMessage));

    private static void DisplayCriticalAlert(string message, OutputWriter outputWriter)
    {
        outputWriter(message);
        for (int i = 0; i < 6; i++)
        {
            outputWriter("\r* ");
            System.Threading.Thread.Sleep(1000);
            outputWriter("\r *");
            System.Threading.Thread.Sleep(1000);
        }
    }
}
