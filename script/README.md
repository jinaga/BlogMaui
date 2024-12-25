# Automating Apple Provisioning

This notebook is a step-by-step guide to automate the Apple provisioning process using Python.
The provisioning process is a necessary step to deploy iOS applications on real devices.
The process involves creating a certificate, an App ID, and a provisioning profile.

The provisioning process is a bit cumbersome and involves multiple steps.
This notebook will guide you through the process and automate it using Python.

## Prerequisites

### Obtain Apple credentials

You will need the following credentials from your Apple Developer account:
- Issuer ID
- Key ID
- The .p8 private key file (AuthKey_XXXXXXXXXX.p8)

You can find these in the App Store Connect under [Users and Access, Integrations, App Store Connect API](https://appstoreconnect.apple.com/access/integrations/api).
The issuer ID looks like a GUID.
The key ID is a 10-character string with numbers and letters.
Store the .p8 file in the `keys` directory.
Set environment variables for the Issuer ID and Key ID:

```bash
export APPLE_ISSUER_ID="XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
export APPLE_KEY_ID="XXXXXXXXXX"
```

If you write these export statements to a file in the `keys` folder named `.env`, you can load them into your environment by running:

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

## Generate your private key and CSR

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

## Create a distribution certificate

Run the Python script `apple-distribution-certificate.py` to upload the CSR to Apple and create a distribution certificate.

```bash
python apple-distribution-certificate.py
```

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
openssl pkcs12 -export -out keys/distribution.p12 -inkey keys/ios-dev.key -in keys/distribution.pem -passout pass:$DISTRIBUTION_P12_PASSWORD
```

## Configuring the GitHub Action Workflow

Edit the secrets in your GitHub organization settings.
Set `DISRIBUTION_P12_PASSWORD` to the password you generated to protect the p12 file.
Set `APPSTORE_ISSUER_ID` to the issuer ID (GUID) from the App Store Connect API key.
Set `APPSTORE_KEY_ID` to the key ID (10 character code) from the App Store Connect API key.
Set `APPSTORE_PRIVATE_KEY` to the contents of the .p8 file.

You can do that using the `gh` command line tool:

```bash
gh secret set DISRIBUTION_P12_PASSWORD --body "$DISTRIBUTION_P12_PASSWORD"
gh secret set APPSTORE_ISSUER_ID --body "$APPLE_ISSUER_ID"
gh secret set APPSTORE_KEY_ID --body "$APPLE_KEY_ID"
gh secret set APPSTORE_PRIVATE_KEY --body "$(cat keys/AuthKey_$APPLE_KEY_ID.p8)"
```

Copy the `distribution.p12` file to the root of the repository and commit it.

```bash
cp keys/distribution.p12 ..
```