﻿using System;

namespace RM.UzTicket.Telegram.Contracts
{
	public enum Command
	{
		None = 0,
		Station,
		Train,
		Status,
		Reset,
#if DEBUG
		Test = 999999
#endif
	}
}
