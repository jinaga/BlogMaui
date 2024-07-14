namespace BlogMaui;
public partial class Settings
{
    // Create a file named Settings.Local.cs
    // Set the properties in the Settings constructor
    // Do not check Settings.Local.cs into source control

    // For example:
    /*
    partial class Settings
    {
        public Settings()
        {
            ReplicatorUrl = "https://repdev.jinaga.com/xxxXXXXxxxyyyyYYYyyy";
            AppleAuthUrl = "...";
            ...
        }
    }
    */

    public string? ReplicatorUrl { get; }
    public string? AppleAuthUrl { get; }
    public string? GoogleAuthUrl { get; }
    public string? AccessTokenUrl { get; }
    public string? RevokeUrl { get; }
    public string? ClientId { get; }
    public string Scope { get; } = "profile read write";
    public string CallbackUrl { get; } = "blogmaui://callback";
}
