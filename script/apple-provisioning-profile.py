import base64
import requests
import os
from jwt_helper import generate_jwt_token

# Load these from environment variables
ISSUER_ID = os.getenv("APPLE_ISSUER_ID")
KEY_ID = os.getenv("APPLE_KEY_ID")
P8_PRIVATE_KEY_PATH = f"keys/AuthKey_{KEY_ID}.p8"  # The .p8 file from App Store Connect

# Provide the info for the provisioning profile
NAME = os.getenv("APP_NAME")
PROFILE_NAME = f"{NAME} Profile"
PROFILE_TYPE = "IOS_APP_STORE"  # For TestFlight or App Store distribution
CERTIFICATE_ID = os.getenv("CERTIFICATE_ID")  # The ID of the distribution certificate
APP_ID = os.getenv("APP_ID")
CERTIFICATE_IDS = [CERTIFICATE_ID]

def create_profile(token, profile_name, profile_type, app_id, certificate_ids):
    url = "https://api.appstoreconnect.apple.com/v1/profiles"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    body = {
        "data": {
            "type": "profiles",
            "attributes": {
                "name": profile_name,
                "profileType": profile_type
            },
            "relationships": {
                "bundleId": {
                    "data": {
                        "type": "bundleIds",
                        "id": app_id
                    }
                },
                "certificates": {
                    "data": [
                        {
                            "type": "certificates",
                            "id": cert_id
                        } for cert_id in certificate_ids
                    ]
                }
            }
        }
    }

    resp = requests.post(url, headers=headers, json=body)
    if resp.status_code >= 400:
        print(f"Error: {resp.status_code}")
        print(resp.text)
        resp.raise_for_status()  # raise an exception if there's a 4xx/5xx error
    return resp.json()

if __name__ == "__main__":
    # 1) Load your .p8 private key
    with open(P8_PRIVATE_KEY_PATH, "r") as f:
        p8_content = f.read()

    # 2) Generate a JWT token
    token = generate_jwt_token(ISSUER_ID, KEY_ID, p8_content)

    # 3) Create the provisioning profile
    response_json = create_profile(
        token,
        PROFILE_NAME,
        PROFILE_TYPE,
        APP_ID,
        CERTIFICATE_IDS
    )

    # 4) Extract the provisioning profile from the response
    profile_attributes = response_json["data"]["attributes"]
    profile_content_b64 = profile_attributes["profileContent"]
    profile_data = base64.b64decode(profile_content_b64)

    # 5) Save to a .mobileprovision file
    filename = f"keys/{PROFILE_NAME}.mobileprovision"
    with open(filename, "wb") as f:
        f.write(profile_data)

    print(f"Provisioning profile created and saved as '{filename}'")
