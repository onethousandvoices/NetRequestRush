using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public sealed class RequestQueueService : IRequestQueueService, IDisposable
    {
        private static readonly TextRequestResult CANCELLED_TEXT = new(false, null, "Cancelled");
        private static readonly BinaryRequestResult CANCELLED_BYTES = new(false, null, "Cancelled");
        private static readonly TextRequestResult DISPOSED_TEXT = new(false, null, "Disposed");
        private static readonly BinaryRequestResult DISPOSED_BYTES = new(false, null, "Disposed");

        private readonly Queue<RequestHandle> _queue = new();
        private RequestHandle _activeRequest;
        private bool _isProcessing;
        private bool _isDisposed;

        public Awaitable<TextRequestResult> EnqueueText(string url, string tag, CancellationToken ct) =>
            Enqueue(new RequestHandle<TextRequestResult>(
                url,
                tag,
                ct,
                static error => new(false, null, error),
                static request => new(true, request.downloadHandler.text, null),
                CANCELLED_TEXT,
                DISPOSED_TEXT));

        public Awaitable<BinaryRequestResult> EnqueueBytes(string url, string tag, CancellationToken ct) =>
            Enqueue(new RequestHandle<BinaryRequestResult>(
                url,
                tag,
                ct,
                static error => new(false, null, error),
                static request => new(true, request.downloadHandler.data, null),
                CANCELLED_BYTES,
                DISPOSED_BYTES));

        public void Cancel(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            if (_activeRequest is { Tag: not null } activeRequest && activeRequest.Tag == tag && !activeRequest.IsCancellationRequested(_isDisposed))
                activeRequest.CompleteCancelled();

            var count = _queue.Count;

            for (var i = 0; i < count; i++)
            {
                var request = _queue.Dequeue();

                if (request.Tag == tag && !request.IsCancellationRequested(_isDisposed))
                {
                    request.CompleteCancelled();
                    continue;
                }

                _queue.Enqueue(request);
            }
        }

        private async Awaitable ProcessQueue()
        {
            _isProcessing = true;

            while (!_isDisposed && _queue.Count > 0)
            {
                var request = _queue.Dequeue();
                _activeRequest = request;

                if (request.IsCancellationRequested(_isDisposed))
                {
                    request.CompleteCancelled();
                    ClearActiveRequest();
                    continue;
                }

                try
                {
                    await request.Execute(this);
                }
                catch (OperationCanceledException)
                {
                    if (_isDisposed)
                        request.CompleteDisposed();
                    else
                        request.CompleteCancelled();
                }
                finally
                {
                    ClearActiveRequest();
                }
            }

            _isProcessing = false;
        }

        private Awaitable<TResult> Enqueue<TResult>(RequestHandle<TResult> request)
        {
            if (_isDisposed)
            {
                request.CompleteDisposed();
                return request.Awaitable;
            }

            _queue.Enqueue(request);
            if (!_isProcessing)
                ProcessQueue().Forget();

            return request.Awaitable;
        }

        public void Dispose()
        {
            _isDisposed = true;
            _activeRequest?.CompleteDisposed();

            while (_queue.Count > 0)
            {
                var request = _queue.Dequeue();
                request.CompleteDisposed();
            }
        }

        private void ThrowIfCancellationRequested(RequestHandle queuedRequest, UnityWebRequest request)
        {
            if (!queuedRequest.IsCancellationRequested(_isDisposed))
                return;

            request.Abort();
            throw new OperationCanceledException();
        }

        private async Awaitable<TResult> ExecuteRequest<TResult>(RequestHandle<TResult> queuedRequest)
        {
            using var request = UnityWebRequest.Get(queuedRequest.Url);
            request.SetRequestHeader("User-Agent", "NetRequestRush/1.0");
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                ThrowIfCancellationRequested(queuedRequest, request);
                await Awaitable.NextFrameAsync();
            }

            ThrowIfCancellationRequested(queuedRequest, request);
            return request.result != UnityWebRequest.Result.Success
                ? queuedRequest.CreateFailureResult(request.error)
                : queuedRequest.CreateSuccessResult(request);
        }

        private void ClearActiveRequest() => _activeRequest = null;

        private abstract class RequestHandle
        {
            private bool _isCancelled;
            private bool _isDisposed;
            private readonly CancellationToken _externalToken;

            protected RequestHandle(string url, string tag, CancellationToken externalToken)
            {
                Url = url;
                Tag = tag;
                _externalToken = externalToken;
            }

            public string Url { get; }
            public string Tag { get; }

            public bool IsCancellationRequested(bool isServiceDisposed) => _isCancelled || _isDisposed || isServiceDisposed || _externalToken.IsCancellationRequested;

            public abstract Awaitable Execute(RequestQueueService service);
            public abstract void CompleteCancelled();
            public abstract void CompleteDisposed();
            protected void MarkCancelled() => _isCancelled = true;
            protected void MarkDisposed() => _isDisposed = true;
        }

        private sealed class RequestHandle<TResult> : RequestHandle
        {
            private readonly Func<string, TResult> _createFailureResult;
            private readonly Func<UnityWebRequest, TResult> _createSuccessResult;
            private readonly TResult _cancelledResult;
            private readonly TResult _disposedResult;
            private readonly AwaitableCompletionSource<TResult> _completionSource = new();

            public RequestHandle(
                string url,
                string tag,
                CancellationToken externalToken,
                Func<string, TResult> createFailureResult,
                Func<UnityWebRequest, TResult> createSuccessResult,
                TResult cancelledResult,
                TResult disposedResult) : base(url, tag, externalToken)
            {
                _createFailureResult = createFailureResult;
                _createSuccessResult = createSuccessResult;
                _cancelledResult = cancelledResult;
                _disposedResult = disposedResult;
            }

            public Awaitable<TResult> Awaitable => _completionSource.Awaitable;

            public override async Awaitable Execute(RequestQueueService service) => _completionSource.TrySetResult(await service.ExecuteRequest(this));
            public override void CompleteCancelled()
            {
                MarkCancelled();
                _completionSource.TrySetResult(_cancelledResult);
            }

            public override void CompleteDisposed()
            {
                MarkDisposed();
                _completionSource.TrySetResult(_disposedResult);
            }

            public TResult CreateFailureResult(string error) => _createFailureResult(error);
            public TResult CreateSuccessResult(UnityWebRequest request) => _createSuccessResult(request);
        }
    }
}
