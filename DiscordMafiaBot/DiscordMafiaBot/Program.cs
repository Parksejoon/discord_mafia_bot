using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace DiscordMafiaBot
{
	class Program
	{
		private DiscordSocketClient client;
		private CommandService commands;
		private IServiceProvider services;

		static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();


		private async Task MainAsync()
		{
			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug
			});

			commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = true,
				DefaultRunMode = RunMode.Async,
				LogLevel = LogSeverity.Debug
			});

			services = new ServiceCollection().BuildServiceProvider();

			client.MessageReceived += Client_MessageReceived;
			await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

			client.Ready += Client_Ready;
			client.Log += Client_Log;

			string Token = "";
			using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.1", @"Data\Token.txt"), FileMode.Open, FileAccess.Read))
			using (var ReadToken = new StreamReader(Stream))
			{
				Token = ReadToken.ReadToEnd();
			}
			
			await client.LoginAsync(TokenType.Bot, Token);
			await client.StartAsync();

			await Task.Delay(-1);
		}

		private async Task Client_Log(LogMessage message)
		{
			Console.WriteLine($"{DateTime.Now} at {message.Source}] {message.Message}");
		}

		private async Task Client_Ready()
		{
			await client.SetGameAsync("Mafia Bot - test", "", ActivityType.Playing);
		}

		private async Task Client_MessageReceived(SocketMessage messageParam)
		{
			var message = messageParam as SocketUserMessage;
			var context = new SocketCommandContext(client, message);

			if (context.Message == null || context.Message.Content == "") return;
			if (context.User.IsBot) return;

			int argPos = 0;
			if (!(message.HasStringPrefix("s.", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

			var result = await commands.ExecuteAsync(context, argPos, services, MultiMatchHandling.Best);
			if (!result.IsSuccess)
			{
				Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {context.Message.Content} | Error: {result.ErrorReason}");
			}
		}
	}

}
