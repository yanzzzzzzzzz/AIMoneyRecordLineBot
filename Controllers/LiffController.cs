using AIMoneyRecordLineBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIMoneyRecordLineBot.Controllers
{
    [AllowAnonymous]
    public class LiffController : Controller
    {
        private readonly string liffId;

        public LiffController(IOptions<LineBotSettings> settings)
        {
            liffId = settings.Value.LiffId;
        }

        public IActionResult Index()
        {
            Console.WriteLine("index");
            return View(new LiffModel { LiffId = liffId });
        }
        public IActionResult Test()
        {
            Console.WriteLine("test");
            return View(new LiffModel { LiffId = liffId });
        }
    }
    public class LiffModel
    {
        public string LiffId { get; set; }
    }
}
