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
- The .p8 private key file (AuthKey.p8)

You can find these in the App Store Connect under "Users and Access" -> "Keys".

### Ensure `openssl` is installed

Make sure `openssl` is installed on your system. You can check by running:

```bash
openssl version
```

If it is not installed, you can install it using your package manager. For example, on macOS you can use:

```bash
brew install openssl
```

### Set up a Python virtual environment

It is recommended to set up a Python virtual environment to manage dependencies. You can do this by running:

```bash
python3 -m venv venv
source venv/bin/activate  # On Windows use `venv\Scripts\activate`
```

### Install required Python packages

Before running the script, make sure to install the required Python packages. You can do this by running:

```bash
pip install -r requirements.txt
```

## Generate your private key and CSR

If you haven’t already, create the private key/CSR pair locally (this key is different from the .p8 Apple gives you; this is specifically for the iOS distribution certificate).
Enter a password when prompted.
You'll need this password later.

```bash
openssl genrsa -out keys/ios-dev.key 2048
openssl req -new -key keys/ios-dev.key -out keys/ios-dev.csr \
  -subj "/C=US/ST=TX/L=Dallas/O=JinagaLLC/CN=JinagaLLC"
```

You'll upload this CSR to Apple via the script below.

## Create a distribution certificate

Run the Python script to upload the CSR to Apple and create a distribution certificate.