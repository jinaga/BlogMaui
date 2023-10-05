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
            AuthUrl = "...";
            ...
        }
    }
    */

    public string ReplicatorUrl { get; } = "https://repdev.jinaga.com/yourreplicator";
    public string AuthUrl { get; } = "https://repdev.jinaga.com/yourreplicator/auth/apple";
    public string AccessTokenUrl { get; } = "https://repdev.jinaga.com/yourreplicator/auth/token";
    public string ClientId { get; } = "yourclientid";
    public string Scope { get; } = "profile read write";
    public string CallbackUrl { get; } = "blogmaui://callback";

    public void Verify()
    {
        if (ReplicatorUrl == "https://repdev.jinaga.com/yourreplicator")
        {
            throw new Exception("Please override settings in your own Settings.Local.cs");
        }
    }
}
