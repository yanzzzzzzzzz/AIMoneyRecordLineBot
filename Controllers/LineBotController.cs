using AIMoneyRecordLineBot.Entity;
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
        private readonly string _channelAccessToken;
        private readonly AIMoneyRecordLineBotContext context;
        private readonly ExpenseRecordService expenseService;
        public LineBotController(IOptions<LineBotSettings> settings, AIMoneyRecordLineBotContext context, ExpenseRecordService expenseService)
        {
            this._channelAccessToken = settings.Value.ChannelAccessToken;
            this.context = context;
            this.expenseService = expenseService;
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
                    var user = context.Users.SingleOrDefault(x => x.LineUserId == messageEvent.Source.UserId);
                    if (user == null)
                    {
                        var userProfile = await lineService.GetUserProfile(messageEvent.Source.UserId);
                        user = new User
                        {
                            LineUserId = messageEvent.Source.UserId,
                            LineDisplayName = userProfile.DisplayName,
                            CreateDateTime = DateTime.UtcNow
                        };
                        context.Users.Add(user);
                        await context.SaveChangesAsync();
                    }
                    List<Message> replyMessages = null;
                    if (messageEvent.Message.Type == "text")
                    {
                        replyMessages = await expenseService.HandleTextExpense(messageEvent);

                    }
                    else if (messageEvent.Message.Type == "image")
                    {
                        replyMessages = await expenseService.HandleImageExpense(messageEvent);
                    }
                    if(replyMessages != null)
                    {
                        await lineService.MessageReply(new SendReplyMessage
                        {
                            ReplyToken = messageEvent.ReplyToken,
                            Messages = replyMessages,
                        });
                    }
                }
            }

            return Ok();
        }
    }


}
