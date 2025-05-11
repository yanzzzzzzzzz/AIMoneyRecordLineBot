using AIMoneyRecordLineBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIMoneyRecordLineBot.Services
{
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
    }
}
