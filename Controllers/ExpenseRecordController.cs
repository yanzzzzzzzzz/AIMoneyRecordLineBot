using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIMoneyRecordLineBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseRecordController : ControllerBase
    {
        private readonly ExpenseRecordService expenseRecordService;
        public ExpenseRecordController(ExpenseRecordService expenseRecordService)
        {
            this.expenseRecordService = expenseRecordService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<ExpenseRecord>>> GetExpenseRecords(string userId)
        {
            var records = await expenseRecordService.GetExpenseRecordsByUserId(userId);
            return Ok(records);
        }
    }
}
