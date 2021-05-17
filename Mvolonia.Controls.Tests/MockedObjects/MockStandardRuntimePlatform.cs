using System;
using System.Threading;
using Avalonia.Platform;

namespace Mvolonia.Controls.Tests.MockedObjects
{
    public class MockStandardRuntimePlatform : IRuntimePlatform
    {
        public IDisposable StartSystemTimer(TimeSpan interval, Action tick)
        {
            return new Timer(_ => tick(), null, interval, interval);
        }

        public RuntimePlatformInfo GetRuntimeInfo()
        {
            return new RuntimePlatformInfo();
        }

        public IUnmanagedBlob AllocBlob(int size)=> new UnmanagedBlob(this, size);


        private class UnmanagedBlob : IUnmanagedBlob
        {
            private bool _isDisposed = false;
            public UnmanagedBlob(MockStandardRuntimePlatform mockStandardRuntimePlatform, int size)
            {
               
            }

            public void Dispose()
            {
                _isDisposed = true;
            }

            public IntPtr Address => IntPtr.Zero;
            public int Size => 0;
            public bool IsDisposed => _isDisposed;
        }
    }
}