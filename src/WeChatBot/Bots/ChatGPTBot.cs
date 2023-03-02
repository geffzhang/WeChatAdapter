using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System.Diagnostics;

namespace WeChatBot.Bots
{
    public class ChatGPTBot : ActivityHandler
    {
        private readonly ILogger _logger;
        private readonly OpenAISetting _openAISetting;
        protected readonly Model CHATGPT_MODEL = Model.DavinciText;
        protected readonly string _endToken = "<|im_end|>";
        protected readonly string _sepToken = "<|im_sep|>";

        public ChatGPTBot(ILogger<ChatGPTBot> logger, OpenAISetting openAISetting)
        {
            _logger = logger;
            _openAISetting = openAISetting;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Debug.WriteLine("Message received: " + turnContext.Activity.Text + " || " + DateTime.Now);
            _logger.LogCritical("Message received: " + turnContext.Activity.Text + " || " + DateTime.Now);

           var text = turnContext.Activity.Text;
 
            var api = new OpenAI_API.OpenAIAPI(_openAISetting.ApiKey);
            var result = await api.Completions.CreateCompletionAsync(new CompletionRequest(text, model: CHATGPT_MODEL, max_tokens: 2000, temperature: 0.8, top_p: 1, presencePenalty: 1, stopSequences: new string[] { _endToken })); 
            var replyActivity = MessageFactory.Text(result.ToString().Trim());

           _logger.LogCritical("Message response: " + replyActivity.Text + " || " + DateTime.Now);
            Debug.WriteLine("Message response: " + replyActivity.Text + " || " + DateTime.Now);

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);

        }

        /// <summary>
        /// This method is called when there is a participant added to the chat.
        /// </summary>
        /// <param name="membersAdded">Member being added to the chat</param>
        /// <param name="turnContext">TurnContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"欢迎体验GPT"), cancellationToken);
                }
            }
        }
    }
}
