using Stardust.Interstellar.Rest.Common;

namespace Veracity.Common.OAuth
{
    public interface IDataProtector
    {
        byte[] Protect(byte[] data);
        byte[] Unprotect(byte[] data);
    }

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
