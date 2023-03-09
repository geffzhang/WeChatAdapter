using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace WeChatBot.Bots
{
    public class ChatGPTBot : ActivityHandler
    {
        private readonly ILogger _logger;
        private readonly OpenAISetting _openAISetting;
        protected readonly Model CHATGPT_MODEL = Model.ChatGPTTurbo;
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
            await turnContext.SendActivityAsync(MessageFactory.Text(""), cancellationToken);
        
            var api = new OpenAI_API.OpenAIAPI(_openAISetting.ApiKey);

            List<ChatMessage> chatMessages = new List<ChatMessage>
            {
                new ChatMessage("system","你是一个有用的助手。使用提供的文本来形成你的答案，尽可能使用你自己的话。将答案保持在 5 句话以内。准确、有用、简洁、清晰"),
                new ChatMessage("user", content: text)
            };

            var chatRequest = new ChatRequest()
            {
                 Model = CHATGPT_MODEL,
                 MaxTokens = 2500,
                  Temperature = 0.9,
                  TopP = 1,
                  PresencePenalty = 0.6,
                Messages =  chatMessages.ToArray()
            };
 
            var result = await api.Chat.CreateChatAsync(chatRequest);
            var choices = result.Choices;
            var replycontent = string.Join(",", choices.Select(x=>x.Message.Content).ToArray()); 

            var replyActivity = MessageFactory.Text( replycontent );

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
