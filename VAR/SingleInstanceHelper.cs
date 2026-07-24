using System;
using System.IO;
using System.Threading;

namespace VAR
{
    public class SingleInstanceHelper : IDisposable
    {
        private Mutex? _mutex;
        private bool _hasHandle = false;
        private readonly string _mutexName;

        public SingleInstanceHelper(string applicationName)
        {
            _mutexName = $"Global\\{applicationName}";
        }

        public bool TryAcquire()
        {
            try
            {
                _mutex = new Mutex(true, _mutexName, out _hasHandle);

                if (!_hasHandle)
                {
                    // Try to acquire for a short time
                    _hasHandle = _mutex.WaitOne(100, false);
                }

                return _hasHandle;
            }
            catch (AbandonedMutexException)
            {
                // Previous instance crashed, we can take over
                _hasHandle = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_hasHandle && _mutex != null)
            {
                _mutex.ReleaseMutex();
                _hasHandle = false;
            }
            _mutex?.Dispose();
        }
    }
}
