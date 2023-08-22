using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlogMaui.Authentication;

public class OAuthClient
{
    private readonly string authUrl;
    private readonly string accessTokenUrl;
    private readonly string callbackUrl;
    private readonly string clientId;
    private readonly string scope;

    private string codeVerifier = "";

    public OAuthClient(string authUrl, string accessTokenUrl, string callbackUrl, string clientId, string scope)
    {
        this.authUrl = authUrl;
        this.accessTokenUrl = accessTokenUrl;
        this.callbackUrl = callbackUrl;
        this.clientId = clientId;
        this.scope = scope;
    }

    public string BuildRequestUrl()
    {
        // Generate a random string for the code verifier
        codeVerifier = GenerateRandomString();
        string state = GenerateRandomString();

        // Hash the code verifier
        var codeVerifierBytes = Encoding.UTF8.GetBytes(codeVerifier);
        var codeChallengeBytes = SHA256.HashData(codeVerifierBytes);
        var codeChallenge = UrlSafeBase64String(codeChallengeBytes);

        // Build the authorization URL
        var builder = new UriBuilder(authUrl);
        var urlEncodedCallbackUrl = WebUtility.UrlEncode(callbackUrl);
        var urlEncodedScope = WebUtility.UrlEncode(scope);
        builder.Query = $"response_type=code&client_id={clientId}&redirect_uri={urlEncodedCallbackUrl}&scope={urlEncodedScope}&state={state}&code_challenge={codeChallenge}&code_challenge_method=S256";
        return builder.ToString();
    }

    public async Task<TokenResponse> GetTokenResponse(string code)
    {
        // Build the access token request
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, accessTokenUrl);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "redirect_uri", callbackUrl },
            { "code_verifier", codeVerifier },
            { "code", code }
        });

        // Send the access token request
        var client = new HttpClient();
        var tokenResponse = await client.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get token response: {tokenResponse.StatusCode} {tokenResponse.ReasonPhrase}");
        }
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

        // Get the access token
        var token = JsonSerializer.Deserialize<TokenResponse>(tokenContent);
        if (token == null)
        {
            throw new Exception("Unable to parse token response");
        }

        return token;
    }

    private static string GenerateRandomString()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return UrlSafeBase64String(randomBytes);
    }

    private static string UrlSafeBase64String(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}
