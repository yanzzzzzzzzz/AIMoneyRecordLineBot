using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace AIMoneyRecordLineBot.Services
{
    public class ChatService
    {
        private readonly string openAPIKey;
        private readonly AIMoneyRecordLineBotContext context;
        public ChatService(IOptions<LineBotSettings> settings, AIMoneyRecordLineBotContext context)
        {
            openAPIKey = settings.Value.OpenaiApiKey;
            this.context = context;
        }

        public async Task<string> ProcessMoneyRecord(string message)
        {
            var chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: openAPIKey);
            string prompt = $@"  
               請從以下文字中提取出多筆記錄，並進行分類。請根據每個項目的描述來分類金額、描述、類別、消費時間(月/日)等資料，並且使用以下格式回傳結果：  
               每筆記錄需要有 'description', 'amount', 'category', 'consumptionTime'。如果無法判斷類別，設為 '其他'。如果無法判斷消費時間，設為 null。請返回每筆資料的 JSON 格式";
            var chatMessages = new List<ChatMessage>()
               {
                   new SystemChatMessage(prompt),
                   new UserChatMessage(message),
               };
            ChatCompletion completion = await chatClient.CompleteChatAsync(chatMessages);
            string response = completion.Content[0].Text;

            var extractJson = ExtractJsonFromResponse(response);
            var expenses = JsonConvert.DeserializeObject<List<ExpenseItem>>(extractJson);
            var expenseRecords = new List<ExpenseRecord>();
            if (expenses != null)
            {
                var nowTime = DateTime.UtcNow;
                foreach (var expense in expenses)
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
                        UserId = 1, //TODO
                    });
                }
                await context.ExpenseRecords.AddRangeAsync(expenseRecords);
                await context.SaveChangesAsync();
            }

            return response;
        }
        static string ExtractJsonFromResponse(string response)
        {
            if (response.Contains("```json"))
            {
                int startIndex = response.IndexOf("```json") + 7;
                int endIndex = response.LastIndexOf("```");
                if (startIndex >= 7 && endIndex > startIndex)
                {
                    return response[startIndex..endIndex].Trim();
                }
            }

            int jsonStart = response.IndexOf('[');
            int jsonEnd = response.LastIndexOf(']');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                return response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }
            Console.WriteLine("Can't extract JSON format");
            return "";
        }
    }
}
