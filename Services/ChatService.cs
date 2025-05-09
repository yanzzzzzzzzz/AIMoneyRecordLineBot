using AIMoneyRecordLineBot.Models;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AIMoneyRecordLineBot.Services
{
    public class ChatService
    {
        private readonly string openAPIKey;
        public ChatService(IOptions<LineBotSettings> settings)
        {
            openAPIKey = settings.Value.OpenaiApiKey;
        }

        public async Task<string> ProcessMoneyRecord(string message)
        {
            var chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: openAPIKey);
            string prompt = $@"
            請從以下文字中提取出多筆記錄，並進行分類。請根據每個項目的描述來分類金額、描述、類別、時間等資料，並且使用以下格式回傳結果：
            每筆記錄需要有 'description', 'amount', 'category'。如果無法判斷類別，設為 '其他'。請返回每筆資料的 JSON 格式";
            var chatMessages = new List<ChatMessage>()
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(message),
            };
            ChatCompletion completion = await chatClient.CompleteChatAsync(chatMessages);
            return completion.Content[0].Text;
        }
    }
}
