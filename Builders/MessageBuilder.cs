using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Models;

namespace AIMoneyRecordLineBot.Builders;

public class MessageBuilder
{
    public List<Message> BuildInvalidInputMessage()
    {
        return new List<Message>
        {
            new Message
            {
                Type = "text",
                Text = "系統無法辨識你輸入的資訊, 請填寫如下範例: 早餐200, 電話費499, 健身房50"
            }
        };
    }
    public List<Message> BuildFlexMessages(List<ExpenseRecord> records)
    {
        var grouped = records.GroupBy(r => r.ConsumptionTime.ToLocalTime().Date)
                              .OrderBy(g => g.Key);
        var bubbles = grouped.Select(group =>
        {
            var components = new List<FlexComponent>
            {
                new FlexText { Text = group.Key.ToString("yyyy/MM/dd"), Size = "lg", Weight = "bold", Margin = "md" }
            };

            foreach (var record in group)
            {
                components.Add(new FlexBox
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
                    Contents = components
                }
            };
        }).ToList();

        return new List<Message>
        {
            new Message
            {
                Type = "flex",
                AltText = "以下是你填寫的消費資訊",
                Contents = new FlexCarousel { Contents = bubbles }
            }
        };
    }
}
