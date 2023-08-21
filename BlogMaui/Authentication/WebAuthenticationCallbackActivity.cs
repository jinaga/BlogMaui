#if ANDROID

using Android.App;
using Android.Content.PM;

//
// For complete instructions, please see:
// https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication?tabs=android
//

namespace BlogMaui.Authentication;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Android.Content.Intent.ActionView },
              Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
              DataScheme = CALLBACK_SCHEME)]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "blogmaui";
}

#endif