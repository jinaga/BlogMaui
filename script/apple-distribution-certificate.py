import base64
import requests
import os
from jwt_helper import generate_jwt_token

# Load these from environment variables
ISSUER_ID = os.getenv("APPLE_ISSUER_ID")
KEY_ID = os.getenv("APPLE_KEY_ID")
P8_PRIVATE_KEY_PATH = f"keys/AuthKey_{KEY_ID}.p8"  # The .p8 file from App Store Connect
CSR_PATH = "keys/ios-dev.csr"   # The CSR you generated in step 1


def create_distribution_certificate(token, base64_csr):
    url = "https://api.appstoreconnect.apple.com/v1/certificates"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    body = {
        "data": {
            "type": "certificates",
            "attributes": {
                "certificateType": "IOS_DISTRIBUTION",
                "csrContent": base64_csr
            }
        }
    }

    try:
        resp = requests.post(url, headers=headers, json=body)
        resp.raise_for_status()  # raise exception if HTTP errors occur
    except requests.exceptions.HTTPError as e:
        if e.response.status_code == 409:
            print("Error: You have already created three distribution certificates. Please delete old certificates in your Apple Developer account.")
        else:
            raise
    return resp.json()

if __name__ == "__main__":
    # Load your .p8 private key
    with open(P8_PRIVATE_KEY_PATH, "r") as f:
        p8_key_content = f.read()

    # Generate a JWT token
    token = generate_jwt_token(ISSUER_ID, KEY_ID, p8_key_content)

    # Read and base64-encode the CSR
    with open(CSR_PATH, "rb") as f:
        csr_data = f.read()
    base64_csr = csr_data.decode("utf-8")

    # Create the certificate via Apple’s API
    response_json = create_distribution_certificate(token, base64_csr)

    # The certificate comes back as base64-encoded .cer content
    cert_content_b64 = response_json["data"]["attributes"]["certificateContent"]
    cert_data = base64.b64decode(cert_content_b64)

    # Write the .cer file
    with open("keys/distribution.cer", "wb") as f:
        f.write(cert_data)

    # The certificate ID is in the response
    cert_id = response_json["data"]["id"]
    display_name = response_json["data"]["attributes"]["displayName"]
    name = response_json["data"]["attributes"]["name"]
    print(f"Successfully created distribution certificate with ID: {cert_id}, Name: {name}, Display Name: {display_name}")
