namespace CaterinGO.Configuration;

public sealed class LanguageState
{
    public string Current { get; private set; } = "pl";

    public event Action? Changed;

    public void Set(string language)
    {
        var normalized = language.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "pl";
        if (Current == normalized)
        {
            return;
        }

        Current = normalized;
        Changed?.Invoke();
    }
}
