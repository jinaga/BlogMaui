import requests
import os
import requests
from jwt_helper import generate_jwt_token

# Load these from environment variables
ISSUER_ID = os.getenv("APPLE_ISSUER_ID")
KEY_ID = os.getenv("APPLE_KEY_ID")
P8_PRIVATE_KEY_PATH = f"keys/AuthKey_{KEY_ID}.p8"  # The .p8 file from App Store Connect
NAME = os.getenv("APP_NAME")
IDENTIFIER = os.getenv("APP_BUNDLE_ID")

# Provide the info for the provisioning profile
PROFILE_NAME = f"{NAME} Profile"
PROFILE_TYPE = "IOS_APP_STORE"  # For TestFlight or App Store distribution

def create_bundle_id(token, name, identifier, platform="IOS"):
    url = "https://api.appstoreconnect.apple.com/v1/bundleIds"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    body = {
        "data": {
            "type": "bundleIds",
            "attributes": {
                "name": name,         # A human-readable name in the developer portal
                "identifier": identifier,  # e.g. "com.example.myapp"
                "platform": platform  # "IOS" is most common
            }
        }
    }

    resp = requests.post(url, headers=headers, json=body)
    resp.raise_for_status()  # Raise on 4xx/5xx
    return resp.json()

if __name__ == "__main__":
    # Read your .p8 private key
    with open(P8_PRIVATE_KEY_PATH, "r") as f:
        p8_key_contents = f.read()

    # Generate a JWT
    token = generate_jwt_token(ISSUER_ID, KEY_ID, p8_key_contents)

    platform = "IOS"

    result = create_bundle_id(token, NAME, IDENTIFIER, platform)
    bundle_id = result["data"]["id"]
    print("Created Bundle ID:", bundle_id)
