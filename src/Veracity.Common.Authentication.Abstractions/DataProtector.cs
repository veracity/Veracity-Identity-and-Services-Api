using System;
using Stardust.Particles.Collection.Arrays;
namespace Veracity.Common.Authentication
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

    public sealed class DataProtector<T> : DataProtector
    {

        private readonly T _protectorProvider;
        private readonly Func<T, byte[], byte[]> _protect;
        private readonly Func<T, byte[], byte[]> _unprotect;

        public DataProtector(T protectorProvider, Func<T, byte[], byte[]> protect, Func<T, byte[], byte[]> unprotect)
        {
            _protectorProvider = protectorProvider;
            _protect = protect;
            _unprotect = unprotect;
        }

        public override byte[] Protect(byte[] data)
        {
            if (data.IsEmpty()) return data;
            Logger.Message("protecting data!");
            return _protect.Invoke(_protectorProvider, data);
        }

        public override byte[] Unprotect(byte[] data)
        {
            if (data.IsEmpty()) return data;
            Logger.Message("unprotecting data!");
            return _unprotect.Invoke(_protectorProvider, data);
        }
    }
}