namespace BlogMaui;
internal partial class Settings
{
    // Create a file named Settings.Local.cs
    // Set the properties in the Settings constructor
    // Do not check Settings.Local.cs into source control

    // For example:
    //
    // public Settings()
    // {
    //     ReplicatorUrl = "https://repdev.jinaga.com/xxxXXXXxxxyyyyYYYyyy";
    // }
    public string ReplicatorUrl { get; } = "https://repdev.jinaga.com/yourreplicator";
}
