using AIMoneyRecordLineBot.Models;
using AIMoneyRecordLineBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AIMoneyRecordLineBot.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class LineBotController : ControllerBase
    {
        private readonly ChatService chatService;
        private readonly string _channelAccessToken;
        public LineBotController(ChatService chatService, IOptions<LineBotSettings> settings)
        {
            this.chatService = chatService;
            this._channelAccessToken = settings.Value.ChannelAccessToken;
        }

        [HttpPost]
        public async Task<ActionResult> LineWebhook(LineWebhookObject webhookObject)
        {
            Console.WriteLine("Received webhook event");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            var lineService = new LineService(httpClient);

            foreach (var webhookEvent in webhookObject.Events)
            {
                if (webhookEvent != null && webhookEvent is JsonElement eventJsonElement)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var messageEvent = JsonSerializer.Deserialize<LineWebhookEvent>(eventJsonElement.GetRawText(), options);
                    if (messageEvent == null)
                    {
                        throw new Exception("message error");
                    }

                    if (messageEvent.Message.Type == "text")
                    {
                        var expenseRecords = await chatService.ProcessMoneyRecord(messageEvent.Message.Text);
                        var result = "";
                        if(expenseRecords.Count == 0)
                        {
                            result = "系統無法辨識你輸入的資訊, 請填寫如下範例: 早餐200, 電話費499, 健身房50";
                        }
                        else
                        {
                            result = "以下是你填寫的消費資訊\n";
                            foreach (var record in expenseRecords)
                            {
                                result += $"描述: {record.Description}, 類別: {record.Category},金額: {record.Amount}, 消費時間: {record.ConsumptionTime.ToLocalTime():yyyy/MM/dd}\n";
                            }
                        }
                        await lineService.MessageReply(new SendReplyMessage
                        {
                            ReplyToken = messageEvent.ReplyToken,
                            Messages = new List<Message>
                            {
                                new Message
                                {
                                    Type = "text",
                                    Text = result
                                }
                            }
                        });
                    }
                }
            }

            return Ok();
        }
    }

    public class LineWebhookObject
    {
        public string Destination { get; set; }
        public object[] Events { get; set; }
    }

    public class LineWebhookEvent
    {
        public string ReplyToken { get; set; }
        public string Type { get; set; }
        public string Mode { get; set; }
        public long Timestamp { get; set; }
        public Source Source { get; set; }
        public string WebhookEventId { get; set; }
        public DeliveryContext DeliveryContext { get; set; }
        public Message Message { get; set; }
    }

    public class Source
    {
        public string Type { get; set; }
        public string GroupId { get; set; }
        public string UserId { get; set; }
    }

    public class DeliveryContext
    {
        public bool IsRedelivery { get; set; }
    }

    public class Message
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string QuotedMessageId { get; set; }
        public string QuoteToken { get; set; }
        public string Text { get; set; }

        public string Title { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string OriginalContentUrl { get; set; }
        public string PreviewImageUrl { get; set; }

        public string AltText { get; set; }
    }

}
