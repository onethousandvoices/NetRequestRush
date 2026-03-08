using System;
using System.Threading;

namespace Services
{
    public sealed class QueuedRequestFlow
    {
        private readonly Action _cancelQueuedRequest;
        private readonly Action _showLoading;
        private readonly Action _hideLoading;

        private CancellationTokenSource _cts;
        private int _version;
        private int _pendingRequestCount;

        public QueuedRequestFlow(Action cancelQueuedRequest, Action showLoading = null, Action hideLoading = null)
        {
            _cancelQueuedRequest = cancelQueuedRequest;
            _showLoading = showLoading;
            _hideLoading = hideLoading;
        }

        public void StartNewSession(out CancellationToken token, out int version)
        {
            CancelCurrent(false, false);
            _cts = new();
            version = ++_version;
            token = _cts.Token;
        }

        public bool IsCurrent(CancellationToken token, int version) => _cts != null && version == _version && !token.IsCancellationRequested;

        public void BeginTrackedRequest()
        {
            if (_cts == null || _showLoading == null)
                return;

            _pendingRequestCount++;
            _showLoading();
        }

        public void EndTrackedRequest(int version)
        {
            if (_showLoading == null)
                return;
            if (_pendingRequestCount > 0)
                _pendingRequestCount--;
            if (_pendingRequestCount > 0 || version != _version)
                return;

            _hideLoading?.Invoke();
        }

        public void Cancel(bool updateLoading) => CancelCurrent(updateLoading, true);

        private void CancelCurrent(bool updateLoading, bool advanceVersion)
        {
            if (advanceVersion)
                _version++;

            _pendingRequestCount = 0;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _cancelQueuedRequest?.Invoke();
            if (updateLoading)
                _hideLoading?.Invoke();
        }
    }
}
