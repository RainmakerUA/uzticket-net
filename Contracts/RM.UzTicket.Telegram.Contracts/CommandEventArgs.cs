using System;

namespace RM.UzTicket.Telegram.Contracts
{
	public sealed class CommandEventArgs : BotEventArgs
	{
		public CommandEventArgs(long sender, Command command, string[] arguments)
			: base(sender)
		{
			Command = command;
			Arguments = arguments;
		}

		public Command Command { get; }

		public string[] Arguments { get; }
	}
}
