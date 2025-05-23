using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIMoneyRecordLineBot.Controllers
{
    [AllowAnonymous]
    public class LiffController : Controller
    {
        private readonly string liffId;

        public LiffController()
        {
            this.liffId = "";
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
