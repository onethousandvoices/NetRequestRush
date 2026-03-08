using System.Threading;
using UnityEngine;

namespace Services
{
    public interface IRequestQueueService
    {
        Awaitable<TextRequestResult> EnqueueText(string url, string tag = null, CancellationToken ct = default);
        Awaitable<BinaryRequestResult> EnqueueBytes(string url, string tag = null, CancellationToken ct = default);
        void Cancel(string tag);
    }
}
