using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace AIMoneyRecordLineBot.Services
{
    public class ChatService
    {
        private readonly string openAPIKey;
        private readonly string geminiApiKey;
        private readonly AIMoneyRecordLineBotContext context;
        public ChatService(IOptions<LineBotSettings> settings, AIMoneyRecordLineBotContext context)
        {
            openAPIKey = settings.Value.OpenaiApiKey;
            geminiApiKey = settings.Value.GeminiApiKey;
            this.context = context;
        }

        public async Task<List<ExpenseModel>> ProcessMoneyRecord(string message)
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
            var expenses = JsonConvert.DeserializeObject<List<ExpenseModel>>(extractJson);

            return expenses ?? [];
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

        public async Task<List<ExpenseModel>> ProcessImage(byte[] imageBytes)
        {
            const string API_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
            var base64Image = Convert.ToBase64String(imageBytes);
            var payload = new
            {
                contents = new[]
    {
                new
                {
                    parts = new object[]
                    {
                        new { text = @"
                                You are a financial receipt extraction model. Follow these steps carefully:

                                1. From the given image (which may be a receipt, invoice, or handwritten expense list), identify up to 30 expense records.

                                2. For each detected record, generate a JSON object containing:
                                    - ""description"": the item or service description, written in Traditional Chinese (繁體中文).
                                    - ""amount"": the monetary amount in integer or decimal.
                                    - ""category"": the spending category such as ""餐飲"", ""交通"", ""娛樂"", etc. If unclear, set to ""其他"".
                                    - ""consumptionTime"": the consumption date in ""yyyy/MM/dd"" format. If unknown, set to null.

                                3. Return all results as a JSON array.

                                Rules:
                                - Do not include any explanation, commentary, or additional text.
                                - Ignore lines such as Totals, Taxes, Discounts, Change, Payments or summary information
                                - Only output a pure JSON array.
                                - If fewer than 30 items are detected, just return the available items."
                            },
                        new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                    }
                }
            }
            };
            var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{API_ENDPOINT}?key={geminiApiKey}", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API error: {responseString}");
            }
            using JsonDocument doc = JsonDocument.Parse(responseString);
            string textResponse = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            string jsonPart = ExtractJsonFromResponse(textResponse);
            var records = JsonConvert.DeserializeObject<List<ExpenseModel>>(jsonPart);
            return records ?? [];
        }
    }
}
