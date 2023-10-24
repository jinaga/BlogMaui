# Blog MAUI

An example Jinaga application showcasing disconnected mobile apps in MAUI.

## Developer Setup

To use this application, you will want to set up your own [Jinaga Replicator](https://jinaga.com/documents/replicator/).
You can use the replicator as a service at [dev.jinaga.com](https://dev.jinaga.com/).
This service is free.

Create a new replicator called Blog and set the environment to dev.
Put the URL into a file in the `BlogMaui` folder called `Settings.Local.cs`:

```c#
namespace BlogMaui;
partial class Settings
{
    public Settings()
    {
        ReplicatorUrl = "https://repdev.jinaga.com/xxxxxxxxxxxxxxx";
    }
}
```

Also create a file in the `Replicators` folder called `.env.local`:

```
replicatorUrl=https://repdev.jinaga.com/xxxxxxxxxxxxxxx
```

### Authentication

This application authenticates with the user's Apple credentials.
For the next step, you will need an [Apple Developer](https://developer.apple.com/) account.
This costs $99 per year.
Click "Authentication" in the action button (+) on your replicator page for a guided walkthrough.

Once you have configured authentication, add your new settings to both `Settings.Local.cs` and `.env.local`.

```c#
namespace BlogMaui;
partial class Settings
{
    public Settings()
    {
        ReplicatorUrl = "https://repdev.jinaga.com/xxxxxxxxxxxxxxx";
        AuthUrl = "https://repdev.jinaga.com/xxxxxxxxxxxxxxx/auth/apple";
        AccessTokenUrl = "https://repdev.jinaga.com/xxxxxxxxxxxxxxx/auth/token";
        ClientId = "xxxxxxxxxxxxxxx";
    }
}
```

```
replicatorUrl=https://repdev.jinaga.com/xxxxxxxxxxxxxxx

oauth2_authorizationEndpoint=https://repdev.jinaga.com/xxxxxxxxxxxxxxx/auth/apple
oauth2_tokenEndpoint=https://repdev.jinaga.com/xxxxxxxxxxxxxxx/auth/token
oauth2_clientId=xxxxxxxxxxxxxxx
oauth2_usePkce=true
```

### Authorization and Distribution

Once the user is authenticated, they must be authorized to create facts.
The application contains rules describing those facts and which users are authorized to create them.
The authorization and distribution rules appear in `BlogMaui\JinagaConfig.cs`.
You will need to deploy those rules to your replicator so that it can enforce them.

To deploy rules, install the .NET global tool [`Jinaga.Tool`](https://www.nuget.org/packages/Jinaga.Tool):

```powershell
dotnet tool install -g Jinaga.Tool
```

Then go back to your replicator and click the "Authorization" command in the action button.
Generate a secret and go back to the replicator page.
Now you can copy your authorization endpoint, distribution endpoint, and secret.
Create three environment variables:

- `JINAGA_BLOG_AUTHORIZATION_URL`
- `JINAGA_BLOG_DISTRIBUTION_URL`
- `JINAGA_BLOG_SECRET`

These environment variables are used in the script `DeployRules.ps1`.
Run this script now to deploy security rules to your replicator.

At this point, you may publish your replicator.
Click the "Publish" button to activate your endpoint.

### Create Test Data

Once your replicator is secured and published, you can initialize it with some test data.
This step is optional; you may choose to first experience the app as a brand new user.

To deploy test data, you will need to install the Visual Studio Code extension [httpYac](https://marketplace.visualstudio.com/items?itemName=anweber.vscode-httpyac).
With this extension, you can open the `http` files in the `Replicators` folder.

Open `Create post with auth.http`.
Click the "env" link at the top to selcet the "local" environment.
This maps to your `.env.local` file.
Once this is selected, you can click the "send" link to upload the data.

A browser window will open for you to log in with your Apple credentials.
You do not need to use your Apple Developer account.
This is the account that you will use to log into the application.

Now you can start the application and log in.
The test data will appear within the app.

## Architecture

### Authentication Guard

The AppState property is used in this file to control navigation based on authentication.
Specifically, it is used to determine whether the user is logged in or not, and to show or hide certain navigation elements accordingly.

In the TabBar element, the IsVisible property is bound to the AppState property using a StringEqualsConverter converter.
This means that the TabBar will only be visible if the AppState property is equal to the string value "NotLoggedIn".

Inside the TabBar, there are two Tab elements: one for the "Visitor" page and one for the "Log In" page.
The Route property of each Tab element is set to a unique string value that represents the navigation hierarchy of the application.

The ContentTemplate property of the ShellContent element inside the "Visitor" Tab is set to a DataTemplate that specifies the VisitorPage as its content.
Similarly, the ContentTemplate property of the ShellContent element inside the "Log In" Tab is set to a DataTemplate that specifies the LoginPage as its content.

When the user is not logged in (i.e., when the AppState property is equal to "NotLoggedIn"), the "Visitor" Tab will be visible and the "Log In" Tab will be hidden.
Conversely, when the user is logged in (i.e., when the AppState property is not equal to "NotLoggedIn"), the "Visitor" Tab will be hidden and the "Log In" Tab will be visible. This allows the application to control which pages are visible to the user based on their authentication status.