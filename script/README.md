# Automating Apple Provisioning

This notebook is a step-by-step guide to automate the Apple provisioning process using Python.
The provisioning process is a necessary step to deploy iOS applications on real devices.
The process involves creating a certificate, an App ID, and a provisioning profile.

The provisioning process is a bit cumbersome and involves multiple steps.
This notebook will guide you through the process and automate it using Python.

## Prerequisites

### Define your app variables

These scripts will need to know about your app.
Define the following environment variables in a file named `.app` in the `keys` directory:

```bash
export APP_NAME="MyApp"
export APP_BUNDLE_ID="com.mycompany.myapp"
```

You can load them into your environment by running:

```bash
source keys/.app
```

### Obtain Apple credentials

You will need the following credentials from your Apple Developer account:
- Issuer ID
- Key ID
- The .p8 private key file (AuthKey_XXXXXXXXXX.p8)

You can find these in the App Store Connect under [Users and Access, Integrations, App Store Connect API](https://appstoreconnect.apple.com/access/integrations/api).
The issuer ID looks like a GUID.
The key ID is a 10-character string with numbers and letters.
Store the .p8 file in the `keys` directory.
Set environment variables for the Issuer ID and Key ID in a file named `.env` in the `keys` directory:

```bash
export APPLE_ISSUER_ID="XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
export APPLE_KEY_ID="XXXXXXXXXX"
```

You can load them into your environment by running:

```bash
source keys/.env
```

### Ensure `openssl` is installed

Make sure `openssl` is installed on your system. You can check by running:

```bash
openssl version
```

If it is not installed, you can install it using your package manager.
For example, on macOS you can use:

```bash
brew install openssl
```

### Set up a Python virtual environment

It is recommended to set up a Python virtual environment to manage dependencies.
You can do this by running:

```bash
python3 -m venv venv
source venv/bin/activate  # On Windows use `venv\Scripts\activate`
```

### Install required Python packages

Before running the script, make sure to install the required Python packages.
You can do this by running:

```bash
pip install -r requirements.txt
```

### Install the GitHub CLI

You will need the GitHub CLI to set secrets in your GitHub repository.
You can install it by following the instructions [here](https://cli.github.com/).

On macOS, you can install it using Homebrew:

```bash
brew install gh
```

Log in to GitHub using the CLI:

```bash
gh auth login
```

## Generate an iOS distribution certificate

You will need an iOS distribution certificate to deploy your app to TestFlight.
This certificate is different from the development certificate you use to run your app on a device during development.

### Generate your private key and CSR

Create a private key/CSR pair (this key is different from the .p8 Apple gives you; this is specifically for the iOS distribution certificate).
Enter a password when prompted.
You'll need this password later.

```bash
openssl genrsa -out keys/ios-dev.key 2048
openssl req -new -key keys/ios-dev.key -out keys/ios-dev.csr \
  -subj "/C=US/ST=Texas/L=Dallas/O=Jinaga LLC/CN=jinaga.com" \
  -passin stdin
```

You'll upload this CSR to Apple via the script below.

### Create a distribution certificate

Run the Python script `apple-distribution-certificate.py` to upload the CSR to Apple and create a distribution certificate.

```bash
python apple-distribution-certificate.py
```

This script outputs the certificate ID.
Save this ID as you will need it later.

Convert the downloaded `distribution.cer` file to a `distribution.pem` file:

```bash
openssl x509 -inform der -in keys/distribution.cer -out keys/distribution.pem
```

Generate a password to protect the p12 file:

```bash
DISTRIBUTION_P12_PASSWORD="$(openssl rand -base64 32)"
```

Combine the private key and certificate into a single `distribution.p12` file.
Use the generated password to protect the p12 file.

```bash
openssl pkcs12 -export \
  -inkey keys/ios-dev.key \
  -in keys/distribution.pem \
  -out keys/distribution.p12 \
  -descert -keypbe PBE-SHA1-3DES -certpbe PBE-SHA1-3DES \
  -macalg sha1 -iter 2048 \
  -passout pass:$DISTRIBUTION_P12_PASSWORD
```

Verify the p12 file:

```bash
openssl pkcs12 -info -in keys/distribution.p12 -passin pass:$DISTRIBUTION_P12_PASSWORD -noout
```

It should write out the following information:

```
MAC: sha1, Iteration 2048
MAC length: 20, salt length: 8
PKCS7 Encrypted data: pbeWithSHA1And3-KeyTripleDES-CBC, Iteration 2048
Certificate bag
PKCS7 Data
Shrouded Keybag: pbeWithSHA1And3-KeyTripleDES-CBC, Iteration 2048
```

### Configuring the GitHub Action Workflow

Edit the secrets in your GitHub organization settings.
Set `DISTRIBUTION_P12` to the contents of the `distribution.p12` file.
Set `DISTRIBUTION_P12_PASSWORD` to the password you generated to protect the p12 file.
Set `APPSTORE_ISSUER_ID` to the issuer ID (GUID) from the App Store Connect API key.
Set `APPSTORE_KEY_ID` to the key ID (10 character code) from the App Store Connect API key.
Set `APPSTORE_PRIVATE_KEY` to the contents of the .p8 file.

You can do that using the `gh` command line tool:

```bash
gh secret set DISTRIBUTION_P12 --body "$(base64 -i keys/distribution.p12)"
gh secret set DISTRIBUTION_P12_PASSWORD --body "$DISTRIBUTION_P12_PASSWORD"
gh secret set APPSTORE_ISSUER_ID --body "$APPLE_ISSUER_ID"
gh secret set APPSTORE_KEY_ID --body "$APPLE_KEY_ID"
gh secret set APPSTORE_PRIVATE_KEY --body "$(cat keys/AuthKey_$APPLE_KEY_ID.p8)"
```

## Create a Provisioning Profile

You will need a provisioning profile to distribute your app to TestFlight.
This profile allows your TestFlight users to install your app on their devices.

### Save the Certificate ID

You will need the certificate ID from the previous step.
If you did not collect this ID, then run the Python script `list-certificates.py` to list the certificates.

```bash
python list-certificates.py
```

Save the certificate ID to a variable:

```bash
CERTIFICATE_ID="XXXXXXXXXX"
export CERTIFICATE_ID
```

Also update the `.csproj` file with the certificate ID:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) and '$(Configuration)' == 'Release'">
    <RuntimeIdentifier>ios-arm64</RuntimeIdentifier>
    <CodesignKey>iOS Distribution: My Name (XXXXXXXXXX)</CodesignKey>
    <CodesignProvision>My App Provisioning</CodesignProvision>
    <CodesignEntitlements>Platforms\iOS\Entitlements.plist</CodesignEntitlements>
</PropertyGroup>
```

### Create a Bundle ID

Run the Python script `apple-bundle-id.py` to create a bundle ID.

```bash
python apple-bundle-id.py
```

If the script fails with a 409 error, then the bundle ID already exists.
You can list the bundle IDs to verify:

```bash
python list-bundle-ids.py
```

### Create an App ID

Run the Python script `apple-app-id.py` to create an App ID.
This script outputs the App ID.
Save this ID as you will need it later.

```bash
python apple-app-id.py
```

If the script fails with a 409 error, then the App ID already exists.
You can list the App IDs to find the one you need:

```bash
python list-app-ids.py
```

Save the App ID to a variable:

```bash
APP_ID="XXXXXXXXXX"
export APP_ID
```

### Create a Provisioning Profile

To create a provisioning profile, run the Python script `apple-provisioning-profile.py`.

```bash
python apple-provisioning-profile.py
```