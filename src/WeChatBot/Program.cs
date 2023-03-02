using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Bot.Builder;
using Serilog;
using WeChatAdapter;
using WeChatBot.Bots;
using WeChatBot.Controllers;

namespace WeChatBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());

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

            var openAISettings = new OpenAISetting();
            builder.Configuration.Bind("OpenAI", openAISettings);
            builder.Services.AddSingleton(openAISettings);

            // Configure hosted serivce.
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddHostedService<QueuedHostedService>();
            builder.Services.AddSingleton<WeChatAdapterWithErrorHandler>();

            // The Dialog that will be run by the bot.
            builder.Services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            builder.Services.AddTransient<IBot, ChatGPTBot>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
                .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}
            app.MapControllers();

            app.Run();
        }
    }
}