namespace Scripts.GameStateMachine
{
    public class BadGameStateException : System.Exception
    {
        public BadGameStateException() { }
        public BadGameStateException(string message) : base(message) { }
        public BadGameStateException(string message, System.Exception inner) : base(message, inner) { }
    }
}
