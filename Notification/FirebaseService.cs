using Area.Notification;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

public class FirebaseService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleCredential _googleCredential;
    private readonly string _projectId;

    public FirebaseService(IOptions<FirebaseSettings> firebaseSettings)
    {
        _projectId = firebaseSettings.Value.ProjectId;
        _httpClient = new HttpClient();
        _googleCredential = GoogleCredential.FromFile(firebaseSettings.Value.ServiceAccountPath)
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        return accessToken;
    }

    public async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var accessToken = await GetAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var message = new
        {
            message = new
            {
                token = deviceToken,
                notification = new
                {
                    title,
                    body
                }
            }
        };

        var jsonMessage = JsonSerializer.Serialize(message);

        var response = await _httpClient.PostAsync(
            $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send",
            new StringContent(jsonMessage, System.Text.Encoding.UTF8, "application/json")
        );

        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to send notification: {result}");
        }
    }

    internal async Task SendNotificationAsync(string token, string notificationTitle, string notificationBody, string product_Name)
    {
        throw new NotImplementedException();
    }
}