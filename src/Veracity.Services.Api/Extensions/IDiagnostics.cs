using System;
using System.Runtime.CompilerServices;

namespace Veracity.Services.Api.Extensions
{
    public interface IDiagnostics
    {
        void AddErrorTraceOuter(Exception exception);

        void AddErrorTrace(Exception exception, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath]string filepaht = null);
        void AddCounter(string serviceName, string actionName, long measure, string unit);
    }
}
