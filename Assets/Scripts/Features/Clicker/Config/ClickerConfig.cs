using UnityEngine;

namespace Features.Clicker
{
    [CreateAssetMenu(fileName = "ClickerConfig", menuName = "Configs/ClickerConfig")]
    public sealed class ClickerConfig : ScriptableObject
    {
        [Header("Currency")]
        [SerializeField] private int _currencyPerClick = 1;

        [Header("Energy")]
        [SerializeField] private int _energyPerClick = 1;
        [SerializeField] private int _maxEnergy = 1000;
        [SerializeField] private int _startEnergy = 1000;
        [SerializeField] private int _energyRegenAmount = 10;
        [SerializeField] private float _energyRegenInterval = 10f;

        [Header("Auto Collect")]
        [SerializeField] private float _autoCollectInterval = 3f;

        [Header("VFX")]
        [SerializeField] private float _buttonPunchScale = 0.15f;
        [SerializeField] private float _buttonPunchDuration = 0.2f;
        [SerializeField] private float _currencyFlyDuration = 0.6f;

        public int CurrencyPerClick => _currencyPerClick;
        public int EnergyPerClick => _energyPerClick;
        public int MaxEnergy => _maxEnergy;
        public int StartEnergy => _startEnergy;
        public int EnergyRegenAmount => _energyRegenAmount;
        public float EnergyRegenInterval => _energyRegenInterval;
        public float AutoCollectInterval => _autoCollectInterval;
        public float ButtonPunchScale => _buttonPunchScale;
        public float ButtonPunchDuration => _buttonPunchDuration;
        public float CurrencyFlyDuration => _currencyFlyDuration;
    }
}
