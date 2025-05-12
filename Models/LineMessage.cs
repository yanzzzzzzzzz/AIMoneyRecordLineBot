namespace AIMoneyRecordLineBot.Models;

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

    public FlexContainer Contents { get; set; }
}
public abstract class FlexContainer
{
    public string Type { get; set; }
}

public class FlexBubble : FlexContainer
{
    public FlexBubble()
    {
        Type = "bubble";
    }

    public FlexBox Body { get; set; }
    public FlexBox Header { get; set; }
    public FlexBox Footer { get; set; }
    public FlexImage Hero { get; set; }
    public FlexStyle Styles { get; set; }
}

public class FlexCarousel : FlexContainer
{
    public FlexCarousel()
    {
        Type = "carousel";
    }

    public List<FlexBubble> Contents { get; set; }
}

public abstract class FlexComponent
{
    public string Type { get; set; }
}

public class FlexBox : FlexComponent
{
    public FlexBox()
    {
        Type = "box";
    }

    public string Layout { get; set; }  // vertical / horizontal / baseline
    public string Margin { get; set; }
    public List<FlexComponent> Contents { get; set; }
}

public class FlexText : FlexComponent
{
    public FlexText()
    {
        Type = "text";
    }

    public string Text { get; set; }
    public string Size { get; set; }
    public string Weight { get; set; }
    public string Color { get; set; }
    public bool? Wrap { get; set; }
    public string Margin { get; set; }
}

public class FlexImage : FlexComponent
{
    public FlexImage()
    {
        Type = "image";
    }

    public string Url { get; set; }
    public string Size { get; set; }
    public string AspectRatio { get; set; }
    public string AspectMode { get; set; }
    public string Margin { get; set; }
}
public class FlexStyle
{
    public FlexBlockStyle Header { get; set; }
    public FlexBlockStyle Body { get; set; }
    public FlexBlockStyle Footer { get; set; }
}

public class FlexBlockStyle
{
    public string BackgroundColor { get; set; }
    public bool? Separator { get; set; }
}
