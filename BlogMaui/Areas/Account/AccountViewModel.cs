using BlogMaui.Areas.Blog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace BlogMaui.Areas.Account;
public partial class AccountViewModel : ObservableObject, ILifecycleManaged
{
    [ObservableProperty]
    public string userName = string.Empty;

    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;
    private readonly ILogger<AccountViewModel> logger;
    private readonly bool exportToShare;

    private UserProvider.Handler? handler;

    public ICommand ExportFactsCommand { get; }

    public AccountViewModel(JinagaClient jinagaClient, UserProvider userProvider, ILogger<AccountViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
        this.logger = logger;
        this.exportToShare = false;

        ExportFactsCommand = new AsyncRelayCommand(HandleExportFacts);
    }

    public void Load()
    {
        if (handler != null)
        {
            return;
        }

        handler = userProvider.AddHandler(user =>
        {
            var namesOfUser = Given<User>.Match((user, facts) =>
                from name in facts.OfType<UserName>()
                where name.user == user &&
                    !facts.Any<UserName>(next => next.prior.Contains(name))
                select name.value
            );

            var observer = jinagaClient.Watch(namesOfUser, user, projection =>
            {
                UserName = projection;
            });

            Monitor(observer);

            return () =>
            {
                observer.Stop();
                observer = null;
                UserName = string.Empty;
            };
        });
    }

    private void Monitor(IObserver observer)
    {
        observer.Loaded.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                UserName = "Failed to load user information";
            }
            else if (string.IsNullOrEmpty(UserName))
            {
                UserName = "User name unknown";
            }
        });
    }

    public void Unload()
    {
        if (handler != null)
        {
            userProvider.RemoveHandler(handler);
            handler = null;
        }
    }

    private async Task HandleExportFacts()
    {
        if (exportToShare)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                // Export facts to the temporary file
                logger.LogInformation("Exporting facts to {tempFile}", tempFile);
                using (var fileStream = File.OpenWrite(tempFile))
                {
                    await jinagaClient.ExportFactsToFactual(fileStream);
                }

                // Share the file
                logger.LogInformation("Sharing {tempFile}", tempFile);
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Exported Facts (Factual)",
                    File = new ShareFile(tempFile)
                });

                // RequestAsync does not wait for the user to complete the share operation.
                // We therefore cannot delete the temporary file here.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to export facts");

                // Clean up the temporary file if something went wrong
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                throw;
            }
        }
        else
        {
            // Export facts to a string
            using var memoryStream = new MemoryStream();
            await jinagaClient.ExportFactsToFactual(memoryStream);
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var exportedFacts = await reader.ReadToEndAsync();

            // Navigate to the ExportedFactsPage
            var exportedFactsPage = new ExportedFactsPage(exportedFacts);
            await Shell.Current.Navigation.PushModalAsync(exportedFactsPage);
        }
    }
}
