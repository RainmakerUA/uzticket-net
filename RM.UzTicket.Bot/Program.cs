﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using RM.Lib.Hosting;
using RM.Lib.Hosting.Contracts;
using RM.Lib.StateMachine.Contracts;
using RM.Lib.Utility;
using RM.UzTicket.Telegram.Contracts;

namespace RM.UzTicket.Bot
{
	internal static class Program
	{
		private class Closure
		{
			private readonly IApplicationHost _host;

			public Closure(IApplicationHost host)
			{
				_host = host;
			}

			public void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
			{
				if (_host.State == HostState.Started)
				{
					_host.Stop(); //TODO: Timeout with Ctrl+Break
				}

				if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
				{
					Console.WriteLine("Terminating app in 10 seconds...");
					Thread.Sleep(TimeSpan.FromSeconds(10));
					Environment.Exit(0);
				}
			}
		}
		
		private static void Main(string[] args)
		{
			using (var locker = new AutoResetEvent(false))
			{
				var host = new ApplicationHost<DefaultHostInitializer>();
				var closure = new Closure(host);
				
				//Console.InputEncoding = Encoding.UTF8;
				Console.OutputEncoding = Encoding.UTF8;
				Console.CancelKeyPress += closure.CancelKeyPressHandler;


				LogFactory.SetDefaultLog(new ConsoleLog());

				host.Initialize();

				var resolver = host.Environment.Resolver;
				var telebot = resolver.Get<ITelegramBot>();

				telebot.Error += (o, e) => { };

				//var provider = resolver.Get<ISettingsProvider>();
				//var settings = provider.GetSettings();
				//_proxyProvider = resolver.Get<IProxyProvider>();

				//var bot = new TelegramBotClient(settings.TeleBotKey);
				//bot.OnMessage += BotOnOnMessage;
				//bot.OnReceiveError += (sender, eventArgs) => Console.WriteLine("Bot receive error:{0}{1}", Environment.NewLine, eventArgs.ApiRequestException);
				//bot.OnReceiveGeneralError += (sender, eventArgs) => Console.WriteLine("Bot general receive error:{0}{1}", Environment.NewLine, eventArgs.Exception);

				//host.Started += (sender, eventArgs) => bot.StartReceiving();
				//host.Stopping += (sender, eventArgs) => bot.StopReceiving();
				host.Stopped += (sender, eventArgs) => locker.Set();
/*
				var assembly = Assembly.GetExecutingAssembly();
				var asmName = assembly.GetName().Name;
				var builder = resolver.Get<IStateMachineBuilder<TestState, StateMachineStuff, string>>();
				IStateMachine<TestState, StateMachineStuff, string> sm;

				using (var str = assembly.GetManifestResourceStream($"{asmName}.Properties.testmachine.scxml"))
				{
					sm = builder.BuildFromXml(str, new StateMachineStuff());
				}
				
//				var sm = resolver.Get<IStateMachineBuilder<DayOfWeek, EventArgs, string>>()
//							.AddDefaultStates((e, dw, inp) => Console.WriteLine($"Came here by input: {inp}"), null, null)
//							.AddTransition(DayOfWeek.Sunday, DayOfWeek.Monday, (e, dw, dwNew, inp) => true)
//							.Build(EventArgs.Empty);
//				sm.MoveNext("test");
*/				
				//RunBot(bot, locker);
				host.Start();
				
				/*
				string inp;


				while (!String.IsNullOrEmpty(inp = Console.ReadLine()))
				{
					sm.MoveNext(inp);
				}
*/
				locker.WaitOne();

				Console.WriteLine("Got shutdown signal. Stopping application...");
				Console.CancelKeyPress -= closure.CancelKeyPressHandler;

				//Console.ReadLine();
			}
		}
		/*
		private static async void RunBot(TelegramBotClient bot, AutoResetEvent ev)
		{
			//var asyncLock = new Utils.AsyncLock(_locker);

			try
			{
				var me = await bot.GetMeAsync();
				Console.WriteLine("Bot online: {0}{1}Press [Ctrl+C] to stop bot", me.Username, Environment.NewLine);
				//Console.ReadLine();
				//Console.WriteLine("Got proxy: {0}", await _proxyMgr.GetProxyAsync());
				//ev.Set();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				//Console.ReadLine();
			}
			finally
			{
				//asyncLock.Dispose();
			}
			
		}
		*/
	}
}
