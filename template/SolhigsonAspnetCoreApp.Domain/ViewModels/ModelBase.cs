namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record ModelBase
{
    public string StatusImage => GetBooleanImage(Enabled);

    public string StatusColor => GetBooleanColor(Enabled);

    public bool Enabled { get; set; }

    public string EnabledChecked => Enabled ? "1" : "0";

    public static string GetBooleanImage(bool value)
    {
        return value ? "check" : "close";
    }

    public static string GetBooleanColor(bool value)
    {
        return value ? "green" : "red";
    }
}