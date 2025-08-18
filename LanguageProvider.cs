using System.Collections.Generic;

// Multi-language support with extensible design
public enum Language
{
    English,
    German
}

public static class LanguageProvider
{
    public static Language CurrentLanguage { get; set; } = Language.English;
    
    private static readonly Dictionary<(Language, string), string> Translations = new()
    {
        // Age Groups
        { (Language.German, "Newborn (0-12 months)"), "Neugeborenes (0-12 Monate)" },
        { (Language.German, "Child (1-3 years)"), "Kind (1-3 Jahre)" },
        { (Language.German, "Child (3-5 years)"), "Kind (3-5 Jahre)" },
        { (Language.German, "Child (6-10 years)"), "Kind (6-10 Jahre)" },
        { (Language.German, "Adolescent (11-14 years)"), "Jugendlicher (11-14 Jahre)" },
        { (Language.German, "Adult (15+ years)"), "Erwachsener (15+ Jahre)" },
        
        // Status Messages
        { (Language.German, "Vitals received within normal range"), "Vitalwerte im normalen Bereich empfangen" },
        { (Language.German, "Patient Age"), "Patientenalter" },
        { (Language.German, "years"), "Jahre" },
        { (Language.German, "Normal range"), "Normaler Bereich" },
        { (Language.German, "Value"), "Wert" },
        
        // Warning Messages
        { (Language.German, "Warning: Approaching hypothermia"), "Warnung: Unterkühlung droht" },
        { (Language.German, "Warning: Approaching hyperthermia"), "Warnung: Überhitzung droht" },
        { (Language.German, "Warning: Approaching bradycardia"), "Warnung: Bradykardie droht" },
        { (Language.German, "Warning: Approaching tachycardia"), "Warnung: Tachykardie droht" },
        { (Language.German, "Warning: Approaching hypoxemia"), "Warnung: Hypoxämie droht" },
        { (Language.German, "Warning: Approaching hyperoxemia"), "Warnung: Hyperoxämie droht" },
        
        // Critical Messages
        { (Language.German, "critical!"), "kritisch!" },
        { (Language.German, "normal"), "normal" },
        
        // Vital Names
        { (Language.German, "Temperature"), "Temperatur" },
        { (Language.German, "Pulse Rate"), "Pulsfrequenz" },
        { (Language.German, "Oxygen Saturation"), "Sauerstoffsättigung" }
    };

    public static string Translate(string text) =>
        CurrentLanguage == Language.English || !Translations.ContainsKey((CurrentLanguage, text))
            ? text
            : Translations[(CurrentLanguage, text)];
}
