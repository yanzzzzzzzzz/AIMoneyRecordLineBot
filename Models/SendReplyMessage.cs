using AIMoneyRecordLineBot.Controllers;

namespace AIMoneyRecordLineBot.Models
{
    public class SendReplyMessage
    {
        public string ReplyToken { get; set; }
        public List<Message> Messages { get; set; }
    }
}
