import os
import requests
from jwt_helper import generate_jwt_token

# Load these from environment variables
ISSUER_ID = os.getenv("APPLE_ISSUER_ID")
KEY_ID = os.getenv("APPLE_KEY_ID")
P8_PRIVATE_KEY_PATH = f"keys/AuthKey_{KEY_ID}.p8"  # The .p8 file from App Store Connect

def list_certificates(token):
    url = "https://api.appstoreconnect.apple.com/v1/certificates"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    resp = requests.get(url, headers=headers)
    resp.raise_for_status()  # raise an exception if there's a 4xx/5xx error
    return resp.json()

if __name__ == "__main__":
    # Load your .p8 private key
    with open(P8_PRIVATE_KEY_PATH, "r") as f:
        p8_key_content = f.read()

    # Generate a JWT token
    token = generate_jwt_token(ISSUER_ID, KEY_ID, p8_key_content)

    # List certificates
    response_json = list_certificates(token)

    # Print certificate IDs and names
    for certificate in response_json["data"]:
        cert_id = certificate["id"]
        cert_name = certificate["attributes"]["name"]
        print(f"Certificate ID: {cert_id}, Name: {cert_name}")
