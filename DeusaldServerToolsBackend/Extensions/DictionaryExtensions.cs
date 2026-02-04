using NLog;

namespace DeusaldServerToolsBackend;

public static class DictionaryExtensions
{
    public static void AddToAdditionalScopedProperties(this Dictionary<string, string> additionalScopedProperties, string key, string value)
    {
        ScopeContext.PushProperty(key, value);
        additionalScopedProperties[key] = value;
    }
    
    public static void AddToAdditionalScopedProperties(this Dictionary<string, string> additionalScopedProperties, string key, Guid value)
    {
        ScopeContext.PushProperty(key, value);
        additionalScopedProperties[key] = value.ToString();
    }

    public static void ApplyAdditionalScopedProperties(this Dictionary<string, string> additionalScopedProperties)
    {
        foreach (KeyValuePair<string, string> pair in additionalScopedProperties) ScopeContext.PushProperty(pair.Key, pair.Value);
    }
}