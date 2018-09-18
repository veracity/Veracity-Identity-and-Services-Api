using System;
using Stardust.Interstellar.Rest.Common;

namespace Veracity.Services.Api.AutofacAdapter
{
    public class NullLogger : ILogger
    {
        public void Error(Exception error)
        {

        }

        public void Message(string message)
        {
        }

        public void Message(string format, params object[] args)
        {

        }
    }
}