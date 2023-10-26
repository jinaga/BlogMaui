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

The architecture of a Jinaga application is similar to other MAUI apps.
However, there are a few areas where Jinaga and the replicator take on what would ordinarily be application responsibilities.
These responsibilities include:

- Calling backend APIs
- Client and server storage
- Authentication and authorization

This section describes how a MAUI application uses Jinaga to accomplish these things.

### Jinaga Configuration

Configuring the Jinaga client takes place in the `JinagaConfig` class.
It contains several methods that initialize individual components.
The first method loads the configuration settings that you overrode during developer setup.

The `Settings` class is partial.
One part is checked into the source code repository, and the other part is developer-specific.
This allows each developer to provide their own replicator configuraiton.
It also allows the CI/CD pipeline to supply replicator settings prior to the test or production build.

The Jinaga configuration class uses `JinagaSQLiteClient.Create` to create a Jinaga client that uses a SQLite database for local persistence.
This database runs queries against cached data first so that both connected and offline users get a fast response.
It then tries to fetch updates from the replicator.
The local store also saves any facts that the user creates and puts them into a queue.
The user can immediately work with their own local data, and the app will send queued facts to the replicator when connected.

### Model

Jinaga facts are described using C# records.
Those records appear in the `Model.cs` file.
An application might have multiple model files, one for each functional area.

The parameters of a fact record might include other facts.
These are predecessors.
The predecessor relationship sets up a connection between two facts that can be used in a specification.

The application creates facts and calls `JinagaClient.Fact` to save them.
This not only stores them locally, but also queues them to be sent to the server and starts the upload process.

### Authorization and Distribution

Authorization rules control who is allowed to create facts.
The rules are defined in `JinagaConfig`.
The `Authorize` function can compose rules by calling authorization functions defined in different model files.

Distribution rules control who is allowed to run specifications.
These rules are also defined in `JinagaConfig`.

The replicator enforces authorization and distribution rules.
To upload the rules to the repliator, run the script described in developer setup.
Run this script again whenever you change authorization or distribution rules.

### Authentication Guard

App navigation relies upon the Shell framework component in MAUI.
It defines a hierarchy that subdivides the experience into two navigation trees: one if the user is not logged in and another if they are.

The `AppShellViewModel` contains the switch between these two states and the methods to move between them.
The `AppState` property determines whether the user is logged in or not.
The view binds to this property to show or hide certain navigation elements accordingly.

The view defines two `TabBar` elements.
They each bind to the `AppState` property.
One is visible when the state is `NotLoggedIn`, and the other when the state is `LoggedIn`.

The view also defines a gatekeeper shell component as the starting point.
The `GatekeeperViewModel` is responsible for initializing the authentication state of the application.
When the gatekeeper view loads, it calls `Initialize`.
This method in turn initializes the authentication provider and user provider, which load the access token and user fact from secure storage.
It then sets the `AppState` property and navigates to the corresponding tab bar.

### View Models and Specifications

As in other MAUI apps, view models in a Jinaga app implement `INotifyPropertyChanged` to support data binding.
This example uses Comunity Toolkit MVVM to do so.
View models inherit `ObservableObject`.
They are also partial classes so that the source generator can inject code for each `ObservableProperty`.

Jinaga view models differ in how they load data into observable properties.
Each top level view model has a `Load` method, which may take parameters.
This method defines a specification, which describes how to load facts from the local store and replicator.

The `Load` method creates the starting facts, possibly using the parameters, and then calls `JinagaClient.Watch`.
This sets up an observer that is called for each result of the specification.
Inside of the callback function, add each result to an `ObservableCollection`.

A specification may project child specifications.
Use the method `facts.Observable` to set up that child collection.
Then call `OnAdded` to handle each child projection.
It is common to set an observable property on the child object.

Each callback function can return a parameterless lambda.
This lambda is called whenever the projection is removed.
If you have added an object to an observable collection, return a lambda that removes it.

### Jinaga.Maui

The `Jinaga.Maui` folder contains code that is not specific to the application.
This code will likely be moved to a NuGet repository in the near future.
Currently, this includes authentication support.
It may later include common patterns like support with editing mutable properties.
As other features are farmed from working applications, they will be added here.