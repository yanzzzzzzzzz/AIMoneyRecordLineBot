using AIMoneyRecordLineBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;


namespace AIMoneyRecordLineBot.Services;
public class LineService(HttpClient httpClient)
{
    public Task<HttpResponseMessage> MessageReply(SendReplyMessage sendReplyMessage)
    {
        var json = JsonConvert.SerializeObject(sendReplyMessage, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return httpClient.PostAsync("https://api.line.me/v2/bot/message/reply", httpContent);
    }

    public async Task<UserProfile> GetUserProfile(string userId)
    {
        var response = await httpClient.GetAsync($"https://api.line.me/v2/bot/profile/{userId}");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserProfile>(responseBody);
            if (userProfile == null)
            {
                throw new Exception("GetUserProfile Exception: Deserialized UserProfile is null.");
            }
            return userProfile;
        }
        else
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception("GetUserProfile Exception:" + responseBody);
        }
    }

    public async Task<byte[]> GetMessageContent(string messageId)
    {
        var response = await httpClient.GetAsync($"https://api-data.line.me/v2/bot/message/{messageId}/content");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to download image");
        }
        return await response.Content.ReadAsByteArrayAsync();

    }
}
