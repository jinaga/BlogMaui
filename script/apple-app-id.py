import os
import requests
import jwt
import time

# Load environment variables
issuer_id = os.getenv('APPLE_ISSUER_ID')
key_id = os.getenv('APPLE_KEY_ID')
private_key_path = f"keys/AuthKey_{key_id}.p8"
bundle_id = os.getenv('BUNDLE_ID')
app_name = os.getenv('APP_NAME')

# Read the private key
with open(private_key_path, 'r') as f:
    private_key = f.read()

# Create a JWT token
token = jwt.encode(
    {
        'iss': issuer_id,
        'iat': int(time.time()),
        'exp': int(time.time()) + 20 * 60,
        'aud': 'appstoreconnect-v1'
    },
    private_key,
    algorithm='ES256',
    headers={'kid': key_id}
)

# Define the API endpoint and headers
url = 'https://api.appstoreconnect.apple.com/v1/bundleIds'
headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

# Define the payload
payload = {
    'data': {
        'type': 'bundleIds',
        'attributes': {
            'identifier': bundle_id,
            'name': app_name,
            'platform': 'IOS'
        }
    }
}

# Make the request to create the App ID
response = requests.post(url, json=payload, headers=headers)

# Check the response
if response.status_code == 201:
    app_id = response.json()['data']['id']
    print(f'App ID created: {app_id}')
else:
    print(f'Error creating App ID: {response.json()}')
