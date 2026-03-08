using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Features.Clicker
{
    public sealed class CurrencyFlyEffect : MonoBehaviour
    {
        [SerializeField] private Image _icon;

        private Sequence _sequence;
        private Pool _pool;

        public void Init(Vector3 from, Vector3 to, float duration, Pool pool)
        {
            _pool = pool;
            transform.position = from;
            gameObject.SetActiveSafe(true);

            var c = _icon.color;
            c.a = 1f;
            _icon.color = c;

            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOMove(to, duration).SetEase(Ease.InOutQuad));
            _sequence.Join(_icon.DOFade(0f, duration).SetEase(Ease.InQuad));
            _sequence.OnComplete(ReturnToPool);
        }

        private void ReturnToPool()
        {
            gameObject.SetActiveSafe(false);
            _pool?.Despawn(this);
        }

        public sealed class Pool : MemoryPool<CurrencyFlyEffect>
        {
            protected override void OnCreated(CurrencyFlyEffect item) => item.gameObject.SetActiveSafe(false);

            protected override void OnDespawned(CurrencyFlyEffect item)
            {
                if (item._sequence != null && item._sequence.IsActive())
                    item._sequence.Kill();

                item._sequence = null;
                item.gameObject.SetActiveSafe(false);
            }
        }
    }
}
