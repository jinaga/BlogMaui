using Jinaga;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BlogMaui;
internal static class JinagaConfig
{
    public static JinagaClient j = JinagaClient.Create(opt =>
    {
        var settings = new Settings();
        opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
    });
}
