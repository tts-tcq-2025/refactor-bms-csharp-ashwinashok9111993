using System.Collections.Concurrent;
using System.Collections.Generic;

// Multi-language support with extensible design
public enum Language
{
    English,
    German
}

// Simple translation interface for easy extension
public interface ITranslationProvider
{
    Language Language { get; }
    Dictionary<string, string> GetTranslations();
}

// German translations organized by category
public class GermanTranslations : ITranslationProvider
{
    public Language Language => Language.German;
    
    public Dictionary<string, string> GetTranslations()
    {
        var translations = new Dictionary<string, string>();
        
        AddAgeGroupTranslations(translations);
        AddStatusTranslations(translations);
        AddWarningTranslations(translations);
        AddVitalNameTranslations(translations);
        
        return translations;
    }
    
    private static void AddAgeGroupTranslations(Dictionary<string, string> translations)
    {
        translations.Add(VitalSignConstants.AgeGroups.Newborn, "Neugeborenes (0-12 Monate)");
        translations.Add(VitalSignConstants.AgeGroups.Child1To3, "Kind (1-3 Jahre)");
        translations.Add(VitalSignConstants.AgeGroups.Child3To5, "Kind (3-5 Jahre)");
        translations.Add(VitalSignConstants.AgeGroups.Child6To10, "Kind (6-10 Jahre)");
        translations.Add(VitalSignConstants.AgeGroups.Adolescent, "Jugendlicher (11-14 Jahre)");
        translations.Add(VitalSignConstants.AgeGroups.Adult, "Erwachsener (15+ Jahre)");
    }
    
    private static void AddStatusTranslations(Dictionary<string, string> translations)
    {
        translations.Add(VitalSignConstants.VitalsNormalRange, "Vitalwerte im normalen Bereich empfangen");
        translations.Add(VitalSignConstants.PatientAge, "Patientenalter");
        translations.Add(VitalSignConstants.Years, "Jahre");
        translations.Add(VitalSignConstants.NormalRange, "Normaler Bereich");
        translations.Add(VitalSignConstants.Value, "Wert");
        translations.Add(VitalSignConstants.Critical, "kritisch!");
        translations.Add(VitalSignConstants.Normal, "normal");
    }
    
    private static void AddWarningTranslations(Dictionary<string, string> translations)
    {
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingHypothermia, "Warnung: Unterkühlung droht");
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingHyperthermia, "Warnung: Überhitzung droht");
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingBradycardia, "Warnung: Bradykardie droht");
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingTachycardia, "Warnung: Tachykardie droht");
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingHypoxemia, "Warnung: Hypoxämie droht");
        translations.Add(VitalSignConstants.WarningTemplates.ApproachingHyperoxemia, "Warnung: Hyperoxämie droht");
    }
    
    private static void AddVitalNameTranslations(Dictionary<string, string> translations)
    {
        translations.Add(VitalSignConstants.Temperature, "Temperatur");
        translations.Add(VitalSignConstants.PulseRate, "Pulsfrequenz");
        translations.Add(VitalSignConstants.OxygenSaturation, "Sauerstoffsättigung");
    }
}

// Main language provider with simple, extensible design
public static class LanguageProvider
{
    public static Language CurrentLanguage { get; set; } = Language.English;
    
    private static readonly Dictionary<Language, ITranslationProvider> Providers = new()
    {
        { Language.German, new GermanTranslations() }
    };
    
    private static readonly ConcurrentDictionary<Language, Dictionary<string, string>> TranslationCache = new();
    
    public static string Translate(string text)
    {
        if (CurrentLanguage == Language.English)
            return text;
            
        var translations = GetTranslationsForLanguage(CurrentLanguage);
        return translations.TryGetValue(text, out var translation) ? translation : text;
    }
    
    private static Dictionary<string, string> GetTranslationsForLanguage(Language language)
    {
        if (TranslationCache.TryGetValue(language, out var cached))
            return cached;
            
        if (!Providers.TryGetValue(language, out var provider))
            return new Dictionary<string, string>();
            
        var translations = provider.GetTranslations();
        TranslationCache[language] = translations;
        return translations;
    }
    
    // Extension point for adding new languages
    public static void AddLanguageProvider(ITranslationProvider provider)
    {
        Providers[provider.Language] = provider;
        TranslationCache.TryRemove(provider.Language, out _); // Clear cache to reload
    }
}
