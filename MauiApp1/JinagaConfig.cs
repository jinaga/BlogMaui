using Jinaga;

namespace MauiApp1;
internal static class JinagaConfig
{
    public static JinagaClient j = JinagaClient.Create(opt =>
    {
        opt.HttpEndpoint = new Uri("https://repdev.jinaga.com/N25EVWOs91edOIao79xosTUjEpDHF4HrxOx0GrpZtbMq3ZHqu7DyeiDmEgmhnbBLTdQCBS79OzdzOzTRLi54VQ");
    });
}
