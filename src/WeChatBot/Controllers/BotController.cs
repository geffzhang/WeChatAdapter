using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using System.Diagnostics;
using WeChatAdapter.Schema;
using WeChatBot.Models;

namespace WeChatBot.Controllers
{
    [Route("api/{Controller}")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly WeChatAdapterWithErrorHandler _wechatAdapter;

        public BotController(IBot bot, WeChatAdapterWithErrorHandler wechatAdapter)
        {
            _bot = bot;
            _wechatAdapter = wechatAdapter;
        }

        [HttpGet("/WeChat")]
        [HttpPost("/WeChat")]
        public async Task PostWeChatAsync([FromQuery] SecretInfo postModel)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _wechatAdapter.ProcessAsync(Request, Response, _bot, postModel);
        }
    }
}