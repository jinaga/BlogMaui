# jwt_helper.py
import time
import uuid
import jwt

def generate_jwt_token(issuer_id, key_id, p8_private_key, valid_minutes=20):
    """Generate a signed JWT for App Store Connect API requests."""
    issued_at = int(time.time())
    expiration_time = issued_at + (valid_minutes * 60)

    headers = {
        "alg": "ES256",
        "kid": key_id,
        "typ": "JWT"
    }

    payload = {
        "iss": issuer_id,
        "iat": issued_at,
        "exp": expiration_time,
        "aud": "appstoreconnect-v1",
        "jti": str(uuid.uuid4())
    }

    token = jwt.encode(payload, p8_private_key, algorithm="ES256", headers=headers)
    return token
