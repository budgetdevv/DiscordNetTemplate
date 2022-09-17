using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetTemplate.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TextCommandFramework
{
    // This is a minimal example of using Discord.Net's command
    // framework - by no means does it show everything the framework
    // is capable of.
    //
    // You can find samples of using the command framework:
    // - Here, under the 02_commands_framework sample
    // - https://github.com/foxbot/DiscordBotBase - a bare-bones bot template
    // - https://github.com/foxbot/patek - a more feature-filled bot, utilizing more aspects of the library
    class Program
    {
        // There is no need to implement IDisposable like before as we are
        // using dependency injection, which handles calling Dispose for us.
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public struct Config
        {
            public string Token;

            public Config(string token)
            {
                Token = token;
            }
        }
        
        public async Task MainAsync()
        {
            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            await using var services = ConfigureServices();
            
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            const string ConfigPath = "Config.json";
            
            if (!File.Exists(ConfigPath))
            {
                goto LoginFailure;
            }

            try
            {
                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hard coding.
                await client.LoginAsync(TokenType.Bot, JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(ConfigPath)).Token);
            }

            catch
            {
                goto LoginFailure;
            }

            await client.StartAsync();
            
            // Here we initialize the logic required to register our commands.
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
            
            LoginFailure:
            await File.WriteAllTextAsync(ConfigPath, JsonSerializer.Serialize(new Config("Insert token here")));

            throw new Exception("Please input bot token in Config.json!");
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }
    }
}
