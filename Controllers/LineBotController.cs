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
        private readonly ChatService chatService;
        private readonly string _channelAccessToken;
        private readonly AIMoneyRecordLineBotContext context;
        public LineBotController(ChatService chatService, IOptions<LineBotSettings> settings, AIMoneyRecordLineBotContext context)
        {
            this.chatService = chatService;
            this._channelAccessToken = settings.Value.ChannelAccessToken;
            this.context = context;
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

                    if (messageEvent.Message.Type == "text")
                    {
                        var expenseItems = await chatService.ProcessMoneyRecord(messageEvent.Message.Text);
                        var result = "";
                        var message = new List<Message>();
                        if(expenseItems.Count == 0)
                        {
                            message.Add(new Message
                            {
                                Type = "系統無法辨識你輸入的資訊, 請填寫如下範例: 早餐200, 電話費499, 健身房50",
                                Text = result
                            });
                        }
                        else
                        {
                            var expenseRecords = new List<ExpenseRecord>();

                            var nowTime = DateTime.UtcNow;
                            foreach (var expense in expenseItems)
                            {
                                expenseRecords.Add(new ExpenseRecord
                                {
                                    Id = 0,
                                    Source = "Line",
                                    Amount = expense.Amount,
                                    Category = expense.Category,
                                    Description = expense.Description,
                                    ConsumptionTime = expense.ConsumptionTime ?? nowTime,
                                    CreateDateTime = nowTime,
                                    UserId = user.Id,
                                });
                            }
                            await context.ExpenseRecords.AddRangeAsync(expenseRecords);
                            await context.SaveChangesAsync();

                            var groupedRecords = expenseRecords
                                .GroupBy(r => r.ConsumptionTime.ToLocalTime().Date)
                                .OrderBy(g => g.Key)
                                .ToList();
                            var bubbles = groupedRecords.Select(group =>
                            {
                                var dateText = group.Key.ToString("yyyy/MM/dd");

                                var contents = new List<FlexComponent>
                                {
                                    new FlexText { Text = $" {dateText}", Weight = "bold", Size = "lg", Margin = "md" }
                                };

                                foreach (var record in group)
                                {
                                    contents.Add(new FlexBox
                                    {
                                        Layout = "vertical",
                                        Margin = "sm",
                                        Contents = new List<FlexComponent>
                                        {
                                            new FlexText { Text = $"• {record.Description}（{record.Category ?? "未分類"}）", Wrap = true },
                                            new FlexText { Text = $"金額：{record.Amount} 元", Size = "sm", Color = "#888888" }
                                        }
                                    });
                                }

                                return new FlexBubble
                                {
                                    Body = new FlexBox
                                    {
                                        Layout = "vertical",
                                        Contents = contents
                                    }
                                };
                            }).ToList();

                            var flexMessage = new Message
                            {
                                Type = "flex",
                                AltText = "以下是你填寫的消費資訊",
                                Contents = new FlexCarousel
                                {
                                    Contents = bubbles
                                }
                            };
                            message.Add(flexMessage);
                        }
                        await lineService.MessageReply(new SendReplyMessage
                        {
                            ReplyToken = messageEvent.ReplyToken,
                            Messages = message
                        });
                    }
                }
            }

            return Ok();
        }
    }


}
