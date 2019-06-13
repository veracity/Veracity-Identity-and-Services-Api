using System;

namespace Veracity.Common.Authentication
{
    public interface ILogger
    {
        void Error(Exception error);
        void Message(string message);
        void Message(string format, params object[] args);
    }
}