using System;

namespace RM.UzTicket.Bot
{
    public enum TestState
    {
        Unknown = 0,
        Initialized,
        Started,
    }
    
    public class StateMachineStuff
    {
        public void OnStateEnter(TestState state, string input)
        {
            Console.WriteLine($"Entered state {state} after receiving '{input}'");
        }

        public void OnStateLeave(TestState state, string input)
        {
            Console.WriteLine($"Left state {state} after receiving '{input}'");
        }

        public void OnTransitionError(TestState state, string input)
        {
            Console.WriteLine($"Cannot transition from state {state} after receiving '{input}'!");
        }

        public bool CanTransition(TestState fromState, TestState toState, string input)
        {
            var toStateName = Enum.GetName(typeof(TestState), toState);
            return toStateName.Substring(0, 4).Equals(input, StringComparison.OrdinalIgnoreCase)
                    || toStateName.Substring(0, 5).Equals(input, StringComparison.OrdinalIgnoreCase)
                    || toStateName.Equals(input, StringComparison.OrdinalIgnoreCase);
        }
    }
}