using UnityEngine;

namespace UI
{
    public sealed class LoadingIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _spinnerIcon;
        [SerializeField] private float _rotationSpeed = 360f;

        private void Update() => _spinnerIcon.Rotate(0f, 0f, -_rotationSpeed * Time.unscaledDeltaTime);

        public void Show() => gameObject.SetActiveSafe(true);
        public void Hide() => gameObject.SetActiveSafe(false);
    }
}
