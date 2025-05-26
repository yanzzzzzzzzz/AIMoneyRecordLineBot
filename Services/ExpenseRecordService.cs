using AIMoneyRecordLineBot.Builders;
using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AIMoneyRecordLineBot.Services;

public class ExpenseRecordService
{
    private readonly ChatService _chatService;
    private readonly AIMoneyRecordLineBotContext context;
    private readonly MessageBuilder _messageBuilder = new MessageBuilder();
    private readonly string _channelAccessToken;

    public ExpenseRecordService(ChatService chatService, AIMoneyRecordLineBotContext context, IOptions<LineBotSettings> settings)
    {
        _chatService = chatService;
        this.context = context;
        _channelAccessToken = settings.Value.ChannelAccessToken;
    }
    public async Task<List<Message>> HandleTextExpense(LineWebhookEvent messageEvent)
    {
        var expenseItems = await _chatService.ProcessMoneyRecord(messageEvent.Message.Text);
        var user = await context.Users.SingleOrDefaultAsync(user => user.LineUserId == messageEvent.Source.UserId);
        return await ProcessExpenseItems(expenseItems, user);
    }
    public async Task<List<Message>> HandleImageExpense(LineWebhookEvent messageEvent)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
        var lineService = new LineService(httpClient);
        var imageContent = await lineService.GetMessageContent(messageEvent.Message.Id);
        var expenseItems = await _chatService.ProcessImage(imageContent);
        var user = await context.Users.SingleOrDefaultAsync(user => user.LineUserId == messageEvent.Source.UserId);
        return await ProcessExpenseItems(expenseItems, user);
    }
    private async Task<List<Message>> ProcessExpenseItems(List<ExpenseModel> expenseItems, User user)
    {
        if (!expenseItems.Any())
            return _messageBuilder.BuildInvalidInputMessage();

        var now = DateTime.UtcNow;
        var records = expenseItems.Select(expense => new ExpenseRecord
        {
            Id = 0,
            UserId = user.Id,
            Source = "Line",
            Amount = expense.Amount,
            Category = expense.Category,
            Description = expense.Description,
            ConsumptionTime = expense.ConsumptionTime ?? now,
            CreateDateTime = now
        }).ToList();

        await context.ExpenseRecords.AddRangeAsync(records);
        return _messageBuilder.BuildFlexMessages(records);
    }
}
