using System;

namespace Features.Clicker
{
    public sealed class ClickerModel
    {
        public event Action<int> CurrencyChanged;
        public event Action<int> EnergyChanged;

        private readonly int _maxEnergy;
        private int _currency;
        private int _energy;

        public int Currency => _currency;
        public int Energy => _energy;
        public int MaxEnergy => _maxEnergy;

        public ClickerModel(ClickerConfig config)
        {
            _maxEnergy = config.MaxEnergy;
            _energy = config.StartEnergy;
        }

        public bool TrySpendEnergy(int amount)
        {
            if (_energy < amount)
                return false;

            _energy -= amount;
            EnergyChanged?.Invoke(_energy);
            return true;
        }

        public void AddCurrency(int amount)
        {
            _currency += amount;
            CurrencyChanged?.Invoke(_currency);
        }

        public void AddEnergy(int amount)
        {
            var nextEnergy = Math.Min(_energy + amount, _maxEnergy);
            if (nextEnergy == _energy)
                return;

            _energy = nextEnergy;
            EnergyChanged?.Invoke(_energy);
        }
    }
}
