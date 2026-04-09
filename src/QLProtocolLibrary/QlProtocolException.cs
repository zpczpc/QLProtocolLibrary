namespace QLProtocolLibrary
{
    using System;

    public sealed class QlProtocolException : Exception
    {
        public QlProtocolException(string message)
            : base(message)
        {
        }
    }
}
