using System;
using System.Runtime.Serialization;

namespace RM.Lib.StateMachine.Contracts
{
	public class StateMachineException : ApplicationException
	{
		public StateMachineException()
		{
		}

		public StateMachineException(string message)
				: base(message)
		{
		}

		public StateMachineException(string message, Exception innerException)
				: base(message, innerException)
		{
		}

		protected StateMachineException(SerializationInfo info, StreamingContext context)
				: base(info, context)
		{
		}
	}
}
