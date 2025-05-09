using AIMoneyRecordLineBot.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIMoneyRecordLineBot.Services
{
    public class LineService(HttpClient httpClient)
    {
        public Task<HttpResponseMessage> MessageReply(SendReplyMessage sendReplyMessage)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(sendReplyMessage, options);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            return httpClient.PostAsync("https://api.line.me/v2/bot/message/reply", httpContent);
        }
    }
}
