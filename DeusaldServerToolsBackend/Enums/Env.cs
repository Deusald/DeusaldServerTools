namespace DeusaldServerToolsBackend;

/// <summary> Process hosting environment </summary>
public enum Env
{
    /// <summary> Environment used by developers to test changes locally (without deployment). </summary>
    Manual,

    /// <summary> Environment used by developers to test changes locally (with deployment). </summary>
    Local,

    /// <summary> Environment used for testing changes internally. </summary>
    Development,

    /// <summary> Environment used for testing full versions before they go live. </summary>
    Staging,

    /// <summary> Live environment. </summary>
    Production
}