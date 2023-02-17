using Microsoft.Bot.Builder;
using WeChatAdapter;
using WeChatBot.Controllers;

namespace WeChatBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            builder.Services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            builder.Services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            builder.Services.AddSingleton<ConversationState>();

            // Load WeChat settings.
            var wechatSettings = new WeChatSettings();
            builder.Configuration.Bind("WeChatSettings", wechatSettings);
            builder.Services.AddSingleton<WeChatSettings>(wechatSettings);

            // Configure hosted serivce.
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddHostedService<QueuedHostedService>();
            builder.Services.AddSingleton<WeChatAdapterWithErrorHandler>();

            // The Dialog that will be run by the bot.
            builder.Services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            builder.Services.AddTransient<IBot, RichCardsBot>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}