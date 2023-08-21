using Jinaga;

namespace BlogMaui;
internal static class JinagaConfig
{
    public static JinagaClient j = JinagaClient.Create(opt =>
    {
        var settings = new Settings();
        settings.Verify();
        opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
    });
}
