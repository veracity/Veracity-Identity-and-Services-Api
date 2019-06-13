
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth
{
    

    public abstract class DataProtector : IDataProtector
    {
        internal void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; private set; }
        public abstract byte[] Protect(byte[] data);
        public abstract byte[] Unprotect(byte[] data);
    }
}
