import os
import requests
from jwt_helper import generate_jwt_token

# Load these from environment variables
ISSUER_ID = os.getenv("APPLE_ISSUER_ID")
KEY_ID = os.getenv("APPLE_KEY_ID")
P8_PRIVATE_KEY_PATH = f"keys/AuthKey_{KEY_ID}.p8"  # The .p8 file from App Store Connect

def list_app_ids(token):
    url = "https://api.appstoreconnect.apple.com/v1/bundleIds"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    resp = requests.get(url, headers=headers)
    resp.raise_for_status()  # Raise on 4xx/5xx
    return resp.json()

if __name__ == "__main__":
    # Read your .p8 private key
    with open(P8_PRIVATE_KEY_PATH, "r") as f:
        p8_key_contents = f.read()

    # Generate a JWT
    token = generate_jwt_token(ISSUER_ID, KEY_ID, p8_key_contents)

    result = list_app_ids(token)
    for app_id in result["data"]:
        print(f"App ID: {app_id['id']}, Name: {app_id['attributes']['name']}, Identifier: {app_id['attributes']['identifier']}")