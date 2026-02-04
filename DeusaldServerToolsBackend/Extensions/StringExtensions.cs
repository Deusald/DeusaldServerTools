namespace DeusaldServerToolsBackend;

public static class StringExtensions
{
    public static string? GetEnvironmentVariable(this string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName);
    }
}