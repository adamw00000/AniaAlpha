using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Discord.Addons.Interactive;

namespace AniaAlpha
{
    static class Program
    {
        private static CommandService commands;
        private static DiscordSocketClient client;
        private static IServiceProvider services;
        private static Lavalink lavalink;

        public static async Task Main(string[] args)
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            client.Log += Client_Log;
            client.Ready += Client_Ready;
            client.Disconnected += Client_Disconnected;

            string token = Environment.GetEnvironmentVariable("ANIA_ALPHA_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("ANIA_ALPHA_TOKEN environment variable not found.");
                return;
            }

            lavalink = new Lavalink();
            lavalink.Log += Lavalink_Log;
            
            services = new ServiceCollection()
                .AddSingleton(new InteractiveService(client))
                .AddSingleton(client)
                .AddSingleton(lavalink)
                .BuildServiceProvider();

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public static async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        public static async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            if (!(messageParam is SocketUserMessage message)) return;

            if (message.Author.IsBot) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new SocketCommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private static Task Client_Disconnected(Exception arg)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }

        private static async Task Client_Ready()
        {
            var node = await lavalink.AddNodeAsync(client, new Configuration { });
        }

        private static Task Client_Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private static Task Lavalink_Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
    }
}
