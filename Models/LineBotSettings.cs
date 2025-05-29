namespace AIMoneyRecordLineBot.Models
{
    public class LineBotSettings
    {
        public string ChannelAccessToken { get; set; }
        public string OpenaiApiKey { get; set; }
        public string GeminiApiKey { get; set; }
        public string LiffId { get; set; }
    }
}
